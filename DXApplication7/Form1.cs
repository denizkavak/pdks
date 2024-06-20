using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using PDKS_17042024;
using PDKS2.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PDKS2.Class.Connection;

namespace DXApplication7
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        public static readonly cihazEkleForm cihazEkleForm = new cihazEkleForm();
        public static readonly kullanıcıEkleForm kullanıcıEkleForm = new kullanıcıEkleForm();
        public static readonly Form2 parmakIzı = new Form2();
        public static readonly personeliDuzenle per = new personeliDuzenle();

        ConnectionSQL con = new ConnectionSQL();
        String cihazId = null;
        String cihazIp = null;
        String userId = null;
        int cihazPort;
        public int parmakIziKayıt;
        private Connection conDevice;

        public Form1()
        {
            InitializeComponent();
            SetupDataGridView();
            sqlDataSource1.Fill();
            SetupDateEdit();
            SetupCheckedComboBoxPersonel();
            conDevice = new Connection();
            LoadVardiyalar();
        }

        private void tileBar_SelectedItemChanged(object sender, TileItemEventArgs e)
        {
            navigationFrame.SelectedPageIndex = tileBarGroupTables.Items.IndexOf(e.Item);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.pdks_devicesTableAdapter.Fill(this.pDKSDataSet1.pdks_devices);
            this.pdks_personelTableAdapter.Fill(this.pDKSDataSet.pdks_personel);

            dataGridDevices.DataSource = con.GetDevices();
        }

        private void fillByToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.pdks_personelTableAdapter.FillBy(this.pDKSDataSet.pdks_personel);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void cihazEkle_Click(object sender, EventArgs e)
        {
            cihazEkleForm.Show();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            dataGridDevices.DataSource = con.GetDevices();
            dataGridUsers.DataSource = con.GetPerson();
        }

        private void cihazSil_Click(object sender, EventArgs e)
        {
            if (cihazId != null)
            {
                DialogResult result = MessageBox.Show("Cihazı silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.OK)
                {
                    con.RemoveDevice(cihazId);
                    dataGridDevices.DataSource = con.GetDevices();
                }
            }
            else
            {
                MessageBox.Show("Lütfen Cihaz Seçiniz!");
            }
        }
        //dataGridCihaz tablosundan cihaza tıklandığında bilgileri alır
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                cihazId = dataGridDevices.Rows[e.RowIndex].Cells["Cihaz Id"].Value.ToString();
                cihazIp = dataGridDevices.Rows[e.RowIndex].Cells["İp Adresi"].Value.ToString();
                cihazPort = Convert.ToInt32(dataGridDevices.Rows[e.RowIndex].Cells["Port"].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        
        //Personel ve Cihazlar Tablolarını ayarlar
        private void SetupDataGridView()
        {
            DataGridViewTextBoxColumn deviceIdColumn = new DataGridViewTextBoxColumn();
            deviceIdColumn.HeaderText = "Cihaz Id";
            deviceIdColumn.DataPropertyName = "device_id";
            deviceIdColumn.Name = "Cihaz Id";
            dataGridDevices.Columns.Add(deviceIdColumn);

            DataGridViewTextBoxColumn deviceNameColumn = new DataGridViewTextBoxColumn();
            deviceNameColumn.HeaderText = "Cihaz adı";
            deviceNameColumn.DataPropertyName = "device_name";
            deviceNameColumn.Name = "Cihaz adı";
            dataGridDevices.Columns.Add(deviceNameColumn);

            DataGridViewTextBoxColumn deviceIpColumn = new DataGridViewTextBoxColumn();
            deviceIpColumn.HeaderText = "İp Adresi";
            deviceIpColumn.DataPropertyName = "device_ipAdress";
            deviceIpColumn.Name = "İp Adresi";
            dataGridDevices.Columns.Add(deviceIpColumn);

            DataGridViewTextBoxColumn portColumn = new DataGridViewTextBoxColumn();
            portColumn.HeaderText = "Port";
            portColumn.DataPropertyName = "device_port";
            portColumn.Name = "Port";
            dataGridDevices.Columns.Add(portColumn);

            DataGridViewTextBoxColumn firmwareColumn = new DataGridViewTextBoxColumn();
            firmwareColumn.HeaderText = "Firmware";
            firmwareColumn.DataPropertyName = "device_firmware";
            firmwareColumn.Name = "Firmware";
            dataGridDevices.Columns.Add(firmwareColumn);

            DataGridViewTextBoxColumn serialColumn = new DataGridViewTextBoxColumn();
            serialColumn.HeaderText = "Seri Numarası";
            serialColumn.DataPropertyName = "device_serial";
            serialColumn.Name = "Seri Numarası";
            dataGridDevices.Columns.Add(serialColumn);

            dataGridDevices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            DataGridViewTextBoxColumn userIdColumn = new DataGridViewTextBoxColumn();
            userIdColumn.HeaderText = "User Id";
            userIdColumn.DataPropertyName = "personel_id";
            userIdColumn.Name = "User Id";
            dataGridUsers.Columns.Add(userIdColumn);

            DataGridViewTextBoxColumn personelNameColumn = new DataGridViewTextBoxColumn();
            personelNameColumn.HeaderText = "Adı";
            personelNameColumn.DataPropertyName = "personel_adi";
            personelNameColumn.Name = "Adı";
            dataGridUsers.Columns.Add(personelNameColumn);

            DataGridViewTextBoxColumn personelSurnameColumn = new DataGridViewTextBoxColumn();
            personelSurnameColumn.HeaderText = "Soyadı";
            personelSurnameColumn.DataPropertyName = "personel_soyadi";
            personelSurnameColumn.Name = "Soyadı";
            dataGridUsers.Columns.Add(personelSurnameColumn);

            DataGridViewTextBoxColumn personelPrivilegeColumn = new DataGridViewTextBoxColumn();
            personelPrivilegeColumn.HeaderText = "Privelege";
            personelPrivilegeColumn.DataPropertyName = "personel_privilege";
            personelPrivilegeColumn.Name = "Privelege";
            dataGridUsers.Columns.Add(personelPrivilegeColumn);

            DataGridViewTextBoxColumn personelCardColumn = new DataGridViewTextBoxColumn();
            personelCardColumn.HeaderText = "Kart Numarası";
            personelCardColumn.DataPropertyName = "personel_kartNo";
            personelCardColumn.Name = "Kart Numarası";
            dataGridUsers.Columns.Add(personelCardColumn);

            DataGridViewTextBoxColumn personelSifreColumn = new DataGridViewTextBoxColumn();
            personelSifreColumn.HeaderText = "Şifre";
            personelSifreColumn.DataPropertyName = "personel_sifre";
            personelSifreColumn.Name = "Şifre";
            dataGridUsers.Columns.Add(personelSifreColumn);

            dataGridUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Vardiya bilgileri
            DataGridViewTextBoxColumn vardiyaIdColumn = new DataGridViewTextBoxColumn();
            vardiyaIdColumn.HeaderText = "Vardiya Id";
            vardiyaIdColumn.DataPropertyName = "vardiya_id";
            vardiyaIdColumn.Name = "Vardiya Id";
            dataGridVardiya.Columns.Add(vardiyaIdColumn);

            DataGridViewTextBoxColumn vardiyaAdiColumn = new DataGridViewTextBoxColumn();
            vardiyaAdiColumn.HeaderText = "Vardiya Adı";
            vardiyaAdiColumn.DataPropertyName = "vardiya_adi";
            vardiyaAdiColumn.Name = "Vardiya Adı";
            dataGridVardiya.Columns.Add(vardiyaAdiColumn);

            DataGridViewTextBoxColumn vardiyaBaslangicColumn = new DataGridViewTextBoxColumn();
            vardiyaBaslangicColumn.HeaderText = "Başlangıç";
            vardiyaBaslangicColumn.DataPropertyName = "vardiya_baslangic";
            vardiyaBaslangicColumn.Name = "Başlangıç";
            dataGridVardiya.Columns.Add(vardiyaBaslangicColumn);

            DataGridViewTextBoxColumn vardiyaBitisColumn = new DataGridViewTextBoxColumn();
            vardiyaBitisColumn.HeaderText = "Bitiş";
            vardiyaBitisColumn.DataPropertyName = "vardiya_bitis";
            vardiyaBitisColumn.Name = "Bitiş";
            dataGridVardiya.Columns.Add(vardiyaBitisColumn);

            DataGridViewTextBoxColumn gunDonumuColumn = new DataGridViewTextBoxColumn();
            gunDonumuColumn.HeaderText = "Gün Dönümü";
            gunDonumuColumn.DataPropertyName = "gunDonumu";
            gunDonumuColumn.Name = "Gün Dönümü";
            dataGridVardiya.Columns.Add(gunDonumuColumn);

            DataGridViewTextBoxColumn gunlerColumn = new DataGridViewTextBoxColumn();
            gunlerColumn.HeaderText = "Günler";
            gunlerColumn.DataPropertyName = "gunler";
            gunlerColumn.Name = "Günler";
            dataGridVardiya.Columns.Add(gunlerColumn);

            DataGridViewTextBoxColumn aciklamaColumn = new DataGridViewTextBoxColumn();
            aciklamaColumn.HeaderText = "Açıklama";
            aciklamaColumn.DataPropertyName = "aciklama";
            aciklamaColumn.Name = "Açıklama";
            dataGridVardiya.Columns.Add(aciklamaColumn);

            dataGridVardiya.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void cihazBagla_Click(object sender, EventArgs e)
        {
            conDevice.IpAddress = cihazIp;
            conDevice.Port = cihazPort;
            conDevice.Connect();
            if (!conDevice.Connect())
            {
                MessageBox.Show("Cihaza bağlanılamadı. Lütfen bağlantı ayarlarını kontrol ediniz.", "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string firmwareVersiyon = conDevice.GetFirmwareVersion();
                string seriNumarası = conDevice.GetSerialNumber();
                con.RegisterDevice(firmwareVersiyon, seriNumarası, Convert.ToInt32(cihazId));
                dataGridDevices.DataSource = con.GetDevices();
            }
        }
        //Hareket verilerini aktarıp veritabanına kayıt eder
        private void hareketVerileriAktarma_Click(object sender, EventArgs e)
        {
            try
            {
                var newMovements = conDevice.FetchAllTransactionData();
                conDevice.SaveMovementsToDatabase(newMovements);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //Cihazdan tüm kullanıcı verilerini çeker
        private void personelVerileriAktarma_Click(object sender, EventArgs e)
        {
            List<UserInfo> users = conDevice.GetAllUsers();
            conDevice.SaveUsersToDatabase(users);
        }
         
        //Kullanıcı EKleme Sayfasını Açar
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            kullanıcıEkleForm.Show();
        }

        //Personeller Tablosundan UserID alır
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                userId = dataGridUsers.Rows[e.RowIndex].Cells["User Id"].Value.ToString();
                per.perID = dataGridUsers.Rows[e.RowIndex].Cells["User Id"].Value.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //kullanıcı silme butonu
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (userId != null)
            {
                DialogResult result = MessageBox.Show("Kullanıcıyı silmek istediğinize emin misiniz??", "Onay", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                {
                    con.RemoveUser(userId);
                    dataGridUsers.DataSource = con.GetPerson();
                }
            }
            else
            {
                MessageBox.Show("Lütfen Cihaz Seçiniz!");
            }
        }

        //Parmak İzi Sayfasını Açar
        private void simpleButton4_Click(object sender, EventArgs e)
        {
            parmakIzı.Show();
            parmakIzı.id = Convert.ToInt32(userId);
        }

        #region Raporlama Sayfası
        private void simpleButtonRapor_Click(object sender, EventArgs e)
        {
            var selectedValues = checkedComboBoxPersonel.EditValue?.ToString();

            if (string.IsNullOrEmpty(selectedValues))
            {
                gridControlRapor.DataSource = null;
                MessageBox.Show("Lütfen en az bir personel seçiniz.");
                return;
            }

            DateTime baslangic = dateEditBaslangic.DateTime; // Seçilen başlangıç tarihi.
            DateTime bitis = dateEditBitis.DateTime; // Seçilen bitiş tarihi.

            var reportData = GetReportData(selectedValues, baslangic, bitis);
            gridControlRapor.DataSource = reportData;

            HighlightLateEarlyMovements(); // Rapor verilerini aldıktan sonra vurgulama yap
        }

        private void HighlightLateEarlyMovements()
        {
            GridView view = gridControlRapor.MainView as GridView;
            if (view == null)
                return;

            view.RowCellStyle += (sender, e) =>
            {
                if (e.Column.FieldName == "GeçKalmaSüresi" || e.Column.FieldName == "ErkenÇıkmaSüresi")
                {
                    string value = view.GetRowCellValue(e.RowHandle, e.Column).ToString();
                    if (value != "00:00")
                    {
                        e.Appearance.BackColor = Color.Red;
                        e.Appearance.ForeColor = Color.White;
                    }
                }
            };
        }

        private string GetAllUserIDs()
        {
            string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
            List<string> ids = new List<string>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT personel_id FROM pdks_personel";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ids.Add(reader["personel_id"].ToString());
                        }
                    }
                }
            }
            return String.Join(",", ids);
        }

        private void SetupCheckedComboBoxPersonel()
        {
            var dt = GetPersonelData();
            checkedComboBoxPersonel.Properties.DataSource = dt;
            checkedComboBoxPersonel.Properties.DisplayMember = "personel_adi";
            checkedComboBoxPersonel.Properties.ValueMember = "personel_id";
            checkedComboBoxPersonel.Properties.SelectAllItemVisible = true;
        }

        private void SetupDateEdit()
        {
            dateEditBaslangic.Properties.EditMask = "dd.MM.yyyy";
            dateEditBaslangic.Properties.Mask.UseMaskAsDisplayFormat = true;
            dateEditBitis.Properties.EditMask = "dd.MM.yyyy";
            dateEditBitis.Properties.Mask.UseMaskAsDisplayFormat = true;
            dateEditBaslangic.DateTime = DateTime.Today;
            dateEditBitis.DateTime = DateTime.Today;
        }

        private DataTable GetPersonelData()
        {
            string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT personel_id, personel_adi + ' ' + personel_soyadi AS personel_adi FROM pdks_personel ORDER BY personel_adi", conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        private DataTable GetReportData(string selectedUserIDs, DateTime baslangic, DateTime bitis)
        {
            string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var ids = selectedUserIDs.Split(',');
                var parameters = new List<string>();
                for (int i = 0; i < ids.Length; i++)
                {
                    parameters.Add("@PersonelID" + i);
                }

                string query = $@"
            SELECT 
                p.personel_adi + ' ' + p.personel_soyadi AS PersonelAdSoyad,
                v.vardiya_adi AS Vardiya,
                v.vardiya_baslangic AS VardiyaBaşlangıç,
                v.vardiya_bitis AS VardiyaBitiş,
                tarih AS Tarih,
                MIN(GirişSaati) AS İlkGiriş,
                MAX(ÇıkışSaati) AS SonÇıkış,
                İlkCihaz AS İlkGirişCihazı,
                SonCihaz AS SonÇıkışCihazı,
                CASE 
                    WHEN MIN(GirişSaati) > v.vardiya_baslangic THEN 
                        RIGHT('0' + CAST(DATEDIFF(MINUTE, v.vardiya_baslangic, MIN(GirişSaati)) / 60 AS VARCHAR), 2) + ':' + RIGHT('0' + CAST(DATEDIFF(MINUTE, v.vardiya_baslangic, MIN(GirişSaati)) % 60 AS VARCHAR), 2)
                    ELSE '00:00'
                END AS GeçKalmaSüresi,
                CASE 
                    WHEN MAX(ÇıkışSaati) < v.vardiya_bitis THEN 
                        RIGHT('0' + CAST(DATEDIFF(MINUTE, MAX(ÇıkışSaati), v.vardiya_bitis) / 60 AS VARCHAR), 2) + ':' + RIGHT('0' + CAST(DATEDIFF(MINUTE, MAX(ÇıkışSaati), v.vardiya_bitis) % 60 AS VARCHAR), 2)
                    ELSE '00:00'
                END AS ErkenÇıkmaSüresi,
                CAST((DATEDIFF(MINUTE, MIN(GirişSaati), MAX(ÇıkışSaati)) / 60) AS VARCHAR) + ' saat ' + 
                CAST((DATEDIFF(MINUTE, MIN(GirişSaati), MAX(ÇıkışSaati)) % 60) AS VARCHAR) + ' dakika' AS ÇalışmaSüresi
            FROM (
                SELECT 
                    CONVERT(char(10), h.tarih, 126) AS Tarih,
                    h.saat AS GirişSaati,
                    h.saat AS ÇıkışSaati,
                    FIRST_VALUE(d.device_name) OVER (PARTITION BY h.tarih ORDER BY h.saat ASC) AS İlkCihaz,
                    LAST_VALUE(d.device_name) OVER (PARTITION BY h.tarih ORDER BY h.saat DESC) AS SonCihaz,
                    h.kullanıcı_id
                FROM 
                    pdks_hareketler h
                INNER JOIN 
                    pdks_devices d ON h.bagliCihaz = d.device_serial
                WHERE 
                    h.kullanıcı_id IN ({String.Join(",", parameters)}) 
                    AND h.tarih BETWEEN @Baslangic AND @Bitis
            ) AS SubQuery
            INNER JOIN 
                pdks_personel p ON SubQuery.kullanıcı_id = p.personel_id
            LEFT JOIN 
                pdks_vardiya v ON p.vardiyaId = v.vardiya_id
            GROUP BY 
                Tarih, İlkCihaz, SonCihaz, p.personel_adi, p.personel_soyadi, v.vardiya_adi, v.vardiya_baslangic, v.vardiya_bitis
            ORDER BY 
                Tarih ASC;
        ";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        cmd.Parameters.AddWithValue(parameters[i], ids[i]);
                    }
                    cmd.Parameters.AddWithValue("@Baslangic", baslangic.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Bitis", bitis.ToString("yyyy-MM-dd"));

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }


        private void raporlamSayfasi_Enter(object sender, EventArgs e)
        {
            SetupCheckedComboBoxPersonel();
        }


        #endregion

        private void tümCihazlardanHareketVerileriniCek_Click(object sender, EventArgs e)
        {
            conDevice.FetchAllMovementsFromAllDevices();
        }
         
        //Kayıtlı olan bütün bilgileri kayıtlı bütün cihazlara gönderir
        private void butonSenkronizasyon_Click(object sender, EventArgs e)
        {
            conDevice.SyncAllUsersToDevice();
        }

        //Personeller Sayfasını Açar
        private void simpleButton3_Click(object sender, EventArgs e)
        {
            per.Show();
        }

        //Vardiya Ayaları
        private void LoadVardiyalar()
        {
            dataGridVardiya.DataSource = con.GetVardiya();
        }
    }
}
