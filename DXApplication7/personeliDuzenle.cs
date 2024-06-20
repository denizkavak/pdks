using DevExpress.CodeParser;
using PDKS_17042024;
using PDKS2.Class;
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
    public partial class personeliDuzenle : Form
    {

        public string perID;
        ConnectionSQL con = new ConnectionSQL();
        Connection conDevice;

        public personeliDuzenle()
        {
            InitializeComponent();
            this.con = new ConnectionSQL();
            this.conDevice = new Connection();
        }

        private void SetupYetkiComboBox()
        {
            comboBoxYetki.Items.Clear(); // Mevcut öğeleri temizle
            comboBoxYetki.Items.Add("Normal Kullanıcı");
            comboBoxYetki.Items.Add("Süpervizör");
        }

        private void SetupAktifComboBox()
        {
            comboBoxAktif.Items.Clear(); // Mevcut öğeleri temizle
            comboBoxAktif.Items.Add("Aktif");
            comboBoxAktif.Items.Add("Deaktif");
        }

        private void SetupVardiyaComboBox()
        {
            DataTable vardiyalar = con.GetVardiyaOptions();
            comboBoxVardiya.DataSource = null;
            comboBoxVardiya.DisplayMember = "vardiya_adi";
            comboBoxVardiya.ValueMember = "vardiya_id";
            comboBoxVardiya.DataSource = vardiyalar;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Hide();
            conDevice = new Connection();
        }



        private void LoadPersonelData()
        {
            DataRow personelData = con.GetPersonelById(perID);
            if (personelData != null)
            {
                textEditAdi.Text = personelData["personel_adi"].ToString();
                textEditSoyadi.Text = personelData["personel_soyadi"].ToString();

                // comboBoxYetki ve comboBoxAktif öğelerinin yüklenmesini sağlayın.

                SetupYetkiComboBox();
                SetupAktifComboBox();

                // comboBoxYetki ayarı
                int privilegeIndex = Convert.ToInt32(personelData["personel_privilege"]) == 3 ? 1 : 0;
                if (comboBoxYetki.Items.Count > privilegeIndex) // Öğelerin yüklendiğinden emin ol
                    comboBoxYetki.SelectedIndex = privilegeIndex;

                // comboBoxAktif ayarı
                int enabledIndex = Convert.ToBoolean(personelData["personel_Enabled"]) ? 0 : 1;
                if (comboBoxAktif.Items.Count > enabledIndex) // Öğelerin yüklendiğinden emin ol
                    comboBoxAktif.SelectedIndex = enabledIndex;

                textEditKartNo.Text = personelData["personel_kartNo"].ToString();
                textEditSifre.Text = personelData["personel_sifre"].ToString();

                // comboBoxVardiya.SelectedItem kontrolü
                if (personelData != null && personelData["vardiyaId"] != DBNull.Value)
                {
                    var vardiyaId = Convert.ToInt32(personelData["vardiyaId"]);
                    if (vardiyaId > 0 && comboBoxVardiya.Items.Cast<ComboBoxItem>().Any(item => Convert.ToInt32(item.Value) == vardiyaId))
                    {
                        comboBoxVardiya.SelectedValue = vardiyaId;
                    }
                    else
                    {
                        comboBoxVardiya.SelectedIndex = -1; // Varsayılan olarak seçim yok
                        MessageBox.Show("Seçilen personel için uygun vardiya bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    comboBoxVardiya.SelectedIndex = -1; // Varsayılan olarak seçim yok
                    MessageBox.Show("Seçilen personel için vardiya bilgisi bulunamadı veya personel bilgileri eksik.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                MessageBox.Show("Personel bilgileri yüklenirken bir hata oluştu.");
            }
        }


        private void personeliDuzenle_Activated(object sender, EventArgs e)
        {
            SetupYetkiComboBox();
            SetupAktifComboBox();
            SetupVardiyaComboBox();
            LoadPersonelData();
        }

        private void guncelle_Click(object sender, EventArgs e)
        {
            string firstName = textEditAdi.Text;
            string lastName = textEditSoyadi.Text;
            int privilege = (comboBoxYetki.SelectedItem as ComboBoxItem).Value;
            bool enabled = comboBoxAktif.SelectedItem.ToString() == "Aktif";
            string cardNumber = textEditKartNo.Text;
            string password = textEditSifre.Text;
            int vardiyaID = Convert.ToInt32(comboBoxVardiya.SelectedValue);

            // Veritabanında güncelle
            bool isUpdated = con.UpdatePersonel(perID, firstName, lastName, privilege, enabled, cardNumber, password, vardiyaID);

            if (isUpdated)
            {
                // Cihazlarda güncelle
                conDevice.UpdateUserOnAllDevices(perID, $"{firstName} {lastName}", privilege, enabled, cardNumber, password);
                MessageBox.Show("Personel bilgileri başarıyla güncellendi.");
                temizle();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Personel bilgileri güncellenirken bir hata oluştu.");
            }
        }

       
        private void temizle()
        {
            textEditAdi.Text = "";
            textEditSoyadi.Text = "";
            comboBoxYetki.SelectedIndex = -1;
            comboBoxAktif.SelectedIndex = -1;
            textEditKartNo.Text = "";
            textEditSifre.Text = "";
            comboBoxVardiya.SelectedIndex = -1;
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
    }
}
