using EnterpriseUserManagement.Application.DTOs;

namespace EnterpriseUserManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}

