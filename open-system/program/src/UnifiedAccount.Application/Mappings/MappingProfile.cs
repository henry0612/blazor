using AutoMapper;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Domain.Entities.Changes;
using UnifiedAccount.Domain.Entities.Cho;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Domain.Entities.Zengin;

namespace UnifiedAccount.Application.Mappings;

/// <summary>
/// AutoMapper プロファイル (Entity↔DTO)
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Core
        CreateMap<Company, CompanyDto>().ReverseMap();
        CreateMap<Contract, ContractDto>().ReverseMap();
        CreateMap<BankBranch, BankBranchDto>().ReverseMap();
        CreateMap<TransferTransaction, TransferTransactionDto>().ReverseMap();
        CreateMap<TransferAmount, TransferAmountDto>().ReverseMap();
        CreateMap<TransferFailure, TransferFailureDto>().ReverseMap();
        CreateMap<NotificationMessage, NotificationDto>()
            .ForMember(d => d.AvisCode, o => o.MapFrom(s => s.AvisCode));

        // Zengin
        CreateMap<ZenginBatch, ZenginBatchDto>().ReverseMap();
        CreateMap<ZenginTransaction, ZenginTransactionDto>().ReverseMap();

        // Cho
        CreateMap<CooperativeTransfer, CooperativeTransferDto>().ReverseMap();

        // Changes
        CreateMap<ContractChangeRequest, ContractChangeRequestDto>().ReverseMap();

        // Calendar
        CreateMap<ProcessingCalendar, CalendarDto>()
            .ForMember(d => d.IsBusinessDay, o => o.MapFrom(_ => true));
        CreateMap<ProcessingSlot, CalendarSlotDto>();
    }
}
