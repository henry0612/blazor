using Microsoft.Extensions.Logging;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Application.Services.Reports;

namespace UnifiedAccount.Infrastructure.Services.Reports;

/// <summary>
/// 帳票テンプレートの検索と検証を行うサービス（ステップ2）。
/// ファイルシステムとデータベースから テンプレート定義を取得する。
/// COBOL移行元: KSYMZERO (テンプレート検証ロジック)
/// </summary>
public class ReportTemplateResolver : IReportTemplateResolver
{
    /// <summary>
    /// ロガーを保持する。
    /// </summary>
    private readonly ILogger<ReportTemplateResolver> _logger;
    /// <summary>
    /// テンプレート基準パスを保持する。
    /// </summary>
    private readonly string _templateBasePath;
    /// <summary>
    /// キャッシュを保持する。
    /// </summary>
    private Dictionary<string, ReportTemplate> _cache;
    /// <summary>
    /// キャッシュ時刻を保持する。
    /// </summary>
    private DateTime _cacheTime;
    private const int CacheExpireMinutes = 60;

    public ReportTemplateResolver(ILogger<ReportTemplateResolver> logger, string templateBasePath = "./Templates")
    {
        _logger = logger;
        _templateBasePath = templateBasePath ?? "./Templates";
        _cache = new Dictionary<string, ReportTemplate>();
        _cacheTime = DateTime.MinValue;
    }

    /// <summary>
    /// テンプレートIDからテンプレート定義を検索し、ファイルの存在と妥当性を検証する。
    /// </summary>
    /// <param name="templateId">テンプレートIDを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<ReportTemplate?> ResolveAsync(string templateId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateId))
        {
            _logger.LogError("テンプレートID が空です。");
            throw new ArgumentException("テンプレートID は必須です。", nameof(templateId));
        }

        // キャッシュから取得を試みる
        if (IsValidCache() && _cache.TryGetValue(templateId, out var cachedTemplate))
        {
            _logger.LogDebug("キャッシュからテンプレート取得: {TemplateId}", templateId);
            return cachedTemplate;
        }

        // ファイルシステムから取得（或いはDB）
        var template = await LoadTemplateFromFileSystemAsync(templateId, cancellationToken);

        if (template is null)
        {
            _logger.LogError("テンプレートが見つかりません: {TemplateId}", templateId);
            throw new FileNotFoundException($"テンプレート '{templateId}' が見つかりません。");
        }

        // 妥当性検証
        ValidateTemplate(template);

        // キャッシュに登録
        _cache[templateId] = template;

        return template;
    }

    /// <summary>
    /// 複数のテンプレートを一括取得。
    /// </summary>
    public async Task<IEnumerable<ReportTemplate>> ResolveAllAsync(CancellationToken cancellationToken = default)
    {
        if (IsValidCache())
        {
            _logger.LogDebug("キャッシュからテンプレート一覧を取得");
            return _cache.Values;
        }

        var templates = new List<ReportTemplate>();

        // テンプレートディレクトリをスキャン
        if (!Directory.Exists(_templateBasePath))
        {
            _logger.LogWarning("テンプレートディレクトリが存在しません: {Path}", _templateBasePath);
            return templates;
        }

        var templateFiles = Directory.GetFiles(_templateBasePath, "*.dcx", SearchOption.TopDirectoryOnly);

        foreach (var filePath in templateFiles)
        {
            try
            {
                var template = await LoadTemplateFromFileAsync(filePath, cancellationToken);
                if (template != null)
                {
                    ValidateTemplate(template);
                    templates.Add(template);
                    _cache[template.TemplateId] = template;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "テンプレートファイルの読み込みに失敗: {FilePath}", filePath);
            }
        }

        _cacheTime = LocalDateTimeProvider.Now;
        return templates;
    }

    /// <summary>
    /// テンプレートが使用可能状態か確認。
    /// </summary>
    /// <param name="templateId">テンプレートIDを指定する。</param>
    /// <param name="cancellationToken">キャンセルトークンを指定する。</param>
    public async Task<bool> IsValidAsync(string templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await ResolveAsync(templateId, cancellationToken);
            return template?.IsActive ?? false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<ReportTemplate?> LoadTemplateFromFileSystemAsync(string templateId, CancellationToken cancellationToken)
    {
        // .dcx ファイルを検索
        var dcxFileName = $"{templateId}.dcx";
        var filePath = Path.Combine(_templateBasePath, dcxFileName);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("テンプレートファイルが見つかりません: {FilePath}", filePath);
            return null;
        }

        return await LoadTemplateFromFileAsync(filePath, cancellationToken);
    }

    private async Task<ReportTemplate?> LoadTemplateFromFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            // ファイル情報から基本情報を取得
            var fileInfo = new FileInfo(filePath);

            var template = new ReportTemplate
            {
                TemplateId = fileName,
                TemplateName = fileName,
                TemplateFilePath = filePath,
                OutputFormat = "PDF",
                PageWidth = 210,
                PageHeight = 297,
                CreatedAt = fileInfo.CreationTime,
                UpdatedAt = fileInfo.LastWriteTime,
                IsActive = true,
                RequiredFields = new List<string>(),
                DetailFields = new List<string>(),
                SummaryFields = new List<string>()
            };

            // .dcx メタデータファイル（XML）があれば詳細情報を読み込み
            var metaFilePath = Path.ChangeExtension(filePath, ".xml");
            if (File.Exists(metaFilePath))
            {
                await LoadTemplateMetadataAsync(template, metaFilePath, cancellationToken);
            }

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "テンプレートファイルの読み込みエラー: {FilePath}", filePath);
            throw;
        }
    }

    private async Task LoadTemplateMetadataAsync(ReportTemplate template, string metaFilePath, CancellationToken cancellationToken)
    {
        // TODO: XML メタデータから詳細情報を解析
        // 例: 必須フィールド、出力形式、ページサイズなど
        await Task.CompletedTask;
    }

    private void ValidateTemplate(ReportTemplate template)
    {
        if (string.IsNullOrWhiteSpace(template.TemplateId))
            throw new InvalidOperationException("テンプレートID は必須です。");

        if (!File.Exists(template.TemplateFilePath))
            throw new FileNotFoundException($"テンプレートファイルが見つかりません: {template.TemplateFilePath}");

        var formFilePath = Path.ChangeExtension(template.TemplateFilePath, ".cfx");
        if (!File.Exists(formFilePath))
            throw new FileNotFoundException($".cfx フォームファイルが見つかりません: {formFilePath}");

        if (template.PageWidth <= 0 || template.PageHeight <= 0)
            throw new InvalidOperationException("ページサイズは 0 より大きい値である必要があります。");

        _logger.LogDebug("テンプレート検証成功: {TemplateId}", template.TemplateId);
    }

    private bool IsValidCache()
    {
        return LocalDateTimeProvider.Now - _cacheTime < TimeSpan.FromMinutes(CacheExpireMinutes);
    }
}

