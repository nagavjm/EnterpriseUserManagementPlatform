using EnterpriseUserManagement.Api.Controllers;
using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EnterpriseUserManagement.UnitTests.Controllers;

public class DashboardControllerTests
{
    private readonly Mock<IDashboardService> _dashboardServiceMock = new();
    private readonly Mock<ILogger<DashboardController>> _loggerMock = new();
    private readonly DashboardController _sut;

    public DashboardControllerTests()
    {
        _sut = new DashboardController(_dashboardServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetSummary_ReturnsOkObjectResultWithSummaryDto()
    {
        // Arrange
        var summaryDto = new DashboardSummaryDto
        {
            TotalUsers = 10,
            ActiveUsers = 8,
            InactiveUsers = 2,
            UsersByDepartment = new Dictionary<string, int> { { "Engineering", 5 } },
            UsersByRole = new Dictionary<string, int> { { "Admin", 2 } },
            RecentUsers = new List<UserDto>()
        };

        _dashboardServiceMock.Setup(s => s.GetSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaryDto);

        // Act
        var result = await _sut.GetSummary(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<DashboardSummaryDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(10, apiResponse.Data?.TotalUsers);
        Assert.Equal(8, apiResponse.Data?.ActiveUsers);
    }
}
