using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Application.Services;
using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;
using Moq;

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
}

