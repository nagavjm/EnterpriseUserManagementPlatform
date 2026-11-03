using System.Text.Json.Serialization;
using EnterpriseUserManagement.Api.Middleware;
using EnterpriseUserManagement.Application;
using EnterpriseUserManagement.Infrastructure;
using EnterpriseUserManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers with String Enum support
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// 2. Add CORS for Angular frontend and cloud deployments
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure Forwarded Headers for reverse proxies, Azure Ingress, and Kubernetes Service routing
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 3. Register Application & Infrastructure Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName);

// 4. Configure Health Checks (checks Azure SQL Database readiness)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("azure_sql", tags: new[] { "ready" });

// 5. Configure OpenAPI (.NET 9 standard)
builder.Services.AddOpenApi();

var app = builder.Build();

// Enable Forwarded Headers first
app.UseForwardedHeaders();

// 6. Global Exception Handling Middleware (must be first in the pipeline)
app.UseGlobalExceptionHandling();

// 7. Interactive API Documentation (Scalar / Swagger)
// Enabled in Development or when explicitly enabled via configuration (e.g. Kubernetes ConfigMap)
var enableSwagger = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger", true);
if (enableSwagger)
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Enterprise User Management Platform API")
               .WithTheme(ScalarTheme.Moon);
    });
}

// Redirect root / and /swagger to /scalar/v1 for developer convenience
app.MapGet("/", () => Results.Redirect("/scalar/v1"));
app.MapGet("/swagger", () => Results.Redirect("/scalar/v1"));

// 8. Health Check Endpoints
// Liveness probe (Kubernetes: /health)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

// Readiness probe (Kubernetes: /health/ready - checks database accessibility)
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.UseCors("AllowAngularDev");

// In containerized/Kubernetes environments (like AKS), TLS termination occurs at the Ingress controller
// or Azure Load Balancer. Only enforce internal HTTPS redirection if explicitly enabled.
if (builder.Configuration.GetValue<bool>("EnableHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

// 9. Startup initialization: Apply EF Core Code-First migrations or ensure created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        if (dbContext.Database.IsRelational())
        {
            logger.LogInformation("Applying EF Core migrations to Azure SQL database...");
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await dbContext.Database.MigrateAsync();
            });
            logger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying EF Core migrations / initializing database.");
        throw;
    }
}

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
