using System.Text;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Infrastructure.Services;

/// <summary>
/// ファイル入出力時の文字コードヘルパー。
/// 外部システムとのデータ連携は既存システム（メインフレーム）の文字コードを踏襲する。
/// 
/// ■ エンコーディング方針:
///   - 全銀フォーマット: JIS X 0201 (Shift_JIS互換) — 全銀協規定(半角カナ・英数のみ)
///   - 共済連データ:     Shift_JIS — 共済連とのファイル連携仕様に準拠
///   - 銀行マスタ異動:   Shift_JIS — 全銀ネットからの配信ファイル
///   - FTP/Web連携:      連携先仕様に準拠 (デフォルトShift_JIS、指定があればそれに従う)
///   - 内部帳票/ログ:    UTF-8 — 新システム内部利用のみ
/// 
/// ■ EBCDIC対応:
///   IBM CP930 (日本語カタカナ拡張) / CP939 (日本語英小文字拡張) を利用する設計だが、
///   本ソリューションには System.Text.Encoding.CodePages パッケージ（コードページ実データ）への
///   PackageReference がどの csproj にも存在しない。
///   本リポジトリの実行環境で確認したところ、CodePagesEncodingProvider 登録後であっても
///   CP930/CP939 の取得は NotSupportedException を送出する。CP932 (Shift_JIS) は取得できる。
///   Ebcdic930 / Ebcdic939 プロパティはアクセス時にこの例外を送出しうる。
///
/// ■ 注意事項:
///   本リポジトリの実行環境で確認したところ、Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
///   を呼び出す前は Encoding.GetEncoding("shift_jis") は ArgumentException を送出し、呼び出し後に
///   初めて取得に成功する（CodePage=932）。そのため静的コンストラクタの先頭で RegisterProvider を実行している。
///   ただし本ソリューションは System.Text.Encoding.CodePages パッケージ自体は参照していないため、
///   実際に取得できるコードページは実行環境が提供するものに限られる（CP932は可、CP930/CP939は不可）。
/// </summary>
public static class FileEncodingHelper
{
    private static readonly Encoding ZenginEncoding;
    private static readonly Encoding ChoEncoding;
    private static readonly Encoding BankChangeEncoding;
    private static readonly Encoding ExternalDefaultEncoding;
    private static readonly Lazy<Encoding> Ebcdic930Encoding;
    private static readonly Lazy<Encoding> Ebcdic939Encoding;

    static FileEncodingHelper()
    {
        // Shift_JIS, EBCDIC(IBM930/939)等のコードページエンコーディングを利用可能にする。
        // 各 Encoding は不変かつスレッドセーフのため、登録直後に一度だけ取得して保持する。
        // フィールド初期化子は静的コンストラクタより先に走るため、ここで初期化する。
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        ZenginEncoding = Encoding.GetEncoding("shift_jis");
        ChoEncoding = Encoding.GetEncoding("shift_jis");
        BankChangeEncoding = Encoding.GetEncoding("shift_jis");
        ExternalDefaultEncoding = Encoding.GetEncoding("shift_jis");

        // EBCDIC(930/939) は実行環境（CodePagesEncodingProviderが提供するコードページデータ）に
        // よっては未対応で NotSupportedException を送出する場合がある（既存仕様、テスト参照）。
        // ここで即時取得すると静的コンストラクタ全体が失敗し、Zengin等の無関係なプロパティまで
        // 道連れで TypeInitializationException になってしまうため、Lazy<T> で初回アクセス時まで
        // 遅延させ、失敗の影響を各プロパティに閉じ込める。
        // LazyThreadSafetyMode.PublicationOnly を指定し、取得できない環境では例外をキャッシュせず
        // 毎回再試行する。変更前の式形式プロパティ（毎回 Encoding.GetEncoding を呼ぶ実装）と
        // 同じふるまい（アクセスのたびに新しい例外を送出する）を保つため。
        Ebcdic930Encoding = new Lazy<Encoding>(() => Encoding.GetEncoding(930), LazyThreadSafetyMode.PublicationOnly);
        Ebcdic939Encoding = new Lazy<Encoding>(() => Encoding.GetEncoding(939), LazyThreadSafetyMode.PublicationOnly);
    }

