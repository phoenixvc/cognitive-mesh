using FoundationLayer.AzureCosmosDB.Abstractions;

namespace FoundationLayer.AzureCosmosDB
{
    /// <summary>
    /// Implementation of ICosmosDbAdapter using Microsoft.Azure.Cosmos
    /// </summary>
    public class CosmosDbAdapter : ICosmosDbAdapter, IDisposable
    {
        private readonly ILogger<CosmosDbAdapter> _logger;
        private readonly CosmosClient _cosmosClient;
        private Database _database;
        private bool _disposed = false;
        private readonly string _databaseId;
        private readonly string _containerId;
        private readonly string _partitionKeyPath;
        private readonly int _throughput;
        private readonly bool _createIfNotExists;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbAdapter"/> class.
        /// </summary>
        public CosmosDbAdapter(
            string connectionString,
            string databaseId,
            string containerId,
            string partitionKeyPath = "/id",
            int throughput = 400,
            bool createIfNotExists = true,
            ILogger<CosmosDbAdapter> logger = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            if (string.IsNullOrEmpty(databaseId))
                throw new ArgumentException("Database ID cannot be null or empty", nameof(databaseId));
            if (string.IsNullOrEmpty(containerId))
                throw new ArgumentException("Container ID cannot be null or empty", nameof(containerId));
            if (string.IsNullOrEmpty(partitionKeyPath))
                throw new ArgumentException("Partition key path cannot be null or empty", nameof(partitionKeyPath));

            _logger = logger;
            _databaseId = databaseId;
            _containerId = containerId;
            _partitionKeyPath = partitionKeyPath;
            _throughput = throughput;
            _createIfNotExists = createIfNotExists;

            var clientOptions = new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryAttemptsOnRateLimitedRequests = 9,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60)
            };

            _cosmosClient = new CosmosClient(connectionString, clientOptions);
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(string databaseId = null, string containerId = null, string partitionKeyPath = null, 
            int throughput = 400, bool createIfNotExists = true, CancellationToken cancellationToken = default)
        {
            databaseId = databaseId ?? _databaseId;
            containerId = containerId ?? _containerId;
            partitionKeyPath = partitionKeyPath ?? _partitionKeyPath;
            throughput = throughput > 0 ? throughput : _throughput;
            
            try
            {
                _logger?.LogInformation("Initializing Cosmos DB adapter for database: {DatabaseId}, container: {ContainerId}", 
                    databaseId, containerId);
                
                // Create database if it doesn't exist
                var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId, throughput: null, 
                    cancellationToken: cancellationToken);
                _database = databaseResponse.Database;
                
                _logger?.LogInformation("Database {DatabaseId} is ready", databaseId);
                
                // Create container if it doesn't exist
                if (createIfNotExists)
                {
                    await CreateContainerIfNotExistsAsync(containerId, partitionKeyPath, throughput, cancellationToken);
                }
                
                _logger?.LogInformation("Cosmos DB adapter initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing Cosmos DB adapter");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetItemAsync<T>(string id, string partitionKey, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            if (string.IsNullOrEmpty(partitionKey))
                throw new ArgumentException("Partition key cannot be null or empty", nameof(partitionKey));
                
            try
            {
                var container = _cosmosClient.GetContainer(_databaseId, _containerId);
                var response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey), 
                    cancellationToken: cancellationToken);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger?.LogWarning("Item with ID {ItemId} and partition key {PartitionKey} not found", id, partitionKey);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting item with ID {ItemId} and partition key {PartitionKey}", id, partitionKey);
                throw;
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<T> GetItemsAsync<T>(string query, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Query cannot be null or empty", nameof(query));
                
            var container = _cosmosClient.GetContainer(_databaseId, _containerId);
            var queryDefinition = new QueryDefinition(query);
            
            using var feedIterator = container.GetItemQueryIterator<T>(queryDefinition);
            
            while (feedIterator.HasMoreResults && !cancellationToken.IsCancellationRequested)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                foreach (var item in response)
                {
                    yield return item;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<T> UpsertItemAsync<T>(T item, string partitionKey, CancellationToken cancellationToken = default) where T : class
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrEmpty(partitionKey))
                throw new ArgumentException("Partition key cannot be null or empty", nameof(partitionKey));
                
            try
            {
                var container = _cosmosClient.GetContainer(_databaseId, _containerId);
                var response = await container.UpsertItemAsync(item, new PartitionKey(partitionKey), 
                    cancellationToken: cancellationToken);
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error upserting item with partition key {PartitionKey}", partitionKey);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteItemAsync<T>(string id, string partitionKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));
            if (string.IsNullOrEmpty(partitionKey))
                throw new ArgumentException("Partition key cannot be null or empty", nameof(partitionKey));
                
            try
            {
                var container = _cosmosClient.GetContainer(_databaseId, _containerId);
                await container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey), 
                    cancellationToken: cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger?.LogWarning("Item with ID {ItemId} and partition key {PartitionKey} not found for deletion", 
                    id, partitionKey);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting item with ID {ItemId} and partition key {PartitionKey}", 
                    id, partitionKey);
                throw;
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<T> QueryItemsAsync<T>(
            string query, 
            Dictionary<string, object> parameters = null, 
            string partitionKey = null,
            CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Query cannot be null or empty", nameof(query));
                
            var container = _cosmosClient.GetContainer(_databaseId, _containerId);
            var queryDefinition = new QueryDefinition(query);
            
            // Add parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                queryDefinition = queryDefinition.WithParameter(param.Key, param.Value);
            }
            
            var requestOptions = partitionKey != null 
                ? new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) } 
                : null;
            
