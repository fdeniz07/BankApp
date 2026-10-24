using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.Dashboard.Queries.GetSummary;

public class GetDashboardSummaryRequest : IRequest<GetDashboardSummaryResponse>, ISecuredRequest
{
    public string[] Roles =>
        [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager];
}
