# Application and Data Integration Utilities

Windows utilities and prototypes for ERP, SQL Server, document, and email workflows. This repository is a portfolio collection rather than a single downloadable product.

[Source code](https://github.com/haribo841/applications-and-data-integration) | [Project guide](docs/PROJECTS.md) | [Report an issue](https://github.com/haribo841/applications-and-data-integration/issues)

## Included utilities

| Project | What it demonstrates | Status |
| --- | --- | --- |
| `Database Overwrite Program` | Starting a configured SQL Server Agent job from a WinForms interface through PowerShell. | Reference implementation; requires a configured SQL Server environment. |
| `Program For Sending Documents By Email` | An ERP extension for initiating confirmation email workflows from an application window. | Requires private ERP libraries and target-system configuration. |
| `Tree CRUD` | Editing a hierarchical list stored in a SQL Server table. | Requires an appropriate database schema and connection string. |

## Quick start

There is no public binary release because each utility depends on an organization-specific environment.

1. Clone the repository.

   ```powershell
   git clone https://github.com/haribo841/applications-and-data-integration.git
   ```

2. On Windows, open `applications-and-data-integration.sln` in Visual Studio 2022.
3. Choose one project and install its prerequisites from the [project guide](docs/PROJECTS.md).
4. Configure only an authorized test environment before running any database or ERP integration code.

## Supported environment

| Environment | Support |
| --- | --- |
| Windows 10/11 | Source projects use WinForms and are intended for Windows. |
| .NET 9 for Windows | Required by `Database Overwrite Program`. |
| .NET Framework 4.8 | Required by the email and Tree CRUD utilities. |
| SQL Server and ERP environment | Required only when evaluating the relevant integration utility. |

## Key capabilities

- WinForms interfaces for integration-oriented workflows.
- SQL Server access and hierarchical CRUD operations.
- PowerShell-driven SQL Server Agent job invocation.
- An ERP-window extension for confirmation email workflows.
- Explicit separation of components with different runtime requirements.

## Documentation

See the [project guide](docs/PROJECTS.md) for a component map, dependencies, and safe evaluation boundaries.

## License and issues

No general redistribution license is currently published. The repository refers to environment-specific ERP libraries and database schemas, so it must not be used against a production environment without authorization and a configuration review.

For questions or reproducible issues, use [GitHub Issues](https://github.com/haribo841/applications-and-data-integration/issues).

The previous general description is preserved in [the README archive](docs/archive/README-2026-09-06.md).
