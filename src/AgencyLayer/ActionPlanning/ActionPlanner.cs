using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wolverine;
using CognitiveMesh.Shared.Interfaces;
using CognitiveMesh.Shared.Models;
using CognitiveMesh.AgencyLayer.ActionPlanning.Events;

namespace AgencyLayer.ActionPlanning
{
    /// <summary>
    /// Handles planning and orchestrating actions based on goals and constraints
    /// </summary>
    public class ActionPlanner : IActionPlanner
    {
        private readonly ILogger<ActionPlanner> _logger;
        private readonly IKnowledgeGraphManager _knowledgeGraphManager;
        private readonly ILLMClient _llmClient;
        private readonly ISemanticSearchManager _semanticSearchManager;
        private readonly IMessageBus _bus;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionPlanner"/> class.
        /// </summary>
        public ActionPlanner(
            ILogger<ActionPlanner> logger,
            IKnowledgeGraphManager knowledgeGraphManager,
            ILLMClient llmClient,
            ISemanticSearchManager semanticSearchManager,
            IMessageBus bus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _knowledgeGraphManager = knowledgeGraphManager ?? throw new ArgumentNullException(nameof(knowledgeGraphManager));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
            _semanticSearchManager = semanticSearchManager ?? throw new ArgumentNullException(nameof(semanticSearchManager));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ActionPlan>> GeneratePlanAsync(
            string goal,
            IEnumerable<string>? constraints = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(goal))
                throw new ArgumentException("Goal cannot be null or whitespace.", nameof(goal));

            try
            {
                _logger.LogInformation("Generating action plan for goal: {Goal}", goal);

                // Step 1: Retrieve context from Semantic Search
                var skillsContext = await _semanticSearchManager.SearchAsync("skills-index", goal);

                // Step 2: Retrieve structural context from Knowledge Graph
                var policyQuery = $"MATCH (n:{NodeLabels.Policy}) RETURN n LIMIT 5";
                var policyNodes = await _knowledgeGraphManager.QueryAsync(policyQuery, cancellationToken);
                var policies = policyNodes.Select(p => p["n"].ToString());
                var policiesContext = string.Join("\n", policies);

                // Step 3: Construct the prompt
                var constraintsList = constraints != null ? string.Join(", ", constraints) : "None";

                var systemPrompt = "You are an expert autonomous agent action planner. " +
                                   "Your task is to break down a high-level goal into a sequence of actionable steps (ActionPlans). " +
                                   "Output ONLY a valid JSON array of objects with fields: Name, Description, Priority (int).";

                var userPrompt = $"""
                    Goal: {goal}
                    Constraints: {constraintsList}

                    Relevant Skills/Tools:
                    {skillsContext}

                    Relevant Policies:
                    {policiesContext}

                    Generate a list of action steps to achieve the goal.
                    """;

                var messages = new[]
                {
                    new ChatMessage("system", systemPrompt),
                    new ChatMessage("user", userPrompt)
                };

                // Step 4: Call LLM
                var response = await _llmClient.GenerateChatCompletionAsync(messages, temperature: 0.3f);

                // Step 5: Parse Response
                var plans = ParsePlans(response);

                // Step 6: Persist plans
                foreach (var plan in plans)
                {
                    if (plan.Status != ActionPlanStatus.Failed)
                    {
                        await _knowledgeGraphManager.AddNodeAsync(plan.Id, plan, NodeLabels.ActionPlan, cancellationToken);
                        await _bus.PublishAsync(new PlanGeneratedNotification(plan));
                    }
                }

                return plans;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating action plan for goal: {Goal}", goal);
                throw;
            }
        }

        private IEnumerable<ActionPlan> ParsePlans(string jsonResponse)
        {
            try
            {
                var json = jsonResponse;
                if (json.Contains("```json"))
                {
                    var start = json.IndexOf("```json") + 7;
                    var end = json.LastIndexOf("```");
                    if (end > start)
                    {
                        json = json.Substring(start, end - start);
                    }
                }
                else if (json.Contains("```"))
                {
                    var start = json.IndexOf("```") + 3;
                    var end = json.LastIndexOf("```");
                    if (end > start)
                    {
                        json = json.Substring(start, end - start);
                    }
                }

                var dtos = JsonSerializer.Deserialize<List<ActionPlanDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (dtos == null) return Enumerable.Empty<ActionPlan>();

                return dtos.Select(dto => new ActionPlan
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    Status = ActionPlanStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse LLM response as JSON.");
                return new[]
                {
                    new ActionPlan
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Plan Generation Failed",
                        Description = "Could not parse the AI generated plan. Raw response: " + jsonResponse.Substring(0, Math.Min(100, jsonResponse.Length)),
                        Status = ActionPlanStatus.Failed,
                        CreatedAt = DateTime.UtcNow,
                        Error = "JSON Parsing Error"
                    }
                };
            }
        }

        private class ActionPlanDto
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int Priority { get; set; }
        }

