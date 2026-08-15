#Requires -Version 7.0
<#
.SYNOPSIS
    本番切替実行スクリプト (Step 5.3)
.DESCRIPTION
    切替手順書に基づき、各ステップをインタラクティブに確認しながら実行する。
    各ステップで確認を求め、問題があれば切戻し手順に移行できる。
.PARAMETER Mode
    実行モード: rehearsal (リハーサル) | production (本番)
.PARAMETER ConnectionString
    新システムDB接続文字列
#>
param(
    [ValidateSet("rehearsal", "production")]
    [string]$Mode = "rehearsal",
    [string]$ConnectionString = "Server=localhost;Database=UnifiedAccount;Trusted_Connection=true;TrustServerCertificate=true"
)

$ErrorActionPreference = "Stop"
$cutoverLog = @()
$startTime = Get-Date

function Write-Step {
    param([int]$StepNo, [string]$Description)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host " Step $StepNo : $Description" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
}

function Confirm-Step {
    param([string]$Message)
    if ($Mode -eq "production") {
        $response = Read-Host "$Message (y/n/abort)"
        if ($response -eq "abort") {
            Write-Host "切替を中止します。切戻し手順に移行してください。" -ForegroundColor Red
            exit 1
        }
        return $response -eq "y"
    }
    Write-Host "  [リハーサル] $Message → 自動承認" -ForegroundColor DarkGray
    return $true
}

function Log-Step {
    param([int]$StepNo, [string]$Description, [string]$Status)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $script:cutoverLog += "$timestamp | Step $StepNo | $Description | $Status"
}

# ===== 切替手順 =====

Write-Host ""
Write-Host "╔══════════════════════════════════════════════╗" -ForegroundColor $(if ($Mode -eq "production") { "Red" } else { "Yellow" })
Write-Host "║  統一口座振替システム 本番切替               ║" -ForegroundColor $(if ($Mode -eq "production") { "Red" } else { "Yellow" })
Write-Host "║  モード: $($Mode.PadRight(38))║" -ForegroundColor $(if ($Mode -eq "production") { "Red" } else { "Yellow" })
Write-Host "║  実行日時: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')                 ║" -ForegroundColor $(if ($Mode -eq "production") { "Red" } else { "Yellow" })
Write-Host "╚══════════════════════════════════════════════╝" -ForegroundColor $(if ($Mode -eq "production") { "Red" } else { "Yellow" })

# Step 1: 切替日の最終確認
Write-Step 1 "切替日の最終確認"
Write-Host "  切替実行日: $(Get-Date -Format 'yyyy-MM-dd')"
Write-Host "  モード: $Mode"
if (Confirm-Step "切替日とモードに問題ありませんか？") {
    Log-Step 1 "切替日確認" "完了"
} else {
    Log-Step 1 "切替日確認" "中止"
    exit 1
}

# Step 2: 旧系統の最終バッチ実行確認
Write-Step 2 "旧系統の最終バッチ実行確認"
Write-Host "  旧系統の最終日次バッチが正常完了していることを確認してください。"
if (Confirm-Step "旧系統の最終バッチは正常完了しましたか？") {
    Log-Step 2 "旧系統最終バッチ" "完了確認済"
} else {
    Write-Host "  旧系統バッチ完了後に再実行してください。" -ForegroundColor Yellow
    exit 1
}

# Step 3: 最終データ同期
Write-Step 3 "最終データ同期 (旧→新)"
Write-Host "  移行ツール (UnifiedAccount.Migration) で最終データを同期します。"
Write-Host "  コマンド例:"
Write-Host "    dotnet run --project tools/UnifiedAccount.Migration -- import-banks --file input/banks.csv" -ForegroundColor DarkGray
Write-Host "    dotnet run --project tools/UnifiedAccount.Migration -- import-companies --file input/companies.csv" -ForegroundColor DarkGray
Write-Host "    dotnet run --project tools/UnifiedAccount.Migration -- import-contracts --file input/contracts.csv" -ForegroundColor DarkGray
Write-Host "    dotnet run --project tools/UnifiedAccount.Migration -- validate" -ForegroundColor DarkGray
if (Confirm-Step "最終データ同期は完了しましたか？") {
    Log-Step 3 "最終データ同期" "完了"
} else {
    exit 1
}