            using var feedIterator = container.GetItemQueryIterator<T>(
                queryDefinition, 
                requestOptions: requestOptions);
            
            while (feedIterator.HasMoreResults && !cancellationToken.IsCancellationRequested)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                foreach (var item in response)
                {
                    yield return item;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<T> ExecuteStoredProcedureAsync<T>(
            string storedProcedureId, 
            string partitionKey, 
            dynamic[] procedureParams,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(storedProcedureId))
                throw new ArgumentException("Stored procedure ID cannot be null or empty", nameof(storedProcedureId));
                
            try
            {
                var container = _cosmosClient.GetContainer(_databaseId, _containerId);
                var partitionKeyValue = string.IsNullOrEmpty(partitionKey) 
                    ? PartitionKey.None 
                    : new PartitionKey(partitionKey);
                
                var response = await container.Scripts.ExecuteStoredProcedureAsync<T>(
                    storedProcedureId, 
                    partitionKeyValue, 
                    procedureParams,
                    cancellationToken: cancellationToken);
                    
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing stored procedure {StoredProcedureId}", storedProcedureId);
                throw;
            }
        }

        /// <summary>
        /// Creates a container if it doesn't exist
        /// </summary>
        public async Task<Container> CreateContainerIfNotExistsAsync(
            string containerName, 
            string partitionKeyPath = "/id",
            int? throughput = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentException("Container name cannot be null or empty", nameof(containerName));
                
            try
            {
                var containerProperties = new ContainerProperties(containerName, partitionKeyPath);
                var containerResponse = await _database.CreateContainerIfNotExistsAsync(
                    containerProperties, 
                    throughput,
                    cancellationToken: cancellationToken);
                    
                _logger?.LogInformation("Container {ContainerName} is ready", containerName);
                return containerResponse.Container;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating container {ContainerName}", containerName);
                throw;
            }
        }

        /// <summary>
        /// Gets a container reference
        /// </summary>
        public Container GetContainer(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentException("Container name cannot be null or empty", nameof(containerName));
                
            return _cosmosClient.GetContainer(_databaseId, containerName);
        }

        /// <summary>
        /// Disposes the Cosmos DB client
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the Cosmos DB client
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cosmosClient?.Dispose();
                }
                _disposed = true;
            }
        }
        
        ~CosmosDbAdapter()
        {
            Dispose(false);
        }
    }
}
