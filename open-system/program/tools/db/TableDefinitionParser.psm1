$ErrorActionPreference = 'Stop'

function ConvertTo-NormalizedSqlType {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Type)

    $normalized = $Type.Trim().ToUpperInvariant()
    $normalized = [regex]::Replace($normalized, '\s+', ' ')
    # IDENTITY の開始値と増分は比較対象にしない。全テーブルで (1,1) 固定であり、
    # カラム定義表は桁を書かない記法を採るため。
    $normalized = [regex]::Replace($normalized, 'IDENTITY\s*\(\s*\d+\s*,\s*\d+\s*\)', 'IDENTITY')
    $normalized = [regex]::Replace($normalized, '\(\s*(\d+)\s*,\s*(\d+)\s*\)', '($1,$2)')
    $normalized = [regex]::Replace($normalized, '\(\s*(\d+|MAX)\s*\)', '($1)')
    return $normalized.Trim()
}

function New-TableEntry {
    param([Parameter(Mandatory)][string]$Name, [Parameter(Mandatory)][string]$SourceFile)

    return [PSCustomObject]@{
        Name               = $Name
        Schema             = $null
        DocColumns         = @()
        DdlColumns         = @()
        DdlText            = $null
        IndexStatements    = @()
        ForeignKeys        = @()
        SourceFile         = $SourceFile
        DdlDefinitionCount = 0
    }
}

function Read-ColumnDefinitionRow {
    param(
        [Parameter(Mandatory)][string]$Line,
        [Parameter(Mandatory)][hashtable]$HeaderIndex
    )

    $cells = $Line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
    if ($cells.Count -le $HeaderIndex.Type) { return $null }

    $name = $cells[$HeaderIndex.Name]
    # 区切り行(---)や見出しの再掲を弾く。カラム名は英字始まりの識別子に限る。
    # 範囲表記(ProcessingFlag1〜12)は未展開の誤りとして検査側で報告するため、ここでは通す。
    if ($name -notmatch '^[A-Za-z_][\w〜]*$') { return $null }

    $nullable = ''
    if ($null -ne $HeaderIndex.Nullable -and $cells.Count -gt $HeaderIndex.Nullable) {
        $nullable = $cells[$HeaderIndex.Nullable].ToUpperInvariant()
    }

    return [PSCustomObject]@{
        Name     = $name
        Type     = ConvertTo-NormalizedSqlType -Type $cells[$HeaderIndex.Type]
        Nullable = $nullable
    }
}

function Read-DdlColumns {
    param([Parameter(Mandatory)][string]$Body)

    $columns = @()
    foreach ($rawLine in $Body -split "`n") {
        $line = ($rawLine -split '--')[0].Trim().TrimEnd(',')
        $match = [regex]::Match($line, '^\[(\w+)\]\s+(.+)$')
        if (-not $match.Success) { continue }

        $rest = $match.Groups[2].Value
        $typeMatch = [regex]::Match($rest, '^([A-Za-z]+\d*\s*(?:\(\s*[\dA-Za-z, ]+\s*\))?)')
        if (-not $typeMatch.Success) { continue }

        $type = ConvertTo-NormalizedSqlType -Type $typeMatch.Groups[1].Value
        # IDENTITY は NOT NULL の前後どちらにも書かれる記法があるため、位置に依存せず判定する。
        if ($rest -match '\bIDENTITY\b' -and $type -notmatch 'IDENTITY') {
            $type = "$type IDENTITY"
        }

        $nullable = ''
        if ($rest -match '\bNOT\s+NULL\b') { $nullable = 'NO' }
        elseif ($rest -match '\bNULL\b') { $nullable = 'YES' }

        $columns += [PSCustomObject]@{ Name = $match.Groups[1].Value; Type = $type; Nullable = $nullable }
    }

    return , $columns
}

function Add-DdlBlock {
    param(
        [Parameter(Mandatory)][string]$Sql,
        [Parameter(Mandatory)][hashtable]$Tables,
        [Parameter(Mandatory)][string]$SourceFile
    )

    foreach ($match in [regex]::Matches($Sql, 'CREATE\s+TABLE\s+\[?(\w+)\]?\.\[?(\w+)\]?\s*\((.*?)\n\)\s*;', 'Singleline,IgnoreCase')) {
        $schema = $match.Groups[1].Value
        $table = $match.Groups[2].Value
        $body = $match.Groups[3].Value

        if (-not $Tables.ContainsKey($table)) {
            $Tables[$table] = New-TableEntry -Name $table -SourceFile $SourceFile
        }

        $entry = $Tables[$table]
        $entry.DdlDefinitionCount++
        $entry.Schema = $schema
        $entry.DdlText = $match.Value
        $entry.DdlColumns = Read-DdlColumns -Body $body

        $references = @()
        foreach ($fk in [regex]::Matches($body, 'REFERENCES\s+\[?(\w+)\]?\.\[?(\w+)\]?', 'IgnoreCase')) {
            $references += $fk.Groups[2].Value
        }
        $entry.ForeignKeys = $references
    }

    foreach ($match in [regex]::Matches($Sql, '(CREATE\s+(?:UNIQUE\s+)?(?:NONCLUSTERED\s+)?INDEX\s+\[?\w+\]?\s+ON\s+\[?\w+\]?\.\[?(\w+)\]?.*?;)', 'Singleline,IgnoreCase')) {
        $table = $match.Groups[2].Value
        if (-not $Tables.ContainsKey($table)) {
            $Tables[$table] = New-TableEntry -Name $table -SourceFile $SourceFile
        }
        $Tables[$table].IndexStatements += $match.Groups[1].Value.Trim()
    }
}

