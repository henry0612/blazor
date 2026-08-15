using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Infrastructure.Persistence.Configurations;

/// <summary>
/// JapaneseEraテーブルの構成 + シードデータ (明治〜令和)
/// 新元号追加時はこのシードを追加するか、DBに直接INSERTする。
/// </summary>
public class JapaneseEraConfiguration : IEntityTypeConfiguration<JapaneseEra>
{
    public void Configure(EntityTypeBuilder<JapaneseEra> builder)
    {
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.StartDate);

        // シードデータ: 明治〜令和
        builder.HasData(
            new JapaneseEra
            {
                Id = 1,
                Code = 1,
                Name = "明治",
                Abbreviation = "M",
                StartDate = new DateOnly(1868, 1, 25),
                EndDate = new DateOnly(1912, 7, 29),
                BaseYear = 1867,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0)
            },
            new JapaneseEra
            {
                Id = 2,
                Code = 2,
                Name = "大正",
                Abbreviation = "T",
                StartDate = new DateOnly(1912, 7, 30),
                EndDate = new DateOnly(1926, 12, 24),
                BaseYear = 1911,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0)
            },
            new JapaneseEra
            {
                Id = 3,
                Code = 3,
                Name = "昭和",
                Abbreviation = "S",
                StartDate = new DateOnly(1926, 12, 25),
                EndDate = new DateOnly(1989, 1, 7),
                BaseYear = 1925,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0)
            },
            new JapaneseEra
            {
                Id = 4,
                Code = 4,
                Name = "平成",
                Abbreviation = "H",
                StartDate = new DateOnly(1989, 1, 8),
                EndDate = new DateOnly(2019, 4, 30),
                BaseYear = 1988,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0)
            },
            new JapaneseEra
            {
                Id = 5,
                Code = 5,
                Name = "令和",
                Abbreviation = "R",
                StartDate = new DateOnly(2019, 5, 1),
                EndDate = null,
                BaseYear = 2018,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0)
            }
        );
    }
}
