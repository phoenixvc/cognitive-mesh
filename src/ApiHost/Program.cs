using AgencyLayer.CognitiveSandwich.Infrastructure;
using CognitiveMesh.AgencyLayer.RealTime.Infrastructure;
using CognitiveMesh.BusinessApplications.AdaptiveBalance.Infrastructure;
using CognitiveMesh.BusinessApplications.ImpactMetrics.Infrastructure;
using CognitiveMesh.BusinessApplications.NISTCompliance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Controllers — discovers controllers from all referenced assemblies
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CognitiveMesh.BusinessApplications.AdaptiveBalance.Controllers.AdaptiveBalanceController).Assembly)
    .AddApplicationPart(typeof(CognitiveMesh.BusinessApplications.NISTCompliance.Controllers.NISTComplianceController).Assembly)
    .AddApplicationPart(typeof(AgencyLayer.CognitiveSandwich.Controllers.CognitiveSandwichController).Assembly)
    .AddApplicationPart(typeof(CognitiveMesh.BusinessApplications.Compliance.Controllers.ComplianceController).Assembly)
    .AddApplicationPart(typeof(CognitiveMesh.BusinessApplications.ConvenerServices.ConvenerController).Assembly)
    .AddApplicationPart(typeof(CognitiveMesh.BusinessApplications.ImpactMetrics.Controllers.ImpactMetricsController).Assembly);

// OpenAPI document generation (serves at /openapi/v1.json)
builder.Services.AddOpenApi();

// SignalR for real-time updates
builder.Services.AddSignalR();

// Domain services
builder.Services.AddAdaptiveBalanceServices();
builder.Services.AddNISTComplianceServices();
builder.Services.AddCognitiveSandwichServices();
builder.Services.AddImpactMetricsServices();
builder.Services.AddCognitiveMeshRealTime();

// CORS — allow the Next.js frontend during development
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? (builder.Environment.IsDevelopment()
                ? ["http://localhost:3000"]
                : throw new InvalidOperationException("Cors:AllowedOrigins must be configured in non-development environments"));
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");

app.MapControllers();
app.MapCognitiveMeshHubs();

app.Run();
