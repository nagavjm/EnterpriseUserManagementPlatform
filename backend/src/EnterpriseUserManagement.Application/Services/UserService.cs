using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Exceptions;

namespace EnterpriseUserManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserDto>> GetUsersPagedAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var pagedUsers = await _userRepository.GetPagedAsync(parameters, cancellationToken);
        var dtos = pagedUsers.Items.Select(UserDto.FromEntity).ToList();

        return new PagedResult<UserDto>(dtos, pagedUsers.TotalCount, pagedUsers.PageNumber, pagedUsers.PageSize);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(UserDto.FromEntity).ToList();
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new UserNotFoundException(id);

        return UserDto.FromEntity(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, null, cancellationToken))
            throw new DuplicateEmailException(normalizedEmail);

        var user = User.Create(
            request.FirstName,
            request.LastName,
            normalizedEmail,
            request.Department,
            request.Role);

        var savedUser = await _userRepository.AddAsync(user, cancellationToken);
        return UserDto.FromEntity(savedUser);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new UserNotFoundException(id);

        user.UpdateProfile(request.FirstName, request.LastName, request.Department, request.Role);
        var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);

        return UserDto.FromEntity(updatedUser);
    }

    public async Task<UserDto> UpdateUserStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new UserNotFoundException(id);

        user.SetActiveStatus(isActive);
        var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);

        return UserDto.FromEntity(updatedUser);
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _userRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
            throw new UserNotFoundException(id);

        return true;
    }
}

