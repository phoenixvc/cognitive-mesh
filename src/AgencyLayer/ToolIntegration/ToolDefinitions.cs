using CognitiveMesh.AgencyLayer.ToolIntegration.Models;
using System.Collections.Generic;
using System.Reflection;

namespace CognitiveMesh.AgencyLayer.ToolIntegration
{
    /// <summary>
    /// Provides a central, static registry of predefined ToolDefinition objects.
    /// This replaces the need for numerous individual tool classes by defining their
    /// behavior as configuration data.
    /// </summary>
    public static class ToolDefinitions
    {
        public static ToolDefinition Classification => new()
        {
            Name = "Classification Tool",
            Description = "Performs classification on the provided data",
            ParameterKey = "data",
            LogMessage = "Performing classification on data",
            ResultTemplate = "Classification results for: {0}"
        };

        public static ToolDefinition Clustering => new()
        {
            Name = "Clustering Tool",
            Description = "Performs clustering on the provided data",
            ParameterKey = "data",
            LogMessage = "Performing clustering on data",
            ResultTemplate = "Clustering results for: {0}"
        };

        public static ToolDefinition DataAnalysis => new()
        {
            Name = "Data Analysis Tool",
            Description = "Performs analysis on the provided data",
            ParameterKey = "data",
            LogMessage = "Performing data analysis",
            ResultTemplate = "Data analysis results for: {0}"
        };

        public static ToolDefinition DataCleaning => new()
        {
            Name = "Data Cleaning Tool",
            Description = "Performs cleaning on the provided data",
            ParameterKey = "data",
            LogMessage = "Performing data cleaning",
            ResultTemplate = "Data cleaning results for: {0}"
        };

        public static ToolDefinition DataVisualization => new()
        {
            Name = "Data Visualization Tool",
            Description = "Generates visualizations for the provided data",
            ParameterKey = "data",
            LogMessage = "Generating data visualization",
            ResultTemplate = "Data visualization for: {0}"
        };

        public static ToolDefinition NamedEntityRecognition => new()
        {
            Name = "Named Entity Recognition Tool",
            Description = "Performs named entity recognition on the provided text",
            ParameterKey = "text",
            LogMessage = "Performing named entity recognition",
            ResultTemplate = "Named entity recognition results for: {0}"
        };

        public static ToolDefinition PatternRecognition => new()
        {
            Name = "Pattern Recognition Tool",
            Description = "Performs pattern recognition on the provided data",
            ParameterKey = "data",
            LogMessage = "Performing pattern recognition",
            ResultTemplate = "Pattern recognition results for: {0}"
        };

        public static ToolDefinition PredictiveAnalytics => new()
        {
            Name = "Predictive Analytics Tool",
            Description = "Performs predictive analytics on the provided data",
            ParameterKey = "data",
            LogMessage = "Performing predictive analytics",
            ResultTemplate = "Predictive analytics results for: {0}"
        };

        public static ToolDefinition RecommendationSystem => new()
        {
            Name = "Recommendation System Tool",
            Description = "Generates recommendations based on the provided data",
            ParameterKey = "data",
            LogMessage = "Generating recommendations",
            ResultTemplate = "Recommendations for: {0}"
        };

        public static ToolDefinition SentimentAnalysis => new()
        {
            Name = "Sentiment Analysis Tool",
            Description = "Analyzes sentiment in the provided text",
            ParameterKey = "text",
            LogMessage = "Performing sentiment analysis",
            ResultTemplate = "Sentiment analysis results for: {0}"
        };

        public static ToolDefinition TextGeneration => new()
        {
            Name = "Text Generation Tool",
            Description = "Generates text based on the provided prompt",
            ParameterKey = "prompt",
            LogMessage = "Performing text generation",
            ResultTemplate = "Generated text for prompt: {0}"
        };

        public static ToolDefinition WebScraping => new()
        {
            Name = "Web Scraping Tool",
            Description = "Scrapes content from the provided URL",
            ParameterKey = "url",
            LogMessage = "Scraping content from URL",
            ResultTemplate = "Scraped content from: {0}"
        };

        public static ToolDefinition WebSearch => new()
        {
            Name = "Web Search Tool",
            Description = "Performs web searches based on the provided query",
            ParameterKey = "query",
            LogMessage = "Performing web search for query: {0}",
            ResultTemplate = "Search results for: {0}"
        };

        /// <summary>
        /// Retrieves all predefined tool definitions using reflection.
        /// </summary>
        /// <returns>An enumerable collection of all tool definitions.</returns>
        public static IEnumerable<ToolDefinition> GetAll()
        {
            var properties = typeof(ToolDefinitions).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(ToolDefinition))
                {
                    yield return (ToolDefinition)property.GetValue(null);
                }
            }
        }
    }
}
