using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Domain.Enums;
using Xunit;

namespace EnterpriseUserManagement.IntegrationTests.Controllers;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_ReturnsSuccessAndPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<UserDto>>>(JsonOptions);
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.True(apiResponse.Data.TotalCount > 0);
        Assert.NotEmpty(apiResponse.Data.Items);
    }

    [Fact]
    public async Task GetUsers_WithDepartmentFilter_ReturnsFilteredUsers()
    {
        // Act
        var response = await _client.GetAsync("/api/users?department=Engineering");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<UserDto>>>(JsonOptions);
        Assert.NotNull(apiResponse?.Data);
        Assert.All(apiResponse.Data.Items, u => Assert.Equal(Department.Engineering, u.Department));
    }

    [Fact]
    public async Task GetUsers_WithSearchTerm_ReturnsMatchingUsers()
    {
        // Act
        var response = await _client.GetAsync("/api/users?searchTerm=Eleanor");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<UserDto>>>(JsonOptions);
        Assert.NotNull(apiResponse?.Data);
        Assert.Contains(apiResponse.Data.Items, u => u.FirstName.Contains("Eleanor", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetUserById_WhenExists_ReturnsUser()
    {
        // Arrange - Known seed user
        var seededId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var response = await _client.GetAsync($"/api/users/{seededId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions);
        Assert.NotNull(apiResponse?.Data);
        Assert.Equal(seededId, apiResponse.Data.Id);
        Assert.Equal("Eleanor", apiResponse.Data.FirstName);
        Assert.Equal("Vance", apiResponse.Data.LastName);
    }

    [Fact]
    public async Task GetUserById_WhenNotFound_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/users/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task CreateUser_WithValidData_Returns201AndPersists()
    {
        // Arrange
        var request = new CreateUserRequestDto
        {
            FirstName = "Test",
            LastName = "IntegrationUser",
            Email = $"integration.{Guid.NewGuid():N}@enterprise.com",
            Department = Department.Operations,
            Role = UserRole.Employee
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions);
        Assert.NotNull(created?.Data);
        Assert.Equal(request.FirstName, created.Data.FirstName);
        Assert.Equal(request.LastName, created.Data.LastName);
        Assert.Equal(request.Email, created.Data.Email);
        Assert.True(created.Data.IsActive);

        // Verify persistence by fetching directly
        var getResponse = await _client.GetAsync($"/api/users/{created.Data.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_Returns409Conflict()
    {
        // Arrange
        var request = new CreateUserRequestDto
        {
            FirstName = "Duplicate",
            LastName = "Person",
            Email = "eleanor.vance@enterprise.com", // Existing seed email
            Department = Department.Engineering,
            Role = UserRole.Employee
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>(JsonOptions);
        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Success);
        Assert.Contains("eleanor.vance@enterprise.com", apiResponse.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_Returns200AndUpdates()
    {
        // Arrange - First create a user to update
        var createRequest = new CreateUserRequestDto
        {
            FirstName = "Original",
            LastName = "Name",
            Email = $"update.{Guid.NewGuid():N}@enterprise.com",
            Department = Department.Finance,
            Role = UserRole.Employee
        };
        var createResponse = await _client.PostAsJsonAsync("/api/users", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions);
        var userId = created!.Data!.Id;

        var updateRequest = new UpdateUserRequestDto
        {
            FirstName = "UpdatedFirst",
            LastName = "UpdatedLast",
            Department = Department.Sales,
            Role = UserRole.Manager
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/users/{userId}", updateRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions);
        Assert.NotNull(updated?.Data);
        Assert.Equal("UpdatedFirst", updated.Data.FirstName);
        Assert.Equal("UpdatedLast", updated.Data.LastName);
        Assert.Equal(Department.Sales, updated.Data.Department);
        Assert.Equal(UserRole.Manager, updated.Data.Role);
        Assert.NotNull(updated.Data.ModifiedDate);
    }

    [Fact]
    public async Task UpdateUser_WhenNotFound_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateUserRequestDto
        {
            FirstName = "Ghost",
            LastName = "User",
            Department = Department.Operations,
            Role = UserRole.Employee
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{nonExistentId}", updateRequest, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserStatus_ShouldToggleActiveState()
    {
        // Arrange - Create active user
        var createRequest = new CreateUserRequestDto
        {
            FirstName = "Status",
            LastName = "Test",
            Email = $"status.{Guid.NewGuid():N}@enterprise.com",
            Department = Department.HR,
            Role = UserRole.Employee
        };
        var createResponse = await _client.PostAsJsonAsync("/api/users", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions);
        var userId = created!.Data!.Id;

        // Act 1: Deactivate
        var deactivateResponse = await _client.PatchAsJsonAsync(
            $"/api/users/{userId}/status", 
            new UpdateUserStatusRequestDto { IsActive = false }, 
            JsonOptions);

        // Assert 1
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        var deactivated = await deactivateResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions);
        Assert.NotNull(deactivated?.Data);
        Assert.False(deactivated.Data.IsActive);

        // Act 2: Reactivate
        var reactivateResponse = await _client.PatchAsJsonAsync(
            $"/api/users/{userId}/status", 
            new UpdateUserStatusRequestDto { IsActive = true }, 
            JsonOptions);

        // Assert 2
        Assert.Equal(HttpStatusCode.OK, reactivateResponse.StatusCode);
        var reactivated = await reactivateResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions);
        Assert.NotNull(reactivated?.Data);
        Assert.True(reactivated.Data.IsActive);
    }

    [Fact]
    public async Task DeleteUser_WhenExists_Returns200AndRemovesUser()
    {
        // Arrange - Create user to delete
        var createRequest = new CreateUserRequestDto
        {
            FirstName = "ToDelete",
            LastName = "User",
            Email = $"delete.{Guid.NewGuid():N}@enterprise.com",
            Department = Department.Support,
            Role = UserRole.Employee
        };
        var createResponse = await _client.PostAsJsonAsync("/api/users", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(JsonOptions);
        var userId = created!.Data!.Id;

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>(JsonOptions);
        Assert.NotNull(deleteResult);
        Assert.True(deleteResult.Data);

        // Verify it no longer exists
        var getResponse = await _client.GetAsync($"/api/users/{userId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WhenNotFound_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/users/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
