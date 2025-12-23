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

namespace CognitiveMesh.AgencyLayer.ActionPlanning
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
            IEnumerable<string> constraints = null, 
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
                var response = await _llmClient.GenerateChatCompletionAsync(messages, temperature: 0.3f, cancellationToken: cancellationToken);

                // Step 5: Parse Response
                var plans = ParsePlans(response);

                // Step 6: Persist plans
                foreach (var plan in plans)
                {
                    if (plan.Status != ActionPlanStatus.Failed)
                    {
                        await _knowledgeGraphManager.AddNodeAsync(plan.Id, plan, NodeLabels.ActionPlan, cancellationToken);
                        await _bus.PublishAsync(new PlanGeneratedNotification(plan), cancellationToken: cancellationToken);
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
            public string Name { get; set; }
            public string Description { get; set; }
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
                
                // TODO: Implement plan execution logic
                await Task.Delay(100, cancellationToken); // Simulate work
                
                return new ActionPlan
                {
                    Id = planId,
                    Name = "Executed Plan",
                    Description = "This plan has been executed",
                    Status = ActionPlanStatus.Completed,
                    CompletedAt = DateTime.UtcNow
                };
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
                
                // Update in Knowledge Graph
                await _knowledgeGraphManager.UpdateNodeAsync(plan.Id, plan, cancellationToken);

                // Notify subscribers
                await _bus.PublishAsync(new PlanUpdatedNotification(plan), cancellationToken: cancellationToken);
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
            string reason = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Cancelling action plan: {PlanId}", planId);
                
                // TODO: Implement plan cancellation logic
                await Task.Delay(50, cancellationToken); // Simulate work
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling action plan: {PlanId}", planId);
                throw;
            }
        }
    }

    /// <summary>
    /// Represents an action plan
    /// </summary>
    public class ActionPlan
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public ActionPlanStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Error { get; set; }
    }

    public enum ActionPlanStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }

    public interface IActionPlanner
    {
        Task<IEnumerable<ActionPlan>> GeneratePlanAsync(string goal, IEnumerable<string> constraints = null, CancellationToken cancellationToken = default);
        Task<ActionPlan> ExecutePlanAsync(string planId, CancellationToken cancellationToken = default);
        Task UpdatePlanAsync(ActionPlan plan, CancellationToken cancellationToken = default);
        Task CancelPlanAsync(string planId, string reason = null, CancellationToken cancellationToken = default);
    }
}
