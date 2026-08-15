$ErrorActionPreference = 'Stop'

function Get-TableCreationOrder {
    [CmdletBinding()]
    param([Parameter(Mandatory)][hashtable]$Document)

    # 名前順を起点にすることで、依存関係が同じなら常に同じ順序を出す。
    # 生成物の差分をレビューできる状態に保つため、順序は決定的でなければならない。
    $names = $Document.Keys | Sort-Object

    $dependencies = @{}
    foreach ($name in $names) {
        $parents = @()
        foreach ($parent in ($Document[$name].ForeignKeys | Sort-Object -Unique)) {
            # 自己参照はテーブル作成順に影響しない。
            if ($parent -eq $name) { continue }
            if (-not $Document.ContainsKey($parent)) {
                throw "テーブル '$name' が定義の無いテーブル '$parent' を参照している"
            }
            $parents += $parent
        }
        $dependencies[$name] = $parents
    }

    $order = [System.Collections.Generic.List[string]]::new()
    $state = @{}

    function Visit {
        param([string]$Node, [System.Collections.Generic.List[string]]$Stack)

        if ($state[$Node] -eq 'done') { return }
        if ($state[$Node] -eq 'visiting') {
            $cycle = ($Stack[$Stack.IndexOf($Node)..($Stack.Count - 1)] + $Node) -join ' -> '
            throw "外部キーに循環参照がある: $cycle"
        }

        $state[$Node] = 'visiting'
        $Stack.Add($Node)
        foreach ($parent in $dependencies[$Node]) { Visit -Node $parent -Stack $Stack }
        $Stack.RemoveAt($Stack.Count - 1)
        $state[$Node] = 'done'
        $order.Add($Node)
    }

    foreach ($name in $names) {
        Visit -Node $name -Stack ([System.Collections.Generic.List[string]]::new())
    }

    return , $order.ToArray()
}

Export-ModuleMember -Function Get-TableCreationOrder
