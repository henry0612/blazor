using CsvHelper.Configuration;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Migration.Importers;

/// <summary>
/// 処理カレンダー CSV → ProcessingCalendars テーブル インポーター。
/// CSV 形式:
///   ProcessingDate
///   2025-01-06
///   2025-01-07
/// </summary>
public sealed class CalendarImporter : CsvImporter<ProcessingCalendar, CalendarMap>
{
    public CalendarImporter(AppDbContext db) : base(db) { }

    protected override string TableName => "ProcessingCalendars";
}

public sealed class CalendarMap : ClassMap<ProcessingCalendar>
{
    public CalendarMap()
    {
        Map(m => m.ProcessingDate).Name("ProcessingDate");

        // BaseEntity / Navigation — ignored
        Map(m => m.Id).Ignore();
        Map(m => m.CreatedAt).Ignore();
        Map(m => m.UpdatedAt).Ignore();
        Map(m => m.Slots).Ignore();
    }
}
