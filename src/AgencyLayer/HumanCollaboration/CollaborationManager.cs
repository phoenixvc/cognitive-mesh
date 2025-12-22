using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CognitiveMesh.Shared.Interfaces;
using CognitiveMesh.AgencyLayer.HumanCollaboration.Features.Messages;

namespace CognitiveMesh.AgencyLayer.HumanCollaboration
{
    /// <summary>
    /// Manages human-AI collaboration workflows and interactions
    /// </summary>
    public class CollaborationManager : ICollaborationManager
    {
        private readonly ILogger<CollaborationManager> _logger;
        private readonly IKnowledgeGraphManager _knowledgeGraphManager;
        private readonly ILLMClient _llmClient;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationManager"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="knowledgeGraphManager">The knowledge graph manager.</param>
        /// <param name="llmClient">The LLM client.</param>
        /// <param name="mediator">The mediator.</param>
        public CollaborationManager(
            ILogger<CollaborationManager> logger,
            IKnowledgeGraphManager knowledgeGraphManager,
            ILLMClient llmClient,
            IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _knowledgeGraphManager = knowledgeGraphManager ?? throw new ArgumentNullException(nameof(knowledgeGraphManager));
            _llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <inheritdoc/>
        public async Task<CollaborationSession> CreateSessionAsync(
            string sessionName, 
            string? description,
            IEnumerable<string> participantIds,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionName))
                throw new ArgumentException("Session name cannot be empty", nameof(sessionName));

            try
            {
                _logger.LogInformation("Creating new collaboration session: {SessionName}", sessionName);
                
                var session = new CollaborationSession
                {
                    Id = $"collab-{Guid.NewGuid()}",
                    Name = sessionName,
                    Description = description,
                    Status = CollaborationStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Participants = new List<CollaborationParticipant>(),
                    Messages = new List<CollaborationMessage>()
                };

                // Store session in knowledge graph
                await _knowledgeGraphManager.AddNodeAsync(
                    session.Id,
                    new {
                        session.Name,
                        session.Description,
                        session.Status,
                        session.CreatedAt,
                        session.UpdatedAt
                    },
                    "CollaborationSession",
                    cancellationToken);

                // Add participants and relationships
                foreach (var participantId in participantIds)
                {
                    if (string.IsNullOrWhiteSpace(participantId)) continue;

                    await _knowledgeGraphManager.AddRelationshipAsync(
                        session.Id,
                        participantId,
                        "HAS_PARTICIPANT",
                        null,
                        cancellationToken);

                    // Note: We're not creating participant nodes here assuming they exist or are managed elsewhere
                    // Ideally we would fetch them to populate the session object fully
                }

                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating collaboration session: {SessionName}", sessionName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<CollaborationMessage> AddMessageAsync(
            string sessionId, 
            string senderId, 
            string content, 
            string messageType = "text",
            Dictionary<string, object>? metadata = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(senderId))
                throw new ArgumentException("Sender ID cannot be empty", nameof(senderId));
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be empty", nameof(content));

            try
            {
                _logger.LogInformation("Adding message to session: {SessionId}", sessionId);
                
                var command = new AddMessageCommand
                {
                    SessionId = sessionId,
                    SenderId = senderId,
                    Content = content,
                    MessageType = messageType,
                    Metadata = metadata
                };

                return await _mediator.Send(command, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding message to session: {SessionId}", sessionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CollaborationMessage>> GetSessionMessagesAsync(
            string sessionId, 
            int limit = 50, 
            string? beforeMessageId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

            try
            {
                _logger.LogInformation("Retrieving messages for session: {SessionId}", sessionId);
                
                var query = new GetSessionMessagesQuery
                {
                    SessionId = sessionId,
                    Limit = limit,
                    BeforeMessageId = beforeMessageId
                };

                return await _mediator.Send(query, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for session: {SessionId}", sessionId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateSessionStatusAsync(
            string sessionId, 
            CollaborationStatus newStatus, 
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));

            try
            {
                _logger.LogInformation("Updating status for session {SessionId} to {NewStatus}", 
                    sessionId, newStatus);
                
                await Task.Delay(50, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for session: {SessionId}", sessionId);
                throw;
            }
        }
    }

    /// <summary>
    /// Represents a collaboration session between humans and AI
    /// </summary>
    public class CollaborationSession
    {
        /// <summary>
        /// Unique identifier for the session
        /// </summary>
        public required string Id { get; set; }
        
        /// <summary>
        /// Name of the session
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Description of the session
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Current status of the session
        /// </summary>
        public CollaborationStatus Status { get; set; }
        
        /// <summary>
        /// When the session was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the session was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Participants in the session
        /// </summary>
        public List<CollaborationParticipant> Participants { get; set; } = new();
        
        /// <summary>
        /// Messages in the session
        /// </summary>
        public List<CollaborationMessage> Messages { get; set; } = new();
        
        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a participant in a collaboration session
    /// </summary>
    public class CollaborationParticipant
    {
        /// <summary>
        /// Unique identifier for the participant
        /// </summary>
        public required string Id { get; set; }
        
        /// <summary>
        /// Name of the participant
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Role of the participant
        /// </summary>
        public required string Role { get; set; }
        
        /// <summary>
        /// When the participant joined the session
        /// </summary>
        public DateTime JoinedAt { get; set; }
        
        /// <summary>
        /// When the participant was last active
        /// </summary>
        public DateTime? LastActiveAt { get; set; }
        
        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a message in a collaboration session
    /// </summary>
    public class CollaborationMessage
    {
        /// <summary>
        /// Unique identifier for the message
        /// </summary>
        public required string Id { get; set; }
        
        /// <summary>
        /// ID of the session this message belongs to
        /// </summary>
        public required string SessionId { get; set; }
        
        /// <summary>
        /// ID of the sender
        /// </summary>
        public required string SenderId { get; set; }
        
        /// <summary>
        /// Content of the message
        /// </summary>
        public required string Content { get; set; }
        
        /// <summary>
        /// Type of the message (e.g., text, image, file)
        /// </summary>
        public required string MessageType { get; set; }
        
        /// <summary>
        /// When the message was sent
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Status of a collaboration session
    /// </summary>
    public enum CollaborationStatus
    {
        /// <summary>
        /// Session is active
        /// </summary>
        Active,
        
        /// <summary>
        /// Session is paused
        /// </summary>
        Paused,
        
        /// <summary>
        /// Session is completed
        /// </summary>
        Completed,
        
        /// <summary>
        /// Session was cancelled
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Interface for managing human-AI collaboration
    /// </summary>
    public interface ICollaborationManager
    {
        /// <summary>
        /// Creates a new collaboration session
        /// </summary>
        Task<CollaborationSession> CreateSessionAsync(
            string sessionName, 
            string? description,
            IEnumerable<string> participantIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a message to a collaboration session
        /// </summary>
        Task<CollaborationMessage> AddMessageAsync(
            string sessionId, 
            string senderId, 
            string content, 
            string messageType = "text",
            Dictionary<string, object>? metadata = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets messages from a collaboration session
        /// </summary>
        Task<IEnumerable<CollaborationMessage>> GetSessionMessagesAsync(
            string sessionId, 
            int limit = 50, 
            string? beforeMessageId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the status of a collaboration session
        /// </summary>
        Task UpdateSessionStatusAsync(
            string sessionId, 
            CollaborationStatus newStatus, 
            string? reason = null,
            CancellationToken cancellationToken = default);
    }
}
