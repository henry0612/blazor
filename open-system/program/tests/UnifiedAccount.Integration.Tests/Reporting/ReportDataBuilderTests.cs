using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Infrastructure.Services.Reports;
using UnifiedAccount.Application.Services.Reports;

namespace UnifiedAccount.Integration.Tests.Reporting;

/// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
/// ReportDataBuilder テスト (データベース統合)
/// Buffered/Streamed デュアルモード動作確認
/// </summary>
public class ReportDataBuilderTests : IDisposable
{
    private readonly ReportDataBuilder _builder;
    private readonly IServiceProvider _serviceProvider;

    public ReportDataBuilderTests()
    {
        // テスト用ロガー
        var services = new ServiceCollection().AddLogging(b => b.AddDebug());
        _serviceProvider = services.BuildServiceProvider();
        var logger = _serviceProvider.GetRequiredService<ILogger<ReportDataBuilder>>();

        _builder = new ReportDataBuilder(logger);
    }

    public void Dispose()
    {
    }

    /// <summary>このテストでは 帳票を生成したとき、帳票データや結果が生成される。</summary>
    [Fact]
    public async Task BuildDataAsync_Buffered_ShouldReturnReportDataSet()
    {
        // Arrange
        var template = new ReportTemplate
        {
            TemplateId = "TEST-001",
            TemplateName = "Test Template",
            RequiredFields = Array.Empty<string>(),
            DetailFields = new[] { "CompanyCode", "PersonalCode", "WithdrawalDay" }
        };

        var dataRequest = new ReportDataRequest
        {
            Mode = ReportDataMode.Buffered,
            SourceData = Enumerable.Range(1, 20)
                .Select(i => new Dictionary<string, object>
                {
                    ["CompanyCode"] = "TEST01",
                    ["PersonalCode"] = $"PERS{i:D8}",
                    ["WithdrawalDay"] = 10
                })
                .ToList()
        };

        // Act
        var result = await _builder.BuildDataAsync(dataRequest, template);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ReportDataSet>();

        var dataSet = (ReportDataSet)result;
        dataSet.DetailRows.Should().HaveCount(20);
        dataSet.SummaryData["TotalRows"].Should().Be(20);
        dataSet.DetailRows.First().Should().ContainKey("PersonalCode");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task BuildDataAsync_Streamed_ShouldReturnEfCoreRecordSet()
    {
        // Arrange
        var template = new ReportTemplate
        {
            TemplateId = "TEST-002",
            TemplateName = "Test Template Streamed",
            RequiredFields = Array.Empty<string>(),
            DetailFields = new[] { "CompanyCode", "PersonalCode" }
        };

        var dataRequest = new ReportDataRequest
        {
            Mode = ReportDataMode.Streamed,
            SourceData = Enumerable.Range(1, 20)
                .Select(i => new
                {
                    CompanyCode = "TEST01",
                    PersonalCode = $"PERS{i:D8}"
                })
                .Cast<object>()
                .ToList()
        };

        // Act
        var result = await _builder.BuildDataAsync(dataRequest, template);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<EfCoreRecordSet>();

        var recordSet = (EfCoreRecordSet)result;
        recordSet.RecordCount.Should().Be(-1); // Streamed は不明
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task BuildDataAsync_Auto_ShouldSelectBufferedForSmallDataset()
    {
        // Arrange: 20件 < 10k 閾値 → Buffered を選択するはず
        var template = new ReportTemplate
        {
            TemplateId = "TEST-003",
            TemplateName = "Test Auto Mode",
            RequiredFields = Array.Empty<string>(),
            DetailFields = new[] { "PersonalCode" }
        };

        var dataRequest = new ReportDataRequest
        {
            Mode = ReportDataMode.Auto,
            SourceData = Enumerable.Range(1, 20)
                .Select(i => new Dictionary<string, object>
                {
                    ["PersonalCode"] = $"PERS{i:D8}"
                })
                .ToList()
        };

        // Act
        var result = await _builder.BuildDataAsync(dataRequest, template);

        // Assert
        result.Should().BeOfType<ReportDataSet>(); // Auto で小規模 → Buffered
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task BuildDataAsync_Buffered_WithSourceRows_ShouldUseSourceData()
    {
        var template = new ReportTemplate
        {
            TemplateId = "TEST-004",
            TemplateName = "Source Rows",
            RequiredFields = Array.Empty<string>(),
            DetailFields = new[] { "Text1", "Text2" }
        };

        var dataRequest = new ReportDataRequest
        {
            Mode = ReportDataMode.Buffered,
            SourceData = new List<Dictionary<string, object>>
            {
                new()
                {
                    ["Text1"] = "A001",
                    ["Text2"] = "株式会社A"
                },
                new()
                {
                    ["Text1"] = "A002",
                    ["Text2"] = "株式会社B"
                }
            }
        };

        var result = await _builder.BuildDataAsync(dataRequest, template);

        result.Should().BeOfType<ReportDataSet>();
        var dataSet = (ReportDataSet)result;
        dataSet.DetailRows.Should().HaveCount(2);
        dataSet.DetailRows[0]["Text1"].Should().Be("A001");
        dataSet.DetailRows[1]["Text2"].Should().Be("株式会社B");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task ValidateDataAsync_ValidDataSet_ShouldReturnIsValidTrue()
    {
        // Arrange
        var dataSet = new ReportDataSet
        {
            TemplateId = "TEST-004",
            HeaderData = new Dictionary<string, object> { { "CompanyCode", "TEST01" } },
            DetailRows = new List<Dictionary<string, object>>
            {
                new() { { "CompanyCode", "TEST01" }, { "PersonalCode", "PERS00000001" } }
            },
            SummaryData = new Dictionary<string, object> { { "TotalRows", 1 } }
        };

        var template = new ReportTemplate
        {
            TemplateId = "TEST-004",
            TemplateName = "Test Validate",
            RequiredFields = new[] { "CompanyCode" },
            DetailFields = new[] { "CompanyCode", "PersonalCode" }
        };

        // Act
        var result = await _builder.ValidateDataAsync(dataSet, template);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task ValidateDataAsync_MissingRequiredField_ShouldReturnIsValidFalse()
    {
        // Arrange: CompanyCode なし
        var dataSet = new ReportDataSet
        {
            TemplateId = "TEST-005",
            HeaderData = new Dictionary<string, object>(), // 空
            DetailRows = new List<Dictionary<string, object>>(),
            SummaryData = new Dictionary<string, object>()
        };

        var template = new ReportTemplate
        {
            TemplateId = "TEST-005",
            TemplateName = "Test Validate",
            RequiredFields = new[] { "CompanyCode" },
            DetailFields = Array.Empty<string>()
        };

        // Act
        var result = await _builder.ValidateDataAsync(dataSet, template);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(err => err.Contains("CompanyCode"));
    }

    /// <summary>このテストでは SourceData 未指定時に検証例外を返す。</summary>
    [Fact]
    public async Task BuildDataAsync_Buffered_WithoutSourceData_ShouldThrowArgumentException()
    {
        var template = new ReportTemplate
        {
            TemplateId = "TEST-006",
            TemplateName = "No SourceData",
            RequiredFields = Array.Empty<string>(),
            DetailFields = new[] { "PersonalCode" }
        };

        var dataRequest = new ReportDataRequest
        {
            Mode = ReportDataMode.Buffered,
            SourceData = null
        };

        var act = () => _builder.BuildDataAsync(dataRequest, template);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*SourceData*");
    }
}







