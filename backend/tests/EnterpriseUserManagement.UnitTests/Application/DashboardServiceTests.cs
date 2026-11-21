using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Application.Services;
using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;
using Moq;
using Xunit;

namespace EnterpriseUserManagement.UnitTests.Application;

public class DashboardServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _sut = new DashboardService(_userRepoMock.Object);
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldAggregateMetricsCorrectly()
    {
        // Arrange
        var testUsers = new List<User>
        {
            User.Create("User1", "Test", "user1@test.com", Department.Engineering, UserRole.Admin),
            User.Create("User2", "Test", "user2@test.com", Department.Engineering, UserRole.Employee),
            User.Create("User3", "Test", "user3@test.com", Department.HR, UserRole.Manager),
        };
        testUsers[1].SetActiveStatus(false); // Inactive

        _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(testUsers);

        // Act
        var summary = await _sut.GetSummaryAsync();

        // Assert
        Assert.Equal(3, summary.TotalUsers);
        Assert.Equal(2, summary.ActiveUsers);
        Assert.Equal(1, summary.InactiveUsers);

        Assert.Equal(2, summary.UsersByDepartment[Department.Engineering.ToString()]);
        Assert.Equal(1, summary.UsersByDepartment[Department.HR.ToString()]);
        Assert.Equal(0, summary.UsersByDepartment[Department.Finance.ToString()]);

        Assert.Equal(1, summary.UsersByRole[UserRole.Admin.ToString()]);
        Assert.Equal(1, summary.UsersByRole[UserRole.Manager.ToString()]);
        Assert.Equal(1, summary.UsersByRole[UserRole.Employee.ToString()]);

        Assert.Equal(3, summary.RecentUsers.Count);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenNoUsers_ReturnsZeroCounts()
    {
        // Arrange
        _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        // Act
        var summary = await _sut.GetSummaryAsync();

        // Assert
        Assert.Equal(0, summary.TotalUsers);
        Assert.Equal(0, summary.ActiveUsers);
        Assert.Equal(0, summary.InactiveUsers);
        Assert.Empty(summary.RecentUsers);

        foreach (var dept in Enum.GetValues<Department>())
        {
            Assert.Equal(0, summary.UsersByDepartment[dept.ToString()]);
        }

        foreach (var role in Enum.GetValues<UserRole>())
        {
            Assert.Equal(0, summary.UsersByRole[role.ToString()]);
        }
    }

    [Fact]
    public async Task GetSummaryAsync_WhenMoreThan5Users_ReturnsTop5Recent()
    {
        // Arrange
        var users = new List<User>();
        for (var i = 1; i <= 7; i++)
        {
            var user = User.Create($"User{i}", "Test", $"user{i}@test.com", Department.Engineering, UserRole.Employee);
            user.CreatedDate = DateTime.UtcNow.AddMinutes(i); // Incremental timestamps
            users.Add(user);
        }

        _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var summary = await _sut.GetSummaryAsync();

        // Assert
        Assert.Equal(7, summary.TotalUsers);
        Assert.Equal(5, summary.RecentUsers.Count);
        Assert.Equal("User7", summary.RecentUsers[0].FirstName);
    }
}
