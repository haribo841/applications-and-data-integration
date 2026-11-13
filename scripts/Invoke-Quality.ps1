[CmdletBinding()]
param([switch] $Analyze)

$ErrorActionPreference = 'Stop'

function Invoke-DotNet {
    param([string[]] $Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($Arguments[0]) failed with exit code $LASTEXITCODE." }
}

$repository = Split-Path -Parent $PSScriptRoot
Push-Location $repository
try {
    if ($Analyze -and [string]::IsNullOrWhiteSpace($env:SONAR_TOKEN)) {
        throw 'Set SONAR_TOKEN before running with -Analyze.'
    }

    # A fresh directory prevents stale reports from being imported on later runs.
    $results = 'TestResults/' + [Guid]::NewGuid().ToString('N')
    Invoke-DotNet -Arguments @('restore', 'quality.sln')
    if ($Analyze) {
        Invoke-DotNet -Arguments @('tool', 'restore')
        Invoke-DotNet -Arguments @(
            'sonarscanner', 'begin',
            '/k:haribo841_applications-and-data-integration',
            '/o:haribo841',
            '/d:sonar.host.url=https://sonarcloud.io',
            "/d:sonar.token=$env:SONAR_TOKEN",
            "/d:sonar.cs.opencover.reportsPaths=$results/*/coverage.opencover.xml",
            "/d:sonar.cs.vstest.reportsPaths=$results/*.trx",
            '/d:sonar.scanner.scanAll=false',
            '/d:sonar.qualitygate.wait=true'
        )
    }

    Invoke-DotNet -Arguments @(
        'build', 'quality.sln', '--configuration', 'Release', '--no-restore', '--no-incremental',
        '-p:DebugType=portable', '-p:GenerateResourceMSBuildArchitecture=CurrentArchitecture'
    )
    Invoke-DotNet -Arguments @(
        'test', 'quality.sln', '--configuration', 'Release', '--no-build', '--no-restore',
        '--collect', 'XPlat Code Coverage', '--settings', 'Tests/coverage.runsettings',
        '--results-directory', $results, '--logger', 'trx;LogFilePrefix=tests'
    )
    & (Join-Path $PSScriptRoot 'Test-Coverage.ps1') -ResultsDirectory $results

    if ($Analyze) {
        Invoke-DotNet -Arguments @('sonarscanner', 'end', "/d:sonar.token=$env:SONAR_TOKEN")
    }
    Write-Host "Reports: $results"
}
finally {
    Pop-Location
}
