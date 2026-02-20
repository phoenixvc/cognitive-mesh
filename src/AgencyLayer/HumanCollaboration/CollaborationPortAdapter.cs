using MetacognitiveLayer.UncertaintyQuantification;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.HumanCollaboration
{
    /// <summary>
    /// Adapter that bridges the MetacognitiveLayer's <see cref="ICollaborationPort"/>
    /// to the AgencyLayer's <see cref="ICollaborationManager"/> implementation.
    /// This preserves the dependency direction by having the AgencyLayer implement
    /// a port defined in the MetacognitiveLayer.
    /// </summary>
    public class CollaborationPortAdapter : ICollaborationPort
    {
        private readonly ICollaborationManager _collaborationManager;
        private readonly ILogger<CollaborationPortAdapter> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationPortAdapter"/> class.
        /// </summary>
        /// <param name="collaborationManager">The collaboration manager to delegate to.</param>
        /// <param name="logger">The logger instance.</param>
        public CollaborationPortAdapter(
            ICollaborationManager collaborationManager,
            ILogger<CollaborationPortAdapter> logger)
        {
            _collaborationManager = collaborationManager ?? throw new ArgumentNullException(nameof(collaborationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task CreateCollaborationSessionAsync(
            string sessionName,
            string? description,
            IEnumerable<string> participantIds,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating collaboration session via port adapter: {SessionName}", sessionName);
            await _collaborationManager.CreateSessionAsync(sessionName, description, participantIds, cancellationToken);
        }
    }
}
