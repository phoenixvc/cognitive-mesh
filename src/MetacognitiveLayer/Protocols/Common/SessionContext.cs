namespace MetacognitiveLayer.Protocols.Common
{
    /// <summary>
    /// Represents the context for a session in the protocol system.
    /// Maintains state across multiple protocol interactions.
    /// </summary>
    public class SessionContext
    {
        /// <summary>
        /// Unique identifier for the session
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// User associated with this session
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Conversation ID for this session
        /// </summary>
        public string ConversationId { get; set; } = string.Empty;
        
        /// <summary>
        /// Creation time of the session
        /// </summary>
        public DateTime CreatedTime { get; private set; }
        
        /// <summary>
        /// Last access time of the session
        /// </summary>
        public DateTime LastAccessTime { get; private set; }
        
        /// <summary>
        /// Context values stored in the session
        /// </summary>
        private Dictionary<string, object> _contextValues;
        
        /// <summary>
        /// Memory entries for this session
        /// </summary>
        private Dictionary<string, string> _memory;
        
        /// <summary>
        /// Creates a new session context
        /// </summary>
        public SessionContext(string sessionId, string? userId = null, string? conversationId = null)
        {
            SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            UserId = userId ?? string.Empty;
            ConversationId = conversationId ?? string.Empty;
            CreatedTime = DateTime.UtcNow;
            LastAccessTime = CreatedTime;
            _contextValues = new Dictionary<string, object>();
            _memory = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// Updates the last access time
        /// </summary>
        public void UpdateLastAccessTime()
        {
            LastAccessTime = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Gets a context value
        /// </summary>
        public object GetContextValue(string key)
        {
            UpdateLastAccessTime();
            
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }
            
            return _contextValues.TryGetValue(key, out var value) ? value : null!;
        }
        
        /// <summary>
        /// Sets a context value
        /// </summary>
        public void SetContextValue(string key, object value)
        {
            UpdateLastAccessTime();
            
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }
            
            _contextValues[key] = value;
        }
        
        /// <summary>
        /// Gets a memory entry
        /// </summary>
        public string GetMemory(string key)
        {
            UpdateLastAccessTime();
            
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }
            
            return _memory.TryGetValue(key, out var value) ? value : null!;
        }
        
        /// <summary>
        /// Sets a memory entry
        /// </summary>
        public void SetMemory(string key, string value)
        {
            UpdateLastAccessTime();
            
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }
            
            _memory[key] = value;
        }
        
        /// <summary>
        /// Gets all memory entries
        /// </summary>
        public Dictionary<string, string> GetAllMemory()
        {
            UpdateLastAccessTime();
            return new Dictionary<string, string>(_memory);
        }
        
        /// <summary>
        /// Merges memory updates into the session
        /// </summary>
        public void MergeMemoryUpdates(Dictionary<string, string> updates)
        {
            if (updates == null)
            {
                return;
            }
            
            foreach (var kvp in updates)
            {
                SetMemory(kvp.Key, kvp.Value);
            }
        }
    }
}