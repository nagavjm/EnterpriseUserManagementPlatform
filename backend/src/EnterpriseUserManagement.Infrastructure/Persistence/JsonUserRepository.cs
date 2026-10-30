using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Exceptions;

namespace EnterpriseUserManagement.Infrastructure.Persistence;

public class JsonUserRepository : IUserRepository
{
    private readonly IJsonStorageService _storageService;

    public JsonUserRepository(IJsonStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _storageService.ReadUsersAsync(cancellationToken);
        return users.AsReadOnly();
    }

    public async Task<PagedResult<User>> GetPagedAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var users = await _storageService.ReadUsersAsync(cancellationToken);
        var query = users.AsQueryable();

        // 1. Text Search across FirstName, LastName, and Email
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.FirstName.ToLowerInvariant().Contains(term) ||
                u.LastName.ToLowerInvariant().Contains(term) ||
                u.Email.ToLowerInvariant().Contains(term));
        }

        // 2. Filter by Department
        if (parameters.Department.HasValue)
        {
            query = query.Where(u => u.Department == parameters.Department.Value);
        }

        // 3. Filter by Role
        if (parameters.Role.HasValue)
        {
            query = query.Where(u => u.Role == parameters.Role.Value);
        }

        // 4. Filter by Active status
        if (parameters.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == parameters.IsActive.Value);
        }

        // 5. Total count after filtering
        var totalCount = query.Count();

        // 6. Dynamic Sorting
        query = (parameters.SortBy?.ToLowerInvariant(), parameters.SortDescending) switch
        {
            ("firstname", false) => query.OrderBy(u => u.FirstName),
            ("firstname", true) => query.OrderByDescending(u => u.FirstName),
            ("lastname", false) => query.OrderBy(u => u.LastName),
            ("lastname", true) => query.OrderByDescending(u => u.LastName),
            ("email", false) => query.OrderBy(u => u.Email),
            ("email", true) => query.OrderByDescending(u => u.Email),
            ("department", false) => query.OrderBy(u => u.Department),
            ("department", true) => query.OrderByDescending(u => u.Department),
            ("role", false) => query.OrderBy(u => u.Role),
            ("role", true) => query.OrderByDescending(u => u.Role),
            ("isactive", false) => query.OrderBy(u => u.IsActive),
            ("isactive", true) => query.OrderByDescending(u => u.IsActive),
            ("createddate", false) => query.OrderBy(u => u.CreatedDate),
            _ => query.OrderByDescending(u => u.CreatedDate)
        };

        // 7. Pagination
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 10 : (parameters.PageSize > 100 ? 100 : parameters.PageSize);

        var pagedItems = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<User>(pagedItems, totalCount, pageNumber, pageSize);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var users = await _storageService.ReadUsersAsync(cancellationToken);
        return users.FirstOrDefault(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var users = await _storageService.ReadUsersAsync(cancellationToken);
        return users.FirstOrDefault(u => u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var users = await _storageService.ReadUsersAsync(cancellationToken);

        return users.Any(u =>
            u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            (!excludeUserId.HasValue || u.Id != excludeUserId.Value));
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var users = await _storageService.ReadUsersAsync(cancellationToken);

        if (users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DuplicateEmailException(user.Email);
        }

        users.Add(user);
        await _storageService.WriteUsersAsync(users, cancellationToken);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var users = await _storageService.ReadUsersAsync(cancellationToken);
        var existingIndex = users.FindIndex(u => u.Id == user.Id);

        if (existingIndex == -1)
        {
            throw new UserNotFoundException(user.Id);
        }

        // Validate that another user doesn't already have this email
        if (users.Any(u => u.Id != user.Id && u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DuplicateEmailException(user.Email);
        }

        users[existingIndex] = user;
        await _storageService.WriteUsersAsync(users, cancellationToken);
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var users = await _storageService.ReadUsersAsync(cancellationToken);
        var userToRemove = users.FirstOrDefault(u => u.Id == id);

        if (userToRemove == null)
        {
            return false;
        }

        users.Remove(userToRemove);
        await _storageService.WriteUsersAsync(users, cancellationToken);
        return true;
    }
}

