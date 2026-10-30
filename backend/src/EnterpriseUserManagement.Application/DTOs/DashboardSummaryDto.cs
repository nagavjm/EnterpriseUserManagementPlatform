namespace EnterpriseUserManagement.Application.DTOs;

public class DashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public Dictionary<string, int> UsersByDepartment { get; set; } = new();
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public List<UserDto> RecentUsers { get; set; } = new();
}

