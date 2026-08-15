using System.Globalization;
using System.Security.Cryptography;

namespace UnifiedAccount.Application.Services;

/// <summary>
/// IExecutionNumberService を表すインターフェイス。
/// </summary>
public interface IExecutionNumberService
{
    string GenerateJobExecutionId(string jobId, DateOnly processDate);

    string GenerateReportDataId(string templateId, DateOnly processDate);
}

/// <summary>
/// ExecutionNumberService を表すクラス。
/// </summary>
public class ExecutionNumberService : IExecutionNumberService
{
    public string GenerateJobExecutionId(string jobId, DateOnly processDate)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            throw new ArgumentException("ジョブ ID は必須です。", nameof(jobId));
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "JOB-{0}-{1}-{2}",
            Normalize(jobId),
            processDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            CreateSuffix());
    }

    public string GenerateReportDataId(string templateId, DateOnly processDate)
    {
        if (string.IsNullOrWhiteSpace(templateId))
        {
            throw new ArgumentException("テンプレート ID は必須です。", nameof(templateId));
        }

        return string.Format(
            CultureInfo.InvariantCulture,
            "REPORT-{0}-{1}-{2}",
            Normalize(templateId),
            processDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            CreateSuffix());
    }

    private static string Normalize(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? "UNKNOWN"
            : value.Trim().ToUpperInvariant();

        return string.Concat(normalized.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-'));
    }

    private static string CreateSuffix()
    {
        var random = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return random.ToString("D6", CultureInfo.InvariantCulture);
    }
}

