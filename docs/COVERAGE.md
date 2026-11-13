# Coverage and SonarCloud

Run the quality checks on Windows with the .NET 9 SDK and the .NET Framework 4.8 developer pack:

```powershell
./scripts/Invoke-Quality.ps1
```

The script builds `quality.sln`, runs the .NET Framework and .NET 9 test projects, and creates OpenCover, Cobertura, and TRX reports in a fresh directory under `TestResults`. It requires both applications to reach at least 80% line coverage and 80% branch coverage. Tests reference the application projects, so coverage measures the actual application assemblies.

`quality.sln` contains Tree CRUD, Database Overwrite Program, and their tests. The ERP email extension still requires its private SDK and is outside this quality solution, matching the previously selected partial analysis scope. No SDK binaries are needed for these checks.

Tree CRUD tests use an in-memory ADO.NET substitute and capture prompt and message responses. They verify hierarchy construction, SQL parameters, validation, transaction outcomes, deletion, and error handling. Database Overwrite tests use injected execution and configuration when testing form behavior. Process tests execute only harmless PowerShell scripts that echo test values or return controlled errors. They do not connect to SQL Server or start SQL Agent jobs.

To submit the reports to SonarCloud, set `SONAR_TOKEN` in the process environment and run:

```powershell
./scripts/Invoke-Quality.ps1 -Analyze
```

The `sonarcloud-coverage.yml` workflow runs this command on pushes to `master` and on manual dispatch. The repository must have a valid `SONAR_TOKEN` Actions secret. Automatic Analysis in SonarCloud must be disabled before using the CI scanner. The scanner imports OpenCover reports with `sonar.cs.opencover.reportsPaths` and waits for the project's Quality Gate, including its 80% coverage requirement for new code. It does not change the gate or the new-code baseline.

SonarCloud needs a completed scan of the published revision before its dashboard can confirm the new coverage. Local coverage reports alone do not update the remote project.
