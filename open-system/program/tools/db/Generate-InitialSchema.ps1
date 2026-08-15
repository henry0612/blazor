<#
.SYNOPSIS
209_テーブル定義書から初期スキーマスクリプトを生成する。

.DESCRIPTION
DDL ブロックを抽出し、外部キー依存の順に並べて単一の SQL スクリプトへ出力する。
生成の前にカラム定義表との整合を検査し、違反があれば生成せず終了コード 1 で終了する。
警告を出して生成を続けない。不整合な生成物が配布されることを防ぐためである。
#>
[CmdletBinding()]
param(
    [string]$DefinitionDirectory = (Join-Path $PSScriptRoot '..' '..' '..' 'docs' '12C-基本設計' 'データベース定義書'),
    [string]$OutputPath = (Join-Path $PSScriptRoot '..' '..' 'db' 'migrations' 'V001__InitialSchema.sql'),
    [string]$DatabaseName = 'UnifiedAccount'
)

$ErrorActionPreference = 'Stop'

# Windows の既定コードページでは日本語のメッセージが化けるため、明示的に UTF-8 で出力する。
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Import-Module (Join-Path $PSScriptRoot 'TableDefinitionParser.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'ConsistencyValidator.psm1') -Force
Import-Module (Join-Path $PSScriptRoot 'DependencySorter.psm1') -Force

$files = Get-ChildItem -LiteralPath $DefinitionDirectory -Filter '209_*.md' | Sort-Object Name
if ($files.Count -eq 0) {
    Write-Host "209_*.md が見つからない: $DefinitionDirectory" -ForegroundColor Red
    exit 1
}

$document = Get-TableDefinitionDocument -Path $files.FullName
$violations = (Test-TableDefinitionConsistency -Document $document) +
              (Get-UnparsedDdlViolation -Path $files.FullName -Document $document)

if ($violations.Count -gt 0) {
    Write-Host "整合違反が $($violations.Count) 件あるため生成しない" -ForegroundColor Red
    $violations | Group-Object Category | Sort-Object Count -Descending | ForEach-Object {
        Write-Host ("  {0,5}  {1}" -f $_.Count, $_.Name)
    }
    Write-Host '明細は Test-DefinitionConsistency.ps1 -Detail で確認する'
    exit 1
}

try {
    $order = Get-TableCreationOrder -Document $document
}
catch {
    Write-Host "テーブル作成順を決定できない: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$builder = [System.Text.StringBuilder]::new()
[void]$builder.AppendLine('-- このファイルは Generate-InitialSchema.ps1 が 209_テーブル定義書 から生成した。')
[void]$builder.AppendLine('-- 直接編集しない。定義を変えるときは 209_テーブル定義書 を直して再生成する。')
[void]$builder.AppendLine('')
[void]$builder.AppendLine('SET NOCOUNT ON;')
[void]$builder.AppendLine('')
[void]$builder.AppendLine('-- フィルター付きインデックスの作成には QUOTED_IDENTIFIER ON と ANSI_NULLS ON が必須。')
[void]$builder.AppendLine('-- sqlcmd の既定は QUOTED_IDENTIFIER OFF のため、実行クライアントに依存せず宣言する。')
[void]$builder.AppendLine('SET QUOTED_IDENTIFIER ON;')
[void]$builder.AppendLine('SET ANSI_NULLS ON;')
[void]$builder.AppendLine('')
[void]$builder.AppendLine("IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'$DatabaseName')")
[void]$builder.AppendLine('BEGIN')
[void]$builder.AppendLine("    CREATE DATABASE [$DatabaseName];")
[void]$builder.AppendLine('END')
[void]$builder.AppendLine('GO')
[void]$builder.AppendLine("USE [$DatabaseName];")
[void]$builder.AppendLine('GO')
[void]$builder.AppendLine('')
[void]$builder.AppendLine('-- USE の後も同一セッションで設定は維持されるが、部分実行に備えて再宣言する。')
[void]$builder.AppendLine('SET QUOTED_IDENTIFIER ON;')
[void]$builder.AppendLine('SET ANSI_NULLS ON;')
[void]$builder.AppendLine('GO')
[void]$builder.AppendLine('')
[void]$builder.AppendLine('BEGIN TRY')
[void]$builder.AppendLine('BEGIN TRANSACTION;')
[void]$builder.AppendLine('')

# dbo は既定で存在するため作成しない。
$schemas = $order | ForEach-Object { $document[$_].Schema } | Where-Object { $_ -and $_ -ne 'dbo' } | Sort-Object -Unique
foreach ($schema in $schemas) {
    [void]$builder.AppendLine("IF SCHEMA_ID(N'$schema') IS NULL")
    [void]$builder.AppendLine('BEGIN')
    [void]$builder.AppendLine("    EXEC(N'CREATE SCHEMA [$schema];');")
    [void]$builder.AppendLine('END;')
    [void]$builder.AppendLine('')
}

[void]$builder.AppendLine('-- テーブル作成（外部キー依存の順）')
[void]$builder.AppendLine('')
foreach ($name in $order) {
    $table = $document[$name]
    [void]$builder.AppendLine("IF OBJECT_ID(N'[$($table.Schema)].[$name]', N'U') IS NULL")
    [void]$builder.AppendLine('BEGIN')
    [void]$builder.AppendLine($table.DdlText.TrimEnd())
    [void]$builder.AppendLine('END;')
    [void]$builder.AppendLine('')
}

[void]$builder.AppendLine('-- インデックス作成（全テーブル作成後）')
[void]$builder.AppendLine('')
foreach ($name in $order) {
    $table = $document[$name]
    foreach ($statement in $table.IndexStatements) {
        $indexName = [regex]::Match($statement, 'INDEX\s+\[?(\w+)\]?').Groups[1].Value
        # インデックス名は SQL Server ではテーブル単位で一意であり、データベース全体では一意でない。
        # 名前だけで存在確認すると、別テーブルの同名インデックスを自分のものと誤認する。
        [void]$builder.AppendLine("IF NOT EXISTS (SELECT 1 FROM sys.indexes")
        [void]$builder.AppendLine("               WHERE name = N'$indexName'")
        [void]$builder.AppendLine("                 AND object_id = OBJECT_ID(N'[$($table.Schema)].[$name]'))")
        [void]$builder.AppendLine('BEGIN')
        [void]$builder.AppendLine("    EXEC(N'$($statement.Replace("'", "''").TrimEnd())');")
        [void]$builder.AppendLine('END;')
        [void]$builder.AppendLine('')
    }
}

[void]$builder.AppendLine('COMMIT TRANSACTION;')
[void]$builder.AppendLine('END TRY')
[void]$builder.AppendLine('BEGIN CATCH')
[void]$builder.AppendLine('    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;')
[void]$builder.AppendLine('    THROW;')
[void]$builder.AppendLine('END CATCH')
[void]$builder.AppendLine('GO')

$outputDirectory = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

# sqlcmd -i は BOM 付き UTF-8 を前提にしている。
Set-Content -LiteralPath $OutputPath -Value $builder.ToString() -Encoding utf8BOM -NoNewline

Write-Host "生成した: $OutputPath"
Write-Host "  テーブル: $($order.Count) 件"
Write-Host "  スキーマ: $(($schemas -join ', '))"
exit 0
