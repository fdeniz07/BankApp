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
        CreateMap<CreditSubType, GetListCreditSubTypeListItemDto>()
            .ForMember(d => d.Locale, opt => opt.Ignore())
            .ForMember(d => d.CurrencyCode, opt => opt.Ignore())
            .ForMember(d => d.AmountOptions, opt => opt.Ignore())
            .ForMember(d => d.TermOptions, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<CreateCreditSubTypeRequest, CreditSubType>();
        CreateMap<UpdateCreditSubTypeRequest, CreditSubType>();
        CreateMap<IPaginate<CreditSubType>, GetListResponse<GetListCreditSubTypeListItemDto>>();
    }
}
