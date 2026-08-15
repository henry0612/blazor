namespace UnifiedAccount.Application.DTOs;

public class CodeSettingEditRequest
{
    public long? Id { get; set; }
    public string CodeCategory { get; set; } = string.Empty;
    public string CodeValue { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime? OriginalUpdatedAt { get; set; }
}
