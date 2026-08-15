using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Application.Services;
using UnifiedAccount.Domain.Interfaces;

namespace UnifiedAccount.Web.Components.Pages.Bank;

/// <summary>
/// 銀行マスター異動編集画面の登録・更新・削除操作を制御します。
/// </summary>
public partial class BankEdit : ComponentBase
{
    [Parameter] public string? BankCode { get; set; }
    [Parameter] public string? BranchCode { get; set; }

    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject] public IBankBranchService BankBranchService { get; set; } = default!;
    [Inject] public ICharacterConverter Converter { get; set; } = default!;
    [Inject] public ILogger<BankEdit> Logger { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

    //ユーザID
    private string? _userId;
    //相関ID
    private string? _correlationId;

    private bool _isNewRow;
    private bool _isLoaded;
    private bool _isSaving;

    private string _editBankCode = string.Empty;
    private string _editBranchCode = string.Empty;
    private string? _bankName;
    private string _bankNameKana = string.Empty;
    private string? _branchName;
    private string _branchNameKana = string.Empty;
    private DateTime _originalUpdatedAt;

    private string? _errorMessage;
    private string? _successMessage;
    private string? _bankNameKanaError;
    private string? _branchNameKanaError;

    private bool _showDeleteConfirm;
    private bool _showLeaveConfirm;

    private string? _initBankName;
    private string _initBankNameKana = string.Empty;
    private string? _initBranchName;
    private string _initBranchNameKana = string.Empty;

    private bool IsDirty =>
        _bankName != _initBankName ||
        _bankNameKana != _initBankNameKana ||
        _branchName != _initBranchName ||
        _branchNameKana != _initBranchNameKana;

    /// <summary>
    /// 新規登録または既存編集の初期表示状態を設定します。
    /// </summary>

    protected override async Task OnInitializedAsync()
    {
        //ユーザID取得
        var authState = await AuthenticationStateTask;
        _userId = authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
        //相関ID取得
        _correlationId = HttpContextAccessor.HttpContext?.TraceIdentifier;


        //既存編集
        if (!string.IsNullOrEmpty(BankCode) && !string.IsNullOrEmpty(BranchCode))
        {
            _isNewRow = false;
            //選択した銀行マスタ情報を画面へ表示
            await LoadExistingAsync(BankCode, BranchCode);
        }
        //新規登録
        else
        {
            _isNewRow = true;
            _editBankCode = GetQueryParam("bankCode");
            _editBranchCode = GetQueryParam("branchCode");
            //名称、カナ名欄を初期化
            SetInitValues();
            _isLoaded = true;
        }
    }

    /// <summary>
    /// 指定された銀行コードと支店コードの既存データを編集フォームへ読み込みます。
    /// </summary>
    private async Task LoadExistingAsync(string bankCode, string branchCode)
    {
        try
        {
            var dto = await BankBranchService.GetByKeyAsync(bankCode, branchCode);
            if (dto is null)
            {
                _errorMessage = "対象データが存在しません。";
            }
            else
            {
                _editBankCode = dto.BankCode;
                _editBranchCode = dto.BranchCode;
                _bankName = dto.BankNameKanji;
                _bankNameKana = dto.BankNameKana;
                _branchName = dto.BranchNameKanji;
                _branchNameKana = dto.BranchNameKana;
                _originalUpdatedAt = dto.UpdatedAt;
                SetInitValues();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "データの読み込みに失敗しました。相関ID={CorrelationId} ユーザーID={UserId} 操作種別=編集押下 金融機関コード={BankCode} 支店コード={BranchCode}",
                _correlationId,
                _userId,
                bankCode,
                branchCode);
            _errorMessage = "データの読み込みに失敗しました。";
        }
        finally
        {
            _isLoaded = true;
        }
    }

    /// <summary>
    /// 現在のフォーム値を初期値として保持します。
    /// </summary>

    private void SetInitValues()
    {
        _initBankName = _bankName;
        _initBankNameKana = _bankNameKana;
        _initBranchName = _branchName;
        _initBranchNameKana = _branchNameKana;
    }

    /// <summary>
    /// 新規登録または既存データの更新を実行します。
    /// </summary>
    private async Task SaveAsync()
    {
        if (!ValidateRequired())
        {
            return;
        }

        var bankKana = Converter.NormalizeZenginKana(_bankNameKana);
        var branchKana = Converter.NormalizeZenginKana(_branchNameKana);
        _bankNameKanaError = bankKana.IsValid ? null : string.Join(" / ", bankKana.Errors.Select(e => e.Message));
        _branchNameKanaError = branchKana.IsValid ? null : string.Join(" / ", branchKana.Errors.Select(e => e.Message));
        if (_bankNameKanaError is not null || _branchNameKanaError is not null)
        {
            return;
        }

        _isSaving = true;
        _errorMessage = null;
        _successMessage = null;

        try
        {
            if (_isNewRow)
            {
                var request = new CreateBankBranchRequest(
                    _editBankCode,
                    _editBranchCode,
                    bankKana.NormalizedValue ?? string.Empty,
                    branchKana.NormalizedValue ?? string.Empty,
                    NullIfEmpty(_bankName),
                    NullIfEmpty(_branchName),
                    GetActorIdOrThrow(),
                    _correlationId ?? string.Empty);

                // 新規登録
                var result = await BankBranchService.CreateAsync(request);

                _errorMessage = result switch
                {
                    CreateResult.Success => null,
                    CreateResult.Duplicate => "同じ金融機関コード・支店コードが既に登録されています。",
                    CreateResult.ValidationError e => string.Join(" / ", e.Errors),
                    CreateResult.DbError => "保存中にエラーが発生しました。",
                    _ => "予期しないエラーが発生しました。"
                };

                if (result is CreateResult.Success)
                {
                    Logger.LogInformation(
                        "銀行の更新が完了しました。{BankCode}-{BranchCode}",
                        _editBankCode,
                        _editBranchCode);

                    Navigation.NavigateTo("/bank/master?saved=true");
                }
                else if (result is CreateResult.Duplicate or CreateResult.ValidationError)
                {
                    Logger.LogInformation(
                        "{errorMessage} {BankCode}-{BranchCode}",
                        _errorMessage,
                        _editBankCode,
                        _editBranchCode);
                }
                else if (result is CreateResult.DbError dbError)
                {
                    Logger.LogError(
                    dbError.Exception,
                    "保存中にエラーが発生しました。相関ID ={CorrelationId} ユーザーID={UserId} 操作種別={Operation} 金融機関コード={BankCode} 支店コード={BranchCode}",
                    _correlationId,
                    _userId,
                    "保存ボタン押下",
                    _editBankCode,
                    _editBranchCode);
                }
            }
            else
            {
                var request = new UpdateBankBranchRequest(
                    bankKana.NormalizedValue ?? string.Empty,
                    branchKana.NormalizedValue ?? string.Empty,
                    NullIfEmpty(_bankName),
                    NullIfEmpty(_branchName),
                    _originalUpdatedAt,
                    GetActorIdOrThrow(),
                    _correlationId ?? string.Empty);

                // 既存データの更新
                var result = await BankBranchService.UpdateAsync(_editBankCode, _editBranchCode, request);

                _errorMessage = result switch
                {
                    UpdateResult.Success => null,
                    UpdateResult.NotFound => "対象データが存在しません。",
                    UpdateResult.Conflict => "他のユーザーが更新しました。再読込してから編集してください。",
                    UpdateResult.DbError => "保存中にエラーが発生しました。",
                    _ => "予期しないエラーが発生しました。"
                };

                if (result is UpdateResult.Success s)
                {
                    _originalUpdatedAt = s.Updated.UpdatedAt;
                    SetInitValues();
                    Logger.LogInformation(
                        "銀行の更新が完了しました。{BankCode}-{BranchCode}",
                        _editBankCode,
                        _editBranchCode);

                    Navigation.NavigateTo("/bank/master?saved=true");
                }
                else if (result is UpdateResult.NotFound or UpdateResult.Conflict)
                {
                    Logger.LogInformation(
                        "{errorMessage} {BankCode}-{BranchCode}",
                        _errorMessage,
                        _editBankCode,
                        _editBranchCode);

                }
                else if (result is UpdateResult.DbError dbError)
                {
                    Logger.LogError(
                    dbError.Exception,
                    "保存中にエラーが発生しました。相関ID ={CorrelationId} ユーザーID={UserId} 操作種別={Operation} 金融機関コード={BankCode} 支店コード={BranchCode}",
                    _correlationId,
                    _userId,
                    "保存ボタン押下",
                    _editBankCode,
                    _editBranchCode);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "保存中にエラーが発生しました。相関ID={CorrelationId} ユーザーID={UserId} 操作種別={Operation} 金融機関コード={BankCode} 支店コード={BranchCode}",
                _correlationId,
                _userId,
                "保存ボタン押下",
                BankCode,
                BranchCode);
            _errorMessage = "保存中にエラーが発生しました。";
        }
        finally
        {
            _isSaving = false;
        }
    }

    /// <summary>
    /// エラーチェックを行い、エラーメッセージを設定します。
    /// </summary>

    private bool ValidateRequired()
    {
        _bankNameKanaError = string.IsNullOrWhiteSpace(_bankNameKana) ? "金融機関カナ名は必須です。" : null;
        _branchNameKanaError = string.IsNullOrWhiteSpace(_branchNameKana) ? "支店カナ名は必須です。" : null;
        return _bankNameKanaError is null && _branchNameKanaError is null;
    }

    /// <summary>
    /// キャンセルボタン押下時の処理
    /// </summary>
    private void Cancel()
    {
        if (IsDirty)
            _showLeaveConfirm = true;
        else
            //銀行マスタ異動画面へ戻る
            Navigation.NavigateTo("/bank/master");
    }
    /// <summary>
    /// 一覧へ戻るボタン押下時の処理
    /// </summary>
    private void GoBack()
    {
        if (IsDirty)
            _showLeaveConfirm = true;
        else
            //銀行マスタ異動画面へ戻る
            Navigation.NavigateTo("/bank/master");
    }

    /// <summary>
    /// 確認メッセージの戻るボタン押下時の処理
    /// </summary>
    private void GoBackConfirmed()
    {
        _showLeaveConfirm = false;
        //銀行マスタ異動画面へ戻る
        Navigation.NavigateTo("/bank/master");
    }

    private void StartDelete() => _showDeleteConfirm = true;

    /// <summary>
    /// 画面に表示された銀行マスタを削除します。
    /// </summary>
    private async Task DeleteAsync()
    {
        _showDeleteConfirm = false;
        _errorMessage = null;
        try
        {
            var request = new DeleteBankBranchRequest(
                _originalUpdatedAt,
                GetActorIdOrThrow(),
                _correlationId ?? string.Empty);
            var result = await BankBranchService.DeleteAsync(_editBankCode, _editBranchCode, request);

            //メッセージ
            switch (result)
            {
                case DeleteResult.Success:
                    Logger.LogInformation(
                        "銀行の削除が完了しました。{BankCode}-{BranchCode}",
                        _editBankCode,
                        _editBranchCode
                        );
                    Navigation.NavigateTo("/bank/master?deleted=true");
                    break;
                case DeleteResult.NotFound:
                    Logger.LogInformation(
                        "対象データが存在しません。{BankCode}-{BranchCode}",
                        _editBankCode,
                        _editBranchCode
                        );
                    _errorMessage = "対象データが存在しません。既に削除された可能性があります。";
                    break;
                case DeleteResult.Conflict:
                    Logger.LogInformation(
                        "他のユーザーが更新しました。{BankCode}-{BranchCode}",
                        _editBankCode,
                        _editBranchCode
                        );
                    _errorMessage = "他のユーザーが更新しました。再読込してから編集してください。";
                    break;
                case DeleteResult.DbError e:
                    Logger.LogError(
                        e.Exception,
                        "銀行の削除失敗。相関ID={CorrelationId} ユーザーID={UserId} 操作種別={Operation} 金融機関コード={BankCode} 支店コード={BranchCode}",
                        _correlationId,
                        _userId,
                        "削除ボタン押下",
                        _editBankCode,
                        _editBranchCode);
                    _errorMessage = "削除中にエラーが発生しました。";
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "銀行の削除失敗。相関ID={CorrelationId} ユーザーID={UserId} 操作種別={Operation} 金融機関コード={BankCode} 支店コード={BranchCode}",
                _correlationId,
                _userId,
                "削除ボタン押下",
                _editBankCode,
                _editBranchCode);
            _errorMessage = "削除中にエラーが発生しました。";
        }
    }
    /// <summary>
    /// 監査ログに記録する操作者IDを取得します。BankEditは認可属性で保護されており
    /// 到達時点で認証済みのはずですが、取得できない場合は例外として扱います
    /// （CodeAuditLogEntryFactory.CreateEntryと同じ方針）。
    /// </summary>
    private string GetActorIdOrThrow()
    {
        if (string.IsNullOrWhiteSpace(_userId))
        {
            throw new InvalidOperationException("認証済み利用者のActorIdを取得できません。");
        }
        return _userId;
    }

    /// <summary>
    /// 空文字をNULL値へ変換します。
    /// </summary>
    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    /// <summary>
    /// 現在のURLから指定されたクエリパラメーター値を取得します。
    /// </summary>
    private string GetQueryParam(string name)
    {
        var uri = Navigation.Uri;
        var qIndex = uri.IndexOf('?');
        if (qIndex < 0) return string.Empty;
        foreach (var pair in uri[(qIndex + 1)..].Split('&'))
        {
            var eq = pair.IndexOf('=');
            if (eq < 0) continue;
            if (Uri.UnescapeDataString(pair[..eq]) == name)
                return Uri.UnescapeDataString(pair[(eq + 1)..]);
        }
        return string.Empty;
    }
}
