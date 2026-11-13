using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace TreeCRUD
{
    public partial class Form1 : Form
    {
        private const string AddItemCaption = "Add item";
        private const string EditItemCaption = "Edit item";
        private const string InvalidElementNameMessage = "The element name cannot contain the character '|'.";
        private const string ValidationErrorCaption = "Validation error";
        private const string CriticalErrorCaption = "Critical error";
        private const string SuccessCaption = "Success";
        private const string NoSelectionCaption = "No selection";
        private const string AttributeIdParameter = "@deAId";
        private const string ValueParameter = "@value";

        private static readonly Lazy<string> _connString = new Lazy<string>(() =>
        ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
        public static string ConnString => _connString.Value;
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _subItems
            = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
        private readonly Dictionary<int, List<string>> _dataByListId
            = new Dictionary<int, List<string>>();
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly Func<string, string, string> _prompt;
        private readonly Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> _showMessage;

        public Form1() : this(() => new SqlConnection(ConnString), Prompt.ShowDialog, MessageBox.Show)
        {
        }

        internal Form1(
            Func<IDbConnection> connectionFactory,
            Func<string, string, string> prompt,
            Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> showMessage)
        {
            _connectionFactory = connectionFactory;
            _prompt = prompt;
            _showMessage = showMessage;
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        private static IDbCommand CreateCommand(string sql, IDbConnection connection, IDbTransaction transaction = null)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = transaction;
            return command;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadDataFromDb();
            BuildHierarchyFromRawData();
            listBoxFirst.DataSource = _subItems.Keys.ToList();
            ClearAndDisableLevel(2);
        }
        private void LoadDataFromDb()
        {
            _dataByListId.Clear();
            const string sql = @"
                SELECT DAE_DeAId, DAE_No, DAE_Value
                FROM CDN.DefAtrElem
                WHERE DAE_DeAId IN (2, 1, 3, 7)
                ORDER BY DAE_DeAId, DAE_No";
            using (IDbConnection conn = _connectionFactory())
            using (IDbCommand cmd = CreateCommand(sql, conn))
            {
                conn.Open();
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int listId = reader.GetInt32(reader.GetOrdinal("DAE_DeAId"));
                        string value = reader.GetString(reader.GetOrdinal("DAE_Value"));
                        if (!_dataByListId.TryGetValue(listId, out List<string> list))
                        {
                            list = new List<string>();
                            _dataByListId[listId] = list;
                        }
                        list.Add(value);
                    }
                }
            }
        }
        private void BuildHierarchyFromRawData()
        {
            _subItems.Clear();
            foreach (List<string> paths in _dataByListId.Values)
            {
                foreach (string path in paths)
                {
                    AddPathToHierarchy(path);
                }
            }
        }

        private void AddPathToHierarchy(string path)
        {
            string[] parts = path.Split('|');
            if (parts.Length == 0 || parts.Length > 4)
            {
                return;
            }

            Dictionary<string, Dictionary<string, List<string>>> level2 = GetOrAddLevel2(parts[0]);
            if (parts.Length == 1)
            {
                return;
            }

            Dictionary<string, List<string>> level3 = GetOrAddLevel3(level2, parts[1]);
            if (parts.Length == 2)
            {
                return;
            }

            List<string> level4 = GetOrAddLevel4(level3, parts[2]);
            if (parts.Length == 4 && !level4.Contains(parts[3]))
            {
                level4.Add(parts[3]);
            }
        }

        private Dictionary<string, Dictionary<string, List<string>>> GetOrAddLevel2(string root)
        {
            if (!_subItems.TryGetValue(root, out Dictionary<string, Dictionary<string, List<string>>> level2))
            {
                level2 = new Dictionary<string, Dictionary<string, List<string>>>();
                _subItems.Add(root, level2);
            }

            return level2;
        }

        private static Dictionary<string, List<string>> GetOrAddLevel3(
            Dictionary<string, Dictionary<string, List<string>>> level2,
            string item)
        {
            if (!level2.TryGetValue(item, out Dictionary<string, List<string>> level3))
            {
                level3 = new Dictionary<string, List<string>>();
                level2.Add(item, level3);
            }

            return level3;
        }

        private static List<string> GetOrAddLevel4(Dictionary<string, List<string>> level3, string item)
        {
            if (!level3.TryGetValue(item, out List<string> level4))
            {
                level4 = new List<string>();
                level3.Add(item, level4);
            }

            return level4;
        }
        private void UpdateFirstList()
        {
            listBoxFirst.DataSource = null;
            listBoxFirst.DataSource = _subItems.Keys.ToList();
        }
        private void BtnAddFirst_Click(object sender, EventArgs e)
        {
            string input = _prompt("Enter the name of the first level:", AddItemCaption);
            if (string.IsNullOrWhiteSpace(input)) return;
            const string sql = @"
                INSERT INTO CDN.DefAtrElem (DAE_DeAId, DAE_Value, DAE_No)
                VALUES (@deAId, @value,
                   (SELECT ISNULL(MAX(DAE_Lp), 0) + 1 FROM CDN.DefAtrElem WHERE DAE_DeAId = @deAId)
                )";
            using (IDbConnection conn = _connectionFactory())
            using (IDbCommand cmd = CreateCommand(sql, conn))
            {
                cmd.AddParameter(AttributeIdParameter, 2);
                cmd.AddParameter(ValueParameter, input);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            LoadDataFromDb();
            BuildHierarchyFromRawData();
            UpdateFirstList();
            listBoxFirst.SelectedItem = input;
        }
        private void BtnEditFirst_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string oldKey)
            {
                string newKey = _prompt("Enter a new name for the item:", EditItemCaption);
                if (string.IsNullOrWhiteSpace(newKey) || newKey == oldKey) return;
                if (newKey.Contains("|"))
                {
                    _showMessage(InvalidElementNameMessage, ValidationErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                using (IDbConnection conn = _connectionFactory())
                {
                    conn.Open();
                    IDbTransaction transaction = conn.BeginTransaction();
                    try
                    {
                        const string sql2 = @"
                            UPDATE CDN.DefAtrElem
                            SET DAE_Value = @newValue
                            WHERE DAE_Value = @oldValue AND DAE_DeAId = @deAId2";
                        using (IDbCommand cmd2 = CreateCommand(sql2, conn, transaction))
                        {
                            cmd2.AddParameter("@newValue", newKey.Trim());
                            cmd2.AddParameter("@oldValue", oldKey.Trim());
                            cmd2.AddParameter("@deAId2", 2);
                            cmd2.ExecuteNonQuery();
                        }
                        const string sql2b = @"
                            UPDATE CDN.DefAtrElem
                            SET DAE_Value = @newValue
                            WHERE DAE_Value = @oldValue AND DAE_DeAId = @deAId2b";
                        using (IDbCommand cmd2b = CreateCommand(sql2b, conn, transaction))
                        {
                            cmd2b.AddParameter("@newValue", newKey.Trim());
                            cmd2b.AddParameter("@oldValue", oldKey.Trim());
                            cmd2b.AddParameter("@deAId2b", 2);
                            cmd2b.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        _showMessage("Items were successfully updated in the database.", "Update success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDataFromDb();
                        BuildHierarchyFromRawData();
                        UpdateFirstList();
                        listBoxFirst.SelectedItem = newKey;
                        ClearAndDisableLevel(2);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _showMessage($"An error occurred while updating data: {ex.Message}", CriticalErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                _showMessage("Please select an item to edit from the first list.", "No choice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BtnRemoveFirst_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string selectedKey)
            {
                DialogResult confirmResult = _showMessage(
                    $"Are you sure you want to delete '{selectedKey}' and all its subordinate elements?",
                    "Confirm deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (confirmResult == DialogResult.Yes)
                {
                    const string sql = @"
                        DELETE FROM CDN.DefAtrElem
                        WHERE DAE_Value LIKE @pathToDelete;";
                    using (IDbConnection conn = _connectionFactory())
                    using (IDbCommand cmd = CreateCommand(sql, conn))
                    {
                        cmd.AddParameter("@pathToDelete", selectedKey.Trim() + "%");
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            _showMessage($"Deleted {rowsAffected / 2} records from the database.", "Successful removal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            _showMessage("No records found to delete in the database.", "Deletion error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    LoadDataFromDb();
                    BuildHierarchyFromRawData();
                    UpdateFirstList();
                    ClearAndDisableLevel(2);
                }
            }
            else
            {
                _showMessage("Please select an item to remove from the first list.", "No choice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void ListBoxFirst_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAndDisableLevel(2);
            if (listBoxFirst.SelectedItem is string sel1 && _subItems.TryGetValue(sel1, out Dictionary<string, Dictionary<string, List<string>>> children))
            {
                listBoxSecond.DataSource = children.Keys
                                                    .Select(key => key.Split('|').LastOrDefault())
                                                    .Where(name => !string.IsNullOrEmpty(name))
                                                    .ToList();
                ToggleGroup(2, true);
            }
        }
        private void BtnAddSecond_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string first)
            {
                string input = _prompt("Enter the name of level 2:", AddItemCaption);
                if (string.IsNullOrWhiteSpace(input)) return;
                if (input.Contains("|"))
                {
                    _showMessage(InvalidElementNameMessage, ValidationErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string fullPath = $"{first}|{input}";
                const string sql = @"
                    INSERT INTO CDN.DefAtrElem (DAE_DeAId, DAE_Value, DAE_No)
                    VALUES (@deAId, @value,
                        (SELECT ISNULL(MAX(DAE_Lp),0)+1 
                           FROM CDN.DefAtrElem 
                          WHERE DAE_DeAId = @deAId)
                )";
                try
                {
                    using (IDbConnection conn = _connectionFactory())
                    using (IDbCommand cmd = CreateCommand(sql, conn))
                    {
                        cmd.AddParameter(AttributeIdParameter, 1);
                        cmd.AddParameter(ValueParameter, fullPath);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    LoadDataFromDb();
                    BuildHierarchyFromRawData();
                    int originalFirstSelectedIndex = listBoxFirst.SelectedIndex;
                    listBoxFirst.SelectedIndex = -1;
                    listBoxFirst.SelectedIndex = originalFirstSelectedIndex;
                    if (listBoxSecond.Items.Cast<string>().Contains(input, StringComparer.OrdinalIgnoreCase))
                    {
                        listBoxSecond.SelectedItem = input;
                    }
                    _showMessage("The item was added successfully.", SuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    _showMessage($"An error occurred while adding the item: {ex.Message}", CriticalErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _showMessage("Please select an item from the first list to which you want to add a sub-item.", NoSelectionCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BtnEditSecond_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string parentKey && listBoxSecond.SelectedItem is string oldKey)
            {
                string newKey = _prompt($"Enter a new name for the item '{oldKey}':", EditItemCaption);
                if (string.IsNullOrWhiteSpace(newKey) || newKey == oldKey) return;
                if (newKey.Contains("|"))
                {
                    _showMessage(InvalidElementNameMessage, ValidationErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string oldPathFragment = $"{parentKey}|{oldKey}";
                string newPathFragment = $"{parentKey}|{newKey}";
                try
                {
                    const string sql = @"
                        UPDATE CDN.DefAtrElem
                        SET DAE_Value = REPLACE(DAE_Value, @oldPath, @newPath)
                        WHERE DAE_Value LIKE @likePath";
                    using (IDbConnection conn = _connectionFactory())
                    using (IDbCommand cmd = CreateCommand(sql, conn))
                    {
                        cmd.AddParameter("@oldPath", oldPathFragment);
                        cmd.AddParameter("@newPath", newPathFragment);
                        cmd.AddParameter("@likePath", oldPathFragment + "%");
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            _showMessage("No matching records found in the database to update..", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    LoadDataFromDb();
                    BuildHierarchyFromRawData();
                    listBoxFirst.DataSource = _subItems.Keys.ToList();
                    listBoxFirst.SelectedItem = parentKey;
                    listBoxSecond.SelectedItem = newKey;
                    _showMessage("The item was successfully updated.", SuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    _showMessage($"An error occurred while updating data: {ex.Message}", CriticalErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _showMessage("Please select an item from the first and second level lists.", NoSelectionCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BtnRemoveSecond_Click(object sender, EventArgs e)
        {
            if (!(listBoxFirst.SelectedItem is string first) || !(listBoxSecond.SelectedItem is string second))
            {
                _showMessage("Please select an item to remove from the second list.", "No choice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirmResult = _showMessage(
                $"Are you sure you want to delete the item? '{second}' from level 2 (related to '{first}')?",
                "Confirm deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirmResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int rowsAffected = DeleteSecondLevelPath($"{first}|{second}");
                if (rowsAffected == 0)
                {
                    _showMessage("No item found to delete in the database.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _showMessage("The item was successfully deleted from the database.", "Successful removal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshAfterSecondLevelDeletion(first);
            }
            catch (Exception ex)
            {
                _showMessage($"An error occurred while deleting the item from the database: {ex.Message}", CriticalErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int DeleteSecondLevelPath(string path)
        {
            const string sql = @"
                DELETE FROM CDN.DefAtrElem
                WHERE DAE_DeAId = @deAId AND DAE_Value = @value";
            using (IDbConnection conn = _connectionFactory())
            using (IDbCommand cmd = CreateCommand(sql, conn))
            {
                cmd.AddParameter(AttributeIdParameter, 2);
                cmd.AddParameter(ValueParameter, path);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        private void RefreshAfterSecondLevelDeletion(string first)
        {
            LoadDataFromDb();
            BuildHierarchyFromRawData();

            int originalFirstSelectedIndex = listBoxFirst.SelectedIndex;
            listBoxFirst.SelectedIndex = -1;
            if (originalFirstSelectedIndex >= 0 && originalFirstSelectedIndex < listBoxFirst.Items.Count)
            {
                listBoxFirst.SelectedIndex = originalFirstSelectedIndex;
            }
            else if (listBoxFirst.Items.Count > 0)
            {
                listBoxFirst.SelectedIndex = 0;
            }
            else
            {
                listBoxFirst.SelectedIndex = -1;
            }

            if (!_subItems.TryGetValue(first, out Dictionary<string, Dictionary<string, List<string>>> children) || !children.Any())
            {
                ClearAndDisableLevel(2);
            }
            else
            {
                ClearAndDisableLevel(3);
            }
        }
        private void ListBoxSecond_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearAndDisableLevel(3);
            if (listBoxFirst.SelectedItem is string sel1 && listBoxSecond.SelectedItem is string sel2 &&
                _subItems.TryGetValue(sel1, out Dictionary<string, Dictionary<string, List<string>>> level2Dict) && level2Dict.TryGetValue(sel2, out Dictionary<string, List<string>> children))
            {
                listBoxThird.DataSource = children.Keys.ToList();
                ToggleGroup(3, true);
            }
        }
        private void UpdateThirdList()
        {
            listBoxThird.DataSource = null;
            if (listBoxFirst.SelectedItem is string sel1 &&
                listBoxSecond.SelectedItem is string sel2 &&
                _subItems.TryGetValue(sel1, out Dictionary<string, Dictionary<string, List<string>>> level2Dict) &&
                level2Dict.TryGetValue(sel2, out Dictionary<string, List<string>> children))
            {
                listBoxThird.DataSource = children.Keys.ToList();
            }
        }
        private void BtnAddThird_Click(object sender, EventArgs e)
        {
            string input = _prompt("Enter the name of the third level:", AddItemCaption);
            if (string.IsNullOrWhiteSpace(input)) return;
            const string sql = @"
                INSERT INTO CDN.DefAtrElem (DAE_DeAId, DAE_Value, DAE_Lp)
                VALUES (@deAId, @value,
               (SELECT ISNULL(MAX(DAE_Lp), 0) + 1 FROM CDN.DefAtrElem WHERE DAE_DeAId = @deAId)
            )";
            using (IDbConnection conn = _connectionFactory())
            using (IDbCommand cmd = CreateCommand(sql, conn))
            {
                cmd.AddParameter(AttributeIdParameter, 3);
                cmd.AddParameter(ValueParameter, input);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            _dataByListId[3].Add(input);
            UpdateThirdList();
            listBoxThird.SelectedItem = input;
        }

        private void BtnEditThird_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string parentKey1 &&
                listBoxSecond.SelectedItem is string parentKey2 &&
                listBoxThird.SelectedItem is string oldKey)
            {
                string newKey = _prompt($"Enter a new name for the item '{oldKey}':", EditItemCaption);
                if (string.IsNullOrWhiteSpace(newKey) || newKey == oldKey) return;
                if (newKey.Contains("|"))
                {
                    _showMessage(InvalidElementNameMessage, ValidationErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string oldPathFragment = $"{parentKey1}|{parentKey2}|{oldKey}";
                string newPathFragment = $"{parentKey1}|{parentKey2}|{newKey}";
                try
                {
                    const string sql = @"
                        UPDATE CDN.DefAtrElem
                        SET DAE_Value = REPLACE(DAE_Value, @oldPath, @newPath)
                        WHERE DAE_Value LIKE @likePath";
                    using (IDbConnection conn = _connectionFactory())
                    using (IDbCommand cmd = CreateCommand(sql, conn))
                    {
                        cmd.AddParameter("@oldPath", oldPathFragment);
                        cmd.AddParameter("@newPath", newPathFragment);
                        cmd.AddParameter("@likePath", oldPathFragment + "%");
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            _showMessage("No matching records found in the database to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    LoadDataFromDb();
                    BuildHierarchyFromRawData();
                    listBoxFirst.DataSource = _subItems.Keys.ToList();
                    listBoxFirst.SelectedItem = parentKey1;
                    listBoxSecond.SelectedItem = parentKey2;
                    listBoxThird.SelectedItem = newKey;
                    _showMessage("The item was successfully updated.", SuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    _showMessage($"An error occurred while updating data: {ex.Message}", CriticalErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _showMessage("Please select an item from the first, second, and third level lists..", NoSelectionCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BtnRemoveThird_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string first && listBoxSecond.SelectedItem is string second && listBoxThird.SelectedItem is string third)
            {
                _subItems[first][second].Remove(third);
                UpdateThirdList();
                ClearAndDisableLevel(4);
            }
        }
        private void ListBoxThird_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxFourth.DataSource = _dataByListId[3];
            listBoxFourth.Enabled = listBoxThird.SelectedItem != null;
        }
        private void UpdateFourthList()
        {
            listBoxFourth.DataSource = null;
            if (listBoxFirst.SelectedItem is string sel1 &&
                listBoxSecond.SelectedItem is string sel2 &&
                listBoxThird.SelectedItem is string sel3 &&
                _subItems.TryGetValue(sel1, out Dictionary<string, Dictionary<string, List<string>>> level2Dict) &&
                level2Dict.TryGetValue(sel2, out Dictionary<string, List<string>> level3Dict) &&
                level3Dict.TryGetValue(sel3, out List<string> children))
            {
                listBoxFourth.DataSource = children.ToList();
            }
        }
        private void BtnAddFourth_Click(object sender, EventArgs e)
        {
            string input = _prompt("Enter the name of the fourth level:", AddItemCaption);
            if (string.IsNullOrWhiteSpace(input)) return;
            const string sql = @"
                INSERT INTO CDN.DefAtrElem (DAE_DeAId, DAE_Value, DAE_Lp)
                VALUES (@deAId, @value,
                (SELECT ISNULL(MAX(DAE_Lp), 0) + 1 FROM CDN.DefAtrElem WHERE DAE_DeAId = @deAId)
            )";
            using (IDbConnection conn = _connectionFactory())
            using (IDbCommand cmd = CreateCommand(sql, conn))
            {
                cmd.AddParameter(AttributeIdParameter, 55);
                cmd.AddParameter(ValueParameter, input);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            _dataByListId[3].Add(input);
            UpdateFourthList();
            listBoxFourth.SelectedItem = input;
        }
        private void BtnEditFourth_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string parentKey1 &&
                listBoxSecond.SelectedItem is string parentKey2 &&
                listBoxThird.SelectedItem is string parentKey3 &&
                listBoxFourth.SelectedItem is string oldKey)
            {
                string newKey = _prompt($"Enter a new name for the item '{oldKey}':", EditItemCaption);
                if (string.IsNullOrWhiteSpace(newKey) || newKey == oldKey) return;
                if (newKey.Contains("|"))
                {
                    _showMessage(InvalidElementNameMessage, ValidationErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string oldPathFragment = $"{parentKey1}|{parentKey2}|{parentKey3}|{oldKey}";
                string newPathFragment = $"{parentKey1}|{parentKey2}|{parentKey3}|{newKey}";
                try
                {
                    const string sql = @"
                        UPDATE CDN.DefAtrElem
                        SET DAE_Value = REPLACE(DAE_Value, @oldPath, @newPath)
                        WHERE DAE_Value LIKE @likePath";
                    using (IDbConnection conn = _connectionFactory())
                    using (IDbCommand cmd = CreateCommand(sql, conn))
                    {
                        cmd.AddParameter("@oldPath", oldPathFragment);
                        cmd.AddParameter("@newPath", newPathFragment);
                        cmd.AddParameter("@likePath", oldPathFragment + "%");
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            _showMessage("No matching records found in the database to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    LoadDataFromDb();
                    BuildHierarchyFromRawData();
                    listBoxFirst.DataSource = _subItems.Keys.ToList();
                    listBoxFirst.SelectedItem = parentKey1;
                    listBoxSecond.SelectedItem = parentKey2;
                    listBoxThird.SelectedItem = parentKey3;
                    listBoxFourth.SelectedItem = newKey;
                    _showMessage("The item was successfully updated.", SuccessCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    _showMessage($"An error occurred while updating data: {ex.Message}", CriticalErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                _showMessage("Please select the item on all four levels.", NoSelectionCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BtnRemoveFourth_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string first && listBoxSecond.SelectedItem is string second && listBoxThird.SelectedItem is string third && listBoxFourth.SelectedItem is string fourth)
            {
                _subItems[first][second][third].Remove(fourth);
                UpdateFourthList();
            }
        }
        private void ToggleGroup(int level, bool enabled)
        {
            switch (level)
            {
                case 2:
                    listBoxSecond.Enabled = enabled;
                    btnAddSecond.Enabled = enabled;
                    btnEditSecond.Enabled = enabled;
                    btnRemoveSecond.Enabled = enabled;
                    break;
                case 3:
                    listBoxThird.Enabled = enabled;
                    btnAddThird.Enabled = enabled;
                    btnEditThird.Enabled = enabled;
                    btnRemoveThird.Enabled = enabled;
                    break;
                case 4:
                    listBoxFourth.Enabled = enabled;
                    btnAddFourth.Enabled = enabled;
                    btnEditFourth.Enabled = enabled;
                    btnRemoveFourth.Enabled = enabled;
                    break;
            }
        }
        private void ClearAndDisableLevel(int level)
        {
            if (level == 2)
            {
                listBoxSecond.DataSource = null;
                ToggleGroup(2, false);
            }

            if (level == 2 || level == 3)
            {
                listBoxThird.DataSource = null;
                ToggleGroup(3, false);
            }

            if (level >= 2 && level <= 4)
            {
                listBoxFourth.DataSource = null;
                ToggleGroup(4, false);
            }
        }
    }
    internal static class CommandParameters
    {
        internal static void AddParameter(this IDbCommand command, string name, object value)
        {
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            using (Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                Text = caption
            })
            {
                Label label = new Label { Left = 10, Top = 20, Text = text, AutoSize = true };
                TextBox input = new TextBox { Left = 10, Top = 50, Width = 360 };
                Button ok = new Button { Text = "OK", Left = 220, Top = 80, DialogResult = DialogResult.OK };
                Button cancel = new Button { Text = "Cancel", Left = 300, Top = 80, DialogResult = DialogResult.Cancel };
                prompt.Controls.AddRange(new Control[] { label, input, ok, cancel });
                prompt.AcceptButton = ok;
                prompt.CancelButton = cancel;
                return prompt.ShowDialog() == DialogResult.OK ? input.Text : null;
            }
        }
    }
}
