using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Migration.Importers;

/// <summary>
/// 契約者マスター CSV → Contracts テーブル インポーター。
/// FK 解決: CompanyCode → CompanyId, BankCode+BranchCode → BankBranchId
/// </summary>
public sealed class ContractImporter : CsvImporter<Contract, ContractMap>
{
    public ContractImporter(AppDbContext db) : base(db) { }

    protected override string TableName => "Contracts";

    /// <summary>
    /// バッチ内レコードの FK (CompanyId, BankBranchId) をコードから解決する。
    /// </summary>
    protected override async Task PreProcessAsync(IList<Contract> batch)
    {
        // CompanyCode → Id キャッシュ
        var companyCodes = batch.Select(c => c.CompanyCode).Distinct().ToList();
        var companyMap = await Db.Companies
            .Where(c => companyCodes.Contains(c.CompanyCode))
            .ToDictionaryAsync(c => c.CompanyCode, c => c.Id);

        // BankCode+BranchCode → Id キャッシュ
        var bankKeys = batch
            .Select(c => new { c.BankCode, c.BranchCode })
            .Distinct()
            .ToList();
        var bankCodes = bankKeys.Select(k => k.BankCode).Distinct().ToList();
        var branchLookup = await Db.BankBranches
            .Where(b => bankCodes.Contains(b.BankCode))
            .ToDictionaryAsync(b => $"{b.BankCode}-{b.BranchCode}", b => b.Id);

        foreach (var record in batch)
        {
            // CompanyId 解決
            if (companyMap.TryGetValue(record.CompanyCode, out var companyId))
            {
                record.CompanyId = companyId;
            }
            else
            {
                Log.Warning("Contract {Code}: CompanyCode '{CompanyCode}' not found in Companies table.",
                    record.PersonalCode, record.CompanyCode);
            }

            // BankBranchId 解決
            var bankKey = $"{record.BankCode}-{record.BranchCode}";
            if (branchLookup.TryGetValue(bankKey, out var bankBranchId))
            {
                record.BankBranchId = bankBranchId;
            }
            else
            {
                Log.Warning("Contract {Code}: BankBranch '{BankCode}-{BranchCode}' not found.",
                    record.PersonalCode, record.BankCode, record.BranchCode);
            }
        }
    }
}

public sealed class ContractMap : ClassMap<Contract>
{
    public ContractMap()
    {
        Map(m => m.CompanyCode).Name("CompanyCode");
        Map(m => m.PersonalCode).Name("PersonalCode");
        Map(m => m.CheckDigit).Name("CheckDigit");
        Map(m => m.CodeSave).Name("CodeSave").Default(" ");
        Map(m => m.WithdrawalDay).Name("WithdrawalDay").Default((short)0);
        Map(m => m.StartYearMonth).Name("StartYearMonth").Optional();
        Map(m => m.SuspendFlag).Name("SuspendFlag").Default("0");
        Map(m => m.NewFlag).Name("NewFlag").Default("0");
        Map(m => m.ZenginFlag).Name("ZenginFlag").Default("0");
        Map(m => m.NotifiedFlag).Name("NotifiedFlag").Default("0");
        Map(m => m.ResultFlag).Name("ResultFlag").Default("0");
        Map(m => m.ProcessType).Name("ProcessType").Default("0");
        Map(m => m.CreditCompleteFlag).Name("CreditCompleteFlag").Default("0");
        Map(m => m.TransferMethod).Name("TransferMethod").Default("0");
        Map(m => m.AutoDeleteFlag).Name("AutoDeleteFlag").Default("0");
        Map(m => m.FailureCount).Name("FailureCount").Default((short)0);
        Map(m => m.DepositorNameKana).Name("DepositorNameKana");
        Map(m => m.ContractorNameKana).Name("ContractorNameKana").Optional();
        Map(m => m.DepositorNameKanji).Name("DepositorNameKanji").Optional();
        Map(m => m.ContractorNameKanji).Name("ContractorNameKanji").Optional();
        Map(m => m.PostalCode).Name("PostalCode").Optional();
        Map(m => m.Prefecture).Name("Prefecture").Optional();
        Map(m => m.City).Name("City").Optional();
        Map(m => m.Town1).Name("Town1").Optional();
        Map(m => m.Town2).Name("Town2").Optional();
        Map(m => m.PhoneNumber).Name("PhoneNumber").Optional();
        Map(m => m.BankCode).Name("BankCode");
        Map(m => m.BranchCode).Name("BranchCode");
        Map(m => m.AccountType).Name("AccountType");
        Map(m => m.AccountNo).Name("AccountNo");
        Map(m => m.CreditProductName).Name("CreditProductName").Optional();
        Map(m => m.CreditTotalAmount).Name("CreditTotalAmount").Optional();
        Map(m => m.CreditTotalCount).Name("CreditTotalCount").Optional();
        Map(m => m.CreditCompletedCount).Name("CreditCompletedCount").Optional();
        Map(m => m.CreditPayment1).Name("CreditPayment1").Optional();
        Map(m => m.CreditPayment2).Name("CreditPayment2").Optional();
        Map(m => m.CreditSpecialAddition).Name("CreditSpecialAddition").Optional();
        Map(m => m.CreditBillingAmount).Name("CreditBillingAmount").Optional();
        Map(m => m.PreviousBalance).Name("PreviousBalance").Default(0m);
        Map(m => m.CurrentBillingAmount).Name("CurrentBillingAmount").Default(0m);
        Map(m => m.WithdrawalDate).Name("WithdrawalDate").Optional();
        Map(m => m.BankProcessDate).Name("BankProcessDate").Optional();
        Map(m => m.ExemptionDate).Name("ExemptionDate").Optional();
        Map(m => m.ExemptionType).Name("ExemptionType").Optional();
        Map(m => m.ChangeDate).Name("ChangeDate").Optional();
        Map(m => m.ChangeAction).Name("ChangeAction", "IDOK", "IDOKU").Optional();
        Map(m => m.InvoiceTaxRate1).Name("InvoiceTaxRate1").Optional();
        Map(m => m.InvoiceTaxRate2).Name("InvoiceTaxRate2").Optional();
        Map(m => m.InvoiceBasePrice).Name("InvoiceBasePrice").Optional();
        Map(m => m.InvoiceConsumptionTax).Name("InvoiceConsumptionTax").Optional();

        // FK / Navigation / BaseEntity — ignored (resolved in PreProcess)
        Map(m => m.Id).Ignore();
        Map(m => m.CompanyId).Ignore();
        Map(m => m.BankBranchId).Ignore();
        Map(m => m.CreatedAt).Ignore();
        Map(m => m.UpdatedAt).Ignore();
        Map(m => m.Company).Ignore();
        Map(m => m.BankBranch).Ignore();
        Map(m => m.Types).Ignore();
        Map(m => m.BillingAmounts).Ignore();
        Map(m => m.TransferFailures).Ignore();
    }
}
