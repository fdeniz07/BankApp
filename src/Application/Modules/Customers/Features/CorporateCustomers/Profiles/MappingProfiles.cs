using Application.Modules.Customers.Features.CorporateCustomers.Commands.Create;
using Application.Modules.Customers.Features.CorporateCustomers.Commands.Delete;
using Application.Modules.Customers.Features.CorporateCustomers.Commands.Update;
using Application.Modules.Customers.Features.CorporateCustomers.Queries.GetById;
using Application.Modules.Customers.Features.CorporateCustomers.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;

namespace Application.Modules.Customers.Features.CorporateCustomers.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CorporateCustomer, CreatedCorporateCustomerResponse>().ReverseMap();
        CreateMap<CorporateCustomer, UpdatedCorporateCustomerResponse>().ReverseMap();
        CreateMap<CorporateCustomer, DeletedCorporateCustomerResponse>().ReverseMap();
        CreateMap<CorporateCustomer, GetByIdCorporateCustomerResponse>().ReverseMap();
        CreateMap<CorporateCustomer, GetListCorporateCustomerListItemDto>().ReverseMap();
        CreateMap<CreateCorporateCustomerRequest, CorporateCustomer>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CustomerNumber, opt => opt.Ignore())
            .ForMember(d => d.IsActive, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.CreatedDate, opt => opt.Ignore())
            .ForMember(d => d.UpdatedDate, opt => opt.Ignore())
            .ForMember(d => d.DeletedDate, opt => opt.Ignore())
            .ForMember(d => d.CreditApplications, opt => opt.Ignore());
        CreateMap<UpdateCorporateCustomerRequest, CorporateCustomer>()
            .ForMember(d => d.CustomerNumber, opt => opt.Ignore());
        CreateMap<IPaginate<CorporateCustomer>, GetListResponse<GetListCorporateCustomerListItemDto>>();
    }
}
