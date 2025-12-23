namespace FoundationLayer.SemanticSearch
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSemanticSearch(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<ISemanticSearchManager, SemanticSearchManager>();

            return services;
        }
    }
}
