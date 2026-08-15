$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'TestHelpers.ps1')
Import-Module (Join-Path $PSScriptRoot '..' 'TableDefinitionParser.psm1') -Force
Import-Module (Join-Path $PSScriptRoot '..' 'ConsistencyValidator.psm1') -Force

function New-Column {
    param([string]$Name, [string]$Type, [string]$Nullable)
    return [PSCustomObject]@{ Name = $Name; Type = $Type; Nullable = $Nullable }
}

function New-Table {
    param([string]$Name, [array]$DocColumns, [array]$DdlColumns, [string]$DdlText = 'CREATE TABLE x', [int]$Count = 1)
    return [PSCustomObject]@{
        Name = $Name; Schema = 'dbo'; DocColumns = $DocColumns; DdlColumns = $DdlColumns
        DdlText = $DdlText; IndexStatements = @(); ForeignKeys = @()
        SourceFile = 'test.md'; DdlDefinitionCount = $Count
    }
}

# 一致する場合は違反ゼロ
$ok = @{ 'TM_A' = New-Table -Name 'TM_A' `
    -DocColumns @((New-Column 'Id' 'BIGINT IDENTITY' 'NO')) `
    -DdlColumns @((New-Column 'Id' 'BIGINT IDENTITY' 'NO')) }
Assert-Equal -Expected 0 -Actual (Test-TableDefinitionConsistency -Document $ok).Count `
    -Because '表と DDL が一致すれば違反は出ない'

# 型の食い違い
$typeMismatch = @{ 'TM_B' = New-Table -Name 'TM_B' `
    -DocColumns @((New-Column 'Code' 'CHAR(3)' 'NO')) `
    -DdlColumns @((New-Column 'Code' 'CHAR(4)' 'NO')) }
$violations = Test-TableDefinitionConsistency -Document $typeMismatch
Assert-Equal -Expected 1 -Actual $violations.Count -Because '型の食い違いが1件検出される'
Assert-Equal -Expected 'TypeMismatch' -Actual $violations[0].Category -Because '分類が TypeMismatch になる'
Assert-True -Condition ($violations[0].Message -match 'CHAR\(3\)' -and $violations[0].Message -match 'CHAR\(4\)') `
    -Because 'メッセージに両方の型が含まれる'

# NULL 許容の食い違い
$nullMismatch = @{ 'TM_C' = New-Table -Name 'TM_C' `
    -DocColumns @((New-Column 'X' 'BIGINT' 'YES')) `
    -DdlColumns @((New-Column 'X' 'BIGINT' 'NO')) }
Assert-Equal -Expected 'NullabilityMismatch' `
    -Actual (Test-TableDefinitionConsistency -Document $nullMismatch)[0].Category `
    -Because 'NULL 許容の食い違いを検出する'

# カラムの有無
$columnMismatch = @{ 'TM_D' = New-Table -Name 'TM_D' `
    -DocColumns @((New-Column 'X' 'BIGINT' 'NO'), (New-Column 'Y' 'BIGINT' 'NO')) `
    -DdlColumns @((New-Column 'X' 'BIGINT' 'NO')) }
$violations = Test-TableDefinitionConsistency -Document $columnMismatch
Assert-Equal -Expected 1 -Actual $violations.Count -Because 'カラムの欠落が1件検出される'
Assert-Equal -Expected 'ColumnMismatch' -Actual $violations[0].Category -Because '分類が ColumnMismatch になる'

