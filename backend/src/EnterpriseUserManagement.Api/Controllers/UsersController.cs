using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseUserManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of users with optional filtering and search.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUsersPagedAsync(parameters, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result, "Users retrieved successfully."));
    }

    /// <summary>
    /// Retrieves a specific user by unique GUID.
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetUserById))]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(user, "User retrieved successfully."));
    }

    /// <summary>
    /// Creates a new user record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new user with email {Email}", request.Email);
        var createdUser = await _userService.CreateUserAsync(request, cancellationToken);

        return CreatedAtRoute(
            nameof(GetUserById),
            new { id = createdUser.Id },
            ApiResponse<UserDto>.Ok(createdUser, "User created successfully."));
    }

    /// <summary>
    /// Updates an existing user's profile.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating profile for user ID {UserId}", id);
        var updatedUser = await _userService.UpdateUserAsync(id, request, cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(updatedUser, "User updated successfully."));
    }

    /// <summary>
    /// Activates or deactivates a user.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus([FromRoute] Guid id, [FromBody] UpdateUserStatusRequestDto request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating status for user ID {UserId} to IsActive={IsActive}", id, request.IsActive);
        var updatedUser = await _userService.UpdateUserStatusAsync(id, request.IsActive, cancellationToken);
        var statusText = request.IsActive ? "activated" : "deactivated";
        return Ok(ApiResponse<UserDto>.Ok(updatedUser, $"User successfully {statusText}."));
    }

    /// <summary>
    /// Deletes a user permanently from JSON storage.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting user ID {UserId}", id);
        await _userService.DeleteUserAsync(id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "User deleted successfully."));
    }
}
