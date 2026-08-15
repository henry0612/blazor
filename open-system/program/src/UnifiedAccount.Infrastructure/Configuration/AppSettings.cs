namespace UnifiedAccount.Infrastructure.Configuration;

/// <summary>
/// アプリケーション設定
/// </summary>
public class AppSettings
{
    public int MaxBatchSize { get; set; } = 10000;
    public int ZenginTimeout { get; set; } = 30;
    public int DefaultPageSize { get; set; } = 20;
}