function Get-TableDefinitionDocument {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string[]]$Path)

    $tables = @{}

    foreach ($file in $Path) {
        $fileName = Split-Path -Leaf $file
        $lines = (Get-Content -LiteralPath $file -Raw -Encoding utf8) -split "`r?`n"

        $currentTable = $null
        $mode = 'none'
        $headerIndex = $null
        $inSqlFence = $false
        $sqlBuffer = [System.Text.StringBuilder]::new()

        foreach ($line in $lines) {
            if ($mode -eq 'ddl' -and $line.TrimStart().StartsWith('```')) {
                $inSqlFence = -not $inSqlFence
                if (-not $inSqlFence -and $sqlBuffer.Length -gt 0) {
                    Add-DdlBlock -Sql $sqlBuffer.ToString() -Tables $tables -SourceFile $fileName
                    [void]$sqlBuffer.Clear()
                }
                continue
            }
            if ($inSqlFence) {
                [void]$sqlBuffer.AppendLine($line)
                continue
            }

            $parentMatch = [regex]::Match($line, '^##\s+[\d.]+\.?\s+([A-Za-z_]\w*)\s*[（(]')
            if ($parentMatch.Success) {
                $currentTable = $parentMatch.Groups[1].Value
                if (-not $tables.ContainsKey($currentTable)) {
                    $tables[$currentTable] = New-TableEntry -Name $currentTable -SourceFile $fileName
                }
                $mode = 'none'
                continue
            }

            $childMatch = [regex]::Match($line, '^###\s+[\d.]+\s+子テーブル[:：]\s*([A-Za-z_]\w*)')
            if ($childMatch.Success) {
                $currentTable = $childMatch.Groups[1].Value
                if (-not $tables.ContainsKey($currentTable)) {
                    $tables[$currentTable] = New-TableEntry -Name $currentTable -SourceFile $fileName
                }
                # 子テーブルは見出し直後の表がカラム定義であり、専用の見出しを持たない。
                $mode = 'cols'
                $headerIndex = $null
                continue
            }

            if ($line -match '^###\s+[\d.]+\s+カラム定義') { $mode = 'cols'; $headerIndex = $null; continue }
            if ($line -match '^###\s+[\d.]+\s+DDL') { $mode = 'ddl'; continue }
            if ($line -match '^#{2,4}\s') { $mode = 'none'; continue }

            if ($mode -eq 'cols' -and $line.StartsWith('|') -and $null -ne $currentTable) {
                $cells = $line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
                if ($null -eq $headerIndex) {
                    if ($cells -contains 'カラム名') {
                        $headerIndex = @{
                            Name     = [array]::IndexOf($cells, 'カラム名')
                            Type     = [array]::IndexOf($cells, 'データ型')
                            Nullable = if ($cells -contains 'NULL') { [array]::IndexOf($cells, 'NULL') } else { $null }
                        }
                    }
                    continue
                }

                $column = Read-ColumnDefinitionRow -Line $line -HeaderIndex $headerIndex
                if ($null -ne $column) { $tables[$currentTable].DocColumns += $column }
            }
        }
    }

    return $tables
}

function Measure-RawCreateTableCount {
    <#
    .SYNOPSIS
    ファイル中の CREATE TABLE の出現回数を、構文を解釈せず数える。

    .DESCRIPTION
    Get-TableDefinitionDocument が抽出できた件数と突き合わせることで、
    記法が揃わず解析対象から漏れたブロックの存在を検出する。
    解析器は漏れたブロックを「無かったこと」として扱うため、
    この数え上げが無いと欠落に気付けない。
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string[]]$Path)

    $count = 0
    foreach ($file in $Path) {
        $content = Get-Content -LiteralPath $file -Raw -Encoding utf8
        $count += [regex]::Matches($content, 'CREATE\s+TABLE\s+\[?\w+\]?\.\[?\w+\]?', 'IgnoreCase').Count
    }

    return $count
}

Export-ModuleMember -Function Get-TableDefinitionDocument, ConvertTo-NormalizedSqlType, Measure-RawCreateTableCount
