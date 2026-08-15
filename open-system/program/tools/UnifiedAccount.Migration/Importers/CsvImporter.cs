using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Migration.Importers;

/// <summary>
/// CSV ファイルからエンティティを一括インポートする基底クラス。
/// CsvHelper でレコードを読み取り、EF Core でバッチ INSERT する。
/// </summary>
/// <typeparam name="TEntity">対象エンティティ型</typeparam>
/// <typeparam name="TMap">CsvHelper ClassMap 型</typeparam>
public abstract class CsvImporter<TEntity, TMap>
    where TEntity : BaseEntity
    where TMap : ClassMap<TEntity>
{
    protected readonly AppDbContext Db;

    protected CsvImporter(AppDbContext db)
    {
        Db = db;
    }

    /// <summary>インポート対象テーブルの表示名</summary>
    protected abstract string TableName { get; }

    /// <summary>
    /// インポート前にレコードを加工する(FK 解決など)。
    /// デフォルト実装は何もしない。
    /// </summary>
    protected virtual Task PreProcessAsync(IList<TEntity> batch) => Task.CompletedTask;

    /// <summary>
    /// CSV ファイルを読み込み、バッチ単位で DB に INSERT する。
    /// </summary>
    public async Task ImportAsync(string csvPath, int batchSize = 1000)
    {
        if (!File.Exists(csvPath))
        {
            Log.Error("{Table}: CSV file not found — {Path}", TableName, csvPath);
            throw new FileNotFoundException($"CSV file not found: {csvPath}");
        }

        Log.Information("{Table}: Starting import from {Path}", TableName, csvPath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim,
        };

        using var reader = new StreamReader(csvPath, System.Text.Encoding.UTF8);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<TMap>();

        var records = csv.GetRecords<TEntity>();
        var batch = new List<TEntity>(batchSize);
        int totalCount = 0;
        int errorCount = 0;

        foreach (var record in records)
        {
            batch.Add(record);

            if (batch.Count >= batchSize)
            {
                errorCount += await FlushBatchAsync(batch, totalCount);
                totalCount += batch.Count;
                batch.Clear();
            }
        }

        // 残りをフラッシュ
        if (batch.Count > 0)
        {
            errorCount += await FlushBatchAsync(batch, totalCount);
            totalCount += batch.Count;
        }

        Log.Information("{Table}: Import complete — {Total} records imported, {Errors} errors.",
            TableName, totalCount, errorCount);

        // 検証: DB 上の件数を確認
        var dbCount = await Db.Set<TEntity>().CountAsync();
        Log.Information("{Table}: DB record count = {Count}", TableName, dbCount);
    }

    private async Task<int> FlushBatchAsync(IList<TEntity> batch, int offset)
    {
        int errors = 0;
        try
        {
            await PreProcessAsync(batch);
            Db.Set<TEntity>().AddRange(batch);
            await Db.SaveChangesAsync();
            Log.Information("{Table}: Inserted rows {From}–{To}",
                TableName, offset + 1, offset + batch.Count);
        }
        catch (DbUpdateException ex)
        {
            errors = batch.Count;
            Log.Error(ex, "{Table}: Batch insert failed at offset {Offset}. Attempting row-by-row insert.",
                TableName, offset);

            // ロールバックしてから 1 行ずつ再試行
            foreach (var entry in Db.ChangeTracker.Entries())
                entry.State = EntityState.Detached;

            foreach (var record in batch)
            {
                try
                {
                    Db.Set<TEntity>().Add(record);
                    await Db.SaveChangesAsync();
                    errors--;
                }
                catch (Exception rowEx)
                {
                    Log.Warning(rowEx, "{Table}: Skipped row (Id={Id})", TableName, record.Id);
                    foreach (var entry in Db.ChangeTracker.Entries())
                        entry.State = EntityState.Detached;
                }
            }
        }

        return errors;
    }
}
