using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Domain.Enums;

namespace EnterpriseUserManagement.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUserRepository _userRepository;

    public DashboardService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var allUsers = await _userRepository.GetAllAsync(cancellationToken);

        var totalUsers = allUsers.Count;
        var activeUsers = allUsers.Count(u => u.IsActive);
        var inactiveUsers = totalUsers - activeUsers;

        // Group by department (all departments initialized to 0)
        var departmentCounts = Enum.GetValues<Department>()
            .ToDictionary(d => d.ToString(), _ => 0);

        foreach (var group in allUsers.GroupBy(u => u.Department))
        {
            departmentCounts[group.Key.ToString()] = group.Count();
        }

        // Group by role (all roles initialized to 0)
        var roleCounts = Enum.GetValues<UserRole>()
            .ToDictionary(r => r.ToString(), _ => 0);

        foreach (var group in allUsers.GroupBy(u => u.Role))
        {
            roleCounts[group.Key.ToString()] = group.Count();
        }

        // 5 most recently created users
        var recentUsers = allUsers
            .OrderByDescending(u => u.CreatedDate)
            .Take(5)
            .Select(UserDto.FromEntity)
            .ToList();

        return new DashboardSummaryDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            UsersByDepartment = departmentCounts,
            UsersByRole = roleCounts,
            RecentUsers = recentUsers
        };
    }
}

