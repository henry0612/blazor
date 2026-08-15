<#
.SYNOPSIS
生成した V001__InitialSchema.sql を LocalDB へ適用し、209 と突合する。

.DESCRIPTION
検証用のデータベースを作り直してから適用し、テーブルと列の集合、
インデックス数、外部キー数が 209 と一致することを確認する。
検証後もデータベースは残す。失敗時の調査に使うためである。
#>
[CmdletBinding()]
param(
    [string]$ServerInstance = '(localdb)\MSSQLLocalDB',
    [string]$DatabaseName = 'UnifiedAccountVerify',
    [string]$ScriptPath = (Join-Path $PSScriptRoot '..' '..' 'db' 'migrations' 'V001__InitialSchema.sql'),
    [string]$DefinitionDirectory = (Join-Path $PSScriptRoot '..' '..' '..' 'docs' '12C-基本設計' 'データベース定義書')
)

$ErrorActionPreference = 'Stop'

# Windows の既定コードページでは日本語のメッセージが化けるため、明示的に UTF-8 で出力する。
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Import-Module (Join-Path $PSScriptRoot 'TableDefinitionParser.psm1') -Force

Write-Host "検証用データベースを作り直す: $DatabaseName"
$dropSql = @"
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'$DatabaseName')
BEGIN
    ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$DatabaseName];
END
"@
& sqlcmd -S $ServerInstance -Q $dropSql -b
if ($LASTEXITCODE -ne 0) { throw '検証用データベースの削除に失敗した' }

Write-Host '生成物を適用する'
$sql = (Get-Content -LiteralPath $ScriptPath -Raw).Replace('[UnifiedAccount]', "[$DatabaseName]").Replace("N'UnifiedAccount'", "N'$DatabaseName'")
$tempScript = Join-Path ([System.IO.Path]::GetTempPath()) 'uad-verify-schema.sql'
Set-Content -LiteralPath $tempScript -Value $sql -Encoding utf8BOM -NoNewline

& sqlcmd -S $ServerInstance -i $tempScript -b
if ($LASTEXITCODE -ne 0) { throw '生成物の適用に失敗した' }
Remove-Item -LiteralPath $tempScript -Force

Write-Host '実データベースと 209 を突合する'
$query = @"
SET NOCOUNT ON;
SELECT t.name + '.' + c.name
FROM sys.tables t
INNER JOIN sys.columns c ON c.object_id = t.object_id
ORDER BY t.name, c.name;
"@
$actual = (& sqlcmd -S $ServerInstance -d $DatabaseName -Q $query -h -1 -W -b) |
    Where-Object { $_ -and $_ -notmatch '^\(' } | Sort-Object

$files = Get-ChildItem -LiteralPath $DefinitionDirectory -Filter '209_*.md' | Sort-Object Name
$document = Get-TableDefinitionDocument -Path $files.FullName
$expected = foreach ($name in $document.Keys) {
    foreach ($column in $document[$name].DdlColumns) { "$name.$($column.Name)" }
}
$expected = $expected | Sort-Object

$difference = Compare-Object $expected $actual
if ($difference) {
    Write-Host '209 と実データベースに差分がある' -ForegroundColor Red
    $difference | ForEach-Object {
        $side = if ($_.SideIndicator -eq '<=') { '209のみ' } else { '実DBのみ' }
        Write-Host ("  [{0}] {1}" -f $side, $_.InputObject)
    }
    exit 1
}

Write-Host 'インデックスと外部キーの件数を突合する'
$objectQuery = @"
SET NOCOUNT ON;
SELECT COUNT(*) FROM sys.indexes WHERE object_id IN (SELECT object_id FROM sys.tables) AND is_primary_key = 0 AND type <> 0;
SELECT COUNT(*) FROM sys.foreign_keys;
"@
$counts = (& sqlcmd -S $ServerInstance -d $DatabaseName -Q $objectQuery -h -1 -W -b) |
    Where-Object { $_ -match '^\d+$' }

# 209 側の期待値。CREATE INDEX 文と、DDL 本体に書かれた UNIQUE 制約・FOREIGN KEY 制約を数える。
$expectedIndexes = 0
$expectedForeignKeys = 0
foreach ($name in $document.Keys) {
    $table = $document[$name]
    $expectedIndexes += $table.IndexStatements.Count
    $expectedIndexes += [regex]::Matches($table.DdlText, 'CONSTRAINT\s+\[?\w+\]?\s+UNIQUE', 'IgnoreCase').Count
    $expectedForeignKeys += [regex]::Matches($table.DdlText, 'CONSTRAINT\s+\[?\w+\]?\s+FOREIGN\s+KEY', 'IgnoreCase').Count
}

$actualIndexes = [int]$counts[0]
$actualForeignKeys = [int]$counts[1]
$objectMismatch = $false

if ($actualIndexes -ne $expectedIndexes) {
    Write-Host "インデックス数が一致しない 209=$expectedIndexes 実DB=$actualIndexes" -ForegroundColor Red
    $objectMismatch = $true
}

if ($actualForeignKeys -ne $expectedForeignKeys) {
    Write-Host "外部キー数が一致しない 209=$expectedForeignKeys 実DB=$actualForeignKeys" -ForegroundColor Red
    $objectMismatch = $true
}

if ($objectMismatch) { exit 1 }

Write-Host ("突合一致: テーブル {0} 件 / 列 {1} 件 / インデックス {2} 件 / 外部キー {3} 件" -f `
    $document.Count, $expected.Count, $actualIndexes, $actualForeignKeys) -ForegroundColor Green
exit 0
