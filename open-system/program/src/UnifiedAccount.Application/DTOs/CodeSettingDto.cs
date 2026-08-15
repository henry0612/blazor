namespace UnifiedAccount.Application.DTOs;

public record CodeSettingDto
{
    public long Id { get; init; }
    public string CodeCategory { get; init; } = string.Empty;
    public string CodeValue { get; init; } = string.Empty;
    public string DisplayText { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public DateTime UpdatedAt { get; init; }
}
