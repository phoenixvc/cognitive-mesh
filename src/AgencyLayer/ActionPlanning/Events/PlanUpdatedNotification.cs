using AgencyLayer.ActionPlanning;

namespace CognitiveMesh.AgencyLayer.ActionPlanning.Events
{
    /// <summary>
    /// Notification published when an existing action plan has been updated.
    /// </summary>
    /// <param name="Plan">The updated action plan.</param>
    public record PlanUpdatedNotification(ActionPlan Plan);
}
