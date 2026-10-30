using EnterpriseUserManagement.Domain.Entities;

namespace EnterpriseUserManagement.Application.Interfaces;

public interface IJsonStorageService
{
    Task<List<User>> ReadUsersAsync(CancellationToken cancellationToken = default);
    Task WriteUsersAsync(List<User> users, CancellationToken cancellationToken = default);
    Task EnsureInitializedAsync(CancellationToken cancellationToken = default);
    bool CheckHealth();
}

