[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ResultsDirectory,
    [ValidateRange(0, 100)]
    [double] $Minimum = 80
)

$ErrorActionPreference = 'Stop'
$expectedModules = @('Tree CRUD', 'Database Overwrite Program')
# VSTest also copies attachments below its In/ directory. Read each original once.
$reports = @(Get-ChildItem -LiteralPath $ResultsDirectory -Directory | ForEach-Object {
    Get-ChildItem -LiteralPath $_.FullName -Filter coverage.opencover.xml -File
})
if ($reports.Count -eq 0) { throw 'No OpenCover reports were produced.' }

$measurements = @{}
foreach ($report in $reports) {
    [xml] $coverage = Get-Content -LiteralPath $report.FullName -Raw
    foreach ($module in $coverage.CoverageSession.Modules.Module) {
        $name = [string] $module.ModuleName
        if ($name -notin $expectedModules) { continue }
        if ($measurements.ContainsKey($name)) { throw "Duplicate coverage report for $name." }

        $points = @($module.SelectNodes('.//SequencePoint'))
        $lines = @($points | Group-Object { '{0}:{1}' -f $_.fileid, $_.sl })
        $coveredLines = @($lines | Where-Object { @($_.Group | Where-Object { [int] $_.vc -gt 0 }).Count -gt 0 }).Count
        $branches = @($module.SelectNodes('.//BranchPoint'))
        $coveredBranches = @($branches | Where-Object { [int] $_.vc -gt 0 }).Count
        if ($lines.Count -eq 0) { throw "No executable lines were measured for $name." }
        $lineCoverage = 100.0 * $coveredLines / $lines.Count
        $branchCoverage = if ($branches.Count -eq 0) { 100.0 } else { 100.0 * $coveredBranches / $branches.Count }

        $measurements[$name] = [pscustomobject] @{
            Module = $name
            CoveredLines = $coveredLines
            Lines = $lines.Count
            LineCoverage = [Math]::Round($lineCoverage, 2)
            CoveredBranches = $coveredBranches
            Branches = $branches.Count
            BranchCoverage = [Math]::Round($branchCoverage, 2)
        }
        if ($lineCoverage -lt $Minimum -or $branchCoverage -lt $Minimum) {
            throw ('{0}: line coverage {1:F2}%, branch coverage {2:F2}%; each must reach {3}%.' -f $name, $lineCoverage, $branchCoverage, $Minimum)
        }
    }
}

foreach ($name in $expectedModules) {
    if (-not $measurements.ContainsKey($name)) { throw "The coverage report for $name is missing." }
    $measurements[$name]
}
