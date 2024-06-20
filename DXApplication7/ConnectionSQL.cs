using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using zkemkeeper;

namespace PDKS_17042024
{
    internal class ConnectionSQL
    {
        private string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        public DataTable GetDevices()
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT device_id,device_name,device_ipAdress,device_port,device_firmware,device_serial FROM pdks_devices", connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }

        }
        public DataTable GetPerson()
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT personel_id,personel_adi,personel_soyadi,personel_privilege,personel_Enabled,personel_kartNo,personel_FingerTemplate FROM pdks_personel", connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }

        }


        public void AddDevice(string deviceName, string deviceIp, int port)
        {
            CZKEMClass device = new CZKEMClass();

            if (!device.Connect_Net(deviceIp, port))
            {
                System.Windows.Forms.MessageBox.Show("Cihaza Bağlanılamadı");
                return;
            }

            // Seri numarasını cihaz ID'si olarak kullan
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                string sql = @"INSERT INTO pdks_devices (device_name, device_ipAdress, device_port) 
                       VALUES (@DeviceName, @DeviceIp, @Port)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@DeviceName", deviceName);
                    command.Parameters.AddWithValue("@DeviceIp", deviceIp);
                    command.Parameters.AddWithValue("@Port", port);

                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Cihaz Eklendi");
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Cihaz Eklenemedi");
                    }
                }
            }

            device.Disconnect(); // Bağlantıyı kes
        }


        // Sorguyu veritabanınıza göre düzenleyin

        public void RemoveDevice(string deviceId)
        {
            string deleteQuery = "DELETE FROM pdks_devices WHERE device_id = " + deviceId + "";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.ExecuteNonQuery(); // Komutu çalıştırarak veriyi sil
                }
            }
        }
        public void RemoveUser(string userId)
        {
            string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

            // Kullanıcıya ait hareket verilerini sil
            string deleteMovementsQuery = "DELETE FROM pdks_hareketler WHERE kullanıcı_id = @UserId";
            // Kullanıcı bilgilerini sil
            string deleteUserQuery = "DELETE FROM pdks_personel WHERE personel_id = @UserId";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Hareketleri sil
                using (SqlCommand commandMovements = new SqlCommand(deleteMovementsQuery, connection))
                {
                    commandMovements.Parameters.AddWithValue("@UserId", userId);
                    commandMovements.ExecuteNonQuery(); // Kullanıcı hareketlerini sil
                }

                // Kullanıcıyı sil
                using (SqlCommand commandUser = new SqlCommand(deleteUserQuery, connection))
                {
                    commandUser.Parameters.AddWithValue("@UserId", userId);
                    commandUser.ExecuteNonQuery(); // Kullanıcıyı sil
                }
            }
        }

        public bool IsDeviceRegistered(string serial)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string checkQuery = "SELECT COUNT(*) FROM pdks_devices WHERE device_serial = @serial";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@serial", serial);
                    int recordCount = (int)checkCommand.ExecuteScalar();
                    return recordCount > 0;
                }
            }
        }

        public void RegisterDevice(string firmware, string serial, int deviceId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Kayıt mevcut mu kontrol et
                string checkQuery = "SELECT COUNT(*) FROM pdks_devices WHERE device_id = @id";
                bool recordExists = false;

                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@id", deviceId);
                    int count = (int)checkCommand.ExecuteScalar();
                    recordExists = count > 0;
                }

                if (recordExists)
                {
                    // Güncelleme işlemi
                    string updateQuery = "UPDATE pdks_devices SET device_firmware = @firmware, device_serial = @serial WHERE device_id = @id";
                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@firmware", firmware);
                        updateCommand.Parameters.AddWithValue("@serial", serial);
                        updateCommand.Parameters.AddWithValue("@id", deviceId);
                        updateCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<Vardiya> GetVardiyalar()
        {
            string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

            List<Vardiya> vardiyalar = new List<Vardiya>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT vardiya_id, vardiya_adi FROM pdks_vardiya";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Vardiya vardiya = new Vardiya
                            {
                                VardiyaID = (int)reader["vardiya_id"],
                                VardiyaAdi = reader["vardiya_adi"].ToString()
                            };
                            vardiyalar.Add(vardiya);
                        }
                    }
                }
            }
            return vardiyalar;
        }

        public class Vardiya
        {
            public int VardiyaID { get; set; }
            public string VardiyaAdi { get; set; }
        }

        public int SaveUserToDatabase(string firstName, string lastName, int privilege, bool enabled, string cardNumber, string password, int vardiyaID)
        {
            string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Mevcut en yüksek ID'yi al
                string queryMaxID = "SELECT MAX(personel_id) FROM pdks_personel";
                using (SqlCommand cmdMaxID = new SqlCommand(queryMaxID, conn))
                {
                    object result = cmdMaxID.ExecuteScalar();
                    int maxID = result != DBNull.Value ? Convert.ToInt32(result) : 0; // Eğer sonuç DBNull ise 0 olarak kabul et
                    int newID = maxID + 1;

                    // Yeni kullanıcıyı ekle
                    string queryInsert = @"INSERT INTO pdks_personel (personel_id, personel_adi, personel_soyadi, personel_privilege, personel_Enabled, personel_kartNo, personel_FingerTemplate, personel_sifre, vardiyaId)
                           VALUES (@ID, @FirstName, @LastName, @Privilege, @Enabled, @CardNumber, @FingerPrintTemplates, @Password, @VardiyaID)";

                    using (SqlCommand cmdInsert = new SqlCommand(queryInsert, conn))
                    {
                        cmdInsert.Parameters.AddWithValue("@ID", newID);
                        cmdInsert.Parameters.AddWithValue("@FirstName", firstName);
                        cmdInsert.Parameters.AddWithValue("@LastName", lastName);
                        cmdInsert.Parameters.AddWithValue("@Privilege", privilege);
                        cmdInsert.Parameters.AddWithValue("@Enabled", enabled ? 1 : 0);
                        cmdInsert.Parameters.AddWithValue("@CardNumber", cardNumber);
                        cmdInsert.Parameters.AddWithValue("@FingerPrintTemplates", DBNull.Value);
                        cmdInsert.Parameters.AddWithValue("@Password", password); 
                        cmdInsert.Parameters.AddWithValue("@VardiyaID", vardiyaID);

                        cmdInsert.ExecuteNonQuery();

                        return newID;
                    }
                }
            }
        }

        public DataRow GetPersonelById(string personelID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT personel_adi, personel_soyadi, personel_privilege, personel_Enabled, personel_kartNo, personel_sifre, vardiyaId FROM pdks_personel WHERE personel_id = @PersonelID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PersonelID", personelID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                            return dt.Rows[0];
                        else
                            return null;
                    }
                }
            }
        }

        public DataTable GetVardiyaOptions()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT vardiya_id, vardiya_adi FROM pdks_vardiya ORDER BY vardiya_adi";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }


        public bool UpdatePersonel(string personelID, string firstName, string lastName, int privilege, bool enabled, string cardNumber, string password, int vardiyaID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
            UPDATE pdks_personel 
            SET 
                personel_adi = @FirstName, 
                personel_soyadi = @LastName, 
                personel_privilege = @Privilege, 
                personel_Enabled = @Enabled, 
                personel_kartNo = @CardNumber, 
                personel_FingerTemplate = @FingerPrintTemplates, 
                vardiyaId = @VardiyaID,
                personel_sifre = @Password
            WHERE personel_id = @PersonelID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@Privilege", privilege);
                    cmd.Parameters.AddWithValue("@Enabled", enabled ? 1 : 0);
                    cmd.Parameters.AddWithValue("@CardNumber", cardNumber);
                    cmd.Parameters.AddWithValue("@FingerPrintTemplates", DBNull.Value); // Parmak izi şablonu varsayalım ki boş geçiliyor.
                    cmd.Parameters.AddWithValue("@VardiyaID", vardiyaID);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@PersonelID", personelID);

                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        #region Login Kullanıcılar
        public bool CheckKullanici(string kullaniciAdi, string sifre)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM kullanicilar WHERE kullanici_adi = @kullaniciAdi AND sifre = @sifre", conn);
                cmd.Parameters.AddWithValue("@kullaniciAdi", kullaniciAdi);
                cmd.Parameters.AddWithValue("@sifre", sifre);
                conn.Open();
                int result = (int)cmd.ExecuteScalar();
                return result > 0;
            }
        }
        #endregion

        #region Vardiyalar
        public DataTable GetVardiya()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM pdks_vardiya", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public bool AddVardiyaGrubu(string grupAdi)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("INSERT INTO vardiya_gruplari (grup_adi) VALUES (@grupAdi)", conn);
                cmd.Parameters.AddWithValue("@grupAdi", grupAdi);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
        }

        public bool AddVardiyaToGrup(int grupId, int vardiyaId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("INSERT INTO vardiya_detaylari (grup_id, vardiya_id) VALUES (@grupId, @vardiyaId)", conn);
                cmd.Parameters.AddWithValue("@grupId", grupId);
                cmd.Parameters.AddWithValue("@vardiyaId", vardiyaId);
                conn.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
        }

        public DataTable GetVardiyaGruplari()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM vardiya_gruplari", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetVardiyalarByGrup(int grupId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT v.* FROM pdks_vardiya v JOIN vardiya_detaylari vd ON v.vardiya_id = vd.vardiya_id WHERE vd.grup_id = @grupId", conn);
                cmd.Parameters.AddWithValue("@grupId", grupId);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        #endregion
    }
} 