using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Exceptions;
using EnterpriseUserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseUserManagement.Infrastructure.Persistence;

public class SqlUserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public SqlUserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<User>> GetPagedAsync(UserQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        // 1. Text Search across FirstName, LastName, and Email
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
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
        var totalCount = await query.CountAsync(cancellationToken);

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

        var pagedItems = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<User>(pagedItems, totalCount, pageNumber, pageSize);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Users
            .AnyAsync(u =>
                u.Email.ToLower() == normalizedEmail &&
                (!excludeUserId.HasValue || u.Id != excludeUserId.Value),
                cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = user.Email.Trim().ToLowerInvariant();
        var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
        if (emailExists)
        {
            throw new DuplicateEmailException(user.Email);
        }

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
        if (existingUser == null)
        {
            throw new UserNotFoundException(user.Id);
        }

        var normalizedEmail = user.Email.Trim().ToLowerInvariant();
        var emailExistsOnOther = await _context.Users.AnyAsync(
            u => u.Id != user.Id && u.Email.ToLower() == normalizedEmail,
            cancellationToken);

        if (emailExistsOnOther)
        {
            throw new DuplicateEmailException(user.Email);
        }

        _context.Entry(existingUser).CurrentValues.SetValues(user);
        await _context.SaveChangesAsync(cancellationToken);
        return existingUser;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

