using CsvHelper.Configuration;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Migration.Importers;

/// <summary>
/// 金融機関支店マスター CSV → BankBranches テーブル インポーター。
/// CSV 形式:
///   BankCode,BranchCode,BankNameKana,BranchNameKana,BankNameKanji,BranchNameKanji,OfficeName,KanjiSetFlag
/// </summary>
public sealed class BankBranchImporter : CsvImporter<BankBranch, BankBranchMap>
{
    public BankBranchImporter(AppDbContext db) : base(db) { }

    protected override string TableName => "BankBranches";
}

public sealed class BankBranchMap : ClassMap<BankBranch>
{
    public BankBranchMap()
    {
        Map(m => m.BankCode).Name("BankCode");
        Map(m => m.BranchCode).Name("BranchCode");
        Map(m => m.BankNameKana).Name("BankNameKana");
        Map(m => m.BranchNameKana).Name("BranchNameKana");
        Map(m => m.BankNameKanji).Name("BankNameKanji").Optional();
        Map(m => m.BranchNameKanji).Name("BranchNameKanji").Optional();
        Map(m => m.OfficeName).Name("OfficeName").Optional();
        Map(m => m.KanjiSetFlag).Name("KanjiSetFlag").Default("0");

        // BaseEntity fields — auto-set, skip in CSV
        Map(m => m.Id).Ignore();
        Map(m => m.CreatedAt).Ignore();
        Map(m => m.UpdatedAt).Ignore();
        Map(m => m.Contracts).Ignore();
        Map(m => m.BankBranchChanges).Ignore();
    }
}
