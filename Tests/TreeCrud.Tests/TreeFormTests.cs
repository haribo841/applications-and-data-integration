using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using IntegrationTests;
using TreeCRUD;
using Xunit;

namespace TreeCrud.Tests
{
    public sealed class TreeFormTests
    {
        [Fact]
        public void ProductionConstructorDoesNotConnectBeforeTheFormLoads()
        {
            WinFormsTest.Run(() =>
            {
                using (var form = new Form1())
                {
                    Assert.Equal("Tree CRUD", form.Text);
                }
            });
        }

        [Fact]
        public void LoadsSharedAncestorsAndIgnoresPathsDeeperThanFourLevels()
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.Database.Rows.Clear();
                    fixture.Database.Add(2, "Root", "Root|Branch", "Root|Branch|Twig", "Root|Branch|Twig|Leaf", "Root|Branch|Twig|Leaf", "Other", "Bad|1|2|3|4");
                    fixture.Database.Add(3, "Leaf");
                    fixture.Load();
                    Assert.Equal(new[] { "Root", "Other", "Leaf" }, fixture.Items(1));
                    fixture.Select(1, "Root");
                    Assert.Equal(new[] { "Branch" }, fixture.Items(2));
                    fixture.Select(2, "Branch");
                    Assert.Equal(new[] { "Twig" }, fixture.Items(3));
                    var hierarchy = WinFormsTest.Field<Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>>(fixture.Form, "_subItems");
                    Assert.Equal(new[] { "Leaf" }, hierarchy["Root"]["Branch"]["Twig"]);
                    fixture.Select(1, "Other");
                    Assert.Empty(fixture.Items(2));
                }
            });
        }

        [Theory]
        [InlineData("First", 2)]
        [InlineData("Second", 1)]
        [InlineData("Third", 3)]
        [InlineData("Fourth", 55)]
        public void AddUsesParameterizedCommand(string level, int attributeId)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    fixture.Answer = "Added'Name";
                    fixture.Click("Add", level);
                    var command = Assert.Single(fixture.Database.Commands);
                    Assert.Contains("INSERT INTO CDN.DefAtrElem", command.Sql);
                    Assert.Equal(attributeId, command.Parameters["@deAId"]);
                    Assert.Equal(level == "Second" ? "Root|Added'Name" : "Added'Name", command.Parameters["@value"]);
                    Assert.DoesNotContain("Added'Name", command.Sql);
                }
            });
        }

        [Theory]
        [InlineData("First")]
        [InlineData("Second")]
        [InlineData("Third")]
        [InlineData("Fourth")]
        public void CancelledAddDoesNotWrite(string level)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    fixture.Answer = " ";
                    fixture.Click("Add", level);
                    Assert.Empty(fixture.Database.Commands);
                }
            });
        }

        [Theory]
        [InlineData("First", "Root")]
        [InlineData("Second", "Branch")]
        [InlineData("Third", "Twig")]
        [InlineData("Fourth", "Leaf")]
        public void EditRejectsCancelledUnchangedAndInvalidNames(string level, string current)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    foreach (var answer in new[] { null, " ", current, "Invalid|Name" })
                    {
                        fixture.Answer = answer;
                        fixture.Click("Edit", level);
                    }
                    Assert.Empty(fixture.Database.Commands);
                    Assert.Equal("Validation error", Assert.Single(fixture.Messages).Caption);
                }
            });
        }

        [Theory]
        [InlineData("Add", "Second")]
        [InlineData("Edit", "First")]
        [InlineData("Edit", "Second")]
        [InlineData("Edit", "Third")]
        [InlineData("Edit", "Fourth")]
        [InlineData("Remove", "First")]
        [InlineData("Remove", "Second")]
        public void MissingSelectionDoesNotWrite(string action, string level)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.Load();
                    fixture.List(1).SelectedIndex = -1;
                    fixture.Click(action, level);
                    Assert.Empty(fixture.Database.Commands);
                    Assert.Single(fixture.Messages);
                }
            });
        }

        [Theory]
        [InlineData("First", "Root", 1)]
        [InlineData("Second", "Root|Branch", 2)]
        [InlineData("Third", "Root|Branch|Twig", 3)]
        [InlineData("Fourth", "Root|Branch|Twig|Leaf", 4)]
        public void EditUsesParameterizedPathAndRefreshesData(string level, string oldPath, int depth)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    fixture.Answer = "Renamed";
                    string newPath = string.Join("|", oldPath.Split('|').Take(depth - 1).Concat(new[] { fixture.Answer }));
                    fixture.Database.Execute = command =>
                    {
                        for (int index = 0; index < fixture.Database.Rows.Count; index++)
                        {
                            var row = fixture.Database.Rows[index];
                            fixture.Database.Rows[index] = new KeyValuePair<int, string>(row.Key, row.Value.Replace(oldPath, newPath));
                        }
                        return 1;
                    };
                    fixture.Click("Edit", level);
                    Assert.All(fixture.Database.Commands, command => Assert.Contains("UPDATE CDN.DefAtrElem", command.Sql));
                    Assert.Contains(fixture.Database.Rows, row => row.Value == newPath);
                    if (depth < 4) Assert.Equal("Renamed", fixture.List(depth).SelectedItem);
                    if (depth == 1)
                    {
                        Assert.Equal(2, fixture.Database.Commands.Count);
                        Assert.All(fixture.Database.Commands, command => Assert.True(command.InTransaction));
                        Assert.Equal(1, fixture.Database.Commits);
                    }
                    else
                    {
                        var command = Assert.Single(fixture.Database.Commands);
                        Assert.Equal(oldPath, command.Parameters["@oldPath"]);
                        Assert.Equal(newPath, command.Parameters["@newPath"]);
                        Assert.Equal(oldPath + "%", command.Parameters["@likePath"]);
                    }
                }
            });
        }

        [Theory]
        [InlineData("First")]
        [InlineData("Second")]
        [InlineData("Third")]
        [InlineData("Fourth")]
        public void EditReportsDatabaseErrors(string level)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    fixture.Answer = "Renamed";
                    fixture.Database.Execute = command => throw new InvalidOperationException("test database failure");
                    fixture.Click("Edit", level);
                    var message = Assert.Single(fixture.Messages);
                    Assert.Equal("Critical error", message.Caption);
                    Assert.Contains("test database failure", message.Text);
                    Assert.Equal(level == "First" ? 1 : 0, fixture.Database.Rollbacks);
                    Assert.Equal(0, fixture.Database.Commits);
                }
            });
        }

        [Theory]
        [InlineData("Second")]
        [InlineData("Third")]
        [InlineData("Fourth")]
        public void EditReportsWhenNoRowsMatch(string level)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    fixture.Answer = "Renamed";
                    fixture.Database.Execute = command => 0;
                    fixture.Click("Edit", level);
                    Assert.Equal("Warning", Assert.Single(fixture.Messages).Caption);
                }
            });
        }

        [Theory]
        [InlineData("Invalid|Name", false)]
        [InlineData("Valid", true)]
        public void AddSecondReportsValidationAndDatabaseErrors(string answer, bool databaseError)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    fixture.Answer = answer;
                    fixture.Database.Execute = command => throw new InvalidOperationException("test failure");
                    fixture.Click("Add", "Second");
                    Assert.Equal(databaseError ? "Critical error" : "Validation error", Assert.Single(fixture.Messages).Caption);
                    Assert.Equal(databaseError ? 1 : 0, fixture.Database.Commands.Count);
                }
            });
        }

        [Theory]
        [InlineData("First")]
        [InlineData("Second")]
        public void DeclinedDeletionDoesNotWrite(string level)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    fixture.Confirmation = DialogResult.No;
                    fixture.Click("Remove", level);
                    Assert.Empty(fixture.Database.Commands);
                }
            });
        }

        [Theory]
        [InlineData("First", 0)]
        [InlineData("First", 2)]
        [InlineData("Second", 0)]
        [InlineData("Second", 1)]
        public void DeletionUsesBoundPathAndReportsAffectedRows(string level, int affectedRows)
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    fixture.Database.Execute = command => affectedRows;
                    fixture.Click("Remove", level);
                    var command = Assert.Single(fixture.Database.Commands);
                    Assert.Contains("DELETE FROM CDN.DefAtrElem", command.Sql);
                    if (level == "Second")
                    {
                        Assert.Equal(2, command.Parameters["@deAId"]);
                        Assert.Equal("Root|Branch", command.Parameters["@value"]);
                    }
                    else Assert.Equal("Root%", command.Parameters["@pathToDelete"]);
                    Assert.Equal(2, fixture.Messages.Count);
                    Assert.Equal(affectedRows > 0 ? "Successful removal" : level == "First" ? "Deletion error" : "Information", fixture.Messages[1].Caption);
                }
            });
        }

        [Fact]
        public void SecondLevelDeletionReportsFailure()
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    fixture.Database.Execute = command => throw new InvalidOperationException("cannot delete");
                    fixture.Click("Remove", "Second");
                    Assert.Contains("cannot delete", fixture.Messages.Last().Text);
                    Assert.Equal("Critical error", fixture.Messages.Last().Caption);
                }
            });
        }

        [Fact]
        public void RefreshAfterDeletingLastBranchDisablesChildLists()
        {
            WinFormsTest.Run(() =>
            {
                using (var fixture = new Fixture())
                {
                    fixture.LoadAndSelect();
                    fixture.Database.Rows.Clear();
                    fixture.Database.Add(2, "Root");
                    fixture.Database.Add(3, "Leaf");
                    WinFormsTest.Invoke(fixture.Form, "RefreshAfterSecondLevelDeletion", "Root");
                    Assert.False(fixture.List(2).Enabled);
                    Assert.False(fixture.List(3).Enabled);
                    Assert.False(fixture.List(4).Enabled);
                }
            });
        }

        private sealed class Fixture : IDisposable
        {
            private static readonly string[] Levels = { "First", "Second", "Third", "Fourth" };
            internal FakeDatabase Database { get; } = new FakeDatabase();
            internal Form1 Form { get; }
            internal string Answer { get; set; } = "Added";
            internal DialogResult Confirmation { get; set; } = DialogResult.Yes;
            internal List<Message> Messages { get; } = new List<Message>();

            internal Fixture()
            {
                Database.Add(2, "Root", "Root|Branch");
                Database.Add(1, "Root|Branch|Twig");
                Database.Add(3, "Leaf");
                Database.Add(7, "Root|Branch|Twig|Leaf");
                Form = new Form1(Database.Connect, (text, caption) => Answer,
                    (text, caption, buttons, icon) =>
                    {
                        Messages.Add(new Message { Text = text, Caption = caption });
                        return buttons == MessageBoxButtons.YesNo ? Confirmation : DialogResult.OK;
                    });
                Form.BindingContext = new BindingContext();
            }

            internal void Load() => WinFormsTest.Invoke(Form, "MainForm_Load", Form, EventArgs.Empty);
            internal ListBox List(int depth) => WinFormsTest.Field<ListBox>(Form, "listBox" + Levels[depth - 1]);
            internal string[] Items(int depth) => List(depth).Items.Cast<string>().ToArray();
            internal void Select(int depth, string value)
            {
                List(depth).SelectedIndex = -1;
                List(depth).SelectedItem = value;
            }
            internal void LoadAndSelect()
            {
                Load();
                Select(1, "Root");
                Select(2, "Branch");
                Select(3, "Twig");
                Select(4, "Leaf");
            }
            internal void Click(string action, string level) => WinFormsTest.Invoke(Form, "Btn" + action + level + "_Click", Form, EventArgs.Empty);
            public void Dispose() { Form.Dispose(); }
        }

        private sealed class Message
        {
            internal string Text { get; set; }
            internal string Caption { get; set; }
        }
    }
}
