using cdn_api;
using Hydra; //ERP libraries
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Identity.Client;
using RestSharp;
namespace DocumentsViaEmailSender
{
    class Program
    {
        public static void SendEmail(int int16_1, SqlConnection connection)
        {
            connection.Open();
            SqlCommand sqlCommand1 = new SqlCommand("SELECT Ope_Ident from cdn.OpeKarty where Ope_GIDNumer=@OpeNumer", connection);
            sqlCommand1.Parameters.Add("@OpeNumer", SqlDbType.Int);
            sqlCommand1.Parameters["@OpeNumer"].Value = (object)int16_1;
            _ = sqlCommand1.ExecuteScalar().ToString();
            SqlCommand selectCommand = new SqlCommand("SELECT NazwaP, WartoscP from cdn.ONE_SaldaKonfiguracja", connection);
            DataTable dataTable = new DataTable();
            new SqlDataAdapter(selectCommand).Fill(dataTable);
            DataRow[] source1 = dataTable.Select("NazwaP = 'SmtpServer'");
            if (source1.Count() != 0)
                _ = source1[0]["WartoscP"].ToString();
            DataRow[] source2 = dataTable.Select("NazwaP = 'SmtpPort'");
            if (source2.Count() != 0 && int.TryParse(source2[0]["WartoscP"].ToString(), out int result))
                _ = result;
            DataRow[] source3 = dataTable.Select("NazwaP = 'SmtpFromAdress'");
            if (source3.Count() != 0)
                _ = source3[0]["WartoscP"].ToString();
            DataRow[] source4 = dataTable.Select("NazwaP = 'SmtpUser'");
            string userName = "";
            if (source4.Count() != 0)
                userName = source4[0]["WartoscP"].ToString();
            DataRow[] source5 = dataTable.Select("NazwaP = 'SmtpHaslo'");
            if (source5.Count() != 0)
                _ = source5[0]["WartoscP"].ToString();
            DataRow[] source6 = dataTable.Select("NazwaP = 'SmtpSSL'");
            if (source6.Count() != 0)
            {
                switch (source6[0]["WartoscP"].ToString())
                {
                    case "TAK":
                        _ = new bool?(true);
                        break;
                    case "NIE":
                        _ = new bool?(false);
                        break;
                }
            }
            DataRow[] source7 = dataTable.Select("NazwaP = 'OpisNag'");
            if (source7.Count() != 0)
                _ = source7[0]["WartoscP"].ToString();
            DataRow[] source8 = dataTable.Select("NazwaP = 'OpisStopki'");
            if (source8.Count() != 0)
                _ = source8[0]["WartoscP"].ToString();
            DataRow[] source9 = dataTable.Select("NazwaP = 'OpisStopki2'");
            if (source9.Count() != 0)
                _ = source9[0]["WartoscP"].ToString();
            DataRow[] source10 = dataTable.Select("NazwaP = 'StatusKon'");
            if (source10.Count() != 0)
                _ = source10[0]["WartoscP"].ToString();
            DataRow[] source11 = dataTable.Select("NazwaP = 'KontoOd'");
            if (source11.Count() != 0)
                _ = source11[0]["WartoscP"].ToString();
            DataRow[] source12 = dataTable.Select("NazwaP = 'KontoDo'");
            if (source12.Count() != 0)
                _ = source12[0]["WartoscP"].ToString();
            DataRow[] source13 = dataTable.Select("NazwaP = 'NaDzien'");
            if (((IEnumerable<DataRow>)source13).Count<DataRow>() != 0)
                _ = source13[0]["WartoscP"].ToString();
            DataRow[] source14 = dataTable.Select("NazwaP = 'SciezkaPliku'");
            if (((IEnumerable<DataRow>)source14).Count<DataRow>() != 0)
                _ = source14[0]["WartoscP"].ToString();
            DataRow[] source15 = dataTable.Select("NazwaP = 'Tresc'");
            if (((IEnumerable<DataRow>)source15).Count<DataRow>() != 0)
                _ = source15[0]["WartoscP"].ToString();
            DataRow[] source16 = dataTable.Select("NazwaP = 'AdresDW'");
            if (((IEnumerable<DataRow>)source16).Count<DataRow>() != 0)
                _ = source16[0]["WartoscP"].ToString();
            if (userName == "" || !new bool?().HasValue)
            {
                _ = (int)MessageBox.Show("Nie wypełniono wszystkich wymaganych parametrów w tabeli konfiguracyjnej.\r\nWysyłanie wiadomości zostanie zatrzymane.");
                _ = (int)MessageBox.Show("Host: " + "" + "\nnum6 " + 0.ToString() + "\naddress " + "" + "\npassword " + "");
            }
        }
    }
}