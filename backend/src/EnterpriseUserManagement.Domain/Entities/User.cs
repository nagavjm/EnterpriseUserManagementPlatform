using EnterpriseUserManagement.Domain.Enums;
using EnterpriseUserManagement.Domain.Exceptions;

namespace EnterpriseUserManagement.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Department Department { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Parameterless constructor for System.Text.Json deserialization
    public User() { }

    public static User Create(
        string firstName,
        string lastName,
        string email,
        Department department,
        UserRole role)
    {
        ValidateNames(firstName, lastName);
        ValidateEmail(email);

        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Department = department,
            Role = role,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = null
        };
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        Department department,
        UserRole role)
    {
        ValidateNames(firstName, lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Department = department;
        Role = role;
        ModifiedDate = DateTime.UtcNow;
    }

    public void SetActiveStatus(bool isActive)
    {
        if (IsActive == isActive)
            return;

        IsActive = isActive;
        ModifiedDate = DateTime.UtcNow;
    }

    private static void ValidateNames(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainValidationException("First name is required.");

        if (firstName.Trim().Length > 50)
            throw new DomainValidationException("First name cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainValidationException("Last name is required.");

        if (lastName.Trim().Length > 50)
            throw new DomainValidationException("Last name cannot exceed 50 characters.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("Email is required.");

        if (email.Trim().Length > 100)
            throw new DomainValidationException("Email cannot exceed 100 characters.");

        // Standard regex RFC 5322 simplified email check
        var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(email.Trim()))
            throw new DomainValidationException($"Email '{email}' is not in a valid format.");
    }
}

