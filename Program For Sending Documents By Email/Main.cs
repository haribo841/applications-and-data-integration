using ADODB;
using cdn_api;
using Hydra;
using Hydra.Lite; //ERP libraries
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
namespace DocumentsViaEmailSender
{
    [SubscribeProcedure(Procedures.tableElement, "📧")]
    public class Main : Callback
    {
        private ClaWindow RapButton;
        private ClaWindow FilterButton;
        public ClaWindow Window;
        public ClaWindow Bookmark;
        public ClaWindow List;
        public ClaWindow Anchor;
        private ClaWindow PositionsList;
        private int sValueGlobal = 0;
        private int sTypeGlobal = 0;
        private int sCompanyGlobal = 0;
        public override void Init()
        {
            AddSubscription(true, 0, Events.OpenWindow, new TakeEventDelegate(AddButton));
            AddSubscription(false, 0, Events.ResizeWindow, new TakeEventDelegate(WindowChange));
        }
        public bool AddButton(Procedures ProccessId, int ControlId, Events Event)
        {
            PositionsList = GetWindow().AllChildren["?List"];
            this.Window = this.GetWindow();
            this.Bookmark = this.Window.AllChildren["?byNumber:Tab"];
            this.List = this.Window.AllChildren["?ContractorsList"];
            RapButton = Bookmark.Children.Add(ControlTypes.button);
            RapButton.Visible = true;
            RapButton.TextRaw = "@";
            RapButton.ToolTipRaw = "Send confirmations";
            FilterButton = GetWindow().AllChildren["?PopupButton"];
            RapButton.Bounds = new Rectangle(FilterButton.Bounds.X - 19, FilterButton.Bounds.Y, 17, 15);
            RapButton.OnAfterMouseDown += new TakeEventDelegate(BtnOnMouseDown);
            RapButton.OnBeforeAccepted += new TakeEventDelegate(Load_OnBeforeAccepted);
            return true;
        }
        private bool WindowChange(Procedures ProccessId, int ControlId, Events Event)
        {
            FilterButton = GetWindow().AllChildren["?PopupButton"];
            RapButton.Bounds = new Rectangle(FilterButton.Bounds.X - 19, FilterButton.Bounds.Y, 17, 15);
            return true;
        }
        private bool Load_OnBeforeAccepted(Procedures ProcedureId, int ControlId, Events Event)
        {
            Runtime.WindowController.PostEvent(PositionsList.Id, Events.NewSelection);
            int sValue = (int)Konta.KKS_GIDNumer;
            sValueGlobal = sValue;
            int sType = (int)Konta.KKS_GIDTyp;
            sTypeGlobal = sType;
            int sCompany = (int)Konta.KKS_GIDFirma;
            sCompanyGlobal = sCompany;
            return true;
        }
        private bool BtnOnMouseDown(Procedures ProcedureId, int ControlId, Events Event)
        {
            int OperatorNumber = Runtime.ConfigurationDictionary.OperatorNumber;
            int ProperCenterID = Runtime.ConfigurationDictionary.ProperCenterID;
            try
            {
                Application.Run(new Parameters(OperatorNumber, ProperCenterID, sValueGlobal, sTypeGlobal, sCompanyGlobal));
            }
            catch (Exception e)
            {
                MessageBox.Show("Form opening error: " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return true;
        }
        public override void Cleanup()
        {
        }
    }
}