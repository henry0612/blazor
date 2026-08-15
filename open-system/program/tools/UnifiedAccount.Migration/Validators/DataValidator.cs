using Microsoft.EntityFrameworkCore;
using Serilog;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Migration.Validators;

/// <summary>
/// 移行後データの整合性を検証するバリデーター。
/// - レコード件数の確認
/// - 金額チェックサム (Contract の CurrentBillingAmount 合計など)
/// - FK 参照整合性チェック (Contracts → Companies, BankBranches)
/// - 重複キーの検出
/// </summary>
public sealed class DataValidator
{
    private readonly AppDbContext _db;

    public DataValidator(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>すべてのバリデーションを実行し、成否を返す。</summary>
    public async Task<bool> ValidateAsync()
    {
        Log.Information("=== Data Validation Start ===");
        var allPassed = true;

        allPassed &= await ValidateRecordCountsAsync();
        allPassed &= await ValidateChecksumsAsync();
        allPassed &= await ValidateReferentialIntegrityAsync();
        allPassed &= await ValidateDuplicateKeysAsync();

        Log.Information("=== Data Validation {Result} ===", allPassed ? "PASSED" : "FAILED");
        return allPassed;
    }

    /// <summary>各テーブルのレコード件数を表示する。</summary>
    private async Task<bool> ValidateRecordCountsAsync()
    {
        Log.Information("--- Record Count Summary ---");

        var counts = new Dictionary<string, int>
        {
            ["BankBranches"] = await _db.BankBranches.CountAsync(),
            ["Companies"] = await _db.Companies.CountAsync(),
            ["Contracts"] = await _db.Contracts.CountAsync(),
            ["ProcessingCalendars"] = await _db.ProcessingCalendars.CountAsync(),
        };

        foreach (var (table, count) in counts)
        {
            Log.Information("  {Table}: {Count} records", table, count);
            if (count == 0)
                Log.Warning("  ⚠ {Table} is empty!", table);
        }

        return true; // 件数にしきい値が無いので常に pass（表示目的）
    }

    /// <summary>金額テーブルのチェックサムを計算する。</summary>
    private async Task<bool> ValidateChecksumsAsync()
    {
        Log.Information("--- Checksum Validation ---");

        var billingSum = await _db.Contracts.SumAsync(c => c.CurrentBillingAmount);
        Log.Information("  Contracts.CurrentBillingAmount total = {Sum:N0}", billingSum);

        var balanceSum = await _db.Contracts.SumAsync(c => c.PreviousBalance);
        Log.Information("  Contracts.PreviousBalance total = {Sum:N0}", balanceSum);

        var basicFeeSum = await _db.Companies.SumAsync(c => c.BasicFee);
        Log.Information("  Companies.BasicFee total = {Sum:N0}", basicFeeSum);

        return true; // チェックサムは表示のみ（移行元値と手動比較）
    }

    /// <summary>FK 参照整合性を検証する。</summary>
    private async Task<bool> ValidateReferentialIntegrityAsync()
    {
        Log.Information("--- Referential Integrity ---");
        var passed = true;

        // Contracts → Companies (CompanyId)
        var orphanCompany = await _db.Contracts
            .Where(c => !_db.Companies.Any(co => co.Id == c.CompanyId))
            .CountAsync();
        if (orphanCompany > 0)
        {
            Log.Error("  {Count} Contracts have invalid CompanyId (orphan FK)", orphanCompany);
            passed = false;
        }
        else
        {
            Log.Information("  Contracts → Companies: OK");
        }

        // Contracts → BankBranches (BankBranchId) — nullable
        var orphanBank = await _db.Contracts
            .Where(c => c.BankBranchId != null
                        && !_db.BankBranches.Any(b => b.Id == c.BankBranchId))
            .CountAsync();
        if (orphanBank > 0)
        {
            Log.Error("  {Count} Contracts have invalid BankBranchId (orphan FK)", orphanBank);
            passed = false;
        }
        else
        {
            Log.Information("  Contracts → BankBranches: OK");
        }

        return passed;
    }

    /// <summary>一意キーの重複を検出する。</summary>
    private async Task<bool> ValidateDuplicateKeysAsync()
    {
        Log.Information("--- Duplicate Key Detection ---");
        var passed = true;

        // BankBranches: BankCode + BranchCode
        var dupBanks = await _db.BankBranches
            .GroupBy(b => new { b.BankCode, b.BranchCode })
            .Where(g => g.Count() > 1)
            .CountAsync();
        if (dupBanks > 0)
        {
            Log.Error("  {Count} duplicate BankBranch keys found", dupBanks);
            passed = false;
        }
        else
        {
            Log.Information("  BankBranches unique keys: OK");
        }

        // Companies: CompanyCode
        var dupCompanies = await _db.Companies
            .GroupBy(c => c.CompanyCode)
            .Where(g => g.Count() > 1)
            .CountAsync();
        if (dupCompanies > 0)
        {
            Log.Error("  {Count} duplicate Company codes found", dupCompanies);
            passed = false;
        }
        else
        {
            Log.Information("  Companies unique keys: OK");
        }

        // Contracts: CompanyCode + PersonalCode + CheckDigit
        var dupContracts = await _db.Contracts
            .GroupBy(c => new { c.CompanyCode, c.PersonalCode, c.CheckDigit })
            .Where(g => g.Count() > 1)
            .CountAsync();
        if (dupContracts > 0)
        {
            Log.Error("  {Count} duplicate Contract keys found", dupContracts);
            passed = false;
        }
        else
        {
            Log.Information("  Contracts unique keys: OK");
        }

        // ProcessingCalendars: ProcessingDate
        var dupCalendars = await _db.ProcessingCalendars
            .GroupBy(c => c.ProcessingDate)
            .Where(g => g.Count() > 1)
            .CountAsync();
        if (dupCalendars > 0)
        {
            Log.Error("  {Count} duplicate ProcessingCalendar dates found", dupCalendars);
            passed = false;
        }
        else
        {
            Log.Information("  ProcessingCalendars unique keys: OK");
        }

        return passed;
    }
}
