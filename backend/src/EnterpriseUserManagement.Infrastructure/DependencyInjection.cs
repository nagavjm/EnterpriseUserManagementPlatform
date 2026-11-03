using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Infrastructure.Data;
using EnterpriseUserManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseUserManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration,
        string? environmentName = null)
    {
        var isTesting = string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase);
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase", false) || isTesting;

        if (useInMemory)
        {
            var dbName = configuration["InMemoryDatabaseName"] ?? ("EUMP_TestDb_" + Guid.NewGuid().ToString("N"));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
            });
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
                }
            });
        }

        services.AddScoped<IUserRepository, SqlUserRepository>();

        return services;
    }
}
