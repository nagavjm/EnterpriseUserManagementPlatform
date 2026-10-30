using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Infrastructure.Configuration;
using EnterpriseUserManagement.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseUserManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JsonStorageOptions>(configuration.GetSection(JsonStorageOptions.SectionName));
        services.AddSingleton<IJsonStorageService, JsonStorageService>();
        services.AddScoped<IUserRepository, JsonUserRepository>();

        return services;
    }
}
