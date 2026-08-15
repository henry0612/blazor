using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Application.Services;

namespace UnifiedAccount.Web.Components.Pages.Code;

public partial class CodeSettingEdit : ComponentBase
{
    [Parameter] public string? CategoryCode { get; set; }

    [Inject] public ICodeSettingService CodeSettingService { get; set; } = default!;

    [Inject] public NavigationManager Navigation { get; set; } = default!;

    [Inject] public ILogger<CodeSettingEdit> Logger { get; set; } = default!;

    private readonly List<CodeSettingEditRowDraft> _rows = new();
    private CodeSettingDto? _category;
    private string? _errorToast;
    private string? _successToast;
    private bool _isConfirmDialogVisible;
    private bool _isSaveConfirmationPending;
    private CodeSettingEditRowDraft? _pendingDeleteRow;
    private string _confirmTitle = "確認";
    private string _confirmMessage = "この操作を実行しますか？";
    private string _confirmText = "実行";
    private string _confirmButtonClass = "primary";

    private IQueryable<CodeSettingEditRowDraft> VisibleRows => _rows
        .Where(row => !row.IsDeleted && row.DeleteAction != CodeSettingDeleteAction.Delete)
        .AsQueryable();

    private string? GetCategoryDescription()
    {
        return _category is null
            ? null
            : $"選択中区分: {_category.CodeValue} ({_category.DisplayText})";
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadAsync(clearToasts: true);
    }

    private async Task LoadAsync(bool clearToasts)
    {
        if (clearToasts)
        {
            ClearToasts();
        }

        _rows.Clear();
        _category = null;

        if (string.IsNullOrWhiteSpace(CategoryCode))
        {
            ShowErrorToast("指定された区分が見つかりません。");
            return;
        }

        var category = await CodeSettingService.GetCategoryAsync(CategoryCode);
        if (category is null)
        {
            ShowErrorToast("指定された区分が見つかりません。");
            return;
        }

        _category = category;
        var items = await LoadAllItemsAsync(category.CodeValue);
        foreach (var item in items)
        {
            _rows.Add(CodeSettingEditRowDraft.FromDto(item));
        }
    }

    private async Task<IReadOnlyList<CodeSettingDto>> LoadAllItemsAsync(string categoryCode)
    {
        const int pageSize = 100;
        var page = 1;
        var allItems = new List<CodeSettingDto>();

        while (true)
        {
            var result = await CodeSettingService.SearchAsync(categoryCode, null, page, pageSize);
            allItems.AddRange(result.Items);

            if (allItems.Count >= result.TotalCount || result.Items.Count == 0)
            {
                break;
            }

            page++;
        }

        return allItems;
    }

    private void AddNewRow()
    {
        _rows.Add(CodeSettingEditRowDraft.CreateNew());
    }

    private void HandleDeleteActionChanged(CodeSettingEditRowDraft row, ChangeEventArgs args)
    {
        var action = Enum.TryParse<CodeSettingDeleteAction>(args.Value?.ToString(), out var parsedAction)
            ? parsedAction
            : CodeSettingDeleteAction.Empty;

        if (action != CodeSettingDeleteAction.Delete)
        {
            row.ApplyDeleteConfirmation(false);
            return;
        }

        _pendingDeleteRow = row;
        _isSaveConfirmationPending = false;
        _confirmTitle = "削除確認";
        _confirmMessage = $"コード「{row.CodeValue}」を削除対象にしますか？";
        _confirmText = "削除対象にする";
        _confirmButtonClass = "danger";
        _isConfirmDialogVisible = true;
    }

    private Task RequestSaveAsync()
    {
        if (_category is null)
        {
            ShowErrorToast("区分が未選択です。");
            return Task.CompletedTask;
        }

        var validationErrors = CodeSettingEditValidation.ValidateRows(_rows);
        if (validationErrors.Count > 0)
        {
            ShowErrorToast(string.Join(" / ", validationErrors));
            return Task.CompletedTask;
        }

        _pendingDeleteRow = null;
        _isSaveConfirmationPending = true;
        _confirmTitle = "保存確認";
        _confirmMessage = "入力内容を保存します。よろしいですか？";
        _confirmText = "保存";
        _confirmButtonClass = "primary";
        _isConfirmDialogVisible = true;
        return Task.CompletedTask;
    }

    private async Task ConfirmAsync()
    {
        _isConfirmDialogVisible = false;
        if (_pendingDeleteRow is not null)
        {
            _pendingDeleteRow.ApplyDeleteConfirmation(true);
            _pendingDeleteRow = null;
            return;
        }

        if (_isSaveConfirmationPending)
        {
            _isSaveConfirmationPending = false;
            await SaveAllAsync();
        }
    }

    private Task CancelConfirmationAsync()
    {
        _isConfirmDialogVisible = false;
        _pendingDeleteRow?.ApplyDeleteConfirmation(false);
        _pendingDeleteRow = null;
        _isSaveConfirmationPending = false;
        return Task.CompletedTask;
    }

    private async Task SaveAllAsync()
    {
        if (_category is null)
        {
            ShowErrorToast("区分が未選択です。");
            return;
        }

        var validationErrors = CodeSettingEditValidation.ValidateRows(_rows);
        if (validationErrors.Count > 0)
        {
            ShowErrorToast(string.Join(" / ", validationErrors));
            return;
        }

        try
        {
            var rowsToDelete = _rows
                .Where(row => row.DeleteAction == CodeSettingDeleteAction.Delete && row.Id.HasValue)
                .ToList();
            var persistableRows = CodeSettingEditValidation.GetPersistableRows(_rows);
            var saveRequests = persistableRows
                .Select(row => CodeSettingEditValidation.BuildSaveRequest(_category.CodeValue, row))
                .ToList();
            var deleteRequests = rowsToDelete
                .Select(CodeSettingEditValidation.BuildDeleteRequest)
                .ToList();

            await CodeSettingService.SaveBatchAsync(saveRequests, deleteRequests);
            Logger.LogInformation("コード設定が完了しました。 {CategoryCode} 追加：{AddedCount} 更新：{UpdatedCount} 削除：{DeletedCount}",
                _category.CodeValue, saveRequests.Count, persistableRows.Count, deleteRequests.Count);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Code setting batch save failed for {CategoryCode}.", _category.CodeValue);
            ShowErrorToast(ex is DbUpdateConcurrencyException
                ? "他のユーザーが更新しました。再読込してから編集してください。"
                : "保存に失敗しました。入力内容を確認して再試行してください。");
            return;
        }

        await LoadAsync(clearToasts: false);
        ShowSuccessToast("保存しました。");
    }

    private Task CancelAsync()
    {
        Navigation.NavigateTo("/code-settings");
        return Task.CompletedTask;
    }

    private void ShowErrorToast(string message)
    {
        _successToast = null;
        _errorToast = message;
    }

    private void ShowSuccessToast(string message)
    {
        _errorToast = null;
        _successToast = message;
    }

    private Task ClearErrorToast()
    {
        _errorToast = null;
        return Task.CompletedTask;
    }

    private Task ClearSuccessToast()
    {
        _successToast = null;
        return Task.CompletedTask;
    }

    private void ClearToasts()
    {
        _errorToast = null;
        _successToast = null;
    }
}
