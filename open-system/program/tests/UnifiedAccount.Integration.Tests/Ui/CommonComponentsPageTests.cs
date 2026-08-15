using System.Net;
using FluentAssertions;

namespace UnifiedAccount.Integration.Tests.Ui;

public sealed class CommonComponentsPageTests : IClassFixture<CodeSettingsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CommonComponentsPageTests(CodeSettingsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CodeSettingEdit_ShouldRenderSharedEditableGrid()
    {
        var response = await _client.GetAsync("/code-settings/BANK");
        var content = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("コード設定");
        content.Should().Contain("editable-grid");
    }

    [Fact]
    public async Task CodeCategoryList_ShouldUseSharedPageHeader()
    {
        var response = await _client.GetAsync("/code-settings");
        var content = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("shared-page-header");
    }

    [Fact]
    public async Task CodeSettingEdit_EmptyCategory_ShouldRenderEmptyMessageAndAddFooter()
    {
        // COMPANY は区分マスタ (CodeCategory "00") に存在するが、子コードを持たない空区分。
        var response = await _client.GetAsync("/code-settings/COMPANY");
        var content = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("editable-grid");
        content.Should().Contain("編集対象データがありません。");
        content.Should().Contain("新規追加");
    }

    [Fact]
    public async Task Home_ShouldUseSharedPageHeader()
    {
        var response = await _client.GetAsync("/");
        var content = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("shared-page-header");
        content.Should().Contain("本日以降1週間のカレンダー");
        content.Should().Contain("前週");
        content.Should().Contain("翌週");
    }
}
