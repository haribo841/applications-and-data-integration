using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace TreeCRUD
{
    public partial class Form1 : Form
    {
        private static readonly Lazy<string> _connString = new Lazy<string>(() =>
        ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
        public static string ConnString => _connString.Value;
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _subItems
            = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
        private readonly Dictionary<int, List<string>> _dataByListId
            = new Dictionary<int, List<string>>();
        public Form1()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
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
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int listId = reader.GetInt32(reader.GetOrdinal("DAE_DeAId"));
                        string value = reader.GetString(reader.GetOrdinal("DAE_Wartosc"));
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
            foreach (KeyValuePair<int, List<string>> listIdEntry in _dataByListId)
            {
                foreach (string[] parts in from path in listIdEntry.Value
                                      let parts = path.Split('|')
                                      select parts)
                {
                    if (parts.Length == 1)
                    {
                        if (!_subItems.ContainsKey(parts[0]))
                        {
                            _subItems[parts[0]] = new Dictionary<string, Dictionary<string, List<string>>>();
                        }
                    }
                    else if (parts.Length == 2)
                    {
                        if (_subItems.TryGetValue(parts[0], out Dictionary<string, Dictionary<string, List<string>>> level2Dict))
                        {
                            if (!level2Dict.ContainsKey(parts[1]))
                            {
                                level2Dict[parts[1]] = new Dictionary<string, List<string>>();
                            }
                        }
                    }
                    else if (parts.Length == 3)
                    {
                        if (_subItems.TryGetValue(parts[0], out Dictionary<string, Dictionary<string, List<string>>> level2Dict))
                        {
                            if (level2Dict.TryGetValue(parts[1], out Dictionary<string, List<string>> level3Dict))
                            {
                                if (!level3Dict.ContainsKey(parts[2]))
                                {
                                    level3Dict[parts[2]] = new List<string>();
                                }
                            }
                        }
                    }
                    else if (parts.Length == 4)
                    {
                        if (_subItems.TryGetValue(parts[0], out Dictionary<string, Dictionary<string, List<string>>> level2Dict))
                        {
                            if (level2Dict.TryGetValue(parts[1], out Dictionary<string, List<string>> level3Dict))
                            {
                                if (level3Dict.TryGetValue(parts[2], out List<string> level4List))
                                {
                                    if (!level4List.Contains(parts[3]))
                                    {
                                        level4List.Add(parts[3]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void UpdateFirstList()
        {
            listBoxFirst.DataSource = null;
            listBoxFirst.DataSource = _subItems.Keys.ToList();
        }
        private void BtnAddFirst_Click(object sender, EventArgs e)
        {
            string input = Prompt.ShowDialog("Enter the name of the first level:", "Add item");
            if (string.IsNullOrWhiteSpace(input)) return;
            const string sql = @"
                INSERT INTO CDN.DefAtrElem (DAE_DeAId, DAE_Value, DAE_No)
                VALUES (@deAId, @value,
                   (SELECT ISNULL(MAX(DAE_Lp), 0) + 1 FROM CDN.DefAtrElem WHERE DAE_DeAId = @deAId)
                )";
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@deAId", 2);
                cmd.Parameters.AddWithValue("@value", input);
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
                string newKey = Prompt.ShowDialog("Enter a new name for the item:", "Edit item");
                if (string.IsNullOrWhiteSpace(newKey) || newKey == oldKey) return;
                if (newKey.Contains("|"))
                {
                    MessageBox.Show("The element name cannot contain the character '|'.", "Validation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();
                    try
                    {
                        const string sql2 = @"
                            UPDATE CDN.DefAtrElem
                            SET DAE_Value = @newValue
                            WHERE DAE_Value = @oldValue AND DAE_DeAId = @deAId2";
                        using (SqlCommand cmd2 = new SqlCommand(sql2, conn, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@newValue", newKey.Trim());
                            cmd2.Parameters.AddWithValue("@oldValue", oldKey.Trim());
                            cmd2.Parameters.AddWithValue("@deAId2", 2);
                            cmd2.ExecuteNonQuery();
                        }
                        const string sql2b = @"
                            UPDATE CDN.DefAtrElem
                            SET DAE_Value = @newValue
                            WHERE DAE_Value = @oldValue AND DAE_DeAId = @deAId2b";
                        using (SqlCommand cmd2b = new SqlCommand(sql2b, conn, transaction))
                        {
                            cmd2b.Parameters.AddWithValue("@newValue", newKey.Trim());
                            cmd2b.Parameters.AddWithValue("@oldValue", oldKey.Trim());
                            cmd2b.Parameters.AddWithValue("@deAId2b", 2);
                            cmd2b.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        MessageBox.Show("Items were successfully updated in the database.", "Update success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDataFromDb();
                        BuildHierarchyFromRawData();
                        UpdateFirstList();
                        listBoxFirst.SelectedItem = newKey;
                        ClearAndDisableLevel(2);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"An error occurred while updating data: {ex.Message}", "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an item to edit from the first list.", "No choice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BtnRemoveFirst_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string selectedKey)
            {
                DialogResult confirmResult = MessageBox.Show(
                    $"Are you sure you want to delete '{selectedKey}' and all its subordinate elements?",
                    "Confirm deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (confirmResult == DialogResult.Yes)
                {
                    const string sql = @"
                        DELETE FROM CDN.DefAtrElem
                        WHERE DAE_Value LIKE @pathToDelete;";
                    using (SqlConnection conn = new SqlConnection(ConnString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@pathToDelete", selectedKey.Trim() + "%");
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"Deleted {rowsAffected / 2} records from the database.", "Successful removal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No records found to delete in the database.", "Deletion error", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("Please select an item to remove from the first list.", "No choice", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        private void UpdateSecondList()
        {
            listBoxSecond.DataSource = null;
            if (listBoxFirst.SelectedItem is string sel1 && _subItems.TryGetValue(sel1, out Dictionary<string, Dictionary<string, List<string>>> children))
            {
                listBoxSecond.DataSource = children.Keys.ToList();
            }
        }
        private void BtnAddSecond_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string first)
            {
                string input = Prompt.ShowDialog("Enter the name of level 2:", "Add item");
                if (string.IsNullOrWhiteSpace(input)) return;
                if (input.Contains("|"))
                {
                    MessageBox.Show("The element name cannot contain the character '|'.", "Validation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    using (SqlConnection conn = new SqlConnection(ConnString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@deAId", 1);
                        cmd.Parameters.AddWithValue("@value", fullPath);
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
                    MessageBox.Show("The item was added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while adding the item: {ex.Message}", "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an item from the first list to which you want to add a sub-item.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BtnEditSecond_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string parentKey && listBoxSecond.SelectedItem is string oldKey)
            {
                string newKey = Prompt.ShowDialog($"Enter a new name for the item '{oldKey}':", "Edit item");
                if (string.IsNullOrWhiteSpace(newKey) || newKey == oldKey) return;
                if (newKey.Contains("|"))
                {
                    MessageBox.Show("The element name cannot contain the character '|'.", "Validation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    using (SqlConnection conn = new SqlConnection(ConnString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@oldPath", oldPathFragment);
                        cmd.Parameters.AddWithValue("@newPath", newPathFragment);
                        cmd.Parameters.AddWithValue("@likePath", oldPathFragment + "%");
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            MessageBox.Show("No matching records found in the database to update..", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    LoadDataFromDb();
                    BuildHierarchyFromRawData();
                    listBoxFirst.DataSource = _subItems.Keys.ToList();
                    listBoxFirst.SelectedItem = parentKey;
                    listBoxSecond.SelectedItem = newKey;
                    MessageBox.Show("The item was successfully updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating data: {ex.Message}", "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an item from the first and second level lists.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BtnRemoveSecond_Click(object sender, EventArgs e)
        {
            if (listBoxFirst.SelectedItem is string first && listBoxSecond.SelectedItem is string second)
            {
                DialogResult confirmResult = MessageBox.Show($"Are you sure you want to delete the item? '{second}' from level 2 (related to '{first}')?",
                                                   "Confirm deletion",
                                                   MessageBoxButtons.YesNo,
                                                   MessageBoxIcon.Question);
                if (confirmResult == DialogResult.Yes)
                {
                    string fullPathToDelete = $"{first}|{second}";
                    const string sql = @"
                        DELETE FROM CDN.DefAtrElem
                        WHERE DAE_DeAId = @deAId AND DAE_Value = @value";
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(ConnString))
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@deAId", 2);
                            cmd.Parameters.AddWithValue("@wartosc", fullPathToDelete);
                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("The item was successfully deleted from the database.", "Successful removal", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                            else
                            {
                                MessageBox.Show("No item found to delete in the database.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while deleting the item from the database: {ex.Message}", "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an item to remove from the second list.", "No choice", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            string input = Prompt.ShowDialog("Enter the name of the third level:", "Add item");
            if (string.IsNullOrWhiteSpace(input)) return;
            const string sql = @"
                INSERT INTO CDN.DefAtrElem (DAE_DeAId, DAE_Value, DAE_Lp)
                VALUES (@deAId, @value,
               (SELECT ISNULL(MAX(DAE_Lp), 0) + 1 FROM CDN.DefAtrElem WHERE DAE_DeAId = @deAId)
            )";
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@deAId", 3);
                cmd.Parameters.AddWithValue("@value", input);
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
                string newKey = Prompt.ShowDialog($"Enter a new name for the item '{oldKey}':", "Edit item");
                if (string.IsNullOrWhiteSpace(newKey) || newKey == oldKey) return;
                if (newKey.Contains("|"))
                {
                    MessageBox.Show("The element name cannot contain the character '|'.", "Validation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    using (SqlConnection conn = new SqlConnection(ConnString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@oldPath", oldPathFragment);
                        cmd.Parameters.AddWithValue("@newPath", newPathFragment);
                        cmd.Parameters.AddWithValue("@likePath", oldPathFragment + "%");
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            MessageBox.Show("No matching records found in the database to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    LoadDataFromDb();
                    BuildHierarchyFromRawData();
                    listBoxFirst.DataSource = _subItems.Keys.ToList();
                    listBoxFirst.SelectedItem = parentKey1;
                    listBoxSecond.SelectedItem = parentKey2;
                    listBoxThird.SelectedItem = newKey;
                    MessageBox.Show("The item was successfully updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating data: {ex.Message}", "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an item from the first, second, and third level lists..", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            string input = Prompt.ShowDialog("Enter the name of the fourth level:", "Add item");
            if (string.IsNullOrWhiteSpace(input)) return;
            const string sql = @"
                INSERT INTO CDN.DefAtrElem (DAE_DeAId, DAE_Value, DAE_Lp)
                VALUES (@deAId, @value,
                (SELECT ISNULL(MAX(DAE_Lp), 0) + 1 FROM CDN.DefAtrElem WHERE DAE_DeAId = @deAId)
            )";
            using (SqlConnection conn = new SqlConnection(ConnString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@deAId", 55);
                cmd.Parameters.AddWithValue("@value", input);
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
                string newKey = Prompt.ShowDialog($"Enter a new name for the item '{oldKey}':", "Edit item");
                if (string.IsNullOrWhiteSpace(newKey) || newKey == oldKey) return;
                if (newKey.Contains("|"))
                {
                    MessageBox.Show("The element name cannot contain the character '|'.", "Validation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    using (SqlConnection conn = new SqlConnection(ConnString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@oldPath", oldPathFragment);
                        cmd.Parameters.AddWithValue("@newPath", newPathFragment);
                        cmd.Parameters.AddWithValue("@likePath", oldPathFragment + "%");
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            MessageBox.Show("No matching records found in the database to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("The item was successfully updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating data: {ex.Message}", "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select the item on all four levels.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            switch (level)
            {
                case 2:
                    listBoxSecond.DataSource = null;
                    ToggleGroup(2, false);
                    goto case 3;
                case 3:
                    listBoxThird.DataSource = null;
                    ToggleGroup(3, false);
                    goto case 4;
                case 4:
                    listBoxFourth.DataSource = null;
                    ToggleGroup(4, false);
                    break;
            }
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
