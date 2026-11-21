using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;
using EnterpriseUserManagement.Domain.Exceptions;
using EnterpriseUserManagement.Infrastructure.Data;
using EnterpriseUserManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EnterpriseUserManagement.UnitTests.Infrastructure;

public class SqlUserRepositoryTests
{
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString("N"))
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPersistedUsers()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user1 = User.Create("Alice", "Smith", "alice@example.com", Department.Engineering, UserRole.Admin);
        var user2 = User.Create("Bob", "Jones", "bob@example.com", Department.HR, UserRole.Employee);

        await context.Users.AddRangeAsync(user1, user2);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Email == "alice@example.com");
        Assert.Contains(result, u => u.Email == "bob@example.com");
    }

    [Fact]
    public async Task GetPagedAsync_WithSearchTermAndFilters_FiltersCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user1 = User.Create("Alice", "Wonder", "alice@example.com", Department.Engineering, UserRole.Admin);
        var user2 = User.Create("Alicia", "Keys", "alicia@music.com", Department.Sales, UserRole.Employee);
        var user3 = User.Create("Charlie", "Brown", "charlie@example.com", Department.Engineering, UserRole.Employee);
        user3.SetActiveStatus(false);

        await context.Users.AddRangeAsync(user1, user2, user3);
        await context.SaveChangesAsync();

        // Act: search "alic", filter Department.Engineering, IsActive=true
        var query = new UserQueryParameters
        {
            SearchTerm = "alic",
            Department = Department.Engineering,
            Role = UserRole.Admin,
            IsActive = true,
            PageNumber = 1,
            PageSize = 10
        };
        var result = await repository.GetPagedAsync(query);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Alice", result.Items[0].FirstName);
    }

    [Theory]
    [InlineData("firstname", false)]
    [InlineData("firstname", true)]
    [InlineData("lastname", false)]
    [InlineData("lastname", true)]
    [InlineData("email", false)]
    [InlineData("email", true)]
    [InlineData("department", false)]
    [InlineData("department", true)]
    [InlineData("role", false)]
    [InlineData("role", true)]
    [InlineData("isactive", false)]
    [InlineData("isactive", true)]
    [InlineData("createddate", false)]
    [InlineData("createddate", true)]
    public async Task GetPagedAsync_SortingBranches_ExecuteWithoutErrors(string sortBy, bool sortDescending)
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user1 = User.Create("Aaron", "Alpha", "aaron@example.com", Department.Engineering, UserRole.Admin);
        var user2 = User.Create("Zack", "Zulu", "zack@example.com", Department.Finance, UserRole.Employee);
        await context.Users.AddRangeAsync(user1, user2);
        await context.SaveChangesAsync();

        var query = new UserQueryParameters
        {
            SortBy = sortBy,
            SortDescending = sortDescending,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await repository.GetPagedAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetPagedAsync_WithPaginationBounds_EnforcesConstraints()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var query = new UserQueryParameters
        {
            PageNumber = 0, // should default to 1
            PageSize = 200  // should cap at 100
        };

        // Act
        var result = await repository.GetPagedAsync(query);

        // Assert
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsUser()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user = User.Create("David", "Bowie", "david@music.com", Department.Sales, UserRole.Employee);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("David", result.FirstName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_NormalizesAndFindsUser()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user = User.Create("David", "Gilmour", "david.gilmour@pinkfloyd.com", Department.Operations, UserRole.Manager);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByEmailAsync(" DAVID.GILMOUR@pinkfloyd.com ");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("david.gilmour@pinkfloyd.com", result.Email);
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithAndWithoutExcludedId_ReturnsCorrectBoolean()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user = User.Create("Freddie", "Mercury", "freddie@queen.com", Department.Engineering, UserRole.Admin);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        // Act & Assert
        Assert.True(await repository.ExistsByEmailAsync("freddie@queen.com"));
        Assert.False(await repository.ExistsByEmailAsync("freddie@queen.com", user.Id)); // Excluded self
        Assert.False(await repository.ExistsByEmailAsync("nonexistent@queen.com"));
    }

    [Fact]
    public async Task AddAsync_WhenUnique_SavesAndReturnsUser()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user = User.Create("Brian", "May", "brian@queen.com", Department.Engineering, UserRole.Admin);

        // Act
        var result = await repository.AddAsync(user);

        // Assert
        Assert.NotNull(result);
        var inDb = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(inDb);
    }

    [Fact]
    public async Task AddAsync_WhenDuplicateEmail_ThrowsDuplicateEmailException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user1 = User.Create("Roger", "Taylor", "roger@queen.com", Department.Support, UserRole.Employee);
        await repository.AddAsync(user1);

        var user2 = User.Create("Roger", "Duplicate", "roger@queen.com", Department.Sales, UserRole.Employee);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateEmailException>(() => repository.AddAsync(user2));
    }

    [Fact]
    public async Task UpdateAsync_WhenUserExists_UpdatesAndReturnsUser()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user = User.Create("John", "Deacon", "john@queen.com", Department.Finance, UserRole.Employee);
        await repository.AddAsync(user);

        user.UpdateProfile("Johnny", "Deacon", Department.Finance, UserRole.Manager);

        // Act
        var updated = await repository.UpdateAsync(user);

        // Assert
        Assert.Equal("Johnny", updated.FirstName);
        Assert.Equal(UserRole.Manager, updated.Role);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ThrowsUserNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var nonExistentUser = User.Create("Ghost", "User", "ghost@example.com", Department.HR, UserRole.Employee);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() => repository.UpdateAsync(nonExistentUser));
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailCollidesWithAnotherUser_ThrowsDuplicateEmailException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user1 = User.Create("User1", "Test", "user1@example.com", Department.Engineering, UserRole.Employee);
        var user2 = User.Create("User2", "Test", "user2@example.com", Department.Sales, UserRole.Employee);
        await repository.AddAsync(user1);
        await repository.AddAsync(user2);

        // Modify user2 to have user1's email
        user2.Email = "user1@example.com";

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateEmailException>(() => repository.UpdateAsync(user2));
    }

    [Fact]
    public async Task DeleteAsync_WhenUserExists_DeletesAndReturnsTrue()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        var user = User.Create("Delete", "Me", "delete@example.com", Department.HR, UserRole.Employee);
        await repository.AddAsync(user);

        // Act
        var result = await repository.DeleteAsync(user.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenUserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repository = new SqlUserRepository(context);

        // Act
        var result = await repository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
