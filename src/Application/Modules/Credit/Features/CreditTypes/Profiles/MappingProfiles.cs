using Application.Modules.Credit.Features.CreditTypes.Commands.Create;
using Application.Modules.Credit.Features.CreditTypes.Commands.Delete;
using Application.Modules.Credit.Features.CreditTypes.Commands.Update;
using Application.Modules.Credit.Features.CreditTypes.Queries.GetById;
using Application.Modules.Credit.Features.CreditTypes.Queries.GetList;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;

namespace Application.Modules.Credit.Features.CreditTypes.Profiles;

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
