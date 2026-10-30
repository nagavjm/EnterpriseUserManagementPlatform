using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;
using EnterpriseUserManagement.Domain.Exceptions;

namespace EnterpriseUserManagement.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInstantiateUserCorrectly()
    {
        // Arrange
        var firstName = "Ada";
        var lastName = "Lovelace";
        var email = "Ada.Lovelace@Enterprise.com";
        var department = Department.Engineering;
        var role = UserRole.Admin;

        // Act
        var user = User.Create(firstName, lastName, email, department, role);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Ada", user.FirstName);
        Assert.Equal("Lovelace", user.LastName);
        Assert.Equal("ada.lovelace@enterprise.com", user.Email); // Lowercased normalized
        Assert.Equal(Department.Engineering, user.Department);
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.True(user.IsActive);
        Assert.True((DateTime.UtcNow - user.CreatedDate).TotalSeconds < 2);
        Assert.Null(user.ModifiedDate);
    }

    [Theory]
    [InlineData("", "Lovelace", "test@enterprise.com")]
    [InlineData("   ", "Lovelace", "test@enterprise.com")]
    [InlineData(null, "Lovelace", "test@enterprise.com")]
    public void Create_WithEmptyFirstName_ShouldThrowDomainValidationException(string? firstName, string lastName, string email)
    {
        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            User.Create(firstName!, lastName, email, Department.Engineering, UserRole.Employee));
    }

    [Theory]
    [InlineData("Ada", "", "test@enterprise.com")]
    [InlineData("Ada", "   ", "test@enterprise.com")]
    [InlineData("Ada", null, "test@enterprise.com")]
    public void Create_WithEmptyLastName_ShouldThrowDomainValidationException(string firstName, string? lastName, string email)
    {
        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            User.Create(firstName, lastName!, email, Department.Engineering, UserRole.Employee));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("user@")]
    [InlineData("@enterprise.com")]
    [InlineData("user@domain")]
    public void Create_WithInvalidEmailFormat_ShouldThrowDomainValidationException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<DomainValidationException>(() =>
            User.Create("Grace", "Hopper", invalidEmail, Department.Engineering, UserRole.Admin));
    }

    [Fact]
    public void UpdateProfile_WithValidParameters_ShouldUpdateFieldsAndSetModifiedDate()
    {
        // Arrange
        var user = User.Create("Margaret", "Hamilton", "margaret@enterprise.com", Department.Engineering, UserRole.Employee);
        var initialModified = user.ModifiedDate;

        // Act
        user.UpdateProfile("Margaret E.", "Hamilton", Department.Operations, UserRole.Manager);

        // Assert
        Assert.Equal("Margaret E.", user.FirstName);
        Assert.Equal(Department.Operations, user.Department);
        Assert.Equal(UserRole.Manager, user.Role);
        Assert.NotNull(user.ModifiedDate);
        Assert.NotEqual(initialModified, user.ModifiedDate);
    }

    [Fact]
    public void SetActiveStatus_WhenFlippingStatus_ShouldUpdateIsActiveAndModifiedDate()
    {
        // Arrange
        var user = User.Create("Katherine", "Johnson", "katherine@enterprise.com", Department.Finance, UserRole.Manager);

        // Act
        user.SetActiveStatus(false);

        // Assert
        Assert.False(user.IsActive);
        Assert.NotNull(user.ModifiedDate);
    }
}

