$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'TestHelpers.ps1')
Import-Module (Join-Path $PSScriptRoot '..' 'TableDefinitionParser.psm1') -Force

$sample = @'
# Tier X: サンプル

## 1.1. TM_Sample (サンプルマスター)

### 1.1.1 カラム定義

| # | カラム名 | 日本語名 | データ型 | NULL | PK | 移行元COBOL | 備考 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Id | ID | BIGINT IDENTITY | NO | ✓ | (新規) |
| 2 | SampleCode | サンプルコード | CHAR(6) | NO |  | SM-CODE X(6) |
| 3 | SampleName | サンプル名 | NVARCHAR(30) | YES |  | SM-NAME X(30) |

### 1.1.2 子テーブル: TM_SampleChildren (サンプル子)

| # | カラム名 | 日本語名 | データ型 | NULL | 移行元COBOL | 備考 |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Id | ID | BIGINT IDENTITY | NO | (新規) |
| 2 | SampleId | サンプルID | BIGINT | NO | (FK) |

### 1.1.3 DDL

```sql
CREATE TABLE [dbo].[TM_Sample] (
    [Id]         BIGINT IDENTITY(1,1) NOT NULL,
    [SampleCode] CHAR(6)      NOT NULL,
    [SampleName] NVARCHAR(30) NULL,
    CONSTRAINT [PK_Sample] PRIMARY KEY ([Id])
);

CREATE TABLE [dbo].[TM_SampleChildren] (
    [Id]       BIGINT IDENTITY(1,1) NOT NULL,
    [SampleId] BIGINT NOT NULL,
    CONSTRAINT [PK_SampleChildren] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SampleChildren_Sample] FOREIGN KEY ([SampleId])
        REFERENCES [dbo].[TM_Sample] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_TM_Sample_SampleCode] ON [dbo].[TM_Sample] ([SampleCode]);
```
'@

$path = New-TempMarkdownFile -Content $sample

try {
    $doc = Get-TableDefinitionDocument -Path @($path)

    Assert-Equal -Expected 2 -Actual $doc.Count -Because '親テーブルと子テーブルの2件が解析される'

    $sampleTable = $doc['TM_Sample']
    Assert-Equal -Expected 'dbo' -Actual $sampleTable.Schema -Because 'スキーマが DDL から取得される'
    Assert-Equal -Expected 3 -Actual $sampleTable.DocColumns.Count -Because 'カラム定義表の3行が取れる'
    Assert-Equal -Expected 3 -Actual $sampleTable.DdlColumns.Count -Because 'DDL の3列が取れる。制約行は列として数えない'
    Assert-Equal -Expected 'CHAR(6)' -Actual $sampleTable.DdlColumns[1].Type -Because 'DDL の型が取れる'
    Assert-Equal -Expected 'NO' -Actual $sampleTable.DdlColumns[1].Nullable -Because 'NOT NULL が NO になる'
    Assert-Equal -Expected 'YES' -Actual $sampleTable.DdlColumns[2].Nullable -Because 'NULL が YES になる'
    Assert-Equal -Expected 1 -Actual $sampleTable.IndexStatements.Count -Because 'そのテーブルのインデックス文が紐づく'
    Assert-Equal -Expected 1 -Actual $sampleTable.DdlDefinitionCount -Because '重複定義が無ければ1'

    $childTable = $doc['TM_SampleChildren']
    Assert-Equal -Expected 2 -Actual $childTable.DocColumns.Count -Because '子テーブルの表は PK 列が無くても解析できる'
    Assert-Equal -Expected 'TM_Sample' -Actual ($childTable.ForeignKeys -join ',') -Because '外部キーの参照先が取れる'

    Assert-Equal -Expected 'BIGINT IDENTITY' `
        -Actual (ConvertTo-NormalizedSqlType -Type 'bigint  identity(1, 1)') `
        -Because 'IDENTITY の桁指定を落として正規化する'
    Assert-Equal -Expected 'DECIMAL(10,0)' `
        -Actual (ConvertTo-NormalizedSqlType -Type 'decimal( 10 , 0 )') `
        -Because '桁指定の空白を除去する'

    Assert-Equal -Expected 2 -Actual (Measure-RawCreateTableCount -Path @($path)) `
        -Because '構文を解釈せずに数えた CREATE TABLE の件数が取れる'
}
finally {
    Remove-Item -LiteralPath $path -Force
}

$result = Get-TestResult
$result.Failures | ForEach-Object { Write-Host $_ -ForegroundColor Red }
Write-Host "TableDefinitionParser: $($result.Passed) passed, $($result.Failures.Count) failed"
if ($result.Failures.Count -gt 0) { exit 1 }