# DDL が無い
$noDdl = @{ 'TM_E' = New-Table -Name 'TM_E' `
    -DocColumns @((New-Column 'X' 'BIGINT' 'NO')) -DdlColumns @() -DdlText $null -Count 0 }
Assert-Equal -Expected 'MissingDdl' -Actual (Test-TableDefinitionConsistency -Document $noDdl)[0].Category `
    -Because 'DDL ブロックの欠落を検出する'

# カラム定義表が無い
$noTable = @{ 'TM_F' = New-Table -Name 'TM_F' `
    -DocColumns @() -DdlColumns @((New-Column 'X' 'BIGINT' 'NO')) }
Assert-Equal -Expected 'MissingColumnTable' -Actual (Test-TableDefinitionConsistency -Document $noTable)[0].Category `
    -Because 'カラム定義表の欠落を検出する'

# 二重定義
$duplicate = @{ 'TM_G' = New-Table -Name 'TM_G' `
    -DocColumns @((New-Column 'X' 'BIGINT' 'NO')) `
    -DdlColumns @((New-Column 'X' 'BIGINT' 'NO')) -Count 2 }
Assert-Equal -Expected 'DuplicateDefinition' -Actual (Test-TableDefinitionConsistency -Document $duplicate)[0].Category `
    -Because '同一テーブルの二重定義を検出する'

# DDL 内の列名重複。SQL Server は重複列を許さないため CREATE TABLE が失敗する。
# 突合はカラム名をキーにするため、重複は潰れて型や NULL の比較では現れない。
$duplicateColumn = @{ 'TM_H' = New-Table -Name 'TM_H' `
    -DocColumns @((New-Column 'X' 'BIGINT' 'NO'), (New-Column 'Y' 'CHAR(1)' 'NO')) `
    -DdlColumns @((New-Column 'X' 'BIGINT' 'NO'), (New-Column 'X' 'BIGINT' 'NO'), (New-Column 'Y' 'CHAR(1)' 'NO')) }
$violations = Test-TableDefinitionConsistency -Document $duplicateColumn
Assert-Equal -Expected 1 -Actual $violations.Count -Because 'DDL の列名重複が1件検出される'
Assert-Equal -Expected 'DuplicateColumn' -Actual $violations[0].Category -Because '分類が DuplicateColumn になる'
Assert-Equal -Expected 'X' -Actual $violations[0].Column -Because '重複した列名が報告される'

# カラム定義表側の行重複も同じ分類で報告する
$duplicateDocColumn = @{ 'TM_I' = New-Table -Name 'TM_I' `
    -DocColumns @((New-Column 'X' 'BIGINT' 'NO'), (New-Column 'X' 'BIGINT' 'NO')) `
    -DdlColumns @((New-Column 'X' 'BIGINT' 'NO')) }
Assert-Equal -Expected 'DuplicateColumn' `
    -Actual (Test-TableDefinitionConsistency -Document $duplicateDocColumn)[0].Category `
    -Because 'カラム定義表の行重複も検出する'

# 解析から漏れた CREATE TABLE の検出
$unparsedSample = @'
## 1.1. TM_Ok (正常)

### 1.1.1 DDL

```sql
CREATE TABLE [dbo].[TM_Ok] (
    [Id] BIGINT NOT NULL
);

CREATE TABLE [dbo].[TM_Broken] ( [Id] BIGINT NOT NULL );
```
'@

$unparsedPath = New-TempMarkdownFile -Content $unparsedSample

try {
    $parsed = Get-TableDefinitionDocument -Path @($unparsedPath)
    # 返り値は配列そのものが 1 個の値として返る。@() で包み直すと入れ子になる。
    $unparsedViolations = Get-UnparsedDdlViolation -Path @($unparsedPath) -Document $parsed

    Assert-True -Condition ($unparsedViolations -is [array]) `
        -Because '返り値は配列であり、要素が配列に入れ子になっていない'
    Assert-Equal -Expected 1 -Actual $unparsedViolations.Count `
        -Because '閉じ括弧が独立行にないブロックは解析から漏れ、件数の差として検出される'
    Assert-Equal -Expected 'UnparsedDdl' -Actual $unparsedViolations[0].Category `
        -Because '分類が UnparsedDdl になる'

    $okSample = $unparsedSample -replace 'CREATE TABLE \[dbo\]\.\[TM_Broken\] \( \[Id\] BIGINT NOT NULL \);', ''
    $okPath = New-TempMarkdownFile -Content $okSample
    try {
        $okParsed = Get-TableDefinitionDocument -Path @($okPath)
        Assert-Equal -Expected 0 -Actual (Get-UnparsedDdlViolation -Path @($okPath) -Document $okParsed).Count `
            -Because '全ブロックが解析できれば違反は出ない'
    }
    finally {
        Remove-Item -LiteralPath $okPath -Force
    }
}
finally {
    Remove-Item -LiteralPath $unparsedPath -Force
}

$result = Get-TestResult
$result.Failures | ForEach-Object { Write-Host $_ -ForegroundColor Red }
Write-Host "ConsistencyValidator: $($result.Passed) passed, $($result.Failures.Count) failed"
if ($result.Failures.Count -gt 0) { exit 1 }
