namespace UnifiedAccount.Application.Services.Reports;

/// <summary>
/// 帳票出力ファイルパス生成インターフェース。
/// 帳票出力先の基底フォルダ配下に実行単位のディレクトリを作成し、
/// 出力ファイルの完全パスを返す。
/// </summary>
public interface IReportFilePathProvider
{
    /// <summary>
    /// 帳票出力ファイルパスを生成する。
    /// 呼び出し時に次のディレクトリを作成する。
    ///   {OutputFolder}/{JobExecutionId}/{JobId}_{yyyyMMddHHmmss}/
    /// 返却するファイルパスの形式:
    ///   {OutputFolder}/{JobExecutionId}/{JobId}_{yyyyMMddHHmmss}/{JobExecutionId}_{JobId}_{yyyyMMddHHmmss}.PDF
    /// </summary>
    /// <returns>帳票出力ファイルの完全パス</returns>
    string GetOutputFilePath(string jobExecutionId, string jobId);
}

