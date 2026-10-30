using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;
using Xunit;

namespace EnterpriseUserManagement.IntegrationTests.Controllers;

public class DashboardControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public DashboardControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSummary_ReturnsSuccessAndMetrics()
    {
        // Act
        var response = await _client.GetAsync("/api/dashboard/summary");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardSummaryDto>>(JsonOptions);
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);

        var summary = apiResponse.Data;
        Assert.True(summary.TotalUsers >= 9, "Expected at least 9 initial seeded users");
        Assert.True(summary.ActiveUsers > 0);
        Assert.Equal(summary.TotalUsers, summary.ActiveUsers + summary.InactiveUsers);
        Assert.NotEmpty(summary.UsersByDepartment);
        Assert.NotEmpty(summary.UsersByRole);
        Assert.NotEmpty(summary.RecentUsers);
        Assert.True(summary.RecentUsers.Count <= 5);
    }
}

