using System.Globalization;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Migration.Exporters;

/// <summary>
/// DB テーブルを CSV ファイルにエクスポートし、移行元データとの突合を可能にする。
/// </summary>
public sealed class CsvExporter
{
    private readonly AppDbContext _db;

    public CsvExporter(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>主要テーブルをすべて CSV にエクスポートする。</summary>
    public async Task ExportAllAsync(string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        await ExportTableAsync(_db.BankBranches, Path.Combine(outputDir, "BankBranches.csv"));
        await ExportTableAsync(_db.Companies, Path.Combine(outputDir, "Companies.csv"));
        await ExportTableAsync(_db.Contracts, Path.Combine(outputDir, "Contracts.csv"));
        await ExportTableAsync(_db.ProcessingCalendars, Path.Combine(outputDir, "ProcessingCalendars.csv"));

        Log.Information("Export complete. Files written to {Dir}", outputDir);
    }

    private static async Task ExportTableAsync<T>(DbSet<T> dbSet, string filePath) where T : class
    {
        Log.Information("Exporting {Table} → {File}", typeof(T).Name, filePath);

        var records = await dbSet.AsNoTracking().ToListAsync();

        await using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(records);

        Log.Information("  {Count} records written.", records.Count);
    }
}
