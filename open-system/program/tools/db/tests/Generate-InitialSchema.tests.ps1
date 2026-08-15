$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'TestHelpers.ps1')

$generator = Join-Path $PSScriptRoot '..' 'Generate-InitialSchema.ps1'

$validSample = @'
# サンプル

## 1.1. TM_Parent (親)

### 1.1.1 カラム定義

| # | カラム名 | 日本語名 | データ型 | NULL | PK | 移行元COBOL | 備考 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Id | ID | BIGINT IDENTITY | NO | ✓ | (新規) |

### 1.1.2 DDL

```sql
CREATE TABLE [dbo].[TM_Parent] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    CONSTRAINT [PK_Parent] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_TM_Parent_Id] ON [dbo].[TM_Parent] ([Id]);
```

## 1.2. TD_Child (子)

### 1.2.1 カラム定義

| # | カラム名 | 日本語名 | データ型 | NULL | PK | 移行元COBOL | 備考 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Id | ID | BIGINT IDENTITY | NO | ✓ | (新規) |
| 2 | ParentId | 親ID | BIGINT | NO |  | (FK) |

### 1.2.2 DDL

```sql
CREATE TABLE [cho].[TD_Child] (
    [Id]       BIGINT IDENTITY(1,1) NOT NULL,
    [ParentId] BIGINT NOT NULL,
    CONSTRAINT [PK_Child] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Child_Parent] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[TM_Parent] ([Id])
);
```
'@

$workDir = Join-Path ([System.IO.Path]::GetTempPath()) ("uad-gen-{0}" -f [System.Guid]::NewGuid())
New-Item -ItemType Directory -Path $workDir | Out-Null
$definitionDir = Join-Path $workDir 'definitions'
New-Item -ItemType Directory -Path $definitionDir | Out-Null
Set-Content -LiteralPath (Join-Path $definitionDir '209_sample.md') -Value $validSample -Encoding utf8NoBOM
$outputPath = Join-Path $workDir 'V001__InitialSchema.sql'

try {
    & pwsh -NoProfile -File $generator -DefinitionDirectory $definitionDir -OutputPath $outputPath
    Assert-Equal -Expected 0 -Actual $LASTEXITCODE -Because '整合が取れた定義からは生成が成功する'

    $sql = Get-Content -LiteralPath $outputPath -Raw

    Assert-True -Condition ($sql -match 'SET QUOTED_IDENTIFIER ON') -Because 'QUOTED_IDENTIFIER を明示する'
    Assert-True -Condition ($sql -match 'SET ANSI_NULLS ON') -Because 'ANSI_NULLS を明示する'
    Assert-True -Condition ($sql -match "CREATE SCHEMA \[cho\]") -Because '使用しているスキーマを作成する'
    Assert-True -Condition ($sql -notmatch "CREATE SCHEMA \[dbo\]") -Because 'dbo は既定で存在するため作成しない'

    $parentPosition = $sql.IndexOf('CREATE TABLE [dbo].[TM_Parent]')
    $childPosition = $sql.IndexOf('CREATE TABLE [cho].[TD_Child]')
    Assert-True -Condition ($parentPosition -gt 0 -and $childPosition -gt $parentPosition) `
        -Because '親テーブルが子テーブルより先に出力される'

    $indexPosition = $sql.IndexOf('CREATE INDEX [IX_TM_Parent_Id]')
    Assert-True -Condition ($indexPosition -gt $childPosition) `
        -Because 'インデックスは全テーブル作成後にまとめて出力される'

    # 生成物は決定的である
    $secondPath = Join-Path $workDir 'second.sql'
    & pwsh -NoProfile -File $generator -DefinitionDirectory $definitionDir -OutputPath $secondPath
    Assert-Equal -Expected (Get-Content -LiteralPath $outputPath -Raw) `
        -Actual (Get-Content -LiteralPath $secondPath -Raw) `
        -Because '同じ入力からは同じ出力が得られる'

    # 整合違反があれば生成しない
    $brokenDir = Join-Path $workDir 'broken'
    New-Item -ItemType Directory -Path $brokenDir | Out-Null
    $broken = $validSample -replace '\| 2 \| ParentId \| 親ID \| BIGINT \| NO \|  \| \(FK\) \|', ''
    Set-Content -LiteralPath (Join-Path $brokenDir '209_sample.md') -Value $broken -Encoding utf8NoBOM
    $brokenOutput = Join-Path $workDir 'broken.sql'

    & pwsh -NoProfile -File $generator -DefinitionDirectory $brokenDir -OutputPath $brokenOutput
    Assert-Equal -Expected 1 -Actual $LASTEXITCODE -Because '整合違反があれば異常終了する'
    Assert-True -Condition (-not (Test-Path -LiteralPath $brokenOutput)) `
        -Because '整合違反があれば生成物を書き出さない'
}
finally {
    Remove-Item -LiteralPath $workDir -Recurse -Force
}

$result = Get-TestResult
$result.Failures | ForEach-Object { Write-Host $_ -ForegroundColor Red }
Write-Host "Generate-InitialSchema: $($result.Passed) passed, $($result.Failures.Count) failed"
if ($result.Failures.Count -gt 0) { exit 1 }
