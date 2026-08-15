<#
.SYNOPSIS
209 で CHAR 型として定義されている列を一覧出力する。

.DESCRIPTION
208-データベース基本設計 1.2.1 の判定基準への適合を照合するため、
および実装側への影響範囲を把握するために使う。
#>
[CmdletBinding()]
param(
    [string]$DefinitionDirectory = (Join-Path $PSScriptRoot '..' '..' '..' 'docs' '12C-基本設計' 'データベース定義書'),
    [Parameter(Mandatory)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'

# Windows の既定コードページでは日本語のメッセージが化けるため、明示的に UTF-8 で出力する。
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Import-Module (Join-Path $PSScriptRoot 'TableDefinitionParser.psm1') -Force

$files = Get-ChildItem -LiteralPath $DefinitionDirectory -Filter '209_*.md' | Sort-Object Name
$document = Get-TableDefinitionDocument -Path $files.FullName

$rows = foreach ($name in ($document.Keys | Sort-Object)) {
    $table = $document[$name]
    foreach ($column in $table.DdlColumns) {
        if ($column.Type -notmatch '^CHAR\(') { continue }
        [PSCustomObject]@{
            Table      = $name
            Column     = $column.Name
            Type       = $column.Type
            SourceFile = $table.SourceFile
        }
    }
}

$rows | Export-Csv -LiteralPath $OutputPath -NoTypeInformation -Encoding utf8
Write-Host "CHAR 列: $($rows.Count) 件を $OutputPath へ出力した"
Write-Host "テーブル数: $(($rows | Select-Object -ExpandProperty Table -Unique).Count)"
