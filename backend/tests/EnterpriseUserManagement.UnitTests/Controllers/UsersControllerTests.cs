using EnterpriseUserManagement.Api.Controllers;
using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EnterpriseUserManagement.UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ILogger<UsersController>> _loggerMock = new();
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _sut = new UsersController(_userServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkResultWithPagedUsers()
    {
        // Arrange
        var parameters = new UserQueryParameters { PageNumber = 1, PageSize = 10 };
        var pagedResult = new PagedResult<UserDto>(new List<UserDto>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith", Email = "alice@test.com" }
        }, 1, 1, 10);

        _userServiceMock.Setup(s => s.GetUsersPagedAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sut.GetUsers(parameters, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<PagedResult<UserDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Single(apiResponse.Data.Items);
    }

    [Fact]
    public async Task GetUserById_WhenUserExists_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDto
        {
            Id = userId,
            FirstName = "Bob",
            LastName = "Jones",
            Email = "bob@test.com",
            Department = Department.Engineering,
            Role = UserRole.Employee,
            IsActive = true
        };

        _userServiceMock.Setup(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _sut.GetUserById(userId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UserDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(userId, apiResponse.Data?.Id);
    }

    [Fact]
    public async Task CreateUser_WithValidRequest_ReturnsCreatedAtRoute()
    {
        // Arrange
        var request = new CreateUserRequestDto
        {
            FirstName = "Carol",
            LastName = "Danvers",
            Email = "carol@enterprise.com",
            Department = Department.Operations,
            Role = UserRole.Manager
        };

        var createdDto = new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Department = request.Department,
            Role = request.Role,
            IsActive = true
        };

        _userServiceMock.Setup(s => s.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _sut.CreateUser(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtRouteResult>(result);
        Assert.Equal(nameof(UsersController.GetUserById), createdResult.RouteName);
        Assert.Equal(createdDto.Id, createdResult.RouteValues?["id"]);

        var apiResponse = Assert.IsType<ApiResponse<UserDto>>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(createdDto.Id, apiResponse.Data?.Id);
    }

    [Fact]
    public async Task UpdateUser_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequestDto
        {
            FirstName = "Carol",
            LastName = "Danvers-Updated",
            Department = Department.Sales,
            Role = UserRole.Admin
        };

        var updatedDto = new UserDto
        {
            Id = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = "carol@enterprise.com",
            Department = request.Department,
            Role = request.Role,
            IsActive = true
        };

        _userServiceMock.Setup(s => s.UpdateUserAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.UpdateUser(userId, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UserDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(request.LastName, apiResponse.Data?.LastName);
    }

    [Fact]
    public async Task UpdateUserStatus_Deactivate_ReturnsOkResultWithDeactivatedMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserStatusRequestDto { IsActive = false };
        var updatedDto = new UserDto { Id = userId, IsActive = false };

        _userServiceMock.Setup(s => s.UpdateUserStatusAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.UpdateUserStatus(userId, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UserDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Contains("deactivated", apiResponse.Message);
    }

    [Fact]
    public async Task UpdateUserStatus_Activate_ReturnsOkResultWithActivatedMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserStatusRequestDto { IsActive = true };
        var updatedDto = new UserDto { Id = userId, IsActive = true };

        _userServiceMock.Setup(s => s.UpdateUserStatusAsync(userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _sut.UpdateUserStatus(userId, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UserDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Contains("activated", apiResponse.Message);
    }

    [Fact]
    public async Task DeleteUser_WhenSuccessful_ReturnsOkResultWithTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userServiceMock.Setup(s => s.DeleteUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteUser(userId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<bool>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.True(apiResponse.Data);
    }
}
