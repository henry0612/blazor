using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Integration.Tests.Helpers;

namespace UnifiedAccount.Integration.Tests.Schema;

public class ReportTableSchemaAlignmentTests
{
    [Fact]
    public void ReportEntityMappings_ShouldUseContentOrientedPhysicalNames()
    {
        using var context = TestDbContextFactory.Create(nameof(ReportEntityMappings_ShouldUseContentOrientedPhysicalNames));

        var reportTableNames = GetCurrentReportTableNames(context);

        reportTableNames.Should().Contain("TR_ReportExecutions");
        reportTableNames.Should().Contain("TR_CompanyMasterChangeLogs");
        reportTableNames.Should().Contain("TR_FeeAggregations");
        reportTableNames.Should().Contain("TR_CompanyMonthlySummaries");
        reportTableNames.Should().Contain("TR_ContractPrintRequests");

        reportTableNames.Should().NotContain("TR_KOZ027PrintRecords");
        reportTableNames.Should().NotContain("TR_KOZ223PrintRecords");
        reportTableNames.Should().NotContain("TR_KOZ245PrintRecords");
        reportTableNames.Should().NotContain("TR_KOZ290PrintRecords");
        reportTableNames.Should().NotContain("TR_FEPKPR02PrintRecords");
    }

    [Fact]
    public void ReportEntityMappings_ShouldAvoidLegacyIdDerivedNames()
    {
        using var context = TestDbContextFactory.Create(nameof(ReportEntityMappings_ShouldAvoidLegacyIdDerivedNames));
        var artifactMappings = ParseArtifactMappings();
        var legacyNames = artifactMappings
            .Where(mapping => mapping.NewName != "変更なし")
            .Select(mapping => mapping.OldName)
            .ToHashSet(StringComparer.Ordinal);

        var currentReportTableNames = GetCurrentReportTableNames(context);

        currentReportTableNames.Should().NotIntersectWith(legacyNames);
    }

    [Fact]
    public void CompanyMasterChangeLogReportColumns_ShouldUseCanonicalNames()
    {
        var setupSql = File.ReadAllText(GetRepositoryPath("open-system\\program\\db\\migrations\\V001__InitialSchema.sql"));
        var tableMatch = System.Text.RegularExpressions.Regex.Match(
            setupSql,
            @"CREATE TABLE \[dbo\]\.\[TR_CompanyMasterChangeLogs\] \((?<definition>.*?)\r?\n\);",
            System.Text.RegularExpressions.RegexOptions.Singleline);

        tableMatch.Success.Should().BeTrue();
        var tableDefinition = tableMatch.Groups["definition"].Value;

        foreach (var column in new[]
        {
            "PhoneNumber",
            "Prefecture",
            "City",
            "AdminFee",
            "Transfer1UnitPrice",
            "Transfer2UnitPrice",
            "Transfer1BankCode",
            "Transfer1BranchCode",
            "Transfer1AccountType",
            "Transfer1AccountNo",
            "ConsignorCode",
            "Transfer2BankCode",
            "Transfer2BranchCode",
            "Transfer2AccountType",
            "Transfer2AccountNo"
        })
        {
            tableDefinition.Should().Contain($"[{column}]");
        }

        foreach (var legacyColumn in new[]
        {
            "Tel",
            "PrefectureName",
            "CityName",
            "OfficeFee",
            "TransferUnitPrice1",
            "TransferUnitPrice2",
            "BankCode1",
            "BranchCode1",
            "AccountType1",
            "AccountNo1",
            "ClientCode",
            "BankCode2",
            "BranchCode2",
            "AccountType2",
            "AccountNo2"
        })
        {
            tableDefinition.Should().NotContain($"[{legacyColumn}]");
        }
    }

