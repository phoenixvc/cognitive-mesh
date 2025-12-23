using System;
using Microsoft.Extensions.DependencyInjection;
using CognitiveMesh.Shared.Interfaces;

namespace CognitiveMesh.FoundationLayer.SemanticSearch
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the semantic search services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The updated service collection for chaining.</returns>
        /// <remarks>
        /// <para>
        /// This method registers <see cref="ISemanticSearchManager"/> with a scoped lifetime.
        /// </para>
        /// <para>
        /// <strong>Prerequisites:</strong> Before calling this method, ensure that the following dependencies are registered:
        /// <list type="bullet">
        ///   <item><description><see cref="VectorDatabaseManager"/> - The vector database manager for storing and querying embeddings.</description></item>
        ///   <item><description><see cref="ILLMClient"/> - The LLM client for generating embeddings and completions.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Failure to register these dependencies will result in runtime DI resolution errors.
        /// </para>
        /// </remarks>
        public static IServiceCollection AddSemanticSearch(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<ISemanticSearchManager, SemanticSearchManager>();

            return services;
        }
    }
}
