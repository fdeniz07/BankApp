using Application.Features.CreditSubTypes.Commands.Create;
using Application.Features.CreditSubTypes.Commands.Delete;
using Application.Features.CreditSubTypes.Commands.Update;
using Application.Features.CreditSubTypes.Queries.GetById;
using Application.Features.CreditSubTypes.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;

namespace Application.Features.CreditSubTypes.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CreditSubType, CreatedCreditSubTypeResponse>().ReverseMap();
        CreateMap<CreditSubType, UpdatedCreditSubTypeResponse>().ReverseMap();
        CreateMap<CreditSubType, DeletedCreditSubTypeResponse>().ReverseMap();
        CreateMap<CreditSubType, GetByIdCreditSubTypeResponse>().ReverseMap();
        CreateMap<CreditSubType, GetListCreditSubTypeListItemDto>().ReverseMap();
        CreateMap<CreateCreditSubTypeRequest, CreditSubType>();
        CreateMap<UpdateCreditSubTypeRequest, CreditSubType>();
        CreateMap<IPaginate<CreditSubType>, GetListResponse<GetListCreditSubTypeListItemDto>>();
    }
}
