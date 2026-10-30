using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Application.Services;
using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;
using EnterpriseUserManagement.Domain.Exceptions;
using Moq;

namespace EnterpriseUserManagement.UnitTests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepoMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailIsUnique_ShouldPersistAndReturnUserDto()
    {
        // Arrange
        var request = new CreateUserRequestDto
        {
            FirstName = "Nikola",
            LastName = "Tesla",
            Email = "nikola.tesla@enterprise.com",
            Department = Department.Engineering,
            Role = UserRole.Manager
        };

        _userRepoMock.Setup(r => r.ExistsByEmailAsync("nikola.tesla@enterprise.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // Act
        var result = await _sut.CreateUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Nikola Tesla", result.FullName);
        Assert.Equal("nikola.tesla@enterprise.com", result.Email);
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "nikola.tesla@enterprise.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailAlreadyExists_ShouldThrowDuplicateEmailException()
    {
        // Arrange
        var request = new CreateUserRequestDto
        {
            FirstName = "Thomas",
            LastName = "Edison",
            Email = "duplicate@enterprise.com",
            Department = Department.Sales,
            Role = UserRole.Employee
        };

        _userRepoMock.Setup(r => r.ExistsByEmailAsync("duplicate@enterprise.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateEmailException>(() => _sut.CreateUserAsync(request));
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserNotFound_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() => _sut.GetUserByIdAsync(missingId));
    }

    [Fact]
    public async Task UpdateUserStatusAsync_WhenUserExists_ShouldUpdateStatusAndPersist()
    {
        // Arrange
        var user = User.Create("Marie", "Curie", "marie.curie@enterprise.com", Department.Engineering, UserRole.Admin);
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // Act
        var result = await _sut.UpdateUserStatusAsync(user.Id, false);

        // Assert
        Assert.False(result.IsActive);
        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => !u.IsActive), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenRepositoryReturnsFalse_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepoMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() => _sut.DeleteUserAsync(id));
    }
}

