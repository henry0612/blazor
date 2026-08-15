using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Domain.Enums;

namespace UnifiedAccount.Infrastructure.Persistence.Configurations;

/// <summary>
/// Users テーブルの Fluent API 設定
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.UserId).IsUnique();

        // Role は文字列で保存 (可読性確保)
        builder.Property(u => u.Role)
               .HasConversion(
                    role => role.ToString(),
                    value => ParseUserRole(value))
               .HasMaxLength(30);

        builder.Property(u => u.IsActive)
               .HasDefaultValue(true);
    }

    private static UserRole ParseUserRole(string value)
    {
        if (Enum.TryParse<UserRole>(value, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        // 旧データ互換: ロール名称変更前の値を現行列挙へ読み替える。
        return value switch
        {
            "Administrator" => UserRole.SystemAdmin,
            "Viewer" => UserRole.Operator,
            _ => throw new InvalidOperationException($"未対応のユーザーロールです: {value}")
        };
    }
}

