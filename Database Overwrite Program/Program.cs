using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;

[assembly: InternalsVisibleTo("DatabaseOverwrite.Tests")]

#pragma warning disable IDE0130 // Przestrzeń nazw jest zgodna ze strukturą folderów
namespace PowerShellRunner
#pragma warning restore IDE0130 // Przestrzeń nazw jest zgodna ze strukturą folderów
{
    public class MainForm : Form
    {
        private readonly TextBox txtUsername;
        private readonly TextBox txtPassword;
        private readonly Func<string, string, string, string, string, string> executeCommand;
        private readonly Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> showMessage;
        private readonly Func<string, string?> getEnvironmentVariable;

        public MainForm() : this(ExecuteCommand, MessageBox.Show, Environment.GetEnvironmentVariable)
        {
        }

        internal MainForm(
            Func<string, string, string, string, string, string> executeCommand,
            Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> showMessage,
            Func<string, string?> getEnvironmentVariable)
        {
            this.executeCommand = executeCommand;
            this.showMessage = showMessage;
            this.getEnvironmentVariable = getEnvironmentVariable;
            Text = "Nazwa skryptu";
            Size = new System.Drawing.Size(400, 200);

            System.Windows.Forms.Label lblUsername = new() { Text = "Użytkownik:", Left = 10, Top = 20, Width = 80 };
            txtUsername = new TextBox { Left = 100, Top = 20, Width = 250 };
            System.Windows.Forms.Label lblPassword = new() { Text = "Hasło:", Left = 10, Top = 60, Width = 80 };
            txtPassword = new TextBox { Left = 100, Top = 60, Width = 250, PasswordChar = '*' };
            Button btnRunScript = new() { Text = "Nadpisz Bazę Danych", Left = 100, Top = 100, Width = 200 };
#pragma warning disable CS8622 // Dopuszczanie wartości null dla typów referencyjnych w typie parametru nie jest zgodne z docelowym delegatem (prawdopodobnie z powodu atrybutów dopuszczania wartości null).
            btnRunScript.Click += BtnRunScript_Click;
#pragma warning restore CS8622 // Dopuszczanie wartości null dla typów referencyjnych w typie parametru nie jest zgodne z docelowym delegatem (prawdopodobnie z powodu atrybutów dopuszczania wartości null).
            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnRunScript);
        }

        internal static string ExecuteCommand(string script, string username, string password, string server, string jobName)
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName = FindPowerShellExecutable(),
                Arguments = "-NoProfile -NonInteractive -Command -",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            processStartInfo.Environment["SQLSERVER_USERNAME"] = username;
            processStartInfo.Environment["SQLSERVER_PASSWORD"] = password;
            processStartInfo.Environment["SQLSERVER_INSTANCE"] = server;
            processStartInfo.Environment["SQLSERVER_JOB"] = jobName;

            using Process process = new() { StartInfo = processStartInfo };
            if (!process.Start())
            {
                throw new InvalidOperationException("PowerShell could not be started.");
            }

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> errorTask = process.StandardError.ReadToEndAsync();
            process.StandardInput.WriteLine(script);
            process.StandardInput.Close();
            Task.WaitAll(outputTask, errorTask);
            process.WaitForExit();

            string output = outputTask.Result.Trim();
            string error = errorTask.Result.Trim();
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? "PowerShell reported an error." : error);
            }

            return output;
        }

        internal static string FindPowerShellExecutable(Func<string, bool>? fileExists = null)
        {
            string[] candidates =
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerShell", "7", "pwsh.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "PowerShell", "7", "pwsh.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "WindowsPowerShell", "v1.0", "powershell.exe")
            };

            return candidates.FirstOrDefault(fileExists ?? File.Exists)
                ?? throw new FileNotFoundException("PowerShell 7 or Windows PowerShell could not be found.");
        }

        private void BtnRunScript_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                showMessage("Podaj zarówno nazwę użytkownika, jak i hasło.", "", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }

            string server = getEnvironmentVariable("SQLSERVER_INSTANCE") ?? string.Empty;
            string jobName = getEnvironmentVariable("SQLSERVER_JOB") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(jobName))
            {
                showMessage("Ustaw zmienne środowiskowe SQLSERVER_INSTANCE i SQLSERVER_JOB.", "", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }

            const string script = @"
$ErrorActionPreference = 'Stop'
try {
    if (-not (Get-Module -ListAvailable -Name SqlServer)) {
        Install-Module -Name SqlServer -Scope CurrentUser -Force -ErrorAction Stop
    }
    Import-Module SqlServer -ErrorAction Stop

    Invoke-SqlCmd -Query 'SELECT 1 AS ConnectionTest' `
        -ServerInstance $env:SQLSERVER_INSTANCE `
        -Username $env:SQLSERVER_USERNAME `
        -Password $env:SQLSERVER_PASSWORD `
        -ErrorAction Stop | Out-Null

    $escapedJobName = $env:SQLSERVER_JOB.Replace(""'"", ""''"")
    $query = ""EXEC msdb.dbo.sp_start_job @job_name = N'$escapedJobName';""
    Invoke-SqlCmd -Query $query `
        -ServerInstance $env:SQLSERVER_INSTANCE `
        -Username $env:SQLSERVER_USERNAME `
        -Password $env:SQLSERVER_PASSWORD `
        -ErrorAction Stop | Out-Null

    Write-Output 'Zadanie SQL zostało uruchomione pomyślnie.'
} catch {
    Write-Error $_
    exit 1
}
";

            try
            {
                string output = executeCommand(script, username, password, server, jobName);
                if (!string.IsNullOrWhiteSpace(output))
                {
                    showMessage(output, "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                showMessage($"Błąd: {ex.Message}", "", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            finally
            {
                txtPassword.Clear();
            }
        }

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}
