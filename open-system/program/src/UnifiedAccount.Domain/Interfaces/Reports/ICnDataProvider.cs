namespace UnifiedAccount.Domain.Interfaces.Reports;

/// <summary>
/// CoReports 帳票クリエータ V5 の CnRecordSet 抽象クラス継承パターン。
/// Streamed モードで帳票エンジンが呼び出すレコードセット契約。
/// 
/// 使用例:
///   var myRecordSet = new EfCoreRecordSet(query);
///   creator.DataSources["SampleDS"].Source = myRecordSet;
///   creator.Output(); // 帳票エンジンが Next() / GetValue() / Eof を反復呼び出し
/// </summary>
public abstract class CnRecordSet : IDisposable
{
    /// <summary>
    /// 次のレコードに進める。
    /// true = 次レコード存在、false = EOF に到達
    /// </summary>
    public abstract bool Next();

    /// <summary>
    /// データ末尾に到達したか確認。true = EOF
    /// </summary>
    public abstract bool Eof { get; }

    /// <summary>
    /// 総レコード件数。ストリーミング時は -1 を返す。
    /// </summary>
    public abstract int RecordCount { get; }

    /// <summary>
    /// 列名（キー）でデータを取得。
    /// ストリーミング時は switch ステートメントで実装。
    /// </summary>
    /// <param name="key">キーを指定する。</param>
    public abstract object? GetValue(string key);

    /// <summary>
    /// 現在のレコードを取得する。
    /// ストリーミング実装では現在読み出し中のレコードを返す。
    /// </summary>
    public virtual object? CurrentRecord => null;

    /// <summary>
    /// インデックス（0ベース）でデータを取得。
    /// ストリーミング時は通常サポートしない（null 返却でも可）。
    /// </summary>
    /// <param name="index">インデックスを指定する。</param>
    public abstract object? GetValue(int index);

    /// <summary>
    /// 先頭に戻る。
    /// ストリーミング時は NotSupportedException をスロー。
    /// </summary>
    public abstract void First();

    /// <summary>
    /// リソース解放。エンジンが完了時に呼び出し。
    /// </summary>
    public abstract void Close();

    /// <summary>
    /// 保持しているリソースを解放する。
    /// </summary>
    public abstract void Dispose();
}





