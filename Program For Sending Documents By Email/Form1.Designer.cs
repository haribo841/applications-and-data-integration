using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DocumentsViaEmailSender
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.descriptionHeaderLabel = new System.Windows.Forms.Label();
            this.descriptionHeaderTextBox = new System.Windows.Forms.TextBox();
            this.descriptionFooterLabel = new System.Windows.Forms.Label();
            this.descriptionFooterTextBox = new System.Windows.Forms.TextBox();
            this.descriptionFooter2Label = new System.Windows.Forms.Label();
            this.descriptionFooter2TextBox = new System.Windows.Forms.TextBox();
            this.filePathLabel = new System.Windows.Forms.Label();
            this.filePathTextBox = new System.Windows.Forms.TextBox();
            this.addresCCLabel = new System.Windows.Forms.Label();
            this.addresCCTextBox = new System.Windows.Forms.TextBox();
            this.contentLabel = new System.Windows.Forms.Label();
            this.contentTextBox = new System.Windows.Forms.TextBox();
            this.send = new System.Windows.Forms.Button();
            this.update = new System.Windows.Forms.Button();
            this.preview = new System.Windows.Forms.Button();
            this.dateLabel = new System.Windows.Forms.Label();
            this.dateTextBox = new System.Windows.Forms.TextBox();
            this.onDayLabel = new System.Windows.Forms.Label();
            this.onDayTextBox = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // descriptionHeaderLabel
            // 
            this.descriptionHeaderLabel.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.descriptionHeaderLabel.Location = new System.Drawing.Point(12, 26);
            this.descriptionHeaderLabel.Name = "descriptionHeaderLabel";
            this.descriptionHeaderLabel.Size = new System.Drawing.Size(112, 24);
            this.descriptionHeaderLabel.TabIndex = 0;
            this.descriptionHeaderLabel.Text = "Header description:";
            this.descriptionHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // descriptionHeaderTextBox
            // 
            this.descriptionHeaderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionHeaderTextBox.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.descriptionHeaderTextBox.Location = new System.Drawing.Point(130, 30);
            this.descriptionHeaderTextBox.Name = "descriptionHeaderTextBox";
            this.descriptionHeaderTextBox.Size = new System.Drawing.Size(463, 23);
            this.descriptionHeaderTextBox.TabIndex = 1;
            // 
            // descriptionFooterLabel
            // 
            this.descriptionFooterLabel.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.descriptionFooterLabel.Location = new System.Drawing.Point(15, 61);
            this.descriptionFooterLabel.Name = "descriptionFooterLabel";
            this.descriptionFooterLabel.Size = new System.Drawing.Size(109, 24);
            this.descriptionFooterLabel.TabIndex = 0;
            this.descriptionFooterLabel.Text = "Footer description:";
            this.descriptionFooterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // descriptionFooterTextBox
            // 
            this.descriptionFooterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionFooterTextBox.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.descriptionFooterTextBox.Location = new System.Drawing.Point(130, 62);
            this.descriptionFooterTextBox.Name = "descriptionFooterTextBox";
            this.descriptionFooterTextBox.Size = new System.Drawing.Size(463, 23);
            this.descriptionFooterTextBox.TabIndex = 1;
            // 
            // descriptionFooter2Label
            // 
            this.descriptionFooter2Label.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.descriptionFooter2Label.Location = new System.Drawing.Point(15, 93);
            this.descriptionFooter2Label.Name = "descriptionFooter2Label";
            this.descriptionFooter2Label.Size = new System.Drawing.Size(109, 24);
            this.descriptionFooter2Label.TabIndex = 0;
            this.descriptionFooter2Label.Text = "Footer description 2:";
            this.descriptionFooter2Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // descriptionFooter2TextBox
            // 
            this.descriptionFooter2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionFooter2TextBox.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.descriptionFooter2TextBox.Location = new System.Drawing.Point(130, 94);
            this.descriptionFooter2TextBox.Name = "descriptionFooter2TextBox";
            this.descriptionFooter2TextBox.Size = new System.Drawing.Size(463, 23);
            this.descriptionFooter2TextBox.TabIndex = 1;
            // 
            // filePathLabel
            // 
            this.filePathLabel.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.filePathLabel.Location = new System.Drawing.Point(15, 126);
            this.filePathLabel.Name = "filePathLabel";
            this.filePathLabel.Size = new System.Drawing.Size(109, 24);
            this.filePathLabel.TabIndex = 0;
            this.filePathLabel.Text = "File path:";
            this.filePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // filePathTextBox
            // 
            this.filePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filePathTextBox.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.filePathTextBox.Location = new System.Drawing.Point(130, 126);
            this.filePathTextBox.Name = "filePathTextBox";
            this.filePathTextBox.Size = new System.Drawing.Size(463, 23);
            this.filePathTextBox.TabIndex = 1;
            // 
            // addresCCLabel
            // 
            this.addresCCLabel.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.addresCCLabel.Location = new System.Drawing.Point(18, 158);
            this.addresCCLabel.Name = "addresCCLabel";
            this.addresCCLabel.Size = new System.Drawing.Size(106, 24);
            this.addresCCLabel.TabIndex = 0;
            this.addresCCLabel.Text = "CC address:";
            this.addresCCLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // addresCCTextBox
            // 
            this.addresCCTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addresCCTextBox.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.addresCCTextBox.Location = new System.Drawing.Point(130, 158);
            this.addresCCTextBox.Name = "addresCCTextBox";
            this.addresCCTextBox.Size = new System.Drawing.Size(463, 23);
            this.addresCCTextBox.TabIndex = 1;
            // 
            // contentLabel
            // 
            this.contentLabel.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.contentLabel.Location = new System.Drawing.Point(15, 191);
            this.contentLabel.Name = "contentLabel";
            this.contentLabel.Size = new System.Drawing.Size(109, 24);
            this.contentLabel.TabIndex = 0;
            this.contentLabel.Text = "Contents :";
            this.contentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // contentTextBox
            // 
            this.contentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentTextBox.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.contentTextBox.Location = new System.Drawing.Point(130, 191);
            this.contentTextBox.Name = "contentTextBox";
            this.contentTextBox.Size = new System.Drawing.Size(463, 23);
            this.contentTextBox.TabIndex = 1;
            // 
            // send
            // 
            this.send.Font = new System.Drawing.Font("Tahoma", 10.5F);
            this.send.Location = new System.Drawing.Point(19, 311);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(161, 47);
            this.send.TabIndex = 2;
            this.send.Text = "Send";
            this.send.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // update
            // 
            this.update.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.update.Font = new System.Drawing.Font("Tahoma", 10.5F);
            this.update.Location = new System.Drawing.Point(212, 311);
            this.update.Name = "update";
            this.update.Size = new System.Drawing.Size(161, 47);
            this.update.TabIndex = 3;
            this.update.Text = "Update";
            this.update.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // preview
            // 
            this.preview.Font = new System.Drawing.Font("Tahoma", 10.5F);
            this.preview.Location = new System.Drawing.Point(408, 311);
            this.preview.Name = "preview";
            this.preview.Size = new System.Drawing.Size(161, 47);
            this.preview.TabIndex = 4;
            this.preview.Text = "Preview";
            this.preview.Click += new System.EventHandler(this.PreviewButton_Click);
            // 
            // dateLabel
            // 
            this.dateLabel.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.dateLabel.Location = new System.Drawing.Point(3, 257);
            this.dateLabel.Name = "dateLabel";
            this.dateLabel.Size = new System.Drawing.Size(121, 24);
            this.dateLabel.TabIndex = 0;
            this.dateLabel.Text = "Current date* :";
            this.dateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dateTextBox
            // 
            this.dateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTextBox.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.dateTextBox.ForeColor = System.Drawing.Color.Gray;
            this.dateTextBox.Location = new System.Drawing.Point(130, 257);
            this.dateTextBox.Name = "dateTextBox";
            this.dateTextBox.Size = new System.Drawing.Size(463, 23);
            this.dateTextBox.TabIndex = 1;
            this.dateTextBox.Text = "yyyyMMdd";
            // 
            // onDayLabel
            // 
            this.onDayLabel.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.onDayLabel.Location = new System.Drawing.Point(15, 224);
            this.onDayLabel.Name = "onDayLabel";
            this.onDayLabel.Size = new System.Drawing.Size(109, 24);
            this.onDayLabel.TabIndex = 0;
            this.onDayLabel.Text = "On day :";
            this.onDayLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // onDayTextBox
            // 
            this.onDayTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.onDayTextBox.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.onDayTextBox.Location = new System.Drawing.Point(130, 224);
            this.onDayTextBox.Name = "onDayTextBox";
            this.onDayTextBox.Size = new System.Drawing.Size(463, 23);
            this.onDayTextBox.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(605, 386);
            this.Controls.Add(this.descriptionHeaderLabel);
            this.Controls.Add(this.descriptionHeaderTextBox);
            this.Controls.Add(this.descriptionFooterLabel);
            this.Controls.Add(this.descriptionFooterTextBox);
            this.Controls.Add(this.descriptionFooter2Label);
            this.Controls.Add(this.descriptionFooter2TextBox);
            this.Controls.Add(this.filePathLabel);
            this.Controls.Add(this.filePathTextBox);
            this.Controls.Add(this.addresCCLabel);
            this.Controls.Add(this.addresCCTextBox);
            this.Controls.Add(this.contentLabel);
            this.Controls.Add(this.contentTextBox);
            this.Controls.Add(this.dateLabel);
            this.Controls.Add(this.dateTextBox);
            this.Controls.Add(this.onDayLabel);
            this.Controls.Add(this.onDayTextBox);
            this.Controls.Add(this.send);
            this.Controls.Add(this.update);
            this.Controls.Add(this.preview);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        private int num2;
        private int num1;
        private int int16_1;
        private SqlConnection connection;
        private SqlConnection connection2;
        private string baseDirectory;
        private Label descriptionHeaderLabel;
        private TextBox descriptionHeaderTextBox;
        private Label descriptionFooterLabel;
        private TextBox descriptionFooterTextBox;
        private Label descriptionFooter2Label;
        private TextBox descriptionFooter2TextBox;
        private Label filePathLabel;
        private TextBox filePathTextBox;
        private Label addresCCLabel;
        private TextBox addresCCTextBox;
        private Label contentLabel;
        private TextBox contentTextBox;
        private Label onDayLabel;
        private DateTimePicker onDayTextBox;
        private Button send;
        private Button update;
        private Button preview;
        private Label dateLabel;
        private TextBox dateTextBox;

        #endregion
    }
}