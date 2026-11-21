using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Application.Services;
using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;
using EnterpriseUserManagement.Domain.Exceptions;
using Moq;
using Xunit;

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
    public async Task GetUsersPagedAsync_ReturnsPagedResultOfUserDtos()
    {
        // Arrange
        var parameters = new UserQueryParameters { PageNumber = 1, PageSize = 10 };
        var users = new List<User>
        {
            User.Create("Ada", "Lovelace", "ada@test.com", Department.Engineering, UserRole.Admin),
            User.Create("Grace", "Hopper", "grace@test.com", Department.Engineering, UserRole.Manager)
        };
        var pagedUsers = new PagedResult<User>(users, 2, 1, 10);

        _userRepoMock.Setup(r => r.GetPagedAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedUsers);

        // Act
        var result = await _sut.GetUsersPagedAsync(parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Ada", result.Items[0].FirstName);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsListOfUserDtos()
    {
        // Arrange
        var users = new List<User>
        {
            User.Create("Alan", "Turing", "alan@test.com", Department.Engineering, UserRole.Admin)
        };

        _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Alan Turing", result[0].FullName);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUserDto()
    {
        // Arrange
        var user = User.Create("Katherine", "Johnson", "katherine@test.com", Department.Finance, UserRole.Manager);
        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("katherine@test.com", result.Email);
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
    public async Task UpdateUserAsync_WhenUserExists_ShouldUpdateProfileAndReturnDto()
    {
        // Arrange
        var user = User.Create("Margaret", "Hamilton", "margaret@enterprise.com", Department.Engineering, UserRole.Employee);
        var request = new UpdateUserRequestDto
        {
            FirstName = "Margaret H.",
            LastName = "Hamilton",
            Department = Department.Operations,
            Role = UserRole.Manager
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // Act
        var result = await _sut.UpdateUserAsync(user.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Margaret H.", result.FirstName);
        Assert.Equal(Department.Operations, result.Department);
        Assert.Equal(UserRole.Manager, result.Role);
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WhenUserNotFound_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        var request = new UpdateUserRequestDto
        {
            FirstName = "Ghost",
            LastName = "User",
            Department = Department.HR,
            Role = UserRole.Employee
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() => _sut.UpdateUserAsync(missingId, request));
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
    public async Task UpdateUserStatusAsync_WhenUserNotFound_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() => _sut.UpdateUserStatusAsync(missingId, true));
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepoMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteUserAsync(id);

        // Assert
        Assert.True(result);
        _userRepoMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
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
