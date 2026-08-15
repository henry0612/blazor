using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using UnifiedAccount.Domain.Constants;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Application.Services.Reports;
using UnifiedAccount.Infrastructure.Services.Reports;

namespace UnifiedAccount.Application.Tests.Services.Reports;

/// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
/// ReportOutputService Unit テスト (Moq)
/// オーケストレーション層（ステップ1～4）の動作確認
/// 依存サービスはすべてモック化
/// </summary>
public class ReportOutputServiceTests
{
    private readonly Mock<IReportTemplateResolver> _mockTemplateResolver;
    private readonly Mock<IReportDataBuilder> _mockDataBuilder;
    private readonly Mock<IReportGenerator> _mockGenerator;
    private readonly Mock<IReportOutputDestination> _mockOutputDestination;
    private readonly Mock<IReportExecutionResultService> _mockExecutionService;
    private readonly Mock<IReportFilePathProvider> _mockFilePathProvider;
    private readonly Mock<ILogger<ReportOutputService>> _mockLogger;
    private readonly ReportOutputService _service;

    public ReportOutputServiceTests()
    {
        _mockTemplateResolver = new Mock<IReportTemplateResolver>();
        _mockDataBuilder = new Mock<IReportDataBuilder>();
        _mockGenerator = new Mock<IReportGenerator>();
        _mockOutputDestination = new Mock<IReportOutputDestination>();
        _mockExecutionService = new Mock<IReportExecutionResultService>();
        _mockFilePathProvider = new Mock<IReportFilePathProvider>();
        _mockLogger = new Mock<ILogger<ReportOutputService>>();

        _mockFilePathProvider
            .Setup(x => x.GetOutputFilePath(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("/output/default.pdf");

        _service = new ReportOutputService(
            _mockLogger.Object,
            _mockTemplateResolver.Object,
            _mockDataBuilder.Object,
            _mockGenerator.Object,
            _mockOutputDestination.Object,
            _mockExecutionService.Object,
            _mockFilePathProvider.Object);
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public async Task OutputAsync_WhenOutputFormatAndDestinationAreMissing_ShouldUseDefaults()
    {
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        var templateId = "TEST-001";

        var template = new ReportTemplate
        {
            TemplateId = templateId,
            TemplateName = "Test Template",
            IsActive = true,
            RequiredFields = Array.Empty<string>(),
            DetailFields = Array.Empty<string>()
        };

        var dataSet = new ReportDataSet { TemplateId = templateId };
        var generatedReport = new GeneratedReport
        {
            ExecutionId = executionId,
            OutputFilePath = "/tmp/report.pdf",
            FileSizeBytes = 1024,
            IsPartialOutput = false
        };

        var outputRequest = new ReportOutputRequest
        {
            ExecutionId = executionId,
            TemplateId = templateId,
            DataRequest = new ReportDataRequest
            {
                Mode = ReportDataMode.Buffered,
                SourceData = new Dictionary<string, object>()
            },
            ProcessDate = new DateOnly(2026, 6, 30)
        };

        _mockTemplateResolver
            .Setup(x => x.ResolveAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _mockDataBuilder
            .Setup(x => x.BuildDataAsync(
                It.IsAny<ReportDataRequest>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSet);

        _mockDataBuilder
            .Setup(x => x.ValidateDataAsync(
                It.IsAny<ReportDataSet>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { IsValid = true });

        _mockGenerator
            .Setup(x => x.GenerateReportAsync(
                It.IsAny<GenerateReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generatedReport);

        _mockOutputDestination
            .Setup(x => x.ValidateOutputPathAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOutputDestination
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockExecutionService
            .Setup(x => x.StartExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportExecution { ExecutionId = executionId, TemplateId = templateId });

        _mockExecutionService
            .Setup(x => x.CompleteExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.OutputAsync(outputRequest);

        result.IsSuccessful.Should().BeTrue();
        _mockFilePathProvider.Verify(x => x.GetOutputFilePath(executionId, templateId), Times.Once);
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task OutputAsync_WhenSplitKeyIsProvided_ShouldGenerateOneFilePerGroup()
    {
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        var templateId = "TEST-001";

        var template = new ReportTemplate
        {
            TemplateId = templateId,
            TemplateName = "Test Template",
            IsActive = true,
            RequiredFields = Array.Empty<string>(),
            DetailFields = Array.Empty<string>()
        };

        var dataSet = new ReportDataSet
        {
            TemplateId = templateId,
            DetailRows = new List<Dictionary<string, object>>
            {
                new() { { "Region", "A" } },
                new() { { "Region", "B" } }
            }
        };

        _mockTemplateResolver
            .Setup(x => x.ResolveAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _mockDataBuilder
            .Setup(x => x.BuildDataAsync(
                It.IsAny<ReportDataRequest>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSet);

        _mockDataBuilder
            .Setup(x => x.ValidateDataAsync(
                It.IsAny<ReportDataSet>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { IsValid = true });

        _mockGenerator
            .SetupSequence(x => x.GenerateReportAsync(
                It.IsAny<GenerateReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneratedReport { ExecutionId = executionId, OutputFilePath = "/tmp/report-A.pdf", FileSizeBytes = 1024 })
            .ReturnsAsync(new GeneratedReport { ExecutionId = executionId, OutputFilePath = "/tmp/report-B.pdf", FileSizeBytes = 1024 });

        _mockOutputDestination
            .Setup(x => x.ValidateOutputPathAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOutputDestination
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockExecutionService
            .Setup(x => x.StartExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportExecution { ExecutionId = executionId, TemplateId = templateId });

        _mockExecutionService
            .Setup(x => x.CompleteExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.OutputAsync(new ReportOutputRequest
        {
            ExecutionId = executionId,
            TemplateId = templateId,
            SplitKey = "Region",
            OutputFormat = "PDF",
            DataRequest = new ReportDataRequest
            {
                Mode = ReportDataMode.Buffered,
                SourceData = dataSet
            },
            OutputDestination = new ReportOutputDestinationRequest
            {
                OutputPath = "/output/report.pdf",
                SaveToFile = true
            },
            ProcessDate = new DateOnly(2026, 6, 30)
        });

        result.IsSuccessful.Should().BeTrue();
        _mockGenerator.Verify(x => x.GenerateReportAsync(It.IsAny<GenerateReportRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task OutputAsync_WhenSplitKeyIsProvidedAndDataSourceIsRecordSet_ShouldGenerateOneFilePerGroup()
    {
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        var templateId = "TEST-001";

        var template = new ReportTemplate
        {
            TemplateId = templateId,
            TemplateName = "Test Template",
            IsActive = true,
            RequiredFields = Array.Empty<string>(),
            DetailFields = Array.Empty<string>()
        };

        var recordSet = new EfCoreRecordSet(new object[]
        {
            new TestRecord { Region = "A" },
            new TestRecord { Region = "B" }
        });

        _mockTemplateResolver
            .Setup(x => x.ResolveAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _mockDataBuilder
            .Setup(x => x.BuildDataAsync(
                It.IsAny<ReportDataRequest>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recordSet);

        _mockGenerator
            .SetupSequence(x => x.GenerateReportAsync(
                It.IsAny<GenerateReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneratedReport { ExecutionId = executionId, OutputFilePath = "/tmp/report-A.pdf", FileSizeBytes = 1024 })
            .ReturnsAsync(new GeneratedReport { ExecutionId = executionId, OutputFilePath = "/tmp/report-B.pdf", FileSizeBytes = 1024 });

        _mockOutputDestination
            .Setup(x => x.ValidateOutputPathAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOutputDestination
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockExecutionService
            .Setup(x => x.StartExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportExecution { ExecutionId = executionId, TemplateId = templateId });

        _mockExecutionService
            .Setup(x => x.CompleteExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.OutputAsync(new ReportOutputRequest
        {
            ExecutionId = executionId,
            TemplateId = templateId,
            SplitKey = "Region",
            OutputFormat = "PDF",
            DataRequest = new ReportDataRequest
            {
                Mode = ReportDataMode.Streamed,
                SourceData = recordSet
            },
            OutputDestination = new ReportOutputDestinationRequest
            {
                OutputPath = "/output/report.pdf",
                SaveToFile = true
            },
            ProcessDate = new DateOnly(2026, 6, 30)
        });

        result.IsSuccessful.Should().BeTrue();
        _mockGenerator.Verify(x => x.GenerateReportAsync(It.IsAny<GenerateReportRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    /// <summary>このテストでは 出力先を指定したとき、成功する。</summary>
    [Fact]
    public async Task OutputAsync_Buffered_SuccessfulFlow_ShouldCompleteExecution()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        var templateId = "TEST-001";

        var template = new ReportTemplate
        {
            TemplateId = templateId,
            TemplateName = "Test Template",
            IsActive = true,
            RequiredFields = new[] { "CompanyCode" },
            DetailFields = new[] { "PersonalCode" }
        };

        var dataSet = new ReportDataSet
        {
            TemplateId = templateId,
            HeaderData = new Dictionary<string, object> { { "CompanyCode", "TEST01" } },
            DetailRows = new List<Dictionary<string, object>>
            {
                new() { { "PersonalCode", "PERS001" } }
            },
            SummaryData = new Dictionary<string, object> { { "TotalRows", 1 } }
        };

        var generatedReport = new GeneratedReport
        {
            ExecutionId = executionId,
            OutputFilePath = "/tmp/report.pdf",
            FileSizeBytes = 1024,
            IsPartialOutput = false
        };

        var execution = new ReportExecution
        {
            ExecutionId = executionId,
            TemplateId = templateId,
            Status = ReportExecutionStatuses.Running
        };

        var outputRequest = new ReportOutputRequest
        {
            ExecutionId = executionId,
            TemplateId = templateId,
            OutputFormat = "PDF",
            DataRequest = new ReportDataRequest
            {
                Mode = ReportDataMode.Buffered,
                SourceData = new Dictionary<string, object> { { "CompanyCode", "TEST01" } }
            },
            OutputDestination = new ReportOutputDestinationRequest
            {
                OutputPath = "/output/report.pdf",
                SaveToFile = true
            },
            ProcessDate = new DateOnly(2026, 6, 30)
        };

        // モック設定
        _mockTemplateResolver
            .Setup(x => x.ResolveAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _mockDataBuilder
            .Setup(x => x.BuildDataAsync(
                It.IsAny<ReportDataRequest>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSet);

        _mockDataBuilder
            .Setup(x => x.ValidateDataAsync(
                It.IsAny<ReportDataSet>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { IsValid = true });

        _mockGenerator
            .Setup(x => x.GenerateReportAsync(
                It.IsAny<GenerateReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generatedReport);

        _mockOutputDestination
            .Setup(x => x.ValidateOutputPathAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOutputDestination
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockExecutionService
            .Setup(x => x.StartExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(execution);

        _mockExecutionService
            .Setup(x => x.CompleteExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.OutputAsync(outputRequest);

        // Assert
        result.Should().NotBeNull();
        result!.IsSuccessful.Should().BeTrue();
        result.ExecutionId.Should().Be(executionId);
        result.OutputFilePath.Should().Be("/tmp/report.pdf");

        // 各サービスメソッドが呼ばれたことを確認
        _mockTemplateResolver.Verify(
            x => x.ResolveAsync(templateId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockGenerator.Verify(
            x => x.GenerateReportAsync(
                It.IsAny<GenerateReportRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockOutputDestination.Verify(
            x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockExecutionService.Verify(
            x => x.CompleteExecutionAsync(
                executionId,
                "/output/report.pdf",
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task OutputAsync_TemplateNotFound_ShouldFail()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        var templateId = "NONEXISTENT";

        var outputRequest = new ReportOutputRequest
        {
            ExecutionId = executionId,
            TemplateId = templateId,
            ProcessDate = new DateOnly(2026, 6, 30),
            OutputFormat = "PDF",
            DataRequest = new ReportDataRequest { Mode = ReportDataMode.Buffered },
            OutputDestination = new ReportOutputDestinationRequest
            {
                OutputPath = "/output/report.pdf",
                SaveToFile = true
            }
        };

        _mockTemplateResolver
            .Setup(x => x.ResolveAsync(templateId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("Template not found"));

        // Act
        var result = await _service.OutputAsync(outputRequest);

        // Assert
        result.Should().NotBeNull();
        result!.IsSuccessful.Should().BeFalse();
        result.FailureKind.Should().Be(ReportOutputFailureKind.TemplateNotFound);
        result.ValidationErrors.Should().Contain(e => e.Contains("Template not found"));
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task OutputAsync_DataValidationFails_ShouldFail()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        var templateId = "TEST-001";

        var template = new ReportTemplate
        {
            TemplateId = templateId,
            TemplateName = "Test",
            IsActive = true,
            RequiredFields = new[] { "CompanyCode" },
            DetailFields = new[] { "PersonalCode" }
        };

        var dataSet = new ReportDataSet
        {
            TemplateId = templateId,
            HeaderData = new Dictionary<string, object>(),
            DetailRows = new List<Dictionary<string, object>>(),
            SummaryData = new Dictionary<string, object>()
        };

        var outputRequest = new ReportOutputRequest
        {
            ExecutionId = executionId,
            TemplateId = templateId,
            ProcessDate = new DateOnly(2026, 6, 30),
            OutputFormat = "PDF",
            DataRequest = new ReportDataRequest { Mode = ReportDataMode.Buffered },
            OutputDestination = new ReportOutputDestinationRequest
            {
                OutputPath = "/output/report.pdf",
                SaveToFile = true
            }
        };

        var execution = new ReportExecution { ExecutionId = executionId, TemplateId = templateId };
        var failedReport = new GeneratedReport
        {
            ExecutionId = executionId,
            OutputFilePath = null,
            FileSizeBytes = 0,
            IsPartialOutput = false
        };

        _mockTemplateResolver
            .Setup(x => x.ResolveAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _mockDataBuilder
            .Setup(x => x.BuildDataAsync(
                It.IsAny<ReportDataRequest>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSet);

        _mockDataBuilder
            .Setup(x => x.ValidateDataAsync(
                It.IsAny<ReportDataSet>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { "CompanyCode is required" }
            });

        _mockExecutionService
            .Setup(x => x.StartExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(execution);

        _mockGenerator
            .Setup(x => x.GenerateReportAsync(
                It.IsAny<GenerateReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedReport);

        _mockExecutionService
            .Setup(x => x.FailExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<Exception>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.OutputAsync(outputRequest);

        // Assert
        result.Should().NotBeNull();
        result!.IsSuccessful.Should().BeFalse();
        result.FailureKind.Should().Be(ReportOutputFailureKind.Validation);
        result.ValidationErrors.Should().Contain("帳票生成に失敗しました。");
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task OutputAsync_GeneratorFails_ShouldFailExecution()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        var templateId = "TEST-001";

        var template = new ReportTemplate
        {
            TemplateId = templateId,
            TemplateName = "Test",
            IsActive = true,
            RequiredFields = Array.Empty<string>(),
            DetailFields = Array.Empty<string>()
        };

        var dataSet = new ReportDataSet { TemplateId = templateId };

        var outputRequest = new ReportOutputRequest
        {
            ExecutionId = executionId,
            TemplateId = templateId,
            ProcessDate = new DateOnly(2026, 6, 30),
            OutputFormat = "PDF",
            DataRequest = new ReportDataRequest { Mode = ReportDataMode.Buffered },
            OutputDestination = new ReportOutputDestinationRequest
            {
                OutputPath = "/output/report.pdf",
                SaveToFile = true
            }
        };

        var execution = new ReportExecution { ExecutionId = executionId, TemplateId = templateId };

        _mockTemplateResolver
            .Setup(x => x.ResolveAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _mockDataBuilder
            .Setup(x => x.BuildDataAsync(
                It.IsAny<ReportDataRequest>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSet);

        _mockDataBuilder
            .Setup(x => x.ValidateDataAsync(
                It.IsAny<ReportDataSet>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { IsValid = true });

        _mockExecutionService
            .Setup(x => x.StartExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(execution);

        _mockGenerator
            .Setup(x => x.GenerateReportAsync(
                It.IsAny<GenerateReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("CoReports DLL not found"));

        _mockExecutionService
            .Setup(x => x.FailExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<Exception>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.OutputAsync(outputRequest);

        // Assert
        result.Should().NotBeNull();
        result!.IsSuccessful.Should().BeFalse();
        result.FailureKind.Should().Be(ReportOutputFailureKind.GenerationFailed);
        result.ValidationErrors.Should().Contain(e => e.Contains("CoReports"));

        _mockExecutionService.Verify(
            x => x.FailExecutionAsync(
                executionId,
                It.IsAny<Exception>(),
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>このテストでは 出力先を指定したとき、出力先やパスが生成される。</summary>
    [Fact]
    public async Task OutputAsync_OutputDestinationFails_ShouldFail()
    {
        // Arrange
        var executionId = $"EXEC-{Guid.NewGuid():N}";
        var templateId = "TEST-001";

        var template = new ReportTemplate
        {
            TemplateId = templateId,
            TemplateName = "Test",
            IsActive = true,
            RequiredFields = Array.Empty<string>(),
            DetailFields = Array.Empty<string>()
        };

        var dataSet = new ReportDataSet { TemplateId = templateId };
        var generatedReport = new GeneratedReport
        {
            ExecutionId = executionId,
            OutputFilePath = "/tmp/report.pdf",
            FileSizeBytes = 1024
        };

        var outputRequest = new ReportOutputRequest
        {
            ExecutionId = executionId,
            TemplateId = templateId,
            ProcessDate = new DateOnly(2026, 6, 30),
            OutputFormat = "PDF",
            DataRequest = new ReportDataRequest { Mode = ReportDataMode.Buffered },
            OutputDestination = new ReportOutputDestinationRequest
            {
                OutputPath = "/output/report.pdf",
                SaveToFile = true
            }
        };

        var execution = new ReportExecution { ExecutionId = executionId, TemplateId = templateId };

        _mockTemplateResolver
            .Setup(x => x.ResolveAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _mockDataBuilder
            .Setup(x => x.BuildDataAsync(
                It.IsAny<ReportDataRequest>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSet);

        _mockDataBuilder
            .Setup(x => x.ValidateDataAsync(
                It.IsAny<ReportDataSet>(),
                It.IsAny<ReportTemplate>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { IsValid = true });

        _mockGenerator
            .Setup(x => x.GenerateReportAsync(
                It.IsAny<GenerateReportRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(generatedReport);

        _mockExecutionService
            .Setup(x => x.StartExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(execution);

        _mockOutputDestination
            .Setup(x => x.ValidateOutputPathAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Permission denied"));

        _mockExecutionService
            .Setup(x => x.FailExecutionAsync(
                It.IsAny<string>(),
                It.IsAny<Exception>(),
                null,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.OutputAsync(outputRequest);

        // Assert
        result.Should().NotBeNull();
        result!.IsSuccessful.Should().BeFalse();
        result.FailureKind.Should().Be(ReportOutputFailureKind.DestinationFailed);
        result.ValidationErrors.Should().Contain(e => e.Contains("出力先ルーティング"));
    }

    [Fact]
    public async Task OutputAsync_WhenDataBuilderThrows_ShouldReturnDataBuildFailed()
    {
        var request = CreateFailureTestRequest();
        var template = CreateFailureTestTemplate(request.TemplateId);
        _mockExecutionService
            .Setup(x => x.StartExecutionAsync(
                request.ExecutionId,
                request.JobExecutionId,
                request.TemplateId,
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReportExecution { ExecutionId = request.ExecutionId, TemplateId = request.TemplateId });
        _mockTemplateResolver
            .Setup(x => x.ResolveAsync(request.TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _mockDataBuilder
            .Setup(x => x.BuildDataAsync(
                It.IsAny<ReportDataRequest>(),
                template,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("data builder failed"));

        var result = await _service.OutputAsync(request);

        result.IsSuccessful.Should().BeFalse();
        result.FailureKind.Should().Be(ReportOutputFailureKind.DataBuildFailed);
    }

    [Fact]
    public async Task OutputAsync_WhenUnexpectedDependencyThrows_ShouldReturnUnexpected()
    {
        var request = CreateFailureTestRequest();
        _mockExecutionService
            .Setup(x => x.StartExecutionAsync(
                request.ExecutionId,
                request.JobExecutionId,
                request.TemplateId,
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("unexpected dependency failure"));

        var result = await _service.OutputAsync(request);

        result.IsSuccessful.Should().BeFalse();
        result.FailureKind.Should().Be(ReportOutputFailureKind.Unexpected);
    }

    [Fact]
    public async Task OutputAsync_WhenExecutionIdIsEmpty_ShouldReturnValidation()
    {
        var request = CreateFailureTestRequest();
        request.ExecutionId = string.Empty;

        var result = await _service.OutputAsync(request);

        result.IsSuccessful.Should().BeFalse();
        result.FailureKind.Should().Be(ReportOutputFailureKind.Validation);
    }

    private static ReportOutputRequest CreateFailureTestRequest()
    {
        return new ReportOutputRequest
        {
            ExecutionId = $"EXEC-{Guid.NewGuid():N}",
            TemplateId = "TEST-FAILURE",
            ProcessDate = new DateOnly(2026, 6, 30),
            DataRequest = new ReportDataRequest { Mode = ReportDataMode.Buffered },
            OutputDestination = new ReportOutputDestinationRequest
            {
                OutputPath = "/output/report.pdf",
                SaveToFile = true
            }
        };
    }

    private static ReportTemplate CreateFailureTestTemplate(string templateId)
    {
        return new ReportTemplate
        {
            TemplateId = templateId,
            TemplateName = "Failure Test",
            IsActive = true,
            RequiredFields = Array.Empty<string>(),
            DetailFields = Array.Empty<string>()
        };
    }

    private sealed class TestRecord
    {
        public string Region { get; set; } = string.Empty;
    }
}










