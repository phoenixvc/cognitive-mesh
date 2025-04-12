using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.OpenAI;

public class AzureAIStudioIntegration
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;
    private readonly EnhancedRAGSystem _ragSystem;
    private readonly MultiPerspectiveCognition _mpcSystem;

    public AzureAIStudioIntegration(
        string openAIEndpoint,
        string openAIApiKey,
        string completionDeployment,
        EnhancedRAGSystem ragSystem,
        MultiPerspectiveCognition mpcSystem)
    {
        _openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _completionDeployment = completionDeployment;
        _ragSystem = ragSystem;
        _mpcSystem = mpcSystem;
    }

    public async Task<string> ExecuteSkillAsync(string skillName, string input)
    {
        switch (skillName.ToLower())
        {
            case "rag":
                return await ExecuteRAGSkillAsync(input);
            case "mpc":
                return await ExecuteMPCSkillAsync(input);
            default:
                throw new ArgumentException($"Unknown skill: {skillName}");
        }
    }

    private async Task<string> ExecuteRAGSkillAsync(string input)
    {
        var documents = await _ragSystem.SearchAsync(input, 5);
        if (documents.Count == 0)
        {
            return "No relevant documents found.";
        }

        var context = new System.Text.StringBuilder();
        foreach (var doc in documents)
        {
            context.AppendLine($"--- Document: {doc.Title} ---");
            context.AppendLine($"Source: {doc.Source}");
            context.AppendLine(doc.Content);
            context.AppendLine();
        }

        var systemPrompt = "You are a helpful assistant that answers questions based on the provided context. " +
                           "If the answer cannot be found in the context, say that you don't know.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage($"Context:\n{context}\n\nQuestion: {input}")
            }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        return response.Value.Choices[0].Message.Content;
    }

    private async Task<string> ExecuteMPCSkillAsync(string input)
    {
        var perspectives = new List<string> { "analytical", "creative", "critical", "practical" };
        var analysis = await _mpcSystem.AnalyzeFromMultiplePerspectivesAsync(input, perspectives);

        var response = new System.Text.StringBuilder();
        response.AppendLine("--- Multi-Perspective Analysis ---");
        foreach (var result in analysis.PerspectiveResults)
        {
            response.AppendLine($"--- {result.Perspective} Perspective ---");
            response.AppendLine(result.Analysis);
            response.AppendLine();
        }
        response.AppendLine("--- Synthesis ---");
        response.AppendLine(analysis.Synthesis);

        return response.ToString();
    }

    public async Task<string> ExecutePlanAsync(string planName, string input)
    {
        switch (planName.ToLower())
        {
            case "rag":
                return await ExecuteRAGPlanAsync(input);
            case "mpc":
                return await ExecuteMPCPlanAsync(input);
            default:
                throw new ArgumentException($"Unknown plan: {planName}");
        }
    }

    private async Task<string> ExecuteRAGPlanAsync(string input)
    {
        var documents = await _ragSystem.SearchAsync(input, 5);
        if (documents.Count == 0)
        {
            return "No relevant documents found.";
        }

        var context = new System.Text.StringBuilder();
        foreach (var doc in documents)
        {
            context.AppendLine($"--- Document: {doc.Title} ---");
            context.AppendLine($"Source: {doc.Source}");
            context.AppendLine(doc.Content);
            context.AppendLine();
        }

        var systemPrompt = "You are a helpful assistant that answers questions based on the provided context. " +
                           "If the answer cannot be found in the context, say that you don't know.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage($"Context:\n{context}\n\nQuestion: {input}")
            }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        return response.Value.Choices[0].Message.Content;
    }

    private async Task<string> ExecuteMPCPlanAsync(string input)
    {
        var perspectives = new List<string> { "analytical", "creative", "critical", "practical" };
        var analysis = await _mpcSystem.AnalyzeFromMultiplePerspectivesAsync(input, perspectives);

        var response = new System.Text.StringBuilder();
        response.AppendLine("--- Multi-Perspective Analysis ---");
        foreach (var result in analysis.PerspectiveResults)
        {
            response.AppendLine($"--- {result.Perspective} Perspective ---");
            response.AppendLine(result.Analysis);
            response.AppendLine();
        }
        response.AppendLine("--- Synthesis ---");
        response.AppendLine(analysis.Synthesis);

        return response.ToString();
    }

    public void RegisterPlugins()
    {
        // Register RAG plugin
        var ragPlugin = new RAGPlugin(_ragSystem);
        PluginRegistry.Register("rag", ragPlugin);

        // Register MPC plugin
        var mpcPlugin = new MPCPlugin(_mpcSystem);
        PluginRegistry.Register("mpc", mpcPlugin);
    }
}

public class RAGPlugin : IPlugin
{
    private readonly EnhancedRAGSystem _ragSystem;

    public RAGPlugin(EnhancedRAGSystem ragSystem)
    {
        _ragSystem = ragSystem;
    }

    public async Task<string> ExecuteAsync(string input)
    {
        var documents = await _ragSystem.SearchAsync(input, 5);
        if (documents.Count == 0)
        {
            return "No relevant documents found.";
        }

        var context = new System.Text.StringBuilder();
        foreach (var doc in documents)
        {
            context.AppendLine($"--- Document: {doc.Title} ---");
            context.AppendLine($"Source: {doc.Source}");
            context.AppendLine(doc.Content);
            context.AppendLine();
        }

        var systemPrompt = "You are a helpful assistant that answers questions based on the provided context. " +
                           "If the answer cannot be found in the context, say that you don't know.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage($"Context:\n{context}\n\nQuestion: {input}")
            }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        return response.Value.Choices[0].Message.Content;
    }
}

public class MPCPlugin : IPlugin
{
    private readonly MultiPerspectiveCognition _mpcSystem;

    public MPCPlugin(MultiPerspectiveCognition mpcSystem)
    {
        _mpcSystem = mpcSystem;
    }

    public async Task<string> ExecuteAsync(string input)
    {
        var perspectives = new List<string> { "analytical", "creative", "critical", "practical" };
        var analysis = await _mpcSystem.AnalyzeFromMultiplePerspectivesAsync(input, perspectives);

        var response = new System.Text.StringBuilder();
        response.AppendLine("--- Multi-Perspective Analysis ---");
        foreach (var result in analysis.PerspectiveResults)
        {
            response.AppendLine($"--- {result.Perspective} Perspective ---");
            response.AppendLine(result.Analysis);
            response.AppendLine();
        }
        response.AppendLine("--- Synthesis ---");
        response.AppendLine(analysis.Synthesis);

        return response.ToString();
    }
}

public interface IPlugin
{
    Task<string> ExecuteAsync(string input);
}

public static class PluginRegistry
{
    private static readonly Dictionary<string, IPlugin> _plugins = new Dictionary<string, IPlugin>();

    public static void Register(string name, IPlugin plugin)
    {
        _plugins[name] = plugin;
    }

    public static IPlugin GetPlugin(string name)
    {
        return _plugins.TryGetValue(name, out var plugin) ? plugin : null;
    }
}
