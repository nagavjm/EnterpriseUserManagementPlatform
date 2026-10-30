using EnterpriseUserManagement.Domain.Enums;

namespace EnterpriseUserManagement.Application.DTOs;

public class UserQueryParameters
{
    public string? SearchTerm { get; set; }
    public Department? Department { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedDate";
    public bool SortDescending { get; set; } = true;
}

