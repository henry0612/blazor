using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UnifiedAccount.Application.DTOs;

namespace UnifiedAccount.Web.Components.Pages.Bank
{
    /// <summary>
    /// 銀行マスター異動画面の状態管理と検索・新規追加操作を制御します。
    /// </summary>
    public partial class BankMaster
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
        [Inject]
        private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

        //ユーザID
        private string? _userId;
        //相関ID
        private string? _correlationId;

        private const int PageSize = 50;

        private QuickGrid<BankBranchRow>? _grid;

        private readonly PaginationState _pagination = new()
        {
            ItemsPerPage = PageSize
        };

        private bool _isLoading;
        private int _totalCount;
        private string? _loadErrorMessage;

        private string? _lastSearchBankCode;
        private string? _lastSearchBranchCode;

        private string? _searchBankCode;
        private string? _searchBranchCode;
        private bool _hasSearched;
        private bool _isCheckingNew;

        private string? _errorMessage;
        private string? _successMessage;

        /// <summary>
        /// 初期表示時にURLパラメーターを確認し、保存・削除後の完了メッセージを設定します。
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            var uri = Navigation.Uri;

            if (uri.Contains("saved=true"))
            {
                _successMessage = "保存しました。";
            }
            else if (uri.Contains("deleted=true"))
            {
                _successMessage = "削除しました。";
            }
            //ユーザID取得
            var authState = await AuthenticationStateTask;
            _userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
            //相関ID取得
            _correlationId = HttpContextAccessor.HttpContext?.TraceIdentifier;
        }

        /// <summary>
        /// 銀行支店マスターの検索結果を再読み込みします。
        /// </summary>

        private async Task OnSearchClickedAsync()
        {
            _lastSearchBankCode = _searchBankCode;
            _lastSearchBranchCode = _searchBranchCode;

            _hasSearched = true;
            _loadErrorMessage = null;
            _totalCount = 0;
            _errorMessage = null;
            _successMessage = null;

            // 検索条件を変えたときは 1 ページ目から表示する。
            // リセットしないと、前回表示していたページ位置のまま新しい条件で取得してしまう。
            await _pagination.SetCurrentPageIndexAsync(0);

            if (_grid is not null)
            {
                await _grid.RefreshDataAsync();
            }

            StateHasChanged();
        }

        /// <summary>
        /// 検索条件、検索結果状態、画面メッセージを初期状態に戻します。
        /// </summary>

        private void ClearConditions()
        {
            _searchBankCode = null;
            _searchBranchCode = null;

            _hasSearched = false;
            _totalCount = 0;
            _loadErrorMessage = null;

            _errorMessage = null;
            _successMessage = null;
        }

        /// <summary>
        /// 新規追加に必要な金融機関コード・支店コードを検証し、未登録の場合は新規編集画面へ遷移します。
        /// </summary>

        private async Task StartAdd()
        {
            _successMessage = null;
            _errorMessage = null;

            //エラーチェック
            if (string.IsNullOrEmpty(_searchBankCode)
                || _searchBankCode.Length != 4
                || !_searchBankCode.All(char.IsAsciiDigit))
            {
                _errorMessage = "新規追加する場合は、金融機関コード4桁を入力してください。";
                return;
            }
            if (string.IsNullOrEmpty(_searchBranchCode)
                || _searchBranchCode.Length != 3
                || !_searchBranchCode.All(char.IsAsciiDigit))
            {
                _errorMessage = "新規追加する場合は、支店コードを3桁の数字で入力してください。";
                return;
            }

            _isCheckingNew = true;

            try
            {
                var exists = await BankBranchService.GetByKeyAsync(_searchBankCode, _searchBranchCode);
                if (exists is not null)
                {
                    _errorMessage = $"金融機関コード {_searchBankCode} / 支店コード {_searchBranchCode} は既に登録されています。";
                    return;
                }

                //編集画面へ遷移
                Navigation.NavigateTo($"/bank/edit/new?bankCode={_searchBankCode}&branchCode={_searchBranchCode}");
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    "銀行の確認失敗。相関ID={CorrelationId} ユーザーID={UserId} 操作種別={Operation} 金融機関コード={BankCode} 支店コード={BranchCode}",
                    _correlationId,
                    _userId,
                    "新規追加ボタン押下",
                    _searchBankCode,
                    _searchBranchCode);
                _errorMessage = "確認中にエラーが発生しました。";
            }
            finally
            {
                _isCheckingNew = false;
            }
        }

        /// <summary>
        /// QuickGridの要求に応じて銀行マスタを検索し、一覧表示用データを返します。
        /// </summary>

        private async ValueTask<GridItemsProviderResult<BankBranchRow>> LoadItemsAsync(
            GridItemsProviderRequest<BankBranchRow> request)
        {
            //未検索時に一覧を表示しない
            if (!_hasSearched)
            {
                return GridItemsProviderResult.From(
                    Array.Empty<BankBranchRow>(),
                    0);
            }

            _isLoading = true;
            _loadErrorMessage = null;

            try
            {
                //QuickGridのページを取得
                var itemsPerPage = _pagination.ItemsPerPage > 0 ? _pagination.ItemsPerPage : PageSize;
                var page = itemsPerPage > 0
                    ? (request.StartIndex / itemsPerPage) + 1
                    : 1;

                //画面の検索条件で一覧を再取得
                var result = await BankBranchService.SearchAsync(
                    _lastSearchBankCode,
                    _lastSearchBranchCode,
                    page,
                    itemsPerPage,
                    CancellationToken.None);

                //検索結果件数を格納
                _totalCount = result.TotalCount;

                //取得したページだけを一覧に表示
                return GridItemsProviderResult.From(
                    result.Items.ToArray(),
                    result.TotalCount);

            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    "銀行の検索失敗。相関ID={CorrelationId} ユーザーID={UserId} 操作種別={Operation} 金融機関コード={BankCode} 支店コード={BranchCode}",
                    _correlationId,
                    _userId,
                    "検索ボタン押下",
                    _lastSearchBankCode,
                    _lastSearchBranchCode);

                _loadErrorMessage = "検索中にエラーが発生しました。";
                _totalCount = 0;

                //異常時に一覧をクリアする
                return GridItemsProviderResult.From(
                    Array.Empty<BankBranchRow>(),
                    0);
            }
            finally
            {
                _isLoading = false;
            }
        }
        /// <summary>
        /// 選択された銀行マスター行の編集画面URLを生成します。
        /// </summary>

        private static string GetDetailUrl(BankBranchRow item)
        {
            return $"/bank/edit/{Uri.EscapeDataString(item.BankCode)}/{Uri.EscapeDataString(item.BranchCode)}";
        }

    }
}
