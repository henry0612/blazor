<#
.SYNOPSIS
    開発・単体テスト環境へ業務サンプルデータを投入する。
.DESCRIPTION
    DevelopmentData.sql（マスタ）を投入したうえで、Sample-*.sql を順に投入する。
    投入後に各テーブルの件数を期待値と突合し、差分があれば非ゼロで終了する。
    本番環境では実行しないこと。
.PARAMETER Server
    接続先 SQL Server。既定は . （ローカル既定インスタンス）。
.PARAMETER Database
    対象データベース。既定は UnifiedAccount。
    注意: 投入対象の各 SQL ファイルは冒頭に USE [UnifiedAccount]; を持つため、
    投入は常に UnifiedAccount データベースへ行われる。本パラメータは件数検証の接続先だけに使われる。
    投入先を変える場合は SQL ファイル側の USE 文も変更すること。
#>
[CmdletBinding()]
param(
    [string]$Server = '.',
    [string]$Database = 'UnifiedAccount'
)

$ErrorActionPreference = 'Stop'
# sqlcmd の失敗判定は $LASTEXITCODE の明示チェックだけに一本化する。
# pwsh のバージョンによっては $PSNativeCommandUseErrorActionPreference が既定で有効となり、
# 非ゼロ終了が例外として飛んで下の [NG] メッセージに到達しなくなるため、明示的に無効化する。
$PSNativeCommandUseErrorActionPreference = $false
$scriptDir = $PSScriptRoot

# 投入順。DevelopmentData.sql のマスタが先。Sample-*.sql どうしは相互に依存しない。
$scripts = @(
    'DevelopmentData.sql',
    'Sample-F-ONL-006.sql',
    'Sample-F-CHO-004.sql',
    'Sample-F-REP-005.sql',
    'Sample-F-KOZ-001.sql',
    'Sample-F-REP-001.sql',
    'Sample-F-ONL-007.sql'
)

foreach ($name in $scripts) {
    $path = Join-Path $scriptDir $name
    if (-not (Test-Path $path)) {
        Write-Host "[NG] スクリプトが見つかりません: $path"
        exit 1
    }

    Write-Host "投入中: $name"
    # -b により SQL エラーで sqlcmd が非ゼロ終了する。
    # 注意: 投入対象の各 SQL ファイルは冒頭に USE [UnifiedAccount]; を持つため、
    # 投入は常に UnifiedAccount データベースへ行われる。$Database は件数検証の接続先だけに使われる。
    # 投入先を変える場合は SQL ファイル側の USE 文も変更すること。
    & sqlcmd -S $Server -d $Database -b -i $path
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[NG] 投入に失敗しました: $name (exit=$LASTEXITCODE)"
        exit 1
    }
}

# 期待件数。設計書 §6 の各表と対応する。
$expected = [ordered]@{
    'dbo.TM_BankHeadOffices'          = 10
    'dbo.TM_BankBranches'             = 20
    'dbo.TM_Companies'                = 3
    'dbo.TM_Contracts'                = 4
    'dbo.TD_TransferAmounts'          = 20
    'dbo.TR_ContractMasterDeleteLogs' = 20
    'dbo.TD_CompanyChangeRequests'    = 20
    'dbo.TR_CompanyMasterChangeLogs'  = 20
    'cho.TM_BatchControlParameters'   = 1
    'cho.TD_CooperativeTransferReceipts' = 1
    'cho.TD_CooperativeTransfers'     = 20
}

Write-Host ''
Write-Host '件数検証'
$ng = 0
foreach ($table in $expected.Keys) {
    $raw = & sqlcmd -S $Server -d $Database -b -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM $table;"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[NG] 件数取得に失敗しました: $table"
        $ng++
        continue
    }

    $matched = $raw | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1
    if ($null -eq $matched) {
        Write-Host ("  [NG] {0,-38} 件数を解析できませんでした" -f $table)
        $ng++
        continue
    }

    $actual = [int]$matched
    $want = $expected[$table]
    if ($actual -eq $want) {
        Write-Host ("  [OK] {0,-38} {1,4} 件" -f $table, $actual)
    }
    else {
        Write-Host ("  [NG] {0,-38} {1,4} 件（期待 {2}）" -f $table, $actual, $want)
        $ng++
    }
}

Write-Host ''
if ($ng -gt 0) {
    Write-Host "[NG] 件数が期待と一致しないテーブルが $ng 件あります。"
    exit 1
}

Write-Host '[OK] サンプルデータの投入と件数検証が完了しました。'
exit 0