        /// <inheritdoc/>
        public async Task<ActionPlan> ExecutePlanAsync(
            string planId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Executing action plan: {PlanId}", planId);

                // 1. Retrieve the plan from the Knowledge Graph
                var plan = await _knowledgeGraphManager.GetNodeAsync<ActionPlan>(planId, cancellationToken);

                if (plan == null)
                    throw new KeyNotFoundException($"Action plan with ID {planId} not found.");

                if (plan.Status == ActionPlanStatus.Completed)
                {
                    _logger.LogWarning("Plan {PlanId} is already completed.", planId);
                    return plan;
                }

                // 2. Update status to InProgress
                plan.Status = ActionPlanStatus.InProgress;
                await _knowledgeGraphManager.UpdateNodeAsync(planId, plan, cancellationToken);

                // 3. Execute the plan using the LLM
                try
                {
                    var result = await _llmClient.GenerateCompletionAsync(
                        plan.Description,
                        temperature: 0.3f,
                        maxTokens: 500,
                        cancellationToken: cancellationToken);

                    plan.Result = result;
                    plan.Status = ActionPlanStatus.Completed;
                    plan.CompletedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    plan.Status = ActionPlanStatus.Failed;
                    plan.Error = ex.Message;
                    throw;
                }
                finally
                {
                    // 4. Update the plan in the Knowledge Graph
                    await _knowledgeGraphManager.UpdateNodeAsync(planId, plan, cancellationToken);
                }

                // 5. Notify subscribers
                await _bus.PublishAsync(new PlanUpdatedNotification(plan));

                return plan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action plan: {PlanId}", planId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdatePlanAsync(
            ActionPlan plan,
            CancellationToken cancellationToken = default)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));

            try
            {
                _logger.LogInformation("Updating action plan: {PlanId}", plan.Id);
                await _knowledgeGraphManager.UpdateNodeAsync(plan.Id, plan, cancellationToken);
                await _bus.PublishAsync(new PlanUpdatedNotification(plan));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating action plan: {PlanId}", plan.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task CancelPlanAsync(
            string planId,
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Cancelling action plan: {PlanId}, reason: {Reason}", planId, reason);

                var plan = await _knowledgeGraphManager.GetNodeAsync<ActionPlan>(planId, cancellationToken);
                if (plan == null)
                    throw new KeyNotFoundException($"Action plan with ID {planId} not found.");

                if (plan.Status is ActionPlanStatus.Completed or ActionPlanStatus.Cancelled)
                {
                    _logger.LogWarning("Plan {PlanId} is already {Status}, cannot cancel.", planId, plan.Status);
                    return;
                }

                plan.Status = ActionPlanStatus.Cancelled;
                plan.Error = reason ?? "Cancelled by user";
                plan.CompletedAt = DateTime.UtcNow;

                await _knowledgeGraphManager.UpdateNodeAsync(planId, plan, cancellationToken);
                await _bus.PublishAsync(new PlanUpdatedNotification(plan));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling action plan: {PlanId}", planId);
                throw;
            }
        }
    }

    /// <summary>
    /// Represents an action plan with its current state and metadata.
    /// </summary>
    public class ActionPlan
    {
        /// <summary>Gets or sets the unique identifier of the plan.</summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>Gets or sets the name of the plan.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Gets or sets the description of the plan.</summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>Gets or sets the priority of the plan.</summary>
        public int Priority { get; set; }
        /// <summary>Gets or sets the current status of the plan.</summary>
        public ActionPlanStatus Status { get; set; }
        /// <summary>Gets or sets when the plan was created.</summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>Gets or sets when the plan was completed.</summary>
        public DateTime? CompletedAt { get; set; }
        /// <summary>Gets or sets the error message if the plan failed.</summary>
        public string? Error { get; set; }
        /// <summary>Gets or sets the result of the plan execution.</summary>
        public string? Result { get; set; }
    }

    /// <summary>
    /// Represents the status of an action plan.
    /// </summary>
    public enum ActionPlanStatus
    {
        /// <summary>Plan is pending execution.</summary>
        Pending,
        /// <summary>Plan is currently being executed.</summary>
        InProgress,
        /// <summary>Plan completed successfully.</summary>
        Completed,
        /// <summary>Plan execution failed.</summary>
        Failed,
        /// <summary>Plan was cancelled.</summary>
        Cancelled
    }

    /// <summary>
    /// Defines the contract for action planning operations.
    /// </summary>
    public interface IActionPlanner
    {
        /// <summary>
        /// Generates an action plan for the specified goal and constraints.
        /// </summary>
        Task<IEnumerable<ActionPlan>> GeneratePlanAsync(string goal, IEnumerable<string>? constraints = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// Executes the action plan with the specified identifier.
        /// </summary>
        Task<ActionPlan> ExecutePlanAsync(string planId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Updates an existing action plan.
        /// </summary>
        Task UpdatePlanAsync(ActionPlan plan, CancellationToken cancellationToken = default);
        /// <summary>
        /// Cancels the action plan with the specified identifier.
        /// </summary>
        Task CancelPlanAsync(string planId, string? reason = null, CancellationToken cancellationToken = default);
    }
}
