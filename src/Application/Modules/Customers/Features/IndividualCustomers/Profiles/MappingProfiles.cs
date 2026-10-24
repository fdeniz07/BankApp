using Application.Modules.Customers.Features.IndividualCustomers.Commands.Create;
using Application.Modules.Customers.Features.IndividualCustomers.Commands.Delete;
using Application.Modules.Customers.Features.IndividualCustomers.Commands.Update;
using Application.Modules.Customers.Features.IndividualCustomers.Queries.GetById;
using Application.Modules.Customers.Features.IndividualCustomers.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;

namespace Application.Modules.Customers.Features.IndividualCustomers.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<IndividualCustomer, CreatedIndividualCustomerResponse>().ReverseMap();
        CreateMap<IndividualCustomer, UpdatedIndividualCustomerResponse>().ReverseMap();
        CreateMap<IndividualCustomer, DeletedIndividualCustomerResponse>().ReverseMap();
        CreateMap<IndividualCustomer, GetByIdIndividualCustomerResponse>().ReverseMap();
        CreateMap<IndividualCustomer, GetListIndividualCustomerListItemDto>().ReverseMap();
        CreateMap<CreateIndividualCustomerRequest, IndividualCustomer>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CustomerNumber, opt => opt.Ignore())
            .ForMember(d => d.IsActive, opt => opt.Ignore())
            .ForMember(d => d.IsDeleted, opt => opt.Ignore())
            .ForMember(d => d.CreatedDate, opt => opt.Ignore())
            .ForMember(d => d.UpdatedDate, opt => opt.Ignore())
            .ForMember(d => d.DeletedDate, opt => opt.Ignore())
            .ForMember(d => d.CreditApplications, opt => opt.Ignore());
        CreateMap<UpdateIndividualCustomerRequest, IndividualCustomer>()
            .ForMember(d => d.CustomerNumber, opt => opt.Ignore());
        CreateMap<IPaginate<IndividualCustomer>, GetListResponse<GetListIndividualCustomerListItemDto>>();
    }
}
