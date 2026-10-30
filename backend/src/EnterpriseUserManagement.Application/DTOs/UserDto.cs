using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;

namespace EnterpriseUserManagement.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public Department Department { get; set; }
    public string DepartmentName => Department.ToString();
    public UserRole Role { get; set; }
    public string RoleName => Role.ToString();
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public static UserDto FromEntity(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Department = user.Department,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedDate = user.CreatedDate,
            ModifiedDate = user.ModifiedDate
        };
    }
}

