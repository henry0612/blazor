using CsvHelper.Configuration;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Migration.Importers;

/// <summary>
/// 委託会社マスター CSV → Companies テーブル インポーター。
/// </summary>
public sealed class CompanyImporter : CsvImporter<Company, CompanyMap>
{
    public CompanyImporter(AppDbContext db) : base(db) { }

    protected override string TableName => "Companies";
}

public sealed class CompanyMap : ClassMap<Company>
{
    public CompanyMap()
    {
        Map(m => m.CompanyCode).Name("CompanyCode");
        Map(m => m.CompanyNameKana).Name("CompanyNameKana");
        Map(m => m.CompanyNameKanji).Name("CompanyNameKanji").Optional();
        Map(m => m.PostalCode).Name("PostalCode").Optional();
        Map(m => m.Prefecture).Name("Prefecture").Optional();
        Map(m => m.City).Name("City").Optional();
        Map(m => m.Town1).Name("Town1").Optional();
        Map(m => m.Town2).Name("Town2").Optional();
        Map(m => m.PhoneNumber).Name("PhoneNumber").Optional();
        Map(m => m.FaxNumber).Name("FaxNumber").Optional();
        Map(m => m.ZenginLinkFlag).Name("ZenginLinkFlag").Default("0");
        Map(m => m.PageChangeKey).Name("PageChangeKey").Default((short)0);
        Map(m => m.BasicFee).Name("BasicFee").Default(0m);
        Map(m => m.AdminFee).Name("AdminFee").Default(0m);
        Map(m => m.NewUnitPrice).Name("NewUnitPrice").Default(0m);
        Map(m => m.ModifyUnitPrice).Name("ModifyUnitPrice").Default(0m);
        Map(m => m.Transfer1UnitPrice).Name("Transfer1UnitPrice").Default(0m);
        Map(m => m.Transfer2UnitPrice).Name("Transfer2UnitPrice").Default(0m);
        Map(m => m.ReceiptUnitPrice).Name("ReceiptUnitPrice").Default(0m);
        Map(m => m.Transfer1BankCode).Name("Transfer1BankCode").Optional();
        Map(m => m.Transfer1BranchCode).Name("Transfer1BranchCode").Optional();
        Map(m => m.Transfer1AccountType).Name("Transfer1AccountType").Optional();
        Map(m => m.Transfer1AccountNo).Name("Transfer1AccountNo").Optional();
        Map(m => m.ConsignorCode).Name("ConsignorCode");
        Map(m => m.Transfer2BankCode).Name("Transfer2BankCode").Optional();
        Map(m => m.Transfer2BranchCode).Name("Transfer2BranchCode").Optional();
        Map(m => m.Transfer2AccountType).Name("Transfer2AccountType").Optional();
        Map(m => m.Transfer2AccountNo).Name("Transfer2AccountNo").Optional();
        Map(m => m.PassbookComment).Name("PassbookComment").Optional();
        Map(m => m.TransferCode).Name("TransferCode").Optional();
        Map(m => m.LastTransferDate).Name("LastTransferDate").Optional();
        Map(m => m.ContractSeqNo).Name("ContractSeqNo").Optional();
        Map(m => m.CustomerChargeNo).Name("CustomerChargeNo").Optional();
        Map(m => m.CustomerChargeSubType).Name("CustomerChargeSubType").Optional();
        Map(m => m.AccountHolderName).Name("AccountHolderName").Optional();
        Map(m => m.NewCount).Name("NewCount").Default(0m);
        Map(m => m.ModifyCount).Name("ModifyCount").Default(0m);
        Map(m => m.Transfer1Count).Name("Transfer1Count").Default(0m);
        Map(m => m.Transfer2Count).Name("Transfer2Count").Default(0m);
        Map(m => m.FailureCount).Name("FailureCount").Default(0m);
        Map(m => m.ConvenienceParams).Name("ConvenienceParams").Optional();
        Map(m => m.PostalTransferParams).Name("PostalTransferParams").Optional();
        Map(m => m.CoopRemitParams).Name("CoopRemitParams").Optional();
        Map(m => m.ResultDeliveryParams).Name("ResultDeliveryParams").Optional();
        Map(m => m.SuspendFlag).Name("SuspendFlag").Default("0");

        // BaseEntity / Navigation — ignored
        Map(m => m.Id).Ignore();
        Map(m => m.CreatedAt).Ignore();
        Map(m => m.UpdatedAt).Ignore();
        Map(m => m.WithdrawalDays).Ignore();
        Map(m => m.Types).Ignore();
        Map(m => m.Contracts).Ignore();
        Map(m => m.TransferTransactions).Ignore();
        Map(m => m.NotificationMessages).Ignore();
    }
}
