using Application.Services.Search;
using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Customers.Features.Customers.Queries.GetFacets;

public class GetCustomerFacetsRequest : IRequest<CustomerFacetsResponse>, ISecuredRequest
{
    public string[] Roles =>
        [GeneralOperationClaims.BankManager, GeneralOperationClaims.SuperAdmin];

    public string Kind { get; set; } = CustomerSearchKinds.Individual;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public bool OnlyDeleted { get; set; }
    public Guid? BranchId { get; set; }
}
