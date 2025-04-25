using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

public class CosmosDBManager
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    private readonly ILogger<CosmosDBManager> _logger;

    public CosmosDBManager(string connectionString, string databaseName, string containerName, ILogger<CosmosDBManager> logger)
    {
        _cosmosClient = new CosmosClient(connectionString);
        _container = _cosmosClient.GetContainer(databaseName, containerName);
        _logger = logger;
    }

    public async Task<bool> AddItemAsync<T>(T item, string partitionKey)
    {
        try
        {
            _logger.LogInformation($"Adding item to Cosmos DB: {item}");

            await _container.CreateItemAsync(item, new PartitionKey(partitionKey));

            _logger.LogInformation($"Successfully added item to Cosmos DB: {item}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to add item to Cosmos DB: {ex.Message}");
            return false;
        }
    }

    public async Task<T> GetItemAsync<T>(string id, string partitionKey)
    {
        try
        {
            _logger.LogInformation($"Retrieving item from Cosmos DB with ID: {id}");

            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));

            _logger.LogInformation($"Successfully retrieved item from Cosmos DB with ID: {id}");
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to retrieve item from Cosmos DB with ID: {id}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateItemAsync<T>(string id, T item, string partitionKey)
    {
        try
        {
            _logger.LogInformation($"Updating item in Cosmos DB with ID: {id}");

            await _container.UpsertItemAsync(item, new PartitionKey(partitionKey));

            _logger.LogInformation($"Successfully updated item in Cosmos DB with ID: {id}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to update item in Cosmos DB with ID: {id}. Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteItemAsync(string id, string partitionKey)
    {
        try
        {
            _logger.LogInformation($"Deleting item from Cosmos DB with ID: {id}");

            await _container.DeleteItemAsync<object>(id, new PartitionKey(partitionKey));

            _logger.LogInformation($"Successfully deleted item from Cosmos DB with ID: {id}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to delete item from Cosmos DB with ID: {id}. Error: {ex.Message}");
            return false;
        }
    }
}
