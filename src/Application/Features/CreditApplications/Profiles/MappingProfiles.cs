using Application.Features.CreditApplications.Commands.Approve;
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
        CreateMap<CreditApplication, GetByIdCreditApplicationResponse>().ReverseMap();
        CreateMap<CreditApplication, GetListCreditApplicationListItemDto>().ReverseMap();
        CreateMap<CreateCreditApplicationRequest, CreditApplication>();
        CreateMap<UpdateCreditApplicationRequest, CreditApplication>();
        CreateMap<IPaginate<CreditApplication>, GetListResponse<GetListCreditApplicationListItemDto>>();
    }
}