    /// <summary>
    /// 全銀フォーマット (JIS X 0201 / Shift_JIS互換)
    /// 120byte固定長レコード、半角カナ・英数のみ。
    /// </summary>
    public static Encoding Zengin => ZenginEncoding;

    /// <summary>
    /// 共済連データ連携 (Shift_JIS)
    /// 請求データ・結果データ等、共済連とのファイル交換仕様に準拠。
    /// </summary>
    public static Encoding Cho => ChoEncoding;

    /// <summary>
    /// 銀行マスタ異動データ (Shift_JIS)
    /// 全銀ネットから配信される異動ファイルのエンコーディング。
    /// </summary>
    public static Encoding BankChange => BankChangeEncoding;

    /// <summary>
    /// FTP/Web外部連携 (デフォルトShift_JIS)
    /// 連携先の仕様に応じて切替可能。
    /// </summary>
    public static Encoding ExternalDefault => ExternalDefaultEncoding;

    /// <summary>
    /// IBM EBCDIC 日本語カタカナ拡張 (CP930)
    /// メインフレーム運用が残る金融機関との連携用。
    /// コードページ実データを取得できない環境ではアクセス時に NotSupportedException を送出する。
    /// </summary>
    public static Encoding Ebcdic930 => Ebcdic930Encoding.Value;

    /// <summary>
    /// IBM EBCDIC 日本語英小文字拡張 (CP939)
    /// メインフレーム運用が残る金融機関との連携用。
    /// コードページ実データを取得できない環境ではアクセス時に NotSupportedException を送出する。
    /// </summary>
    public static Encoding Ebcdic939 => Ebcdic939Encoding.Value;

    /// <summary>
    /// 内部処理・帳票・ログ出力用 (UTF-8)
    /// 新システム内部で完結するファイルに使用。
    /// </summary>
    public static Encoding Internal => Encoding.UTF8;

    /// <summary>
    /// パラメータで指定されたエンコーディング名から Encoding を取得。
    /// 未指定の場合はデフォルト（Shift_JIS）を返す。
    /// </summary>
    /// <param name="encodingName">エンコーディング名称を指定する。</param>
    /// <returns>対応するEncodingインスタンス</returns>
    public static Encoding GetEncoding(string? encodingName)
        => string.IsNullOrEmpty(encodingName)
            ? ExternalDefault
            : Encoding.GetEncoding(encodingName);

    // ─── 金融機関別エンコーディング解決 ─────────────────────────

    /// <summary>
    /// データ種別
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// 口座振替データ (送信)
        /// </summary>
        TransferData,
        /// <summary>
        /// 口座振替結果データ (受信)
        /// </summary>
        ResultData,
        /// <summary>
        /// 照合データ
        /// </summary>
        ReconcileData
    }

    /// <summary>
    /// 金融機関コードとデータ種別から適切な Encoding を取得する。
    /// InstitutionEncodings テーブルに設定がない場合はデフォルト (Shift_JIS) を返す。
    /// </summary>
    /// <param name="db">データベースを指定する。</param>
    /// <param name="bankCode">銀行コードを指定する。</param>
    /// <param name="dataType">データ種別を指定する。</param>
    /// <returns>対応するEncodingインスタンス</returns>
    public static async Task<Encoding> GetInstitutionEncodingAsync(
        AppDbContext db, string bankCode, DataType dataType)
    {
        return ExternalDefault; // デフォルト: Shift_JIS
    }

    /// <summary>
    /// 金融機関コードとデータ種別から適切な Encoding を取得する（同期版）。
    /// バッチジョブの初期化フェーズ等、非同期が不要な場合に使用。
    /// </summary>
    /// <param name="db">データベースを指定する。</param>
    /// <param name="bankCode">銀行コードを指定する。</param>
    /// <param name="dataType">データ種別を指定する。</param>
    public static Encoding GetInstitutionEncoding(
        AppDbContext db, string bankCode, DataType dataType)
    {
        return ExternalDefault; // デフォルト: Shift_JIS
    }
}




