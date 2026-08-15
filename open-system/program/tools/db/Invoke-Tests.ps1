$ErrorActionPreference = 'Stop'

# Windows の既定コードページでは日本語のメッセージが化けるため、明示的に UTF-8 で出力する。
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$testFiles = Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'tests') -Filter '*.tests.ps1' | Sort-Object Name
$failed = 0

foreach ($file in $testFiles) {
    Write-Host "--- $($file.Name) ---"
    & pwsh -NoProfile -File $file.FullName
    if ($LASTEXITCODE -ne 0) { $failed++ }
}

if ($failed -gt 0) {
    Write-Host "$failed 件のテストファイルが失敗した" -ForegroundColor Red
    exit 1
}

Write-Host 'すべてのテストが成功した' -ForegroundColor Green
