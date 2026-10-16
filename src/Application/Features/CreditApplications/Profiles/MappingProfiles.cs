using Application.Features.CreditApplications.Commands.Approve;
using Application.Features.CreditApplications.Commands.Cancel;
using Application.Features.CreditApplications.Commands.Create;
using Application.Features.CreditApplications.Commands.Delete;
using Application.Features.CreditApplications.Commands.Reject;
using Application.Features.CreditApplications.Commands.Update;
using Application.Features.CreditApplications.Queries.GetById;
using Application.Features.CreditApplications.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;

namespace Application.Features.CreditApplications.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CreditApplication, CreatedCreditApplicationResponse>().ReverseMap();
        CreateMap<CreditApplication, UpdatedCreditApplicationResponse>().ReverseMap();
        CreateMap<CreditApplication, DeletedCreditApplicationResponse>().ReverseMap();
        CreateMap<CreditApplication, ApprovedCreditApplicationResponse>().ReverseMap();
        CreateMap<CreditApplication, RejectedCreditApplicationResponse>().ReverseMap();
        CreateMap<CreditApplication, CancelledCreditApplicationResponse>().ReverseMap();
        CreateMap<CreditApplication, GetByIdCreditApplicationResponse>()
            .ForMember(d => d.CustomerName, opt => opt.Ignore())
            .ForMember(d => d.CustomerCategory, opt => opt.Ignore())
            .ForMember(d => d.CreditSubTypeName, opt => opt.Ignore())
            .ForMember(d => d.CurrencyCode, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(d => d.Customer, opt => opt.Ignore());

        CreateMap<CreditApplication, GetListCreditApplicationListItemDto>()
            .ForMember(d => d.CurrencyCode, opt => opt.Ignore())
            .ForMember(d => d.CreditSubTypeName, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<CreateCreditApplicationRequest, CreditApplication>();
        CreateMap<UpdateCreditApplicationRequest, CreditApplication>();
        CreateMap<IPaginate<CreditApplication>, GetListResponse<GetListCreditApplicationListItemDto>>();
    }
}
