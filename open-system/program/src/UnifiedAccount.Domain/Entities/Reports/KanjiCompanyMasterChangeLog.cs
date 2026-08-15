using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Entities.Reports;

/// <summary>
/// 漢字会社マスター異動ログ (F-REP-002 帳票出力元データ)
/// </summary>
[Table("TR_KanjiCompanyMasterChangeLogs")]
[Index(nameof(JobExecutionId), nameof(CompanyCode), nameof(SectionNo), nameof(SequenceNo), IsUnique = true,
    Name = "UQ_KanjiCompanyMasterChangeLogs_Key")]
[Index(nameof(JobExecutionId), nameof(CompanyCode), nameof(SectionNo),
    Name = "IX_KanjiCompanyMasterChangeLogs_BatchRun")]
public class KanjiCompanyMasterChangeLog : BaseEntity
{
    /// <summary>
    /// バッチ実行相関ID (JOB-{jobId}-{yyyyMMdd}-{6桁})
    /// </summary>
    [Required, MaxLength(50)]
    public string JobExecutionId { get; set; } = string.Empty;

    /// <summary>
    /// 処理日
    /// </summary>
    public DateOnly ProcessingDate { get; set; }

    /// <summary>
    /// 会社コード
    /// </summary>
    [Required, MaxLength(6)]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// 区番 (CBAN) 1〜4
    /// </summary>
    [Required, MaxLength(1)]
    public string SectionNo { get; set; } = string.Empty;

    /// <summary>
    /// メッセージ区分
    /// </summary>
    [MaxLength(20)]
    public string? MessageType { get; set; }

    /// <summary>
    /// 異動区分 '1'=削除 / '2'=新規 / '3'=修正
    /// </summary>
    [Required, MaxLength(1)]
    public string ChangeAction { get; set; } = string.Empty;

    /// <summary>
    /// 漢字会社名
    /// </summary>
    [MaxLength(60)]
    public string? CompanyNameKanji { get; set; }

    /// <summary>
    /// 都道府県名
    /// </summary>
    [MaxLength(20)]
    public string? PrefectureKanji { get; set; }

    /// <summary>
    /// 市郡名
    /// </summary>
    [MaxLength(20)]
    public string? CityKanji { get; set; }

    /// <summary>
    /// 町村名1
    /// </summary>
    [MaxLength(40)]
    public string? TownKanji1 { get; set; }

    /// <summary>
    /// 町村名2
    /// </summary>
    [MaxLength(40)]
    public string? TownKanji2 { get; set; }

    /// <summary>
    /// 部署名
    /// </summary>
    [MaxLength(40)]
    public string? DepartmentKanji { get; set; }

    /// <summary>
    /// 担当者名
    /// </summary>
    [MaxLength(40)]
    public string? PersonInChargeKanji { get; set; }

    /// <summary>
    /// 処理順連番
    /// </summary>
    public int SequenceNo { get; set; }

}
