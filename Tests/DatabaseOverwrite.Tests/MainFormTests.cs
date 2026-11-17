using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using IntegrationTests;
using PowerShellRunner;
using Xunit;

namespace DatabaseOverwrite.Tests
{
    public sealed class MainFormTests
    {
        private static readonly string[] ExpectedCommandArguments = { "test user", "test password", "test server", "test job" };

        [Theory]
        [InlineData(null, "password")]
        [InlineData(" ", "password")]
        [InlineData("username", null)]
        [InlineData("username", " ")]
        public void MissingCredentialsDoNotRunPowerShell(string username, string password)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.Username.Text = username;
                    fixture.Password.Text = password;
                    fixture.Click();
                    Assert.Equal(0, fixture.Calls);
                    Assert.Contains("nazwę użytkownika", Assert.Single(fixture.Messages));
                }
            });
        }

        [Theory]
        [InlineData(null, "job")]
        [InlineData(" ", "job")]
        [InlineData("server", null)]
        [InlineData("server", " ")]
        public void MissingConfigurationDoesNotRunPowerShell(string server, string job)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.Environment["SQLSERVER_INSTANCE"] = server;
                    fixture.Environment["SQLSERVER_JOB"] = job;
                    fixture.Click();
                    Assert.Equal(0, fixture.Calls);
                    Assert.Contains("SQLSERVER_INSTANCE", Assert.Single(fixture.Messages));
                }
            });
        }

        [Theory]
        [InlineData("job started")]
        [InlineData("")]
        [InlineData(" ")]
        public void SuccessfulRunPassesConfigurationAndClearsPassword(string output)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.Output = output;
                    fixture.Click();
                    Assert.Equal(1, fixture.Calls);
                    Assert.Equal(ExpectedCommandArguments, fixture.Arguments.Skip(1));
                    Assert.Contains("sp_start_job", fixture.Arguments[0]);
                    Assert.Contains("$env:SQLSERVER_JOB.Replace", fixture.Arguments[0]);
                    Assert.Empty(fixture.Password.Text);
                    if (string.IsNullOrWhiteSpace(output)) Assert.Empty(fixture.Messages);
                    else Assert.Equal(output, Assert.Single(fixture.Messages));
                }
            });
        }

        [Fact]
        public void FailedRunReportsErrorAndClearsPassword()
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.Error = new InvalidOperationException("expected failure");
                    fixture.Click();
                    Assert.Contains("expected failure", Assert.Single(fixture.Messages));
                    Assert.Empty(fixture.Password.Text);
                    Assert.Equal(1, fixture.Calls);
                }
            });
        }

        [Fact]
        public void ProductionFormCanBeConstructedWithoutRunningAJob()
        {
            WinFormsTest.Run(() =>
            {
                using (var form = new MainForm())
                {
                    Assert.Equal("Nazwa skryptu", form.Text);
                    Assert.Empty(WinFormsTest.Field<TextBox>(form, "txtPassword").Text);
                }
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void PowerShellSearchUsesFirstExistingCandidate(int existingIndex)
        {
            var candidates = new List<string>();
            string executable = MainForm.FindPowerShellExecutable(path =>
            {
                candidates.Add(path);
                return candidates.Count == existingIndex + 1;
            });
            Assert.Equal(existingIndex + 1, candidates.Count);
            Assert.Equal(candidates.Last(), executable);
            Assert.EndsWith(existingIndex == 2 ? "powershell.exe" : "pwsh.exe", executable);
        }

        [Fact]
        public void MissingPowerShellGivesActionableError()
        {
            var error = Assert.Throws<FileNotFoundException>(() => MainForm.FindPowerShellExecutable(path => false));
            Assert.Contains("PowerShell", error.Message);
        }

        [Fact]
        public void CommandPassesLiteralEnvironmentValuesWithoutChangingParentEnvironment()
        {
            string parentUsername = System.Environment.GetEnvironmentVariable("SQLSERVER_USERNAME");
            string username = "test'; Write-Output injected; #";
            string output = MainForm.ExecuteCommand(
                "Write-Output ($env:SQLSERVER_USERNAME + '|' + $env:SQLSERVER_PASSWORD + '|' + $env:SQLSERVER_INSTANCE + '|' + $env:SQLSERVER_JOB)",
                username, "dummy-password", "dummy-server", "dummy-job");
            Assert.Equal(username + "|dummy-password|dummy-server|dummy-job", output);
            Assert.Equal(parentUsername, System.Environment.GetEnvironmentVariable("SQLSERVER_USERNAME"));
        }

        [Theory]
        [InlineData("[Console]::Error.WriteLine('expected failure'); exit 7", "expected failure")]
        [InlineData("exit 1", "PowerShell reported an error.")]
        public void NonzeroPowerShellExitIsAnError(string script, string expected)
        {
            var error = Assert.Throws<InvalidOperationException>(() => MainForm.ExecuteCommand(script, "test", "test", "test", "test"));
            Assert.Contains(expected, error.Message);
        }

        [Fact]
        public void CommandDrainsBothStreamsWithoutDeadlock()
        {
            string output = MainForm.ExecuteCommand(
                "[Console]::Error.WriteLine(('e' * 100000)); [Console]::Out.WriteLine(('o' * 100000))",
                "test", "test", "test", "test");
            Assert.Equal(new string('o', 100000), output);
        }

        private sealed class Fixture : IDisposable
        {
            internal MainForm Form { get; }
            internal Dictionary<string, string> Environment { get; } = new Dictionary<string, string>
            {
                ["SQLSERVER_INSTANCE"] = "test server", ["SQLSERVER_JOB"] = "test job"
            };
            internal List<string> Messages { get; } = new List<string>();
            internal int Calls { get; private set; }
            internal string[] Arguments { get; private set; }
            internal string Output { get; set; } = "success";
            internal Exception Error { get; set; }
            internal TextBox Username => WinFormsTest.Field<TextBox>(Form, "txtUsername");
            internal TextBox Password => WinFormsTest.Field<TextBox>(Form, "txtPassword");

            internal Fixture()
            {
                Form = new MainForm((script, username, password, server, job) =>
                {
                    Calls++;
                    Arguments = new[] { script, username, password, server, job };
                    if (Error != null) throw Error;
                    return Output;
                }, (message, caption, buttons, icon) =>
                {
                    Messages.Add(message);
                    return DialogResult.OK;
                }, name => Environment[name]);
                Username.Text = "test user";
                Password.Text = "test password";
            }

            internal void Click() => WinFormsTest.Invoke(Form, "BtnRunScript_Click", Form, EventArgs.Empty);
            public void Dispose() { Form.Dispose(); }
        }
    }
}
