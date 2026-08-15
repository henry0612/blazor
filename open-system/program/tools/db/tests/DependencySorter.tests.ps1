$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'TestHelpers.ps1')
Import-Module (Join-Path $PSScriptRoot '..' 'DependencySorter.psm1') -Force

function New-Node {
    param([string]$Name, [string[]]$ForeignKeys = @())
    return [PSCustomObject]@{ Name = $Name; ForeignKeys = $ForeignKeys; DdlText = 'CREATE TABLE x' }
}

# 親が子より先に来る
$doc = @{
    'Child'  = New-Node -Name 'Child' -ForeignKeys @('Parent')
    'Parent' = New-Node -Name 'Parent'
}
$order = Get-TableCreationOrder -Document $doc
Assert-True -Condition ([array]::IndexOf($order, 'Parent') -lt [array]::IndexOf($order, 'Child')) `
    -Because '親テーブルが子テーブルより先に並ぶ'
Assert-Equal -Expected 2 -Actual $order.Count -Because '全テーブルが1回ずつ並ぶ'

# 自己参照は依存として扱わない
$selfRef = @{ 'Node' = New-Node -Name 'Node' -ForeignKeys @('Node') }
Assert-Equal -Expected 'Node' -Actual ((Get-TableCreationOrder -Document $selfRef) -join ',') `
    -Because '自己参照は循環として扱わない'

# 循環参照は例外
$cycle = @{
    'A' = New-Node -Name 'A' -ForeignKeys @('B')
    'B' = New-Node -Name 'B' -ForeignKeys @('A')
}
Assert-Throws -Action { Get-TableCreationOrder -Document $cycle } -ExpectedMessagePattern '循環参照' `
    -Because '循環参照を検出して例外を送出する'

# 未定義の親は例外
$missing = @{ 'A' = New-Node -Name 'A' -ForeignKeys @('Unknown') }
Assert-Throws -Action { Get-TableCreationOrder -Document $missing } -ExpectedMessagePattern 'Unknown' `
    -Because '定義の無いテーブルへの参照を検出して例外を送出する'

# 決定的な順序（同じ入力なら同じ出力）
$stable = @{
    'C' = New-Node -Name 'C'
    'A' = New-Node -Name 'A'
    'B' = New-Node -Name 'B'
}
Assert-Equal -Expected 'A,B,C' -Actual ((Get-TableCreationOrder -Document $stable) -join ',') `
    -Because '依存関係が無ければ名前順に並び、実行のたびに順序が変わらない'

$result = Get-TestResult
$result.Failures | ForEach-Object { Write-Host $_ -ForegroundColor Red }
Write-Host "DependencySorter: $($result.Passed) passed, $($result.Failures.Count) failed"
if ($result.Failures.Count -gt 0) { exit 1 }
