$ErrorActionPreference = 'Stop'

# Windows の既定コードページでは日本語のメッセージが化けるため、明示的に UTF-8 で出力する。
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$script:Failures = @()
$script:Passed = 0

function Assert-Equal {
    param(
        [Parameter(Mandatory)][AllowNull()][AllowEmptyString()]$Expected,
        [Parameter(Mandatory)][AllowNull()][AllowEmptyString()]$Actual,
        [Parameter(Mandatory)][string]$Because
    )

    if ($Expected -ne $Actual) {
        $script:Failures += "[FAIL] $Because`n       期待: '$Expected'`n       実際: '$Actual'"
        return
    }

    $script:Passed++
}

function Assert-True {
    param(
        [Parameter(Mandatory)][bool]$Condition,
        [Parameter(Mandatory)][string]$Because
    )

    if (-not $Condition) {
        $script:Failures += "[FAIL] $Because"
        return
    }

    $script:Passed++
}

function Assert-Throws {
    param(
        [Parameter(Mandatory)][scriptblock]$Action,
        [Parameter(Mandatory)][string]$ExpectedMessagePattern,
        [Parameter(Mandatory)][string]$Because
    )

    try {
        & $Action
    }
    catch {
        if ($_.Exception.Message -match $ExpectedMessagePattern) {
            $script:Passed++
            return
        }

        $script:Failures += "[FAIL] $Because`n       例外メッセージが '$ExpectedMessagePattern' に一致しない: $($_.Exception.Message)"
        return
    }

    $script:Failures += "[FAIL] $Because`n       例外が送出されなかった"
}

function Get-TestResult {
    return [PSCustomObject]@{
        Passed   = $script:Passed
        Failures = $script:Failures
    }
}

function New-TempMarkdownFile {
    param([Parameter(Mandatory)][string]$Content)

    $path = Join-Path ([System.IO.Path]::GetTempPath()) ("uad-209-test-{0}.md" -f [System.Guid]::NewGuid())
    Set-Content -LiteralPath $path -Value $Content -Encoding utf8NoBOM
    return $path
}
