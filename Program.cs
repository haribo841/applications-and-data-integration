using Humanizer;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Management.Automation;
using System.Reflection.Emit;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;

#pragma warning disable IDE0130 // Przestrzeń nazw jest zgodna ze strukturą folderów
namespace PowerShellRunner
#pragma warning restore IDE0130 // Przestrzeń nazw jest zgodna ze strukturą folderów
{
    public class MainForm : Form
    {
        private readonly TextBox txtUsername;
        private readonly TextBox txtPassword;
        private readonly Button btnRunScript;

        public MainForm()
        {
            Text = "Nazwa skryptu";
            Size = new Size(400, 200);
            Environment.SetEnvironmentVariable("Path", @"C:\Program Files\PowerShell\7;" + Environment.GetEnvironmentVariable("Path"));
            var lblUsername = new Label() { Text = "Użytkownik:", Left = 10, Top = 20, Width = 80 };
            txtUsername = new TextBox() { Left = 100, Top = 20, Width = 250 };

            var lblPassword = new Label() { Text = "Hasło:", Left = 10, Top = 60, Width = 80 };
            txtPassword = new TextBox() { Left = 100, Top = 60, Width = 250, PasswordChar = '*' };

            btnRunScript = new Button() { Text = "Nadpisz Bazę Testową", Left = 100, Top = 100, Width = 200 };
#pragma warning disable CS8622 // Dopuszczanie wartości null dla typów referencyjnych w typie parametru nie jest zgodne z docelowym delegatem (prawdopodobnie z powodu atrybutów dopuszczania wartości null).
            btnRunScript.Click += BtnRunScript_Click;
#pragma warning restore CS8622 // Dopuszczanie wartości null dla typów referencyjnych w typie parametru nie jest zgodne z docelowym delegatem (prawdopodobnie z powodu atrybutów dopuszczania wartości null).

            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnRunScript);
        }

        public static void ExecuteCommand(string command)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
        }
        private void BtnRunScript_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Podaj zarówno nazwę użytkownika, jak i hasło.");
                return;
            }
            else if (username == "Login_usera" && password == "haslo_usera")
            {
                try
                {
                    string command = @"
# Sprawdzenie i instalacja modułu SqlServer
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
    Write-Host 'SqlServer module not found. Installing...'
    Install-Module -Name SqlServer -Scope CurrentUser -Force
}
Import-Module SqlServer

# Parametry połączenia
$sqlServerInstance = 'Nazwa_serera_SQL'
$sqlJobName = 'Nazwa_triggera'

# Bezpośrednie uwierzytelnienie SQL
$connectionString = 'Server=TF-SQL;User ID=ID_usera_do_bazy;Password=Haslo_usera_do_bazy;'

# Test połączenia
try {
    $testConnection = Invoke-SqlCmd -Query 'SELECT 1 AS ConnectionTest' -ConnectionString $connectionString -ErrorAction Stop
    if ($testConnection.ConnectionTest -eq 1) {
        Write-Host 'Connection to SQL Server successful.' -ForegroundColor Green
    }
} catch {
    Write-Host 'Failed to connect to SQL Server. Please check your credentials and try again.' -ForegroundColor Red
    exit
}

# Uruchomienie zadania SQL
try {
    $query = 'EXEC msdb.dbo.sp_start_job @job_name = N''Nazwa_triggera'';'
    Invoke-SqlCmd -Query $query -ConnectionString $connectionString -ErrorAction Stop
    Write-Host 'Zadanie SQL zostało uruchomione pomyślnie.' -ForegroundColor Green
} catch {
    Write-Host 'Failed to start SQL job. Error: $_' -ForegroundColor Red
}

# Czyszczenie pamięci
[System.GC]::Collect()
";

                    ExecuteCommand(command);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Nieprawidłowa nazwa użytkownika lub hasło.");
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}