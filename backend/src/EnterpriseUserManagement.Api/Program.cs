using System.Text.Json.Serialization;
using EnterpriseUserManagement.Api.Health;
using EnterpriseUserManagement.Api.Middleware;
using EnterpriseUserManagement.Application;
using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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

// 2. Add CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3. Register Application & Infrastructure Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 4. Configure Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<JsonStorageHealthCheck>("json_storage", tags: new[] { "ready" });

// 5. Configure OpenAPI (.NET 9 standard)
builder.Services.AddOpenApi();

var app = builder.Build();

// 6. Global Exception Handling Middleware (must be first in the pipeline)
app.UseGlobalExceptionHandling();

// 7. Interactive API Documentation (Scalar / Swagger)
if (app.Environment.IsDevelopment())
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

// Readiness probe (Kubernetes: /health/ready - checks storage accessibility)
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// 9. Startup initialization: Seed sample records if users.json does not exist
using (var scope = app.Services.CreateScope())
{
    var storageService = scope.ServiceProvider.GetRequiredService<IJsonStorageService>();
    await storageService.EnsureInitializedAsync();
}

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
