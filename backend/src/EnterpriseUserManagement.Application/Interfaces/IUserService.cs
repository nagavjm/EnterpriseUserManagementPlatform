using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;

namespace EnterpriseUserManagement.Application.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetUsersPagedAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
}

