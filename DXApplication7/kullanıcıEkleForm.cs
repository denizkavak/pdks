using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Filtering.Templates;
using DevExpress.XtraReports.Design;
using DevExpress.XtraRichEdit.Commands.Internal;
using DevExpress.XtraRichEdit.Layout.Engine;
using DevExpress.XtraRichEdit.Model;
using PDKS_17042024;
using PDKS2.Class;
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
using static DevExpress.XtraEditors.Mask.MaskSettings;

namespace DXApplication7
{
    public partial class kullanıcıEkleForm : DevExpress.XtraEditors.XtraForm
    {
        ConnectionSQL con = new ConnectionSQL();

        private PDKS2.Class.Connection conDevice;

        public kullanıcıEkleForm()
        {
            InitializeComponent();
            conDevice = new PDKS2.Class.Connection();
        } 

        private void buttonİptal_Click(object sender, EventArgs e)
        {
            this.Hide();
            temizle();
        }

        private void buttonEkle_Click(object sender, EventArgs e)
        {
            string firstName = textEditAdi.Text;
            string lastName = textEditSoyadi.Text;
            string fullName = $"{firstName} {lastName}";
            int privilege = (comboBoxYetki.SelectedItem as ComboBoxItem).Value;  // Yetki ComboBox'undan seçilen değeri alır
            bool enabled = comboBoxAktif.SelectedItem.ToString() == "Aktif";  // Aktiflik durumunu belirler
            string cardNumber = textEditKartNo.Text;
            string sifre = textEditSifre.Text;
            int vardiyaID = (comboBoxVardiya.SelectedItem as ComboBoxItem).Value;  // Vardiya ComboBox'undan seçilen değeri alır

            ConnectionSQL sqlConnection = new ConnectionSQL();
            int userID = sqlConnection.SaveUserToDatabase(firstName, lastName, privilege, enabled, cardNumber,sifre, vardiyaID); // ID döner

            // Kullanıcıyı cihazlara eklemek için
            conDevice.AddUserToAllDevices(userID.ToString(), fullName, privilege, enabled, cardNumber,sifre); // Kullanıcı ID ve diğer bilgiler

            MessageBox.Show("Kullanıcı başarıyla eklendi.");

            temizle();
            
            MessageBox.Show("Kullanıcı başarıyla eklendi.");
        }
       

        private void LoadFormOptions()
        {
            comboBoxYetki.Items.Add(new ComboBoxItem("Personel", 0));  // 0: Normal Kullanıcı
            comboBoxYetki.Items.Add(new ComboBoxItem("Süpervizör", 3));  // 3: Süpervizör

            comboBoxAktif.Items.Add("Aktif");
            comboBoxAktif.Items.Add("Deaktif");

            var vardiyalar = con.GetVardiyalar();  // Vardiyaları veritabanından al

            foreach (var vardiya in vardiyalar)
            {
                comboBoxVardiya.Items.Add(new ComboBoxItem(vardiya.VardiyaAdi, vardiya.VardiyaID));
            }

            comboBoxVardiya.DisplayMember = "Text";
            comboBoxVardiya.ValueMember = "Value";
        }

        private void temizle()
        {
            textEditAdi.Text = "";
            textEditKartNo.Text = "";
            textEditSoyadi.Text = "";
            comboBoxAktif.Text = "";
            comboBoxVardiya.Text = "";
            comboBoxYetki.Text = "";
            textEditSifre.Text = "";
        }
        public class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }

            public ComboBoxItem(string text, int value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }

        private void kullanıcıEkleForm_Load(object sender, EventArgs e)
        {
            LoadFormOptions(); 
            
        }
    }
}