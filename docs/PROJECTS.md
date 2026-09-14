# Project guide

This repository is a portfolio collection of three Windows utilities. They share an integration-oriented theme but are separate applications with different runtime and deployment requirements.

## Components

| Component | Role | Runtime and prerequisites |
| --- | --- | --- |
| `Database Overwrite Program` | WinForms reference interface for starting a configured SQL Server Agent job through PowerShell. | .NET 9 for Windows, PowerShell, the `SqlServer` PowerShell module, and an authorized SQL Server environment. |
| `Program For Sending Documents By Email` | ERP extension that adds a confirmation-sending action and opens a parameter form. | .NET Framework 4.8, MailKit and the private `Hydra` and `cdn_api` ERP libraries. |
| `Tree CRUD` | WinForms hierarchy editor backed by the `CDN.DefAtrElem` SQL Server table. | .NET Framework 4.8, SQL Server, and the appropriate application connection string and schema. |

## Safe evaluation

The applications are source-level examples, not a public deployment package. `Database Overwrite Program` contains illustrative credential and SQL-connection placeholders. Treat every other configuration file and environment-specific value as sensitive: review it locally, do not copy it into new source files or issues, and use only an authorized non-production environment. The ERP-related project cannot be built or executed without the proprietary dependencies and target-system configuration.

## Suggested workflow

1. Open `applications-and-data-integration.sln` in Visual Studio 2022 on Windows.
2. Select one component and install its required SDK, framework, and packages.
3. Review configuration, database access, and external dependencies before starting the program.
4. Use a disposable test database and test mailbox when adapting integration logic; never trigger an unknown SQL Server Agent job or send mail to production recipients.
