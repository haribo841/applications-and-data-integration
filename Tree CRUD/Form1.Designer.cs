using System;
using System.Windows.Forms;

namespace TreeCRUD
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private ListBox listBoxFirst;
        private Button btnAddFirst;
        private Button btnEditFirst;
        private Button btnRemoveFirst;
        private ListBox listBoxSecond;
        private Button btnAddSecond;
        private Button btnEditSecond;
        private Button btnRemoveSecond;
        private ListBox listBoxThird;
        private Button btnAddThird;
        private Button btnEditThird;
        private Button btnRemoveThird;
        private ListBox listBoxFourth;
        private Button btnAddFourth;
        private Button btnEditFourth;
        private Button btnRemoveFourth;
        private void InitializeComponent()
        {
            this.listBoxFirst = new ListBox();
            this.btnAddFirst = new Button();
            this.btnEditFirst = new Button();
            this.btnRemoveFirst = new Button();
            this.listBoxSecond = new ListBox();
            this.btnAddSecond = new Button();
            this.btnEditSecond = new Button();
            this.btnRemoveSecond = new Button();
            this.listBoxThird = new ListBox();
            this.btnAddThird = new Button();
            this.btnEditThird = new Button();
            this.btnRemoveThird = new Button();
            this.listBoxFourth = new ListBox();
            this.btnAddFourth = new Button();
            this.btnEditFourth = new Button();
            this.btnRemoveFourth = new Button();
            this.SuspendLayout();
            this.listBoxFirst.FormattingEnabled = true;
            this.listBoxFirst.Location = new System.Drawing.Point(12, 12);
            this.listBoxFirst.Size = new System.Drawing.Size(200, 238);
            this.listBoxFirst.SelectedIndexChanged += new System.EventHandler(this.ListBoxFirst_SelectedIndexChanged);
            this.btnAddFirst.Location = new System.Drawing.Point(12, 260);
            this.btnAddFirst.Size = new System.Drawing.Size(60, 30);
            this.btnAddFirst.Text = "Add";
            this.btnAddFirst.Click += new System.EventHandler(this.BtnAddFirst_Click);
            this.btnEditFirst.Location = new System.Drawing.Point(78, 260);
            this.btnEditFirst.Size = new System.Drawing.Size(60, 30);
            this.btnEditFirst.Text = "Edit";
            this.btnEditFirst.Click += new System.EventHandler(this.BtnEditFirst_Click);
            this.btnRemoveFirst.Location = new System.Drawing.Point(144, 260);
            this.btnRemoveFirst.Size = new System.Drawing.Size(60, 30);
            this.btnRemoveFirst.Text = "Delete";
            this.btnRemoveFirst.Click += new System.EventHandler(this.BtnRemoveFirst_Click);
            this.listBoxSecond.FormattingEnabled = true;
            this.listBoxSecond.Location = new System.Drawing.Point(230, 12);
            this.listBoxSecond.Size = new System.Drawing.Size(200, 238);
            this.listBoxSecond.SelectedIndexChanged += new System.EventHandler(this.ListBoxSecond_SelectedIndexChanged);
            this.btnAddSecond.Location = new System.Drawing.Point(230, 260);
            this.btnAddSecond.Size = new System.Drawing.Size(60, 30);
            this.btnAddSecond.Text = "Add";
            this.btnAddSecond.Click += new System.EventHandler(this.BtnAddSecond_Click);
            this.btnEditSecond.Location = new System.Drawing.Point(296, 260);
            this.btnEditSecond.Size = new System.Drawing.Size(60, 30);
            this.btnEditSecond.Text = "Edit";
            this.btnEditSecond.Click += new System.EventHandler(this.BtnEditSecond_Click);
            this.btnRemoveSecond.Location = new System.Drawing.Point(362, 260);
            this.btnRemoveSecond.Size = new System.Drawing.Size(60, 30);
            this.btnRemoveSecond.Text = "Delete";
            this.btnRemoveSecond.Click += new System.EventHandler(this.BtnRemoveSecond_Click);
            this.listBoxThird.FormattingEnabled = true;
            this.listBoxThird.Location = new System.Drawing.Point(448, 12);
            this.listBoxThird.Size = new System.Drawing.Size(200, 238);
            this.listBoxThird.SelectedIndexChanged += new System.EventHandler(this.ListBoxThird_SelectedIndexChanged);
            this.btnAddThird.Location = new System.Drawing.Point(448, 260);
            this.btnAddThird.Size = new System.Drawing.Size(60, 30);
            this.btnAddThird.Text = "Add";
            this.btnAddThird.Click += new System.EventHandler(this.BtnAddThird_Click);
            this.btnEditThird.Location = new System.Drawing.Point(514, 260);
            this.btnEditThird.Size = new System.Drawing.Size(60, 30);
            this.btnEditThird.Text = "Edit";
            this.btnEditThird.Click += new System.EventHandler(this.BtnEditThird_Click);
            this.btnRemoveThird.Location = new System.Drawing.Point(580, 260);
            this.btnRemoveThird.Size = new System.Drawing.Size(60, 30);
            this.btnRemoveThird.Text = "Delete";
            this.btnRemoveThird.Click += new System.EventHandler(this.BtnRemoveThird_Click);
            this.listBoxFourth.FormattingEnabled = true;
            this.listBoxFourth.Location = new System.Drawing.Point(666, 12);
            this.listBoxFourth.Size = new System.Drawing.Size(200, 238);
            this.listBoxFourth.SelectedIndexChanged += new System.EventHandler(this.ListBoxFourth_SelectedIndexChanged); 
            this.btnAddFourth.Location = new System.Drawing.Point(666, 260);
            this.btnAddFourth.Size = new System.Drawing.Size(60, 30);
            this.btnAddFourth.Text = "Add";
            this.btnAddFourth.Click += new System.EventHandler(this.BtnAddFourth_Click);
            this.btnEditFourth.Location = new System.Drawing.Point(732, 260);
            this.btnEditFourth.Size = new System.Drawing.Size(60, 30);
            this.btnEditFourth.Text = "Edit";
            this.btnEditFourth.Click += new System.EventHandler(this.BtnEditFourth_Click);
            this.btnRemoveFourth.Location = new System.Drawing.Point(798, 260);
            this.btnRemoveFourth.Size = new System.Drawing.Size(60, 30);
            this.btnRemoveFourth.Text = "Delete";
            this.btnRemoveFourth.Click += new System.EventHandler(this.BtnRemoveFourth_Click);
            this.ClientSize = new System.Drawing.Size(900, 310);
            this.Controls.Add(this.listBoxFirst);
            this.Controls.Add(this.btnAddFirst);
            this.Controls.Add(this.btnEditFirst);
            this.Controls.Add(this.btnRemoveFirst);
            this.Controls.Add(this.listBoxSecond);
            this.Controls.Add(this.btnAddSecond);
            this.Controls.Add(this.btnEditSecond);
            this.Controls.Add(this.btnRemoveSecond);
            this.Controls.Add(this.listBoxThird);
            this.Controls.Add(this.btnAddThird);
            this.Controls.Add(this.btnEditThird);
            this.Controls.Add(this.btnRemoveThird);
            this.Controls.Add(this.listBoxFourth);
            this.Controls.Add(this.btnAddFourth);
            this.Controls.Add(this.btnEditFourth);
            this.Controls.Add(this.btnRemoveFourth);
            this.Text = "Tree CRUD";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
        }
        private void ListBoxFourth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFourth.SelectedItem is string fourth) ToggleGroup(4, true);
            else ToggleGroup(4, false);
        }
    }
}
