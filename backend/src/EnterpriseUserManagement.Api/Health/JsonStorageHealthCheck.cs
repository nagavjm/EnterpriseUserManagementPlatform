using EnterpriseUserManagement.Application.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EnterpriseUserManagement.Api.Health;

public class JsonStorageHealthCheck : IHealthCheck
{
    private readonly IJsonStorageService _storageService;

    public JsonStorageHealthCheck(IJsonStorageService storageService)
    {
        _storageService = storageService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var isHealthy = _storageService.CheckHealth();

        if (isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("JSON file storage is accessible and write-ready."));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("JSON file storage directory or file is inaccessible."));
    }
}
