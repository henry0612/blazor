using System.Globalization;
using UnifiedAccount.Application.DTOs;

namespace UnifiedAccount.Web.Components.Pages.Code;

internal enum CodeSettingDeleteAction
{
    Empty,
    Delete
}

internal sealed class CodeSettingEditRowDraft
{
    public Guid ClientKey { get; } = Guid.NewGuid();

    public long? Id { get; init; }

    public bool IsNew { get; init; }

    public bool IsDeleted { get; private set; }

    public CodeSettingDeleteAction DeleteAction { get; set; } = CodeSettingDeleteAction.Empty;

    public string CodeValue { get; set; } = string.Empty;

    public string DisplayText { get; set; } = string.Empty;

    public string DisplayOrderText { get; set; } = string.Empty;

    public string OriginalCodeValue { get; init; } = string.Empty;

    public string OriginalDisplayText { get; init; } = string.Empty;

    public string OriginalDisplayOrderText { get; init; } = string.Empty;

    public DateTime? OriginalUpdatedAt { get; init; }

    public static CodeSettingEditRowDraft FromDto(CodeSettingDto dto)
    {
        return new CodeSettingEditRowDraft
        {
            Id = dto.Id,
            CodeValue = dto.CodeValue,
            DisplayText = dto.DisplayText,
            DisplayOrderText = dto.DisplayOrder.ToString(CultureInfo.InvariantCulture),
            OriginalCodeValue = dto.CodeValue,
            OriginalDisplayText = dto.DisplayText,
            OriginalDisplayOrderText = dto.DisplayOrder.ToString(CultureInfo.InvariantCulture),
            OriginalUpdatedAt = dto.UpdatedAt
        };
    }

    public static CodeSettingEditRowDraft CreateNew()
    {
        return new CodeSettingEditRowDraft
        {
            IsNew = true
        };
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
    }

    public void ApplyDeleteConfirmation(bool confirmed)
    {
        DeleteAction = confirmed
            ? CodeSettingDeleteAction.Delete
            : CodeSettingDeleteAction.Empty;
    }

    public bool HasChanges()
    {
        return CodeValue != OriginalCodeValue
            || DisplayText != OriginalDisplayText
            || DisplayOrderText != OriginalDisplayOrderText;
    }

    public bool HasMeaningfulInput()
    {
        return !string.IsNullOrWhiteSpace(CodeValue)
            || !string.IsNullOrWhiteSpace(DisplayText)
            || !string.IsNullOrWhiteSpace(DisplayOrderText);
    }
}
