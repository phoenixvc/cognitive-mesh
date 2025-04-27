using Microsoft.Azure.Cosmos;
using Azure.AI.OpenAI;
using System.Text.Json;

public class CausalUnderstandingComponent
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;
    private readonly CosmosClient _cosmosClient;
    private readonly Container _causalGraphContainer;
    private readonly FeatureFlagManager _featureFlagManager;
    
    public CausalUnderstandingComponent(
        string openAIEndpoint, 
        string openAIApiKey, 
        string completionDeployment,
        string cosmosConnectionString,
        string databaseName,
        string containerName,
        FeatureFlagManager featureFlagManager)
    {
        _openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _completionDeployment = completionDeployment;
        _cosmosClient = new CosmosClient(cosmosConnectionString);
        _causalGraphContainer = _cosmosClient.GetContainer(databaseName, containerName);
        _featureFlagManager = featureFlagManager;
    }
    
    public async Task<CausalGraph> ExtractCausalRelationsAsync(string text, string domainId)
    {
        // Step 1: Extract entities from text
        var entities = await ExtractEntitiesAsync(text);
        
        // Step 2: Extract causal relationships between entities
        var relationships = await ExtractCausalRelationshipsAsync(text, entities);
        
        // Step 3: Create causal graph
        var causalGraph = new CausalGraph
        {
            Id = Guid.NewGuid().ToString(),
            DomainId = domainId,
            SourceText = text,
            Entities = entities,
            Relationships = relationships,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        // Step 4: Store causal graph in Cosmos DB
        await _causalGraphContainer.CreateItemAsync(causalGraph, new PartitionKey(domainId));
        
        return causalGraph;
    }
    
    private async Task<List<Entity>> ExtractEntitiesAsync(string text)
    {
        // Create system prompt for entity extraction
        var systemPrompt = "You are an entity extraction system that identifies key entities (concepts, objects, actors) " +
                          "from text. Focus on entities that participate in causal relationships. " +
                          "For each entity, provide a brief description and entity type.";
                          
        var userPrompt = $"Extract key entities from the following text:\n\n{text}\n\n" +
                        "Format each entity as JSON with 'name', 'description', and 'type' fields.";
        
        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.1f,
            MaxTokens = 1000,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };
        
        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        var entitiesText = response.Value.Choices[0].Message.Content;
        
        // Parse entities from response
        return ParseEntities(entitiesText);
    }
    
    private List<Entity> ParseEntities(string entitiesText)
    {
        var entities = new List<Entity>();
        
        try
        {
            // Extract JSON objects from text
            var jsonPattern = @"\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}";
            var matches = Regex.Matches(entitiesText, jsonPattern);
            
            foreach (Match match in matches)
            {
                try
                {
                    var entity = JsonSerializer.Deserialize<Entity>(match.Value);
                    if (entity != null && !string.IsNullOrEmpty(entity.Name))
                    {
                        entity.Id = Guid.NewGuid().ToString();
                        entities.Add(entity);
                    }
                }
                catch (JsonException)
                {
                    // Skip invalid JSON
                    continue;
                }
            }
        }
        catch (Exception)
        {
            // Fallback to simple parsing if regex approach fails
            var lines = entitiesText.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("name") && line.Contains("description"))
                {
                    var entity = new Entity
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = ExtractValue(line, "name"),
                        Description = ExtractValue(line, "description"),
                        Type = ExtractValue(line, "type")
                    };
                    
                    if (!string.IsNullOrEmpty(entity.Name))
                    {
                        entities.Add(entity);
                    }
                }
            }
        }
        
        return entities;
    }
    
    private string ExtractValue(string line, string key)
    {
        var pattern = $"\"{key}\"\\s*:\\s*\"([^\"]+)\"";
        var match = Regex.Match(line, pattern);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
    
    private async Task<List<CausalRelationship>> ExtractCausalRelationshipsAsync(string text, List<Entity> entities)
    {
        // Format entities for prompt
        var entitiesText = new StringBuilder();
        foreach (var entity in entities)
        {
            entitiesText.AppendLine($"- {entity.Name} ({entity.Type}): {entity.Description}");
        }
        
        // Create system prompt for relationship extraction
        var systemPrompt = "You are a causal relationship extraction system that identifies cause-effect relationships " +
                          "between entities. For each relationship, identify the cause entity, effect entity, relationship type, " +
                          "strength (0.0-1.0), and evidence from the text.";
                          
        var userPrompt = $"Extract causal relationships between the following entities from this text:\n\n" +
                        $"Text: {text}\n\n" +
                        $"Entities:\n{entitiesText}\n\n" +
                        "Format each relationship as JSON with 'causeEntityName', 'effectEntityName', 'relationshipType', " +
                        "'strength', and 'evidence' fields.";
        
        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.1f,
            MaxTokens = 1500,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };
        
        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        var relationshipsText = response.Value.Choices[0].Message.Content;
        
        // Parse relationships from response
        return ParseRelationships(relationshipsText, entities);
    }
    
    private List<CausalRelationship> ParseRelationships(string relationshipsText, List<Entity> entities)
    {
        var relationships = new List<CausalRelationship>();
        
        try
        {
            // Extract JSON objects from text
            var jsonPattern = @"\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}";
            var matches = Regex.Matches(relationshipsText, jsonPattern);
            
            foreach (Match match in matches)
            {
                try
                {
                    var relationship = JsonSerializer.Deserialize<CausalRelationshipDto>(match.Value);
                    if (relationship != null && 
                        !string.IsNullOrEmpty(relationship.CauseEntityName) && 
                        !string.IsNullOrEmpty(relationship.EffectEntityName))
                    {
                        // Find entity IDs
                        var causeEntity = entities.FirstOrDefault(e => e.Name.Equals(relationship.CauseEntityName, StringComparison.OrdinalIgnoreCase));
                        var effectEntity = entities.FirstOrDefault(e => e.Name.Equals(relationship.EffectEntityName, StringComparison.OrdinalIgnoreCase));
                        
                        if (causeEntity != null && effectEntity != null)
                        {
                            relationships.Add(new CausalRelationship
                            {
                                Id = Guid.NewGuid().ToString(),
                                CauseEntityId = causeEntity.Id,
                                EffectEntityId = effectEntity.Id,
                                RelationshipType = relationship.RelationshipType,
                                Strength = relationship.Strength,
                                Evidence = relationship.Evidence
                            });
                        }
                    }
                }
                catch (JsonException)
                {
                    // Skip invalid JSON
                    continue;
                }
            }
        }
        catch (Exception)
        {
            // Fallback parsing if needed
        }
        
        return relationships;
    }
    
    public async Task<List<CausalGraph>> GetCausalGraphsForDomainAsync(string domainId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.domainId = @domainId")
            .WithParameter("@domainId", domainId);
            
        var iterator = _causalGraphContainer.GetItemQueryIterator<CausalGraph>(query);
        
        var results = new List<CausalGraph>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        
        return results;
    }
    
    public async Task<string> PerformCausalReasoningAsync(string query, string domainId)
    {
        // Step 1: Get relevant causal graphs for the domain
        var causalGraphs = await GetCausalGraphsForDomainAsync(domainId);
        
        if (!causalGraphs.Any())
        {
            return "No causal information available for this domain.";
        }
        
        // Step 2: Format causal information for reasoning
        var causalInfo = new StringBuilder();
        foreach (var graph in causalGraphs)
        {
            causalInfo.AppendLine($"--- Causal Graph from: {graph.CreatedAt:yyyy-MM-dd} ---");
            
            // Add entities
            causalInfo.AppendLine("Entities:");
            foreach (var entity in graph.Entities)
            {
                causalInfo.AppendLine($"- {entity.Name} ({entity.Type}): {entity.Description}");
            }
            
            // Add relationships
            causalInfo.AppendLine("\nCausal Relationships:");
            foreach (var rel in graph.Relationships)
            {
                var causeEntity = graph.Entities.FirstOrDefault(e => e.Id == rel.CauseEntityId);
                var effectEntity = graph.Entities.FirstOrDefault(e => e.Id == rel.EffectEntityId);
                
                if (causeEntity != null && effectEntity != null)
                {
                    causalInfo.AppendLine($"- {causeEntity.Name} → {effectEntity.Name} ({rel.RelationshipType}, Strength: {rel.Strength:F1})");
                    causalInfo.AppendLine($"  Evidence: {rel.Evidence}");
                }
            }
            
            causalInfo.AppendLine();
        }
        
        // Step 3: Perform causal reasoning
        var systemPrompt = "You are a causal reasoning system that analyzes cause-effect relationships to answer questions. " +
                          "Use the provided causal information to identify relevant factors, pathways, and potential interventions.";
                          
        var userPrompt = $"Use the following causal information to answer this question:\n\n" +
                        $"Question: {query}\n\n" +
                        $"Causal Information:\n{causalInfo}";
        
        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 1000,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };
        
        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        
        return response.Value.Choices[0].Message.Content;
    }
}

public class Entity
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
}

public class CausalRelationship
{
    public string Id { get; set; }
    public string CauseEntityId { get; set; }
    public string EffectEntityId { get; set; }
    public string RelationshipType { get; set; }
    public double Strength { get; set; }
    public string Evidence { get; set; }
}

public class CausalRelationshipDto
{
    public string CauseEntityName { get; set; }
    public string EffectEntityName { get; set; }
    public string RelationshipType { get; set; }
    public double Strength { get; set; }
    public string Evidence { get; set; }
}

public class CausalGraph
{
    public string Id { get; set; }
    public string DomainId { get; set; }
    public string SourceText { get; set; }
    public List<Entity> Entities { get; set; } = new List<Entity>();
    public List<CausalRelationship> Relationships { get; set; } = new List<CausalRelationship>();
    public DateTimeOffset CreatedAt { get; set; }
}