# Step 4: データ整合性の最終確認
Write-Step 4 "データ整合性の最終確認"
Write-Host "  移行ツールの validate コマンドで整合性を確認:"
Write-Host "    dotnet run --project tools/UnifiedAccount.Migration -- validate" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  確認項目:"
Write-Host "    [_] Companies 件数一致"
Write-Host "    [_] Contracts 件数一致"
Write-Host "    [_] BankBranches 件数一致"
Write-Host "    [_] FK整合性エラー 0件"
Write-Host "    [_] 重複キー 0件"
if (Confirm-Step "データ整合性に問題はありませんか？") {
    Log-Step 4 "データ整合性確認" "合格"
} else {
    Write-Host "  データ修正後に再実行してください。" -ForegroundColor Yellow
    exit 1
}

# Step 5: 旧系統の停止
Write-Step 5 "旧系統の停止"
Write-Host "  旧系統 (メインフレーム AIM/CICS) を計画停止します。"
Write-Host "  ※ この操作は運用担当が実施"
if (Confirm-Step "旧系統を停止しましたか？") {
    Log-Step 5 "旧系統停止" "完了"
} else {
    exit 1
}

# Step 6: 新系統の本番起動
Write-Step 6 "新系統の本番起動"
Write-Host "  Blazor Server (Web) + Batch JobRunner を起動します。"
Write-Host "  コマンド例:"
Write-Host "    # Web アプリ" -ForegroundColor DarkGray
Write-Host "    dotnet run --project src/UnifiedAccount.Web -- --environment Production" -ForegroundColor DarkGray
Write-Host "    # バッチ (日次)" -ForegroundColor DarkGray
Write-Host "    dotnet run --project src/UnifiedAccount.Batch -- --jobs daily" -ForegroundColor DarkGray
if (Confirm-Step "新系統は正常に起動しましたか？") {
    Log-Step 6 "新系統起動" "完了"
} else {
    Write-Host "  === 切戻し手順に移行 ===" -ForegroundColor Red
    Write-Host "  1. 新系統を停止"
    Write-Host "  2. 旧系統を再起動"
    Write-Host "  3. 原因を調査"
    Log-Step 6 "新系統起動" "失敗→切戻し"
    exit 1
}

# Step 7: 起動後の動作確認
Write-Step 7 "起動後の動作確認"
Write-Host "  確認項目:"
Write-Host "    [_] Web画面 (/) にアクセスできる"
Write-Host "    [_] 銀行マスタ照会が正常動作"
Write-Host "    [_] 会社マスタ照会が正常動作"
Write-Host "    [_] 契約者検索が正常動作"
Write-Host "    [_] API (/api/companies) が応答する"
Write-Host "    [_] ログに重大エラーなし"
if (Confirm-Step "起動後の動作確認は全て合格しましたか？") {
    Log-Step 7 "起動後動作確認" "合格"
} else {
    Write-Host "  === 切戻し判断 ===" -ForegroundColor Red
    Write-Host "  問題が軽微な場合: 修正して続行"
    Write-Host "  問題が重大な場合: abort と入力して切戻し"
    Log-Step 7 "起動後動作確認" "要確認"
}

# Step 8: 切替完了通知
Write-Step 8 "関係者への切替完了通知"
Write-Host "  通知先:"
Write-Host "    - プロジェクトマネージャー"
Write-Host "    - 運用チーム"
Write-Host "    - 業務部門"
Write-Host "    - ヘルプデスク"
if (Confirm-Step "切替完了通知を送信しましたか？") {
    Log-Step 8 "切替完了通知" "送信済"
}

# ===== 完了サマリ =====
$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host ""
Write-Host "╔══════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  切替完了                                    ║" -ForegroundColor Green
Write-Host "║  所要時間: $($duration.ToString('hh\:mm\:ss').PadRight(35))║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════╝" -ForegroundColor Green

Write-Host "`n--- 切替ログ ---"
$cutoverLog | ForEach-Object { Write-Host "  $_" }

# ログ出力
$logDir = "./cutover-logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir -Force | Out-Null }
$logFile = Join-Path $logDir "cutover-$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
$cutoverLog | Out-File -FilePath $logFile -Encoding UTF8
Write-Host "`n切替ログ出力: $logFile"

Write-Host "`n--- 切替後監視スケジュール ---"
Write-Host "  1-3日:  24時間監視 (全ジョブ結果確認、エラー即対応)"
Write-Host "  4-7日:  日次監視 (統計値確認、差異検知)"
Write-Host "  2-4週:  週次監視 (月次処理の確認含む)"
Write-Host "  1-3ヶ月: 通常運用 (定期レポート確認)"
