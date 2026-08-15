using FluentAssertions;
using UnifiedAccount.Application.DTOs;
using UnifiedAccount.Web.Components.Pages.Code;

namespace UnifiedAccount.Integration.Tests.Ui;

public class CodeSettingEditValidationTests
{
    [Fact]
    public void ValidateRows_ShouldIgnoreBlankNewRow()
    {
        var rows = new[]
        {
            CodeSettingEditRowDraft.CreateNew()
        };

        var result = CodeSettingEditValidation.ValidateRows(rows);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateRows_ShouldRejectBlankExistingRow()
    {
        var draft = CodeSettingEditRowDraft.FromDto(new CodeSettingDto
        {
            Id = 10,
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "みずほ",
            DisplayOrder = 1
        });
        draft.CodeValue = string.Empty;
        draft.DisplayText = string.Empty;
        draft.DisplayOrderText = string.Empty;

        var rows = new[] { draft };

        var result = CodeSettingEditValidation.ValidateRows(rows);

        result.Should().ContainSingle(message => message.Contains("既存行"));
    }

    [Fact]
    public void ValidateRows_ShouldRejectDuplicateCodeValues()
    {
        var duplicate = CodeSettingEditRowDraft.CreateNew();
        duplicate.CodeValue = "0001";
        duplicate.DisplayText = "三菱";
        duplicate.DisplayOrderText = "2";

        var rows = new[]
        {
            CodeSettingEditRowDraft.FromDto(new CodeSettingDto
            {
                Id = 10,
                CodeCategory = "BANK",
                CodeValue = "0001",
                DisplayText = "みずほ",
                DisplayOrder = 1
            }),
            duplicate
        };

        var result = CodeSettingEditValidation.ValidateRows(rows);

        result.Should().ContainSingle(message => message.Contains("重複"));
    }

    [Fact]
    public void GetPersistableRows_ShouldExcludeRowsMarkedForDelete()
    {
        var row = CodeSettingEditRowDraft.FromDto(new CodeSettingDto
        {
            Id = 10,
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "みずほ",
            DisplayOrder = 1
        });
        row.DeleteAction = CodeSettingDeleteAction.Delete;

        var persistableRows = CodeSettingEditValidation.GetPersistableRows(new[] { row });

        persistableRows.Should().BeEmpty();
    }

    [Fact]
    public void ApplyDeleteConfirmation_ShouldClearDeleteActionWhenCanceled()
    {
        var row = CodeSettingEditRowDraft.FromDto(new CodeSettingDto
        {
            Id = 10,
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "みずほ",
            DisplayOrder = 1
        });
        row.ApplyDeleteConfirmation(false);

        row.DeleteAction.Should().Be(CodeSettingDeleteAction.Empty);
    }

    [Fact]
    public void BuildSaveRequest_ShouldMapBlankDisplayOrderToZero()
    {
        var row = CodeSettingEditRowDraft.CreateNew();
        row.CodeValue = "0001";
        row.DisplayText = "みずほ";

        var request = CodeSettingEditValidation.BuildSaveRequest("BANK", row);

        request.CodeCategory.Should().Be("BANK");
        request.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void BuildSaveRequest_ShouldPreserveOriginalUpdatedAt()
    {
        var updatedAt = new DateTime(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);
        var row = CodeSettingEditRowDraft.FromDto(new CodeSettingDto
        {
            Id = 10,
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "みずほ",
            DisplayOrder = 1,
            UpdatedAt = updatedAt
        });

        var request = CodeSettingEditValidation.BuildSaveRequest("BANK", row);

        request.OriginalUpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void BuildDeleteRequest_ShouldMapExistingRowConcurrencyValue()
    {
        var updatedAt = new DateTime(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);
        var row = CodeSettingEditRowDraft.FromDto(new CodeSettingDto
        {
            Id = 10,
            CodeCategory = "BANK",
            CodeValue = "0001",
            DisplayText = "みずほ",
            DisplayOrder = 1,
            UpdatedAt = updatedAt
        });

        var request = CodeSettingEditValidation.BuildDeleteRequest(row);

        request.Id.Should().Be(10);
        request.OriginalUpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void GetPersistableRows_ShouldExcludeBlankNewRow()
    {
        var rows = new[]
        {
            CodeSettingEditRowDraft.CreateNew(),
            BuildNewRow("0001", "みずほ", string.Empty)
        };

        var persistableRows = CodeSettingEditValidation.GetPersistableRows(rows);

        persistableRows.Should().ContainSingle(row => row.CodeValue == "0001");
    }

    private static CodeSettingEditRowDraft BuildNewRow(string codeValue, string displayText, string displayOrderText)
    {
        var row = CodeSettingEditRowDraft.CreateNew();
        row.CodeValue = codeValue;
        row.DisplayText = displayText;
        row.DisplayOrderText = displayOrderText;
        return row;
    }
}
