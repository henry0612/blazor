<#
.SYNOPSIS
209_テーブル定義書のカラム定義表と DDL ブロックの整合を検査する。

.DESCRIPTION
違反があれば分類ごとの件数と明細を出力し、終了コード 1 で終了する。
違反が無ければ終了コード 0 で終了する。
#>
[CmdletBinding()]
param(
    [string]$DefinitionDirectory = (Join-Path $PSScriptRoot '..' '..' '..' 'docs' '12C-基本設計' 'データベース定義書'),
    [switch]$Detail
)

$ErrorActionPreference = 'Stop'

# Windows の既定コードページでは日本語のメッセージが化けるため、明示的に UTF-8 で出力する。
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Import-Module (Join-Path $PSScriptRoot 'TableDefinitionParser.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'ConsistencyValidator.psm1') -Force

$files = Get-ChildItem -LiteralPath $DefinitionDirectory -Filter '209_*.md' | Sort-Object Name
if ($files.Count -eq 0) {
    throw "209_*.md が見つからない: $DefinitionDirectory"
}

Write-Host "対象ファイル: $($files.Count) 件"
$document = Get-TableDefinitionDocument -Path $files.FullName
Write-Host "解析テーブル数: $($document.Count)"

# 両関数とも配列を 1 個の値として返す。@() で包み直すと入れ子になるため、そのまま連結する。
$violations = (Test-TableDefinitionConsistency -Document $document) +
              (Get-UnparsedDdlViolation -Path $files.FullName -Document $document)

if ($violations.Count -eq 0) {
    Write-Host '整合違反なし' -ForegroundColor Green
    exit 0
}

Write-Host ''
Write-Host "整合違反: $($violations.Count) 件" -ForegroundColor Yellow
$violations | Group-Object Category | Sort-Object Count -Descending | ForEach-Object {
    Write-Host ("  {0,5}  {1}" -f $_.Count, $_.Name)
}

if ($Detail) {
    Write-Host ''
    foreach ($violation in $violations) {
        $target = if ($violation.Column) { "$($violation.Table).$($violation.Column)" } else { $violation.Table }
        Write-Host ("  [{0}] {1}  {2}" -f $violation.Category, $target, $violation.Message)
    }
}

exit 1
