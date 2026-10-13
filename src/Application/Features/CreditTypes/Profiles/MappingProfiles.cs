using Application.Features.CreditTypes.Commands.Create;
using Application.Features.CreditTypes.Commands.Delete;
using Application.Features.CreditTypes.Commands.Update;
using Application.Features.CreditTypes.Queries.GetById;
using Application.Features.CreditTypes.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;

namespace Application.Features.CreditTypes.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CreditType, CreatedCreditTypeResponse>().ReverseMap();
        CreateMap<CreditType, UpdatedCreditTypeResponse>().ReverseMap();
        CreateMap<CreditType, DeletedCreditTypeResponse>().ReverseMap();
        CreateMap<CreditType, GetByIdCreditTypeResponse>().ReverseMap();
        CreateMap<CreditType, GetListCreditTypeListItemDto>().ReverseMap();
        CreateMap<CreateCreditTypeRequest, CreditType>();
        CreateMap<UpdateCreditTypeRequest, CreditType>();
        CreateMap<IPaginate<CreditType>, GetListResponse<GetListCreditTypeListItemDto>>();
    }
}
