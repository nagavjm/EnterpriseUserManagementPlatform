using System.ComponentModel.DataAnnotations;
using EnterpriseUserManagement.Domain.Enums;

namespace EnterpriseUserManagement.Application.DTOs;

public class CreateUserRequestDto
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    public Department Department { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    public UserRole Role { get; set; }
}