    [Fact]
    public void ReportTableMappings_ShouldAlignWithSchemaAndCorrespondenceArtifact()
    {
        using var context = TestDbContextFactory.Create(nameof(ReportTableMappings_ShouldAlignWithSchemaAndCorrespondenceArtifact));
        var setupSql = File.ReadAllText(GetRepositoryPath("open-system\\program\\db\\migrations\\V001__InitialSchema.sql"));
        var artifactMappings = ParseArtifactMappings();

        var currentReportTableNames = GetCurrentReportTableNames(context)
            .Where(name => name.StartsWith("TR_", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);

        var expectedReportTableNames = artifactMappings
            .Select(mapping => mapping.NewName == "変更なし" ? mapping.OldName : mapping.NewName)
            .Where(name => name.StartsWith("TR_", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);
        var physicalMappingNames = artifactMappings
            .Select(mapping => mapping.NewName == "変更なし" ? mapping.OldName : mapping.NewName)
            .Where(name => name.StartsWith("TR_", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);

        currentReportTableNames.Should().BeSubsetOf(expectedReportTableNames);

        var setupReportTableNames = System.Text.RegularExpressions.Regex.Matches(
                setupSql,
                @"CREATE TABLE \[dbo\]\.\[(?<name>TR_[A-Za-z0-9]+)\]")
            .Select(match => match.Groups["name"].Value)
            .ToHashSet(StringComparer.Ordinal);

        // TR_GeneralAffairsDepositListRecords は 209_テーブル定義書 にのみ存在し実装未使用の帳票テーブルであり、
        // 帳票テーブル対応表の根拠文書（168-帳票一覧表、171-帳票管理テーブル設定表）のいずれにも記載が無いため、
        // 対応表の対象外として除外する。
        var setupReportTableNamesInScope = setupReportTableNames
            .Where(name => name != "TR_GeneralAffairsDepositListRecords")
            .ToHashSet(StringComparer.Ordinal);

        setupReportTableNamesInScope.Should().BeSubsetOf(expectedReportTableNames);
        artifactMappings.Should().HaveCount(78);
        artifactMappings.Count(mapping => mapping.NewName != "変更なし").Should().Be(56);
        setupReportTableNames.Should().HaveCount(72);
        expectedReportTableNames.Except(setupReportTableNames).Should().HaveCount(7);
        physicalMappingNames.Should().HaveCount(78);

        foreach (var reportTableName in setupReportTableNames)
        {
            setupSql.Should().Contain($"CREATE TABLE [dbo].[{reportTableName}] (");
        }
    }

    [Fact]
    public void LegacyReportTableNames_ShouldNotAppearInGeneratedSchema()
    {
        var setupSql = File.ReadAllText(GetRepositoryPath("open-system\\program\\db\\migrations\\V001__InitialSchema.sql"));
        var legacyNames = ParseArtifactMappings()
            .Where(mapping => mapping.NewName != "変更なし")
            .Select(mapping => mapping.OldName)
            .ToArray();

        // V001__InitialSchema.sql は 209_テーブル定義書 から一括生成する初期スキーマであり、
        // SetupDatabaseSchema.sql と V012__RenameReportTablesToContentNames.sql が担っていた
        // 「旧物理名から新物理名への改名を後から適用する」経路そのものが存在しない。
        // 検証対象は「旧物理名が生成物に現れないこと」に絞る。
        foreach (var legacyName in legacyNames)
        {
            setupSql.Should().NotContain(legacyName);
        }
    }

    private static IReadOnlyList<string> GetCurrentReportTableNames(AppDbContext context)
    {
        return context.Model.GetEntityTypes()
            .Where(entityType => entityType.ClrType?.Namespace == typeof(ReportExecution).Namespace)
            .Select(entityType => entityType.GetTableName())
            .Where(tableName => !string.IsNullOrWhiteSpace(tableName))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(tableName => tableName, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<ReportNameMapping> ParseArtifactMappings()
    {
        var artifactPath = GetRepositoryPath("open-system\\docs\\12C-基本設計\\帳票テーブル対応表.md");
        var lines = File.ReadAllLines(artifactPath);

        return lines
            .Select(line => System.Text.RegularExpressions.Regex.Match(
                line,
                @"^\|\s*`(?<old>[^`]+)`\s*\|\s*(?<new>[^|]+)\s*\|"))
            .Where(match => match.Success)
            .Select(match => new ReportNameMapping(
                match.Groups["old"].Value.Trim().Trim('`'),
                match.Groups["new"].Value.Trim().Trim('`')))
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.OldName) && !string.IsNullOrWhiteSpace(mapping.NewName))
            .Where(mapping => mapping.OldName.StartsWith("TR_", StringComparison.Ordinal))
            .ToArray();
    }

    private static string GetRepositoryPath(string relativePath)
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null && !File.Exists(Path.Combine(dir, "open-system", "program", "UnifiedAccount.slnx")))
        {
            dir = Directory.GetParent(dir)?.FullName;
        }

        dir.Should().NotBeNull();
        return Path.Combine(dir!, relativePath);
    }

    private sealed record ReportNameMapping(string OldName, string NewName);
}
