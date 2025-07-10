using ADODB;
using cdn_api;
using Hydra; //ERP libraries
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using DocumentsViaEmailSender;
using System.Xml.Linq;
namespace DocumentsViaEmailSender
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        const string TenantId = "{catalogID}";
        private readonly string _tenantId = TenantId;
        const string ClientId = "objectID";
        private readonly string _clientId = ClientId;
        const string ClientSecret = "value_expiress";
        private readonly string _clientSecret = ClientSecret;
        private readonly string _smtpHost = "smtp.website.com";
        const string FromUser = "mail@website.com";
        private readonly int _smtpPort = 587;
        private readonly string _userEmail = FromUser;
        public int APIversion { get; private set; } = 2137; 
        public object STypeGlobal { get; private set; } = 0;
        public object SCompanyGlobal { get; private set; } = 0;
        public object SValueGlobal { get; private set; } = 0;
        public int GlobalSession { get; private set; } = 0;

        public Form1(int operatorNumber, int properCenter, int sValue, int sType, int sCompany)

        {
            SValueGlobal = sValue;
            STypeGlobal = sType;
            SCompanyGlobal = sCompany;
            APIversion = 2137;
            InitializeComponent();
            try
            {
                ERPLoginInfo_2137 ERPLoginInfo2137 = new()
                {
                    Version = APIversion,
                    CreateYourOwnSession = 0,
                    Vignette = -1,
                    BatchMode = 1,
                    ProgramID = "2137"
                };

                // Fix for CS0206: A non ref-returning property or indexer may not be used as an out or ref value
                // The issue occurs because `sesjaGlobalna` is a property, and properties cannot be passed as `ref` or `out` parameters.
                // To fix this, we need to use a local variable instead of the property.

                int localGlobalSession = GlobalSession; // Use a local variable to hold the value of sesjaGlobalna
                int ApiLoginCode = cdn_api.cdn_api.ERPLogin(ERPLoginInfo2137, ref localGlobalSession);
                GlobalSession = localGlobalSession; // Update the property with the value from the local variable
                if (ApiLoginCode != 0)
                {
                    int LoginErrorCode = (int)MessageBox.Show("Login error! (API " + ApiLoginCode.ToString() + ")");
                }
                else
                {
                    ERPLoginInfo_2137 ERPLoginInfo2137 = new()
                    {
                        Wersja = APIversion
                    };
                    if (cdn_api.cdn_api.ERPConnection(ERPLoginInfo2137) != 0)
                    {
                        int LoginErrorCode = (int)MessageBox.Show("SQL database connection error!");
                    }
                    else
                    {
                        SqlConnection connection = new();
                        SqlConnection connection2 = new();
                    }
                    try
                    {
                        connection = new SqlConnection(connectionInfo2137.ConnectString.Substring(connectionInfo2137.ConnectString.IndexOf("Data Source"), connectionInfo2137.ConnectString.Length - connectionInfo2137.ConnectString.IndexOf("Data Source")));
                        connection2 = new SqlConnection((connectionInfo2137.ConnectString.Substring(connectionInfo2137.ConnectString.IndexOf("Data Source"), connectionInfo2137.ConnectString.Length - connectionInfo2137.ConnectString.IndexOf("Data Source")) + "User ID = ...; Password=...").Replace("Integrated Security=SSPI", ""));
                    }
                    catch
                    {
                        int LoginErrorCode = (int)MessageBox.Show("Error generating connection " + connectionInfo2137.ConnectString);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Form opening error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                int LoginErrorCode = (int)MessageBox.Show(ex.Message);
                Console.ReadLine();
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (dateTextBox.Text == DateTime.Today.ToString("yyyyMMdd"))
            {
                Program.SendEmail(this.int16_1, this.connection);
                this.Close();
            }
            else
            {
                MessageBox.Show("Error. To send, please fill in the current date in the format yyyyMMdd", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                dateTextBox.BorderStyle = BorderStyle.None;
                Pen p = new Pen(Color.Red);
                Graphics g = Form1.ActiveForm.CreateGraphics();
                int variance = 2;
                g.DrawRectangle(p, new Rectangle(dateTextBox.Location.X - variance, dateTextBox.Location.Y - variance, dateTextBox.Width + variance, dateTextBox.Height + variance));
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (dateTextBox.Text == DateTime.Today.ToString("yyyyMMdd"))
                {
                    this.connection2.Open();
                    this.UpdateConfiguration("Header description", this.descriptionHeaderTextBox.Text);
                    this.UpdateConfiguration("Footer description", this.descriptionFooterTextBox.Text);
                    this.UpdateConfiguration("Footer description 2", this.descriptionFooter2TextBox.Text);
                    this.UpdateConfiguration("File Path", this.filePathTextBox.Text.Substring(this.filePathTextBox.Text.Length - 1, 1) == "\\" ? this.filePathTextBox.Text : this.filePathTextBox.Text + "\\");
                    this.UpdateConfiguration("Content", this.contentTextBox.Text);
                    this.UpdateConfiguration("CC Address", this.addresCCTextBox.Text);
                    this.UpdateConfiguration("On day", this.onDayTextBox.Value.ToString("yyyyMMdd"));
                    this.connection2.Close();
                    MessageBox.Show("Updated configuration", "Announcement", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Error. To send, please enter the current date in the format yyyyMMdd.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    dateTextBox.BorderStyle = BorderStyle.None;
                    Pen p = new Pen(Color.Red);
                    Graphics g = Form1.ActiveForm.CreateGraphics();
                    int variance = 2;
                    g.DrawRectangle(p, new Rectangle(dateTextBox.Location.X - variance, dateTextBox.Location.Y - variance, dateTextBox.Width + variance, dateTextBox.Height + variance));
                }
            }
            catch (Exception ex)
            {
                _ = (int)MessageBox.Show("Configuration update error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void UpdateConfiguration(string name, string value)
        {
            SqlCommand sqlCommand = new SqlCommand("UPDATE [database].[yable] SET [valueParameter] = @value WHERE nameParameter = @name", this.connection2);
            sqlCommand.Parameters.Add("@value", SqlDbType.VarChar, (int)byte.MaxValue);
            sqlCommand.Parameters.Add("@name", SqlDbType.VarChar, (int)byte.MaxValue);
            sqlCommand.Parameters["@value"].Value = (object)value;
            sqlCommand.Parameters["@name"].Value = (object)name;
            sqlCommand.ExecuteNonQuery();
        }
        private void PreviewButton_Click(object sender, EventArgs e)
        {
            Print(1);
        }

        private void Print(int OutputDevice, int PrintToFile = 0)
        {
            cdn_api.ERPPrintInfo_2137 ERPPrintInfo2137 = new()
            {
                Version = APIversion,
                Source = 1,
                Print = 13,
                Format = 1,
                FiltrSQL = $"table_GIDTyp={STypeGlobal} and table_GIDFirma={SCompanyGlobal} and table_GIDNumer={SValueGlobal}",
                Device = OutputDevice,//Screen
                PrintToFile = PrintToFile,//Default 0/no, which means preview
                TargetFile = "C:\\Directory\\Folder"
            };
            int ErrorCode = cdn_api.cdn_api.ERPDoSpecifiedPrint(ERPPrintInfo2137);
            cdn_api.ERPMessageInfo_2137 ERPMessageInfo2137 = new()
            {
                Version = APIversion,
                Function = 87,//ExecuteGivenPrint
                Error = ErrorCode
            };
            cdn_api.cdn_api.ERPErrorDescription(ERPMessageInfo2137);
            cdn_api.cdn_api.ERPLogout(GlobalSession);
            this.Close();
        }
        private void Panel_MouseEnter(object sender, EventArgs e)
        {
            AllControls_MouseEnter();
        }
        private void Panel_MouseLeave(object sender, EventArgs e)
        {
            AllControls_MouseLeave();
        }
        private void AllControls_MouseEnter()
        {
            preview.UseVisualStyleBackColor = false;
            preview.BackColor = Color.Snow;
            preview.FlatStyle = FlatStyle.Flat;
            preview.FlatAppearance.BorderColor = Color.Red;
            preview.FlatAppearance.BorderSize = 2;
            preview.ForeColor = Color.Red;
        }
        private void AllControls_MouseLeave()
        {
            preview.UseVisualStyleBackColor = true;
            preview.FlatStyle = FlatStyle.Standard;
            preview.FlatAppearance.BorderColor = send.FlatAppearance.BorderColor;
            preview.FlatAppearance.BorderSize = 0;
            preview.ForeColor = send.ForeColor;
        }
        private new void Load(object sender, EventArgs e)
        {
        }
    }
}