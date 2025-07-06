using Microsoft.EntityFrameworkCore;
using CognitiveMesh.FoundationLayer.ConvenerData.Entities;
using CognitiveMesh.Shared.Models;
using CognitiveMesh.FoundationLayer.ConvenerData.Persistence.Configurations;

namespace CognitiveMesh.FoundationLayer.ConvenerData.Persistence
{
    public class ConvenerDbContext : DbContext
    {
        public ConvenerDbContext(DbContextOptions<ConvenerDbContext> options) 
            : base(options)
        {
        }

        public DbSet<AgentDefinition> AgentDefinitions { get; set; }
        public DbSet<PolicyConfiguration> Policies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<AgentDefinition>(entity =>
            {
                entity.HasKey(e => e.AgentId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Version).HasDefaultValue("1.0.0").HasMaxLength(20);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                // Configure JSON serialization for complex properties
                entity.OwnsOne(e => e.Capabilities);
                entity.OwnsOne(e => e.Configuration);
                entity.OwnsOne(e => e.Metadata);
            });

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new PolicyConfigurationEntityTypeConfiguration());
        }
    }
}
