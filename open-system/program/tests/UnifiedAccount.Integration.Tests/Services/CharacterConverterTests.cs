using FluentAssertions;
using UnifiedAccount.Domain.ValueObjects;
using UnifiedAccount.Infrastructure.Services;

namespace UnifiedAccount.Integration.Tests.Services;

public class CharacterConverterTests
{
    private readonly CharacterConverter _converter = new();

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("ABC", "ＡＢＣ")]
    [InlineData("123", "１２３")]
    [InlineData(" ", "\u3000")]
    public void HankakuToZenkaku_ShouldConvertCorrectly(string input, string expected)
    {
        _converter.HankakuToZenkaku(input).Should().Be(expected);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Theory]
    [InlineData("ＡＢＣ", "ABC")]
    [InlineData("１２３", "123")]
    [InlineData("\u3000", " ")]
    public void ZenkakuToHankaku_ShouldConvertCorrectly(string input, string expected)
    {
        _converter.ZenkakuToHankaku(input).Should().Be(expected);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void HankakuToZenkaku_NullOrEmpty_ShouldReturnSame()
    {
        _converter.HankakuToZenkaku(null).Should().BeNull();
        _converter.HankakuToZenkaku("").Should().BeEmpty();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void ZenkakuToHankaku_NullOrEmpty_ShouldReturnSame()
    {
        _converter.ZenkakuToHankaku(null).Should().BeNull();
        _converter.ZenkakuToHankaku("").Should().BeEmpty();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void NormalizeKana_HankakuKatakana_ShouldConvertToZenkaku()
    {
        // ｱｲｳ → アイウ
        var input = "\uFF71\uFF72\uFF73";
        var expected = "\u30A2\u30A4\u30A6";

        _converter.NormalizeKana(input).Should().Be(expected);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void NormalizeKana_WithDakuten_ShouldCompose()
    {
        // ｶﾞ → ガ (カ + 濁点 → ガ)
        var input = "\uFF76\uFF9E";
        var expected = "\u30AC";

        _converter.NormalizeKana(input).Should().Be(expected);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void NormalizeKana_WithHandakuten_ShouldCompose()
    {
        // ﾊﾟ → パ (ハ + 半濁点 → パ)
        var input = "\uFF8A\uFF9F";
        var expected = "\u30D1";

        _converter.NormalizeKana(input).Should().Be(expected);
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public void NormalizeKana_NullOrEmpty_ShouldReturnSame()
    {
        _converter.NormalizeKana(null).Should().BeNull();
        _converter.NormalizeKana("").Should().BeEmpty();
    }

    [Theory]
    [InlineData("ｯｬｭｮｧｨｩｪｫ", "ﾂﾔﾕﾖｱｲｳｴｵ")]
    [InlineData("ガ", "ｶﾞ")]
    [InlineData("パ", "ﾊﾟ")]
    [InlineData("ｰ", "-")]
    public void NormalizeZenginKana_ShouldNormalize(string input, string expected)
    {
        var result = _converter.NormalizeZenginKana(input);

        result.IsValid.Should().BeTrue();
        result.NormalizedValue.Should().Be(expected);
    }

    [Theory]
    [InlineData("Ａ")]
    [InlineData("a")]
    [InlineData("_")]
    [InlineData("　")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void NormalizeZenginKana_ShouldRejectDisallowedCharacters(string input)
    {
        var result = _converter.NormalizeZenginKana(input);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void NormalizeZenginKana_ShouldRejectMoreThan15NormalizedCharacters()
    {
        var result = _converter.NormalizeZenginKana("ｱｱｱｱｱｱｱｱｱｱｱｱｱｱｱｱ");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Message.Contains("15"));
    }

    /// <summary>このテストでは 許可外文字を InvalidCharacter として1件返す。</summary>
    [Fact]
    public void NormalizeZenginKana_DisallowedCharacters_ShouldReturnSingleInvalidCharacterError()
    {
        var result = _converter.NormalizeZenginKana("Ａa_");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be(ZenginKanaErrorCode.InvalidCharacter);
        result.Errors[0].Message.Should().Contain("位置");
    }

    /// <summary>このテストでは 最大長超過を ExceedsMaxLength として1件返す。</summary>
    [Fact]
    public void NormalizeZenginKana_ExceedsMaxLength_ShouldReturnExceedsMaxLengthError()
    {
        var result = _converter.NormalizeZenginKana("ｱｱｱｱｱｱｱｱｱｱｱｱｱｱｱｱ");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be(ZenginKanaErrorCode.ExceedsMaxLength);
    }

    /// <summary>このテストでは 許可外文字と最大長超過が同時に起きたとき2件返す。</summary>
    [Fact]
    public void NormalizeZenginKana_BothViolations_ShouldReturnTwoErrors()
    {
        var result = _converter.NormalizeZenginKana("_________________");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Select(e => e.Code).Should().Contain(ZenginKanaErrorCode.InvalidCharacter);
        result.Errors.Select(e => e.Code).Should().Contain(ZenginKanaErrorCode.ExceedsMaxLength);
    }

    /// <summary>このテストでは 半角記号を全角へ変換する（222 §5 HankakuToZenkaku_記号）。</summary>
    [Fact]
    public void HankakuToZenkaku_Symbols_ShouldConvertToFullWidth()
    {
        _converter.HankakuToZenkaku("!@#").Should().Be("！＠＃");
    }

    /// <summary>このテストでは 濁点と長音の混在を全角カナへ正規化する（222 §5 NormalizeKana_混在）。</summary>
    [Fact]
    public void NormalizeKana_Mixed_ShouldNormalize()
    {
        _converter.NormalizeKana("ﾃﾞｰﾀ").Should().Be("データ");
    }

    /// <summary>このテストでは ｳ＋濁点をヴへ合成する（222 §5 NormalizeKana_ヴ）。</summary>
    [Fact]
    public void NormalizeKana_Vu_ShouldCompose()
    {
        _converter.NormalizeKana("ｳﾞ").Should().Be("ヴ");
    }

    /// <summary>このテストでは 非カナ文字をそのまま保持する（222 §5 NormalizeKana_非カナ混在）。</summary>
    [Fact]
    public void NormalizeKana_WithNonKana_ShouldPreserveNonKana()
    {
        _converter.NormalizeKana("ABC ｶﾅ 漢字").Should().Be("ABC カナ 漢字");
    }

    /// <summary>
    /// このテストでは 全銀許可文字だけで構成した入力を有効と判定する（222 §5 NormalizeZenginKana_許可文字）。
    /// 期待値は 222 §3.4.2 のコードブロックを書き写した独立の定数とする（実装の ZenginAllowedCharacters は使用しない）。
    /// </summary>
    [Fact]
    public void NormalizeZenginKana_AllAllowedCharacters_ShouldBeValid()
    {
        const string allowedFromSpecification =
            "ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜｦﾝﾞﾟ()-.0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ ";

        var result = _converter.NormalizeZenginKana(allowedFromSpecification, allowedFromSpecification.Length);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}









