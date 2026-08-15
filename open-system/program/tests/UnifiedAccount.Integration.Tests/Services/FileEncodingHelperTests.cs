using System.Text;
using FluentAssertions;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Infrastructure.Services;
using UnifiedAccount.Integration.Tests.Helpers;

namespace UnifiedAccount.Integration.Tests.Services;

public class FileEncodingHelperTests
{
    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Zengin_ShouldBeShiftJis()
    {
        FileEncodingHelper.Zengin.WebName.Should().Be("shift_jis");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Cho_ShouldBeShiftJis()
    {
        FileEncodingHelper.Cho.WebName.Should().Be("shift_jis");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void BankChange_ShouldBeShiftJis()
    {
        FileEncodingHelper.BankChange.WebName.Should().Be("shift_jis");
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void Internal_ShouldBeUtf8()
    {
        FileEncodingHelper.Internal.Should().Be(Encoding.UTF8);
    }

    /// <summary>
    /// このテストでは Ebcdic930 プロパティが CP930 を返す（222 F-INF-010 §6「Ebcdic930 プロパティ」）。
    /// 例外を try/catch で握り潰さず Skip で明示する（I10-004）。
    /// </summary>
    [Fact(Skip = "CP930 は System.Text.Encoding.CodePages パッケージが本ソリューションのどの csproj からも参照されておらず、"
        + "実行環境の CodePagesEncodingProvider ではコードページ実データを取得できないため NotSupportedException を送出する（実測済み）。"
        + "同パッケージを参照し、222 F-INF-010 §7 未確定事項 No.2（EBCDIC 連携を先行開発の対象に含めるか）が確定した時点で有効化する。")]
    public void Ebcdic930_ShouldReturnCodePage930()
    {
        FileEncodingHelper.Ebcdic930.CodePage.Should().Be(930);
    }

    /// <summary>
    /// このテストでは Ebcdic939 プロパティが CP939 を返す（222 F-INF-010 §6「Ebcdic939 プロパティ」）。
    /// 例外を try/catch で握り潰さず Skip で明示する（I10-004）。
    /// </summary>
    [Fact(Skip = "CP939 は System.Text.Encoding.CodePages パッケージが本ソリューションのどの csproj からも参照されておらず、"
        + "実行環境の CodePagesEncodingProvider ではコードページ実データを取得できないため NotSupportedException を送出する（実測済み）。"
        + "同パッケージを参照し、222 F-INF-010 §7 未確定事項 No.2（EBCDIC 連携を先行開発の対象に含めるか）が確定した時点で有効化する。")]
    public void Ebcdic939_ShouldReturnCodePage939()
    {
        FileEncodingHelper.Ebcdic939.CodePage.Should().Be(939);
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public void GetEncoding_NullOrEmpty_ShouldReturnDefault()
    {
        FileEncodingHelper.GetEncoding(null).WebName.Should().Be("shift_jis");
        FileEncodingHelper.GetEncoding("").WebName.Should().Be("shift_jis");
    }

    /// <summary>このテストでは 文字コードを設定したとき、文字コード設定が反映される。</summary>
    [Fact]
    public void GetEncoding_ValidName_ShouldReturnCorrectEncoding()
    {
        FileEncodingHelper.GetEncoding("utf-8").Should().Be(Encoding.UTF8);
    }

    /// <summary>このテストでは 文字コードを設定したとき、文字コード設定が反映される。</summary>
    [Fact]
    public void GetEncoding_InvalidName_ShouldThrow()
    {
        var act = () => FileEncodingHelper.GetEncoding("invalid-encoding-xyz");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>このテストでは 初期状態のとき、既定値が設定される。</summary>
    [Fact]
    public async Task GetInstitutionEncodingAsync_NoSetting_ShouldReturnDefault()
    {
        using var db = TestDbContextFactory.Create();

        var encoding = await FileEncodingHelper.GetInstitutionEncodingAsync(
            db, "9999", FileEncodingHelper.DataType.TransferData);

        encoding.WebName.Should().Be("shift_jis");
    }

    /// <summary>このテストでは 同じプロパティが同一の Encoding インスタンスを返す。</summary>
    [Fact]
    public void Zengin_ShouldReturnSameInstanceOnEachAccess()
    {
        FileEncodingHelper.Zengin.Should().BeSameAs(FileEncodingHelper.Zengin);
    }

    /// <summary>
    /// このテストでは Ebcdic930 プロパティへの複数回アクセスが同一の Encoding インスタンスを返すことを確認する
    /// （222 F-INF-010 §6「Ebcdic930 プロパティ」の一環）。例外を try/catch で握り潰さず Skip で明示する（I10-004）。
    /// </summary>
    [Fact(Skip = "CP930 は System.Text.Encoding.CodePages パッケージが本ソリューションのどの csproj からも参照されておらず、"
        + "実行環境の CodePagesEncodingProvider ではコードページ実データを取得できないため NotSupportedException を送出する（実測済み）。"
        + "同パッケージを参照し、222 F-INF-010 §7 未確定事項 No.2（EBCDIC 連携を先行開発の対象に含めるか）が確定した時点で有効化する。")]
    public void Ebcdic930_ShouldReturnSameInstanceOnEachAccess()
    {
        FileEncodingHelper.Ebcdic930.Should().BeSameAs(FileEncodingHelper.Ebcdic930);
    }

    /// <summary>
    /// このテストでは GetEncoding("ibm930") が CP930 を返す（222 F-INF-010 §6「GetEncoding_IBM930」）。
    /// </summary>
    [Fact(Skip = "CP930 は System.Text.Encoding.CodePages パッケージが本ソリューションのどの csproj からも参照されておらず、"
        + "実行環境の CodePagesEncodingProvider ではコードページ実データを取得できないため NotSupportedException を送出する（実測済み）。"
        + "同パッケージを参照し、222 F-INF-010 §7 未確定事項 No.2（EBCDIC 連携を先行開発の対象に含めるか）が確定した時点で有効化する。")]
    public void GetEncoding_Ibm930_ShouldReturnCodePage930()
    {
        FileEncodingHelper.GetEncoding("ibm930").CodePage.Should().Be(930);
    }

    /// <summary>
    /// このテストでは Zengin (CP932) で変換不能文字をエンコードした場合、例外を送出せず既定の
    /// EncoderFallback により '?' (0x3F) へ置換して処理を継続することを確認する
    /// （222 F-INF-010 §6「CP932_置換動作」、8. 特記事項「Fallback 方針」）。
    /// "㐀" (U+3400, CJK統合漢字拡張A) は JIS X 0208 / CP932 のいずれの範囲にも含まれない文字を選定した。
    /// </summary>
    [Fact]
    public void Zengin_UnmappableCharacter_ShouldReplaceWithQuestionMarkWithoutThrowing()
    {
        var bytes = FileEncodingHelper.Zengin.GetBytes("㐀");

        bytes.Should().ContainSingle().Which.Should().Be((byte)'?');
    }
}










