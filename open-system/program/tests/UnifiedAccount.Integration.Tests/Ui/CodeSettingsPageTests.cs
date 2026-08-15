using System.Net;
using FluentAssertions;

namespace UnifiedAccount.Integration.Tests.Ui;

public class CodeSettingsPageTests : IClassFixture<CodeSettingsWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CodeSettingsPageTests(CodeSettingsWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCodeSettingsPage_ShouldRenderCategoryList()
    {
        var response = await _client.GetAsync("/code-settings");

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("コード区分一覧");
        content.Should().Contain("銀行");
    }

    [Fact]
    public async Task GetCodeSettingsPage_ShouldRenderCompactCategoryTable()
    {
        var response = await _client.GetAsync("/code-settings");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        content.Should().Contain("コード区分一覧");
        content.Should().Contain("検索結果");
        content.Should().Contain("コード");
        content.Should().Contain("表示名");
        content.Should().Contain("表示順");
        content.Should().Contain("照会");
        content.Should().Contain("code-category-list-table");
    }

    [Fact]
    public async Task GetCategoryDetailPage_ShouldRenderChildRows()
    {
        var response = await _client.GetAsync("/code-settings/BANK");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        content.Should().Contain("コード設定");
        content.Should().Contain("みずほ");
        content.Should().Contain("三菱");
    }

    [Fact]
    public async Task GetCategoryDetailPage_ShouldRenderBatchQuickGridEditor()
    {
        var response = await _client.GetAsync("/code-settings/BANK");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        content.Should().Contain("選択中区分");
        content.Should().Contain("shared-page-header");
        content.Should().NotContain("class=\"message-area\"");
        content.Should().Contain("editable-grid");
        content.Should().Contain("class=\"editable-grid\"");
        content.Should().NotContain("class=\"editable-grid mock-card\"");
        content.Should().Contain("class=\"editable-grid__footer\"");
        content.Should().Contain("class=\"code-setting-edit-card__footer\"");
        content.Should().Contain("保存");
        content.Should().Contain("キャンセル");
        content.Should().Contain("新規追加");
        content.Should().Contain("削除");
        content.Should().NotContain("toast-container");
        content.Should().NotContain("code-setting-inline-create");
    }

    [Fact]
    public async Task GetCodeSettingsPage_ShouldRequireSystemAdminRole()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/code-settings");
        request.Headers.Add("X-Test-Role", "OperationStaff");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCategoryDetailPage_ShouldRequireSystemAdminRole()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/code-settings/BANK");
        request.Headers.Add("X-Test-Role", "OperationStaff");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
