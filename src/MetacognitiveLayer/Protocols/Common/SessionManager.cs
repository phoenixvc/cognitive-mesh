using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Common
{
    /// <summary>
    /// Manages sessions for the protocol system, including creation, retrieval, and cleanup.
    /// </summary>
    public class SessionManager
    {
        private readonly ConcurrentDictionary<string, SessionContext> _sessions;
        private readonly ILogger<SessionManager> _logger;
        private readonly Timer _cleanupTimer;
        private readonly TimeSpan _sessionTimeout;

        public SessionManager(ILogger<SessionManager> logger, TimeSpan? sessionTimeout = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sessions = new ConcurrentDictionary<string, SessionContext>();
            _sessionTimeout = sessionTimeout ?? TimeSpan.FromHours(1);
            
            // Start cleanup timer to remove expired sessions
            _cleanupTimer = new Timer(CleanupSessions, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Gets an existing session or creates a new one if it doesn't exist.
        /// </summary>
        public Task<SessionContext> GetOrCreateSessionAsync(string sessionId, string? userId = null, string? conversationId = null)
        {
            try
            {
                _logger.LogDebug("Getting or creating session: {SessionId}", sessionId);
                
                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
                }
                
                SessionContext session;
                
                if (_sessions.TryGetValue(sessionId, out session!))
                {
                    _logger.LogDebug("Retrieved existing session: {SessionId}", sessionId);
                    session.UpdateLastAccessTime();
                }
                else
                {
                    session = new SessionContext(sessionId, userId, conversationId);
                    if (_sessions.TryAdd(sessionId, session))
                    {
                        _logger.LogInformation("Created new session: {SessionId}", sessionId);
                    }
                    else if (_sessions.TryGetValue(sessionId, out session!))
                    {
                        _logger.LogWarning("Race condition when creating session: {SessionId}. Using existing session.", sessionId);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Failed to get or create session {sessionId}");
                    }
                }
                
                return Task.FromResult(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating session: {SessionId}", sessionId);
                throw;
            }
        }

        /// <summary>
        /// Updates a session with new context information. If the session exists in the store,
        /// it is replaced with the provided instance. If the session does not exist or has expired,
        /// it is re-added to the store.
        /// </summary>
        public Task UpdateSessionAsync(SessionContext session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (string.IsNullOrEmpty(session.SessionId))
            {
                throw new InvalidOperationException("Cannot update a session without a valid SessionId.");
            }

            _logger.LogDebug("Updating session: {SessionId}", session.SessionId);
            session.UpdateLastAccessTime();

            // Persist the (potentially modified) session back into the concurrent store.
            // AddOrUpdate ensures atomicity: if the session already exists it is replaced,
            // and if it was removed (e.g., by cleanup) it is re-added.
            _sessions.AddOrUpdate(
                session.SessionId,
                session,
                (_, _) => session);

            _logger.LogInformation("Session updated successfully: {SessionId}", session.SessionId);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes a session from the manager.
        /// </summary>
        public Task<bool> RemoveSessionAsync(string sessionId)
        {
            try
            {
                _logger.LogDebug("Removing session: {SessionId}", sessionId);
                
                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
                }
                
                bool removed = _sessions.TryRemove(sessionId, out _);
                if (removed)
                {
                    _logger.LogInformation("Removed session: {SessionId}", sessionId);
                }
                else
                {
                    _logger.LogWarning("Failed to remove session: {SessionId} (not found)", sessionId);
                }
                
                return Task.FromResult(removed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing session: {SessionId}", sessionId);
                throw;
            }
        }

        /// <summary>
        /// Gets the current session count.
        /// </summary>
        public int GetSessionCount()
        {
            return _sessions.Count;
        }

        /// <summary>
        /// Checks if a session exists.
        /// </summary>
        public bool SessionExists(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return false;
            }
            
            return _sessions.ContainsKey(sessionId);
        }

        /// <summary>
        /// Cleans up expired sessions.
        /// </summary>
        private void CleanupSessions(object? state)
        {
            try
            {
                _logger.LogInformation("Starting session cleanup");
                
                var now = DateTime.UtcNow;
                var expiredSessions = 0;
                
                foreach (var session in _sessions)
                {
                    if (now - session.Value.LastAccessTime > _sessionTimeout)
                    {
                        if (_sessions.TryRemove(session.Key, out _))
                        {
                            expiredSessions++;
                            _logger.LogInformation("Removed expired session: {SessionId}", session.Key);
                        }
                    }
                }
                
                _logger.LogInformation("Session cleanup completed. Removed {ExpiredSessions} expired sessions. Active sessions: {ActiveSessions}", 
                    expiredSessions, _sessions.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session cleanup");
            }
        }
    }
}