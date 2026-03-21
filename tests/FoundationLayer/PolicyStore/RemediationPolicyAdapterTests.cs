using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CognitiveMesh.FoundationLayer.PolicyStore.Models;
using CognitiveMesh.FoundationLayer.PolicyStore.Ports;
using CognitiveMesh.FoundationLayer.PolicyStore.Seed;

namespace CognitiveMesh.FoundationLayer.PolicyStore.Tests;

public sealed class RemediationPolicyAdapterTests
{
    private sealed class InMemoryPolicyStore : IRemediationPolicyPort
    {
        private readonly Dictionary<string, RemediationPolicy> _active = new();
        private readonly List<RemediationPolicy> _all = new();

        public Task<RemediationPolicy> GetPolicyAsync(string incidentCategory, string severity, CancellationToken ct = default)
        {
            var key = Key(incidentCategory, severity);
            return _active.TryGetValue(key, out var p)
                ? Task.FromResult(p)
                : Task.FromResult(BuildFallback(incidentCategory, severity));
        }

        public Task<IEnumerable<RemediationPolicy>> ListPoliciesAsync(CancellationToken ct = default) =>
            Task.FromResult<IEnumerable<RemediationPolicy>>(_active.Values.ToList());

        public Task<RemediationPolicy> UpsertPolicyAsync(RemediationPolicy policy, CancellationToken ct = default)
        {
            var key = Key(policy.IncidentCategory, policy.Severity);
            if (_active.TryGetValue(key, out var existing))
            {
                var archived = new RemediationPolicy
                {
                    Id = existing.Id, Version = existing.Version, IsActive = false,
                    IncidentCategory = existing.IncidentCategory, Severity = existing.Severity,
                    AllowedActions = existing.AllowedActions, RankingWeights = existing.RankingWeights,
                    MaxRetries = existing.MaxRetries,
                    RepeatedFailureEscalationThreshold = existing.RepeatedFailureEscalationThreshold,
                    HumanInLoopRequired = existing.HumanInLoopRequired,
                    ApproverRoles = existing.ApproverRoles,
                    CreatedAt = existing.CreatedAt, UpdatedAt = existing.UpdatedAt,
                    CreatedBy = existing.CreatedBy
                };
                _all.Add(archived);
                policy.Version = existing.Version + 1;
                policy.Id = Guid.NewGuid();
            }
            policy.IsActive = true;
            _active[key] = policy;
            _all.Add(policy);
            return Task.FromResult(policy);
        }

        public Task DeletePolicyAsync(Guid id, CancellationToken ct = default)
        {
            var toDelete = _active.Values.FirstOrDefault(p => p.Id == id);
            if (toDelete is not null)
            {
                toDelete.IsActive = false;
                _active.Remove(Key(toDelete.IncidentCategory, toDelete.Severity));
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<RemediationPolicy>> GetPolicyHistoryAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult<IEnumerable<RemediationPolicy>>(
                _all.Where(p => p.Id == id).OrderByDescending(p => p.Version));

        private static string Key(string cat, string sev) => $"{cat}:{sev}";

        private static RemediationPolicy BuildFallback(string cat, string sev) =>
            new()
            {
                IncidentCategory = cat, Severity = sev,
                AllowedActions = RemediationAction.Retry | RemediationAction.Escalate,
                CreatedBy = "fallback"
            };
    }

    [Fact]
    public async Task UpsertPolicyAsync_NewPolicy_SetsVersionToOne()
    {
        var store = new InMemoryPolicyStore();
        var result = await store.UpsertPolicyAsync(new RemediationPolicy
            { IncidentCategory = "infrastructure", Severity = "low", AllowedActions = RemediationAction.Retry });
        result.Version.Should().Be(1);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpsertPolicyAsync_ExistingPolicy_IncrementsVersion()
    {
        var store = new InMemoryPolicyStore();
        await store.UpsertPolicyAsync(new RemediationPolicy
            { IncidentCategory = "application", Severity = "medium", AllowedActions = RemediationAction.Retry });
        var result = await store.UpsertPolicyAsync(new RemediationPolicy
            { IncidentCategory = "application", Severity = "medium", AllowedActions = RemediationAction.Retry | RemediationAction.Rollback });
        result.Version.Should().Be(2);
    }

    [Fact]
    public async Task UpsertPolicyAsync_ExistingPolicy_DeactivatesPreviousVersion()
    {
        var store = new InMemoryPolicyStore();
        await store.UpsertPolicyAsync(new RemediationPolicy
            { IncidentCategory = "data", Severity = "high", AllowedActions = RemediationAction.Escalate });
        await store.UpsertPolicyAsync(new RemediationPolicy
            { IncidentCategory = "data", Severity = "high", AllowedActions = RemediationAction.Escalate | RemediationAction.Retry });
        var active = await store.GetPolicyAsync("data", "high");
        active.IsActive.Should().BeTrue();
        active.Version.Should().Be(2);
    }

    [Fact]
    public async Task GetPolicyAsync_NonExistentPolicy_ReturnsFallbackPolicy()
    {
        var store = new InMemoryPolicyStore();
        var result = await store.GetPolicyAsync("unknown-category", "extreme");
        result.Should().NotBeNull();
        result.AllowedActions.Should().NotBe(RemediationAction.None);
        result.CreatedBy.Should().Be("fallback");
    }

    [Fact]
    public async Task DeletePolicyAsync_ExistingPolicy_RemovesFromActiveList()
    {
        var store = new InMemoryPolicyStore();
        var saved = await store.UpsertPolicyAsync(new RemediationPolicy
            { IncidentCategory = "security", Severity = "critical", AllowedActions = RemediationAction.Escalate });
        await store.DeletePolicyAsync(saved.Id);
        var active = await store.ListPoliciesAsync();
        active.Should().NotContain(p => p.Id == saved.Id && p.IsActive);
    }

    [Fact]
    public async Task ListPoliciesAsync_AfterUpsert_ReturnsAllActivePolicies()
    {
        var store = new InMemoryPolicyStore();
        await store.UpsertPolicyAsync(new RemediationPolicy
            { IncidentCategory = "infrastructure", Severity = "low", AllowedActions = RemediationAction.Retry });
        await store.UpsertPolicyAsync(new RemediationPolicy
            { IncidentCategory = "application", Severity = "medium", AllowedActions = RemediationAction.Rollback });
        var list = await store.ListPoliciesAsync();
        list.Should().HaveCount(2);
        list.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task PolicyStoreInitializer_EmptyStore_SeedsDefaultPolicies()
    {
        var store = new InMemoryPolicyStore();
        var initializer = new PolicyStoreInitializer(store, NullLogger<PolicyStoreInitializer>.Instance);
        await initializer.InitialiseAsync();
        var all = await store.ListPoliciesAsync();
        all.Should().HaveCount(DefaultPolicySeed.GetDefaultPolicies().Count);
    }

    [Fact]
    public async Task PolicyStoreInitializer_NonEmptyStore_DoesNotDuplicateSeed()
    {
        var store = new InMemoryPolicyStore();
        await store.UpsertPolicyAsync(new RemediationPolicy
            { IncidentCategory = "infrastructure", Severity = "low", AllowedActions = RemediationAction.Retry });
        var initializer = new PolicyStoreInitializer(store, NullLogger<PolicyStoreInitializer>.Instance);
        await initializer.InitialiseAsync();
        await initializer.InitialiseAsync();
        var all = await store.ListPoliciesAsync();
        all.Should().HaveCount(1, "store already had data; seed was skipped");
    }
}
