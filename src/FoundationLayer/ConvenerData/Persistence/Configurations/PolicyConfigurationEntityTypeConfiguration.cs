using System.Text.Json;
using CognitiveMesh.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoundationLayer.ConvenerData.Persistence.Configurations
{
    /// <summary>
    /// EF Core configuration for the PolicyConfiguration entity.
    /// This defines how the policy object is mapped to the database schema.
    /// </summary>
    public class PolicyConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<PolicyConfiguration>
    {
        public void Configure(EntityTypeBuilder<PolicyConfiguration> builder)
        {
            builder.ToTable("Policies");
            builder.HasKey(p => p.PolicyId);

            // Ensure that each tenant can only have one policy of a specific version.
            builder.HasIndex(p => new { p.TenantId, p.PolicyVersion }).IsUnique();
            builder.Property(p => p.TenantId).IsRequired();
            builder.Property(p => p.PolicyVersion).IsRequired();

            // Configure JSON serialization for the Rules collection
            builder.Property(p => p.Rules)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    v => JsonSerializer.Deserialize<List<RoutingRule>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new List<RoutingRule>());

            // Configure concurrency token for optimistic concurrency
            builder.Property<byte[]>("RowVersion").IsRowVersion();
        }
    }
}
