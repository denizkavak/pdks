using PDKS_17042024;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DXApplication7
{
    public partial class login : DevExpress.XtraEditors.XtraForm
    {
        ConnectionSQL sql = new ConnectionSQL();
        public login()
        {
            InitializeComponent();
        }
        Form1 from = new Form1();
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            string kullaniciAdi = kullaniciAdiTxt.Text;
            string sifre = sifreTxt.Text;

            if (sql.CheckKullanici(kullaniciAdi, sifre))
            { 
                this.Hide();
                from.Show();
            }
            else
            {
                MessageBox.Show("Kullanıcı adı veya şifre hatalı.");
            }
        }

    }
}
