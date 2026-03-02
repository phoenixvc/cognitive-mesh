using AgencyLayer.ActionPlanning;

namespace CognitiveMesh.AgencyLayer.ActionPlanning.Events
{
    /// <summary>
    /// Notification published when a new action plan has been generated.
    /// </summary>
    /// <param name="Plan">The generated action plan.</param>
    public record PlanGeneratedNotification(ActionPlan Plan);
}
