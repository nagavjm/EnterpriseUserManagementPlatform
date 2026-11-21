using EnterpriseUserManagement.Application;
using EnterpriseUserManagement.Application.Common;
using EnterpriseUserManagement.Application.DTOs;
using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;
using EnterpriseUserManagement.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnterpriseUserManagement.UnitTests.Application;

public class CommonModelTests
{
    [Fact]
    public void ApiResponse_Ok_ConstructsSuccessfulResponse()
    {
        var response = ApiResponse<string>.Ok("TestData", "Operation completed.");
        Assert.True(response.Success);
        Assert.Equal("TestData", response.Data);
        Assert.Equal("Operation completed.", response.Message);
        Assert.Null(response.Errors);
    }

    [Fact]
    public void ApiResponse_Fail_ConstructsFailedResponseWithErrors()
    {
        var errors = new List<string> { "Error 1", "Error 2" };
        var response = ApiResponse<string>.Fail("Failed operation.", errors);

        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Failed operation.", response.Message);
        Assert.NotNull(response.Errors);
        Assert.Equal(2, response.Errors.Count);
    }

    [Fact]
    public void PagedResult_PaginationCalculations_AreAccurate()
    {
        var items = new List<int> { 1, 2, 3 };
        var paged = new PagedResult<int>(items, totalCount: 25, pageNumber: 2, pageSize: 10);

        Assert.Equal(25, paged.TotalCount);
        Assert.Equal(2, paged.PageNumber);
        Assert.Equal(10, paged.PageSize);
        Assert.Equal(3, paged.TotalPages);
        Assert.True(paged.HasPreviousPage);
        Assert.True(paged.HasNextPage);
    }

    [Fact]
    public void PagedResult_FirstAndLastPage_FlagsCalculatedCorrectly()
    {
        var firstPage = new PagedResult<int>(new List<int>(), totalCount: 10, pageNumber: 1, pageSize: 10);
        Assert.False(firstPage.HasPreviousPage);
        Assert.False(firstPage.HasNextPage);
        Assert.Equal(1, firstPage.TotalPages);
    }

    [Fact]
    public void UserDto_FromEntity_MapsAllProperties()
    {
        var user = User.Create("Alan", "Turing", "alan@test.com", Department.Engineering, UserRole.Admin);
        user.SetActiveStatus(false);

        var dto = UserDto.FromEntity(user);

        Assert.Equal(user.Id, dto.Id);
        Assert.Equal("Alan", dto.FirstName);
        Assert.Equal("Turing", dto.LastName);
        Assert.Equal("Alan Turing", dto.FullName);
        Assert.Equal("alan@test.com", dto.Email);
        Assert.Equal(Department.Engineering, dto.Department);
        Assert.Equal(UserRole.Admin, dto.Role);
        Assert.False(dto.IsActive);
        Assert.NotNull(dto.ModifiedDate);
    }

    [Fact]
    public void DependencyInjection_Application_RegistersServices()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        var provider = services.BuildServiceProvider();
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IUserService)));
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IDashboardService)));
    }

    [Fact]
    public void DependencyInjection_Infrastructure_WithTestingEnvironment_RegistersInMemory()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddInfrastructure(configuration, "Testing");

        var provider = services.BuildServiceProvider();
        Assert.NotNull(services.FirstOrDefault(s => s.ServiceType == typeof(IUserRepository)));
    }
}
