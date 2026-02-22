using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages knowledge lifecycle operations (add, retrieve, update, delete) by delegating
/// to an <see cref="IKnowledgeStorePort"/> adapter. The active adapter is selected at
/// composition root time based on feature flags, removing the need for framework-specific
/// branching inside business logic.
/// </summary>
public class KnowledgeManager
{
    private readonly ILogger<KnowledgeManager> _logger;
    private readonly IKnowledgeStorePort _knowledgeStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="KnowledgeManager"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <param name="knowledgeStore">The port for knowledge storage and retrieval operations.</param>
    public KnowledgeManager(ILogger<KnowledgeManager> logger, IKnowledgeStorePort knowledgeStore)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _knowledgeStore = knowledgeStore ?? throw new ArgumentNullException(nameof(knowledgeStore));
    }

    /// <summary>
    /// Adds knowledge content with the specified identifier.
    /// </summary>
    /// <param name="knowledgeId">The unique identifier for the knowledge entry.</param>
    /// <param name="content">The knowledge content to store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the knowledge was added successfully; otherwise, <c>false</c>.</returns>
    public async Task<bool> AddKnowledgeAsync(string knowledgeId, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(knowledgeId))
            throw new ArgumentException("Knowledge ID cannot be empty", nameof(knowledgeId));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        try
        {
            _logger.LogInformation("Adding knowledge with ID: {KnowledgeId}", knowledgeId);

            var result = await _knowledgeStore.AddAsync(knowledgeId, content, cancellationToken).ConfigureAwait(false);

            if (result)
            {
                _logger.LogInformation("Successfully added knowledge with ID: {KnowledgeId}", knowledgeId);
            }
            else
            {
                _logger.LogWarning("Failed to add knowledge with ID: {KnowledgeId}", knowledgeId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add knowledge with ID: {KnowledgeId}", knowledgeId);
            return false;
        }
    }

    /// <summary>
    /// Retrieves knowledge content by its identifier.
    /// </summary>
    /// <param name="knowledgeId">The unique identifier of the knowledge to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The knowledge content, or null if not found.</returns>
    public async Task<string?> RetrieveKnowledgeAsync(string knowledgeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(knowledgeId))
            throw new ArgumentException("Knowledge ID cannot be empty", nameof(knowledgeId));

        try
        {
            _logger.LogInformation("Retrieving knowledge with ID: {KnowledgeId}", knowledgeId);

            var content = await _knowledgeStore.GetAsync(knowledgeId, cancellationToken).ConfigureAwait(false);

            if (content is not null)
            {
                _logger.LogInformation("Successfully retrieved knowledge with ID: {KnowledgeId}", knowledgeId);
            }
            else
            {
                _logger.LogWarning("Knowledge not found with ID: {KnowledgeId}", knowledgeId);
            }

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve knowledge with ID: {KnowledgeId}", knowledgeId);
            throw;
        }
    }

    /// <summary>
    /// Updates existing knowledge content with the specified identifier.
    /// </summary>
    /// <param name="knowledgeId">The unique identifier of the knowledge to update.</param>
    /// <param name="newContent">The updated knowledge content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the knowledge was updated successfully; otherwise, <c>false</c>.</returns>
    public async Task<bool> UpdateKnowledgeAsync(string knowledgeId, string newContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(knowledgeId))
            throw new ArgumentException("Knowledge ID cannot be empty", nameof(knowledgeId));
        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("New content cannot be empty", nameof(newContent));

        try
        {
            _logger.LogInformation("Updating knowledge with ID: {KnowledgeId}", knowledgeId);

            var result = await _knowledgeStore.UpdateAsync(knowledgeId, newContent, cancellationToken).ConfigureAwait(false);

            if (result)
            {
                _logger.LogInformation("Successfully updated knowledge with ID: {KnowledgeId}", knowledgeId);
            }
            else
            {
                _logger.LogWarning("Failed to update knowledge with ID: {KnowledgeId}", knowledgeId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update knowledge with ID: {KnowledgeId}", knowledgeId);
            return false;
        }
    }

    /// <summary>
    /// Deletes knowledge content by its identifier.
    /// </summary>
    /// <param name="knowledgeId">The unique identifier of the knowledge to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the knowledge was deleted successfully; otherwise, <c>false</c>.</returns>
    public async Task<bool> DeleteKnowledgeAsync(string knowledgeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(knowledgeId))
            throw new ArgumentException("Knowledge ID cannot be empty", nameof(knowledgeId));

        try
        {
            _logger.LogInformation("Deleting knowledge with ID: {KnowledgeId}", knowledgeId);

            var result = await _knowledgeStore.DeleteAsync(knowledgeId, cancellationToken).ConfigureAwait(false);

            if (result)
            {
                _logger.LogInformation("Successfully deleted knowledge with ID: {KnowledgeId}", knowledgeId);
            }
            else
            {
                _logger.LogWarning("Failed to delete knowledge with ID: {KnowledgeId}", knowledgeId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete knowledge with ID: {KnowledgeId}", knowledgeId);
            return false;
        }
    }
}
