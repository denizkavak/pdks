using DevExpress.Xpo.Helpers;
using PDKS_17042024;
using PDKS2.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PDKS_17042024.ConnectionSQL;
using static PDKS2.Class.Connection;

namespace DXApplication7
{
    public partial class Form2 : Form
    {
        private Connection conDevice;

        public Form2()
        {
            InitializeComponent();
            conDevice = new Connection(); 
            LoadDevicesIntoComboBox();
        }

        Form1 frm1 = new Form1();
        public int id;

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private string FindSelectedRadioButtonNumber()
        {
            for (int i = 0; i < 10; i++)
            {
                RadioButton rb = this.Controls.Find("parmak" + i, true).FirstOrDefault() as RadioButton;
                if (rb != null && rb.Checked)
                {
                    // Regex ile radyo butonun isminden sayısal kısmı çıkar
                    return Regex.Match(rb.Name, @"\d+").Value;
                }
            }
            return "Hiçbiri seçili değil";
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            string selectedRadioNumber = FindSelectedRadioButtonNumber();
            frm1.parmakIziKayıt = Convert.ToInt32(selectedRadioNumber);
            conDevice.RegisterFingerprint(id,Convert.ToInt32(selectedRadioNumber));
            this.Hide();
            
            
        }
        private void LoadDevicesIntoComboBox()
        {

            // DeviceInfo listesini çekin
            List<DeviceInfo> devices = conDevice.GetDevices("Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");

            // ComboBox'ı doldurun
            comboBoxCihazlar.DisplayMember = "DeviceName";
            comboBoxCihazlar.ValueMember = "DeviceID";
            comboBoxCihazlar.DataSource = devices;

            // ComboBox'taki seçim değişikliklerini işleyecek olayı bağlayın
            comboBoxCihazlar.SelectedIndexChanged += ComboBoxDevices_SelectedIndexChanged;
        }

        private void ComboBoxDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxCihazlar.SelectedItem is DeviceInfo selectedDevice)
            {
                //MessageBox.Show($"Selected Device IP: {selectedDevice.IpAddress}, Port: {selectedDevice.Port}");
                conDevice.IpAddress = selectedDevice.IpAddress;
                conDevice.Port = selectedDevice.Port;
            }
        }


    }
}
