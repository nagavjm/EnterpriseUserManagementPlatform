using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseUserManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
