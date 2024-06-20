using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using PDKS_17042024;
using PDKS2.Class;
namespace DXApplication7
{
    public partial class cihazEkleForm : DevExpress.XtraEditors.XtraForm
    {
        ConnectionSQL con = new ConnectionSQL();
        private Connection conDevice;




        public cihazEkleForm()
        {
            InitializeComponent();
             conDevice = new Connection();
        }

        private void windowsUIButtonPanelMain_Click(object sender, EventArgs e)
        {
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            con.AddDevice(textEdit1.Text, textEdit11.Text, Convert.ToInt32(textEdit12.Text));
            MessageBox.Show("Cihaz Başarıyla Eklendi!!");
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}