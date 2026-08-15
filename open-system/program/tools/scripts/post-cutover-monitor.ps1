#Requires -Version 7.0
<#
.SYNOPSIS
    切替後監視スクリプト (Step 5.3 後続)
.DESCRIPTION
    切替後の監視期間中、システムの健全性を自動チェックする。
    Windows タスクスケジューラで定期実行を想定。
.PARAMETER WebUrl
    Web アプリの URL
.PARAMETER ConnectionString
    DB 接続文字列
.PARAMETER OutputDir
    レポート出力先
#>
param(
    [string]$WebUrl = "https://localhost:5001",
    [string]$ConnectionString = "Server=localhost;Database=UnifiedAccount;Trusted_Connection=true;TrustServerCertificate=true",
    [string]$OutputDir = "./monitoring-reports"
)

$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

$results = @()

Write-Host "=== 切替後システム監視 ===" -ForegroundColor Cyan
Write-Host "実行日時: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"

# --- 1. Web アプリ応答チェック ---
Write-Host "`n[1] Web アプリ応答チェック..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri $WebUrl -TimeoutSec 10 -SkipCertificateCheck -ErrorAction Stop
    $statusCode = $response.StatusCode
    $responseTime = "OK"
    $results += @{ Name = "Web応答"; Status = "PASS"; Detail = "StatusCode=$statusCode" }
    Write-Host "  [OK] StatusCode: $statusCode" -ForegroundColor Green
} catch {
    $results += @{ Name = "Web応答"; Status = "FAIL"; Detail = $_.Exception.Message }
    Write-Host "  [NG] $($_.Exception.Message)" -ForegroundColor Red
}

# --- 2. API ヘルスチェック ---
Write-Host "[2] API ヘルスチェック..." -ForegroundColor Yellow
$apiEndpoints = @(
    "/api/companies?page=1&pageSize=1",
    "/api/bank-branches?page=1&pageSize=1",
    "/api/contracts?page=1&pageSize=1"
)

$apiOk = 0
$apiFail = 0
foreach ($ep in $apiEndpoints) {
    try {
        $r = Invoke-WebRequest -Uri "$WebUrl$ep" -TimeoutSec 10 -SkipCertificateCheck -ErrorAction Stop
        $apiOk++
    } catch {
        $apiFail++
    }
}
$apiStatus = if ($apiFail -eq 0) { "PASS" } else { "FAIL" }
$results += @{ Name = "API応答"; Status = $apiStatus; Detail = "OK=$apiOk, NG=$apiFail / $($apiEndpoints.Count) endpoints" }
Write-Host "  [$apiStatus] OK=$apiOk, NG=$apiFail" -ForegroundColor $(if ($apiFail -eq 0) { "Green" } else { "Red" })

# --- 3. ログファイルエラーチェック ---
Write-Host "[3] ログエラーチェック..." -ForegroundColor Yellow
$logDirs = @("./logs", "./logs/web", "./logs/batch")
$totalErrors = 0
$totalFatals = 0

foreach ($dir in $logDirs) {
    if (Test-Path $dir) {
        $recentLogs = Get-ChildItem -Path $dir -Filter "*.txt" -ErrorAction SilentlyContinue |
            Where-Object { $_.LastWriteTime -gt (Get-Date).AddHours(-24) }
        foreach ($log in $recentLogs) {
            $errors = (Select-String -Path $log.FullName -Pattern "\[ERR\]" -ErrorAction SilentlyContinue | Measure-Object).Count
            $fatals = (Select-String -Path $log.FullName -Pattern "\[FTL\]" -ErrorAction SilentlyContinue | Measure-Object).Count
            $totalErrors += $errors
            $totalFatals += $fatals
        }
    }
}

$logStatus = if ($totalFatals -eq 0) { "PASS" } else { "FAIL" }
$results += @{ Name = "ログエラー"; Status = $logStatus; Detail = "ERROR=$totalErrors, FATAL=$totalFatals (24h)" }
Write-Host "  [$logStatus] ERROR=$totalErrors, FATAL=$totalFatals" -ForegroundColor $(if ($totalFatals -eq 0) { "Green" } else { "Red" })

# --- 4. ディスク容量チェック ---
Write-Host "[4] ディスク容量チェック..." -ForegroundColor Yellow
$drive = (Get-Location).Drive
if ($drive) {
    $freeGB = [math]::Round($drive.Free / 1GB, 1)
    $totalGB = [math]::Round(($drive.Used + $drive.Free) / 1GB, 1)
    $usedPercent = [math]::Round($drive.Used / ($drive.Used + $drive.Free) * 100, 1)
    $diskStatus = if ($usedPercent -lt 90) { "PASS" } else { "FAIL" }
    $results += @{ Name = "ディスク容量"; Status = $diskStatus; Detail = "空き=${freeGB}GB / 全体=${totalGB}GB (使用率=${usedPercent}%)" }
    Write-Host "  [$diskStatus] 空き: ${freeGB}GB (使用率: ${usedPercent}%)" -ForegroundColor $(if ($usedPercent -lt 90) { "Green" } else { "Red" })
} else {
    $results += @{ Name = "ディスク容量"; Status = "SKIP"; Detail = "ドライブ情報取得不可" }
}

# --- 5. プロセス稼働チェック ---
Write-Host "[5] プロセス稼働チェック..." -ForegroundColor Yellow
$processNames = @("dotnet")
$runningCount = 0
foreach ($pname in $processNames) {
    $procs = Get-Process -Name $pname -ErrorAction SilentlyContinue
    if ($procs) { $runningCount += $procs.Count }
}
$procStatus = if ($runningCount -gt 0) { "PASS" } else { "WARN" }
$results += @{ Name = "プロセス稼働"; Status = $procStatus; Detail = "dotnet プロセス: ${runningCount}個" }
Write-Host "  [$procStatus] dotnet プロセス: ${runningCount}個" -ForegroundColor $(if ($runningCount -gt 0) { "Green" } else { "Yellow" })

# --- サマリ出力 ---
$failCount = ($results | Where-Object { $_.Status -eq "FAIL" }).Count
$summary = if ($failCount -eq 0) { "正常" } else { "異常あり (${failCount}件)" }

Write-Host "`n=== 監視結果: $summary ===" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })

# レポート出力
$reportPath = Join-Path $OutputDir "monitor-${timestamp}.txt"
$report = "切替後監視レポート`n実行: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n`n"
foreach ($r in $results) {
    $report += "[$($r.Status)] $($r.Name): $($r.Detail)`n"
}
$report += "`n総合: $summary"
$report | Out-File -FilePath $reportPath -Encoding UTF8
Write-Host "レポート: $reportPath"

if ($failCount -gt 0) { exit 1 }
