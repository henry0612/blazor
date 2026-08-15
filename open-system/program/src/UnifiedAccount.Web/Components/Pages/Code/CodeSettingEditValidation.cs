using System.Globalization;
using UnifiedAccount.Application.DTOs;

namespace UnifiedAccount.Web.Components.Pages.Code;

internal static class CodeSettingEditValidation
{
    public static IReadOnlyList<CodeSettingEditRowDraft> GetPersistableRows(IEnumerable<CodeSettingEditRowDraft> rows)
    {
        return rows
            .Where(row => !row.IsDeleted)
            .Where(row => row.DeleteAction != CodeSettingDeleteAction.Delete)
            .Where(row => row.IsNew ? row.HasMeaningfulInput() : row.HasChanges())
            .ToList();
    }

    public static IReadOnlyList<string> ValidateRows(IEnumerable<CodeSettingEditRowDraft> rows)
    {
        var errors = new List<string>();
        var seenCodeValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            if (row.IsDeleted || row.DeleteAction == CodeSettingDeleteAction.Delete)
            {
                continue;
            }

            var codeEmpty = string.IsNullOrWhiteSpace(row.CodeValue);
            var textEmpty = string.IsNullOrWhiteSpace(row.DisplayText);
            var orderEmpty = string.IsNullOrWhiteSpace(row.DisplayOrderText);
            var allEmpty = codeEmpty && textEmpty && orderEmpty;

            if (row.IsNew && allEmpty)
            {
                continue;
            }

            if (!row.IsNew && allEmpty)
            {
                errors.Add("既存行は3項目すべて空欄のまま保存できません。");
                continue;
            }

            if (codeEmpty && (!textEmpty || !orderEmpty))
            {
                errors.Add("コードが空欄の行は、表示文字列または表示順が入力されていると保存できません。");
                continue;
            }

            if (!orderEmpty && !int.TryParse(row.DisplayOrderText, out _))
            {
                errors.Add("表示順は数値で入力してください。");
                continue;
            }

            if (!codeEmpty && !seenCodeValues.Add(row.CodeValue.Trim()))
            {
                errors.Add("コードが重複しています。");
            }
        }

        return errors;
    }

    public static CodeSettingEditRequest BuildSaveRequest(string categoryCode, CodeSettingEditRowDraft row)
    {
        var displayOrder = 0;
        if (!string.IsNullOrWhiteSpace(row.DisplayOrderText))
        {
            displayOrder = int.Parse(row.DisplayOrderText, CultureInfo.InvariantCulture);
        }

        return new CodeSettingEditRequest
        {
            Id = row.Id,
            CodeCategory = categoryCode,
            CodeValue = row.CodeValue.Trim(),
            DisplayText = row.DisplayText.Trim(),
            DisplayOrder = displayOrder,
            OriginalUpdatedAt = row.OriginalUpdatedAt
        };
    }

    public static CodeSettingDeleteRequest BuildDeleteRequest(CodeSettingEditRowDraft row)
    {
        if (!row.Id.HasValue || !row.OriginalUpdatedAt.HasValue)
        {
            throw new InvalidOperationException("削除対象の既存行に楽観ロック値がありません。");
        }

        return new CodeSettingDeleteRequest
        {
            Id = row.Id.Value,
            OriginalUpdatedAt = row.OriginalUpdatedAt.Value
        };
    }
}
