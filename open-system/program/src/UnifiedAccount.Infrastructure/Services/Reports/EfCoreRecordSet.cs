using System.Reflection;
using UnifiedAccount.Domain.Interfaces.Reports;

namespace UnifiedAccount.Infrastructure.Services.Reports;

/// <summary>
/// EF Core LINQ Query を CnRecordSet でラップし、帳票エンジンに逐次データを供給する実装。
/// Streamed モード用。メモリ効率化（1レコードずつ処理）。
/// 
/// 使用例:
///   var query = context.SalesDetails.AsNoTracking().AsEnumerable();
///   var recordSet = new EfCoreRecordSet(query);
///   creator.DataSources["SampleDS"].Source = recordSet;
///   creator.Output(); // 帳票エンジンが Next() / GetValue() / Eof を反復呼び出し
/// </summary>
public class EfCoreRecordSet : CnRecordSet
{
    /// <summary>
    /// 列挙子を保持する。
    /// </summary>
    private readonly IEnumerator<object> _enumerator;
    /// <summary>
    /// 現在の読込対象レコードを保持する。
    /// </summary>
    private object? _currentRecord;
    /// <summary>
    /// プロパティキャッシュを保持する。
    /// </summary>
    private readonly Dictionary<string, PropertyInfo> _propertyCache;
    /// <summary>
    /// 終端かどうかを示す。
    /// </summary>
    private bool _isEof = false;
    /// <summary>
    /// 解放済みかどうかを示す。
    /// </summary>
    private bool _disposed;

    public EfCoreRecordSet(IEnumerable<object> source)
    {
        _enumerator = source?.GetEnumerator() ?? throw new ArgumentNullException(nameof(source));
        _propertyCache = new Dictionary<string, PropertyInfo>();
        _disposed = false;

        // 最初のレコードに進める
        _isEof = !_enumerator.MoveNext();
        if (!_isEof)
        {
            _currentRecord = _enumerator.Current;
        }
    }

    /// <summary>
    /// 次のレコードに進める。
    /// </summary>
    public override bool Next()
    {
        if (_disposed)
            throw new ObjectDisposedException("EfCoreRecordSet");

        if (_isEof)
            return false;

        if (!_enumerator.MoveNext())
        {
            _isEof = true;
            _currentRecord = null;
            return false;
        }

        _currentRecord = _enumerator.Current;

        // 型が変わった場合はキャッシュをクリア
        if (_currentRecord != null && (_propertyCache.Count == 0 || !_propertyCache.First().Value.DeclaringType!.Equals(_currentRecord.GetType())))
        {
            _propertyCache.Clear();
        }

        return true;
    }

    /// <summary>
    /// データ末尾に到達したか確認。
    /// </summary>
    public override bool Eof => _isEof;

    /// <summary>
    /// 現在のレコードを取得する。
    /// </summary>
    public override object? CurrentRecord => _currentRecord;

    /// <summary>
    /// 総レコード件数。ストリーミングは常に -1 を返す。
    /// </summary>
    public override int RecordCount => -1;

    /// <summary>
    /// 列名（キー）でデータを取得。
    /// キャッシュにより反射のオーバーヘッドを最小化。
    /// </summary>
    /// <param name="key">キーを指定する。</param>
    public override object? GetValue(string key)
    {
        if (_disposed)
            throw new ObjectDisposedException("EfCoreRecordSet");

        if (_currentRecord == null || _isEof)
            return null;

        if (string.IsNullOrEmpty(key))
            return null;

        try
        {
            // キャッシュから PropertyInfo を取得、なければ反射で取得して登録
            if (!_propertyCache.TryGetValue(key, out var propertyInfo))
            {
                propertyInfo = _currentRecord.GetType().GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    _propertyCache[key] = propertyInfo;
                }
            }

            if (propertyInfo == null)
                return null;

            return propertyInfo.GetValue(_currentRecord);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get value for key '{key}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// インデックス（0ベース）でデータを取得。
    /// ストリーミング時はサポートしない。
    /// </summary>
    /// <param name="index">インデックスを指定する。</param>
    public override object? GetValue(int index)
    {
        throw new NotSupportedException("インデックスアクセスはストリーミング時にサポートされていません。列名（キー）でアクセスしてください。");
    }

    /// <summary>
    /// 先頭に戻る。
    /// ストリーミング時はサポートしない。
    /// </summary>
    public override void First()
    {
        throw new NotSupportedException("First() はストリーミングモードではサポートされていません。");
    }

    /// <summary>
    /// リソース解放。
    /// </summary>
    public override void Close()
    {
        Dispose();
    }

    public override void Dispose()
    {
        if (_disposed)
            return;

        _enumerator?.Dispose();
        _propertyCache?.Clear();
        _currentRecord = null;
        _disposed = true;
    }
}




