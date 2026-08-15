$ErrorActionPreference = 'Stop'

function New-Violation {
    param([string]$Category, [string]$Table, [string]$Column, [string]$Message)

    return [PSCustomObject]@{
        Category = $Category
        Table    = $Table
        Column   = $Column
        Message  = $Message
    }
}

function Test-TableDefinitionConsistency {
    [CmdletBinding()]
    param([Parameter(Mandatory)][hashtable]$Document)

    $violations = @()

    foreach ($name in ($Document.Keys | Sort-Object)) {
        $table = $Document[$name]

        if ($table.DdlDefinitionCount -gt 1) {
            $violations += New-Violation -Category 'DuplicateDefinition' -Table $name -Column '' `
                -Message "CREATE TABLE が $($table.DdlDefinitionCount) 回定義されている ($($table.SourceFile))"
        }

        if ($table.DdlDefinitionCount -eq 0) {
            $violations += New-Violation -Category 'MissingDdl' -Table $name -Column '' `
                -Message "カラム定義表のみで DDL ブロックが無い ($($table.SourceFile))"
            continue
        }

        if ($table.DocColumns.Count -eq 0) {
            $violations += New-Violation -Category 'MissingColumnTable' -Table $name -Column '' `
                -Message "DDL ブロックのみでカラム定義表が無い ($($table.SourceFile))"
            continue
        }

        # 以降の突合はカラム名をキーにするため、重複はハッシュテーブル化の時点で潰れて現れない。
        # SQL Server は同名の列を許さず CREATE TABLE が失敗するため、突合の前に検出する。
        foreach ($source in @(
            @{ Label = 'DDL ブロック'; Columns = $table.DdlColumns },
            @{ Label = 'カラム定義表'; Columns = $table.DocColumns }
        )) {
            foreach ($group in ($source.Columns | Group-Object Name | Where-Object { $_.Count -gt 1 })) {
                $violations += New-Violation -Category 'DuplicateColumn' -Table $name -Column $group.Name `
                    -Message "$($source.Label)で同じ列が $($group.Count) 回定義されている"
            }
        }

        $docMap = @{}
        foreach ($column in $table.DocColumns) { $docMap[$column.Name] = $column }
        $ddlMap = @{}
        foreach ($column in $table.DdlColumns) { $ddlMap[$column.Name] = $column }

        foreach ($columnName in ($docMap.Keys | Sort-Object)) {
            if (-not $ddlMap.ContainsKey($columnName)) {
                $violations += New-Violation -Category 'ColumnMismatch' -Table $name -Column $columnName `
                    -Message 'カラム定義表のみに存在する'
            }
        }

        foreach ($columnName in ($ddlMap.Keys | Sort-Object)) {
            if (-not $docMap.ContainsKey($columnName)) {
                $violations += New-Violation -Category 'ColumnMismatch' -Table $name -Column $columnName `
                    -Message 'DDL ブロックのみに存在する'
            }
        }

        foreach ($columnName in ($docMap.Keys | Sort-Object)) {
            if (-not $ddlMap.ContainsKey($columnName)) { continue }

            $doc = $docMap[$columnName]
            $ddl = $ddlMap[$columnName]

            if ($doc.Type -ne $ddl.Type) {
                $violations += New-Violation -Category 'TypeMismatch' -Table $name -Column $columnName `
                    -Message "型が食い違う 表=$($doc.Type) DDL=$($ddl.Type)"
            }

            # 表側が空欄の行は判定材料が無いため、NULL 許容の比較対象から外す。
            if ($doc.Nullable -and $ddl.Nullable -and $doc.Nullable -ne $ddl.Nullable) {
                $violations += New-Violation -Category 'NullabilityMismatch' -Table $name -Column $columnName `
                    -Message "NULL 許容が食い違う 表=$($doc.Nullable) DDL=$($ddl.Nullable)"
            }
        }
    }

    return , $violations
}

function Get-UnparsedDdlViolation {
    <#
    .SYNOPSIS
    記法が揃わず解析から漏れた CREATE TABLE ブロックを検出する。

    .DESCRIPTION
    Test-TableDefinitionConsistency は解析結果だけを見るため、
    解析器が拾えなかったブロックは「存在しない」ものとして扱われ、検出できない。
    ファイル全体の出現回数と解析できた件数を突き合わせて、その漏れを見つける。
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string[]]$Path,
        [Parameter(Mandatory)][hashtable]$Document
    )

    $rawCount = Measure-RawCreateTableCount -Path $Path
    $parsedCount = ($Document.Values | Measure-Object -Property DdlDefinitionCount -Sum).Sum

    # Test-TableDefinitionConsistency と同じく、単項カンマで配列そのものを 1 個の値として返す。
    # 返り値を @() で包み直すと配列が入れ子になり、明細が 1 要素に潰れる。
    if ($rawCount -eq $parsedCount) { return , @() }

    return , @(New-Violation -Category 'UnparsedDdl' -Table '' -Column '' `
        -Message "CREATE TABLE の出現 $rawCount 件に対し解析できたのは $parsedCount 件。閉じ括弧とセミコロンが独立行にない等、記法が揃わないブロックがある")
}

Export-ModuleMember -Function Test-TableDefinitionConsistency, Get-UnparsedDdlViolation
