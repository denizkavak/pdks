using DevExpress.Pdf.Native.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using zkemkeeper;
using System.IO;

namespace PDKS2.Class
{
    internal class Connection
    {

        public zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();

        public CZKEMClass device = new CZKEMClass();

        private static int iMachineNumber = 1;

        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int DeviceID { get; set; } // Cihaz ID'si

        public bool Connect()
        {
            bool connected = device.Connect_Net(IpAddress, Port);
            return connected;
        }
        public bool GetConnectState()
        {
            return false; 
        }
        public void Disconnect()
        {
            device.Disconnect();
        }

        public string GetSerialNumber()
        {
            string serialNumber = "";
            device.GetSerialNumber(1, out serialNumber);
            return serialNumber;
        }

        public int GetMachineNumber()
        {
            return iMachineNumber;
        }

        public string GetFirmwareVersion()
        {
            string firmwareVersion = "";
            device.GetFirmwareVersion(1, ref firmwareVersion);
            return firmwareVersion;
        }

        //Cihazdan Kullanıcıları Çek
        public List<UserInfo> GetAllUsers()
        {
            List<UserInfo> users = new List<UserInfo>();
            if (!device.ReadAllUserID(1))
                return users;

            while (device.SSR_GetAllUserInfo(1, out string userID, out string name, out string password, out int privilege, out bool enableFlag))
            {
                string cardNumber = "";
                device.GetStrCardNumber(out cardNumber);

                string templates = "";
                device.GetUserTmpExStr(1, userID, 0, out _, out templates, out _);

                users.Add(new UserInfo
                {
                    UserID = userID,
                    Name = name,
                    Privilege = privilege,
                    Enabled = enableFlag,
                    CardNumber = cardNumber,
                    FingerPrintTemplates = templates
                });
            }
            return users;
        }
        //Hareket Verilerini Çekiyor
        public List<Hareket> FetchAllTransactionData()
        {
            List<Hareket> movements = new List<Hareket>();
            int dwMachineNumber = 1;

            try
            {
                device.EnableDevice(dwMachineNumber, false);

                if (device.ReadGeneralLogData(dwMachineNumber))
                {
                    string dwEnrollNumber;
                    int dwVerifyMode, dwInOutMode, dwYear, dwDay, dwMonth, dwHour, dwMinute, dwSecond;
                    int dwWorkCode = 0;
                    while (device.SSR_GetGeneralLogData(dwMachineNumber, out dwEnrollNumber, out dwVerifyMode, out dwInOutMode, out dwYear, out dwMonth, out dwDay, out dwHour, out dwMinute, out dwSecond, ref dwWorkCode))
                    {
                        DateTime timestamp = new DateTime(dwYear, dwMonth, dwDay, dwHour, dwMinute, dwSecond);
                        string serialNumber = GetSerialNumber();

                        movements.Add(new Hareket
                        {
                            UserID = dwEnrollNumber,
                            TimeStamp = timestamp,
                            VerifyMode = dwVerifyMode,
                            InOutMode = dwInOutMode,
                            DeviceSerial = serialNumber
                        });
                    }
                }
            }
            finally
            {
                device.EnableDevice(dwMachineNumber, true);
            }

            return movements;
        }


        public void FetchAllMovementsFromAllDevices()
        {
            try
            {
                string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
                List<DeviceInfo> devices = GetDevices(connectionString); // Cihaz listesini al
                int totalNewMovements = 0;

                foreach (var device in devices)
                {
                    Connection connection = new Connection
                    {
                        IpAddress = device.IpAddress,
                        Port = device.Port,
                        DeviceID = device.DeviceID
                    };

                    if (connection.Connect())
                    {
                        List<Hareket> movements = connection.FetchAllTransactionData();
                        connection.SaveMovementsToDatabase(movements); // Veritabanına hareketleri kaydet
                        int movementsCount = movements.Count;
                        totalNewMovements += movementsCount;


                        connection.Disconnect();
                    }
                    else
                    {
                        MessageBox.Show($"Cihaza bağlanılamadı: {device.IpAddress}:{device.Port}");
                    }
                }

                MessageBox.Show($"Toplam {totalNewMovements} yeni hareket tüm cihazlardan çekildi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //Cihazadan Çekilen Verileri Veritabanına Kayıt Eder Eğer o hareket yoksa
        public void SaveMovementsToDatabase(List<Hareket> movements)
        {
            try
            {
                string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (var movement in movements)
                    {
                        if (!int.TryParse(movement.UserID, out int userId))
                        {
                            MessageBox.Show($"Geçersiz UserID: {movement.UserID}");
                            continue; // Geçersiz UserID olduğunda döngünün bu adımını atla
                        }

                        string queryCheck = @"SELECT COUNT(*) FROM pdks_hareketler WHERE kullanıcı_id = @UserID AND tarih = @Date AND saat = @Time AND bagliCihaz = @DeviceSerial";
                        using (SqlCommand cmdCheck = new SqlCommand(queryCheck, conn))
                        {
                            cmdCheck.Parameters.AddWithValue("@UserID", userId);
                            cmdCheck.Parameters.AddWithValue("@Date", movement.TimeStamp.Date);
                            cmdCheck.Parameters.AddWithValue("@Time", movement.TimeStamp.TimeOfDay);
                            cmdCheck.Parameters.AddWithValue("@DeviceSerial", movement.DeviceSerial);

                            int count = (int)cmdCheck.ExecuteScalar();
                            if (count > 0)
                            {
                                continue;
                            } 
                            
                         

                        }

                        string queryInsert = @"INSERT INTO pdks_hareketler (kullanıcı_id, tarih, saat, dogrulamaMethodu, girisCikisModu, bagliCihaz)
           VALUES (@UserID, @Date, @Time, @VerifyMode, @InOutMode, @DeviceSerial)";
                        using (SqlCommand cmdInsert = new SqlCommand(queryInsert, conn))
                        {
                            cmdInsert.Parameters.AddWithValue("@UserID", userId);
                            cmdInsert.Parameters.AddWithValue("@Date", movement.TimeStamp.Date);
                            cmdInsert.Parameters.AddWithValue("@Time", movement.TimeStamp.TimeOfDay);
                            cmdInsert.Parameters.AddWithValue("@VerifyMode", movement.VerifyMode);
                            cmdInsert.Parameters.AddWithValue("@InOutMode", movement.InOutMode);
                            cmdInsert.Parameters.AddWithValue("@DeviceSerial", movement.DeviceSerial);

                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //Var olan hareketleri tespit et
        private HashSet<string> GetExistingMovements(SqlConnection conn)
        {
            HashSet<string> movements = new HashSet<string>();

            string query = "SELECT kullanıcı_id, tarih, saat, dogrulamaMethodu, girisCikisModu FROM pdks_hareketler";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string key = $"{reader["kullanıcı_id"]}:{reader["tarih"]}:{reader["saat"]}:{reader["dogrulamaMethodu"]}:{reader["girisCikisModu"]}";
                        movements.Add(key);
                    }
                }
            }

            return movements;
        }
        
        //Kullanıcıları Veri tabanına kayıt eder
        public void SaveUsersToDatabase(List<UserInfo> users)
        {
            string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                HashSet<string> existingUserIDs = GetExistingUserIDs(conn); // Mevcut kullanıcı ID'lerini getir
                int newUsersCount = 0; // Yeni kullanıcı sayısını tutar

                foreach (var user in users)
                {
                    if (existingUserIDs.Contains(user.UserID))
                    {
                        continue; // Mevcut kullanıcıyı atla
                    }

                    string[] nameParts = user.Name.Split(' '); // İlk boşluğa göre böler
                    string firstName = nameParts.Length > 0 ? nameParts[0] : "";
                    string lastName = nameParts.Length > 1 ? nameParts[1] : "";

                    string queryInsert = @"INSERT INTO pdks_personel (personel_id, personel_adi, personel_soyadi, personel_privilege, personel_kartNo, personel_FingerTemplate, personel_Enabled, personel_sifre)
          VALUES (@UserID, @FirstName, @LastName, @Privilege, @CardNumber, @FingerPrintTemplates, @Enabled, @Password)";
                    using (SqlCommand cmdInsert = new SqlCommand(queryInsert, conn))
                    {
                        cmdInsert.Parameters.AddWithValue("@UserID", user.UserID);
                        cmdInsert.Parameters.AddWithValue("@FirstName", firstName);
                        cmdInsert.Parameters.AddWithValue("@LastName", lastName);
                        cmdInsert.Parameters.AddWithValue("@Privilege", user.Privilege);
                        cmdInsert.Parameters.AddWithValue("@CardNumber", user.CardNumber);
                        cmdInsert.Parameters.AddWithValue("@FingerPrintTemplates", user.FingerPrintTemplates ?? string.Empty);
                        cmdInsert.Parameters.AddWithValue("@Enabled", user.Enabled ? 1 : 0);
                        cmdInsert.Parameters.AddWithValue("@Password", user.Password ?? string.Empty); // Şifre alanını ekleyin

                        cmdInsert.ExecuteNonQuery();
                        newUsersCount++;

                        // Şimdi aynı ID ile cihaza da ekleyelim
                        AddUserToAllDevices(user.UserID, $"{firstName} {lastName}", user.Privilege, user.Enabled, user.CardNumber, user.Password);
                    }
                }

                MessageBox.Show($"Veritabanına {newUsersCount} yeni kullanıcı kaydedildi.");
            }
        }

        //Tüm kullanıcıları silen kod 
        public bool DeleteAllUsers()
        {
            bool success = device.ClearData(1, 5); // "5" parametresi tüm kullanıcı verilerini siler

            if (success)
            {
                MessageBox.Show("Cihazdaki tüm kullanıcı verileri başarıyla silindi.");
            }
            else
            {
                MessageBox.Show("Kullanıcı verilerini silme işleminde bir hata oluştu.");
            }

            return success;
        }

        //Var olan kullanıcı id'yi testpit etme
        private HashSet<string> GetExistingUserIDs(SqlConnection conn)
        {
            HashSet<string> userIDs = new HashSet<string>();

            string query = "SELECT personel_id FROM pdks_personel";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string userID = reader["personel_id"].ToString();
                        userIDs.Add(userID);
                    }
                }
            }

            return userIDs;
        }


        //private int GetDeviceIDFromDatabase(string ip, int port)
        //{
        //    string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        conn.Open();
        //        string query = "SELECT device_id FROM pdks_devices WHERE device_ipAdress = @Ip AND device_port = @Port";
        //        using (SqlCommand cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@Ip", ip);
        //            cmd.Parameters.AddWithValue("@Port", port);

        //            object result = cmd.ExecuteScalar();
        //            return result != null ? Convert.ToInt32(result) : 0; // ID varsa dön, yoksa 0 döner
        //        }
        //    }
        //}
        public bool AddUserToDevice(string userID, string fullName, int privilege, bool enabled, string cardNumber, string password)
        {
            if (device.Connect_Net(IpAddress, Port))
            {
                bool response = device.SSR_SetUserInfo(1, userID, fullName, password, privilege, enabled);
                if (response)
                {
                    if (!string.IsNullOrEmpty(cardNumber))
                    {
                        response = device.SetStrCardNumber(cardNumber);  // Kart numarasını ayarla
                        if (response)
                        {
                            response = device.SSR_SetUserInfo(1, userID, fullName, password, privilege, enabled); // Kart numarası ile birlikte kullanıcı bilgilerini tekrar güncelle
                        }
                    }
                }
                device.Disconnect();
                return response;
            }
            return false;
        }

        //Veritabanındaki Tüm Kullanıcıları Kayıtlı olan cihazlara gönderir
        public void AddUserToAllDevices(string userID, string fullName, int privilege, bool enabled, string cardNumber, string password)
        {
            string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
            List<DeviceInfo> devices = GetDevices(connectionString);

            foreach (var device in devices)
            {
                Connection connection = new Connection
                {
                    IpAddress = device.IpAddress,
                    Port = device.Port
                };

                if (connection.Connect())
                {
                    bool success = connection.AddUserToDevice(userID, fullName, privilege, enabled, cardNumber, password);

                    if (!success)
                    {
                        MessageBox.Show($"Kullanıcı {userID} cihaza eklenirken hata oluştu: {device.IpAddress}:{device.Port}");
                    }

                    connection.Disconnect();
                }
                else
                {
                    MessageBox.Show($"Cihaza bağlanılamadı: {device.IpAddress}:{device.Port}");
                }
            }
        }
        //Veritabanından cihazları çeker
        public List<DeviceInfo> GetDevices(string connectionString)
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT device_id, device_ipAdress, device_port FROM pdks_devices";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["device_id"] != DBNull.Value &&
                                reader["device_ipAdress"] != DBNull.Value &&
                                reader["device_port"] != DBNull.Value)
                            {
                                devices.Add(new DeviceInfo
                                {
                                    DeviceID = Convert.ToInt32(reader["device_id"]),
                                    IpAddress = reader["device_ipAdress"].ToString(),
                                    Port = Convert.ToInt32(reader["device_port"])
                                });
                            }
                        }
                    }
                }
            }

            return devices;
        }
        //Veritabanından Personelleri Çeker
        private List<PersonInfo> GetPersons(string connectionString)
        {
            List<PersonInfo> persons = new List<PersonInfo>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM pdks_personel";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            persons.Add(new PersonInfo
                            {
                                PersonID = Convert.ToInt32(reader["personel_id"]),
                                FirstName = reader["personel_adi"].ToString(),
                                LastName = reader["personel_soyadi"].ToString(),
                                Privilege = Convert.ToInt32(reader["personel_privilege"]),
                                Enabled = Convert.ToBoolean(reader["personel_Enabled"]),
                                CardNumber = reader["personel_kartNo"].ToString(),
                                FingerPrintTemplates = reader["personel_FingerTemplate"].ToString(),
                                VardiyaID = reader["vardiyaId"] != DBNull.Value ? Convert.ToInt32(reader["vardiyaId"]) : (int?)null // Vardiya ID'si varsa dönüştür.
                            });
                        }
                    }
                }
            }

            return persons;
        }
        int idwErrorCode = 0;
        //Cihazdan Parmak İzi kaydını başlatır
        public bool RegisterFingerprint(int userID, int fingerID)
        {
            Connect();
            if (device.StartEnrollEx(userID.ToString(), fingerID, 1))
            { 
                if (device.StartIdentify())
                {
                    MessageBox.Show("Enroll a new User,UserID" + userID);
                }
                ;//After enrolling templates,you should let the device into the 1:N verification condition
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                MessageBox.Show("*Operation failed,ErrorCode=" + idwErrorCode.ToString());
            }
            return true;
        }

        //Tüm kullanıcıları Cihaza Aktarır
        public bool SyncAllUsersToDevice()
        {
            if (!Connect())
            {
                MessageBox.Show("Cihaza bağlanılamadı.", "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            device.EnableDevice(1, false); // Cihazı yönetim için geçici olarak devre dışı bırak

            try
            {
                List<UserInfo> users = GetAllUsersFromDatabase();
                foreach (var user in users)
                {
                    if (!device.SSR_SetUserInfo(1, user.UserID, user.Name, user.Password, user.Privilege, user.Enabled))
                    {
                        MessageBox.Show($"Kullanıcı bilgileri cihaza yüklenemedi: {user.UserID}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }

                    // Kullanıcının parmak izi varsa, cihaza yükle
                    if (!string.IsNullOrEmpty(user.FingerPrintTemplates))
                    {
                        device.SetUserTmpExStr(1, user.UserID, 0, 1, user.FingerPrintTemplates); // Sadece bir parmak izi varsayıyoruz
                    }
                }
            }
            finally
            {
                device.RefreshData(1); // Cihazdaki verileri yenile
                device.EnableDevice(1, true); // Cihazı tekrar etkinleştir
                Disconnect();
            }

            MessageBox.Show("Tüm kullanıcılar cihaza başarıyla senkronize edildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }
        private List<UserInfo> GetAllUsersFromDatabase()
        {
            List<UserInfo> users = new List<UserInfo>();
            string connectionString = "Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT personel_id, personel_adi, personel_sifre, personel_privilege, personel_Enabled FROM pdks_personel";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new UserInfo
                            {
                                UserID = reader["personel_id"].ToString(),
                                Name = reader["personel_adi"].ToString(),
                                Password = reader["personel_sifre"].ToString(),
                                Privilege = Convert.ToInt32(reader["personel_privilege"]),
                                Enabled = Convert.ToBoolean(reader["personel_Enabled"])
                                // FingerPrintTemplates özelliği veritabanı modelinize göre ayarlanmalıdır
                            });
                        }
                    }
                }
            }

            return users;
        }

        public bool UpdateUserOnDevice(string ip, int port, string userID, string fullName, int privilege, bool enabled, string cardNumber, string password)
        {
            Connection connection = new Connection
            {
                IpAddress = ip,
                Port = port
            };

            if (connection.Connect())
            {
                // Kullanıcı bilgilerini cihazda güncelle
                if (!string.IsNullOrEmpty(cardNumber))
                {
                    connection.device.SetStrCardNumber(cardNumber); // Önce kart numarasını cihaza yaz
                }

                bool result = connection.device.SSR_SetUserInfo(1, userID, fullName, password, privilege, enabled);

                connection.Disconnect();

                return result;
            }
            else
            {
                MessageBox.Show($"Cihaza bağlanılamadı: {ip}:{port}");
                return false;
            }
        }

        public void UpdateUserOnAllDevices(string userID, string fullName, int privilege, bool enabled, string cardNumber, string password)
        {
            try
            {
                List<DeviceInfo> devices = GetDevices("Data Source=DESKTOP-A2CGQRG\\SQLTEKNIK;Initial Catalog=PDKS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True");
                foreach (var device in devices)
                {
                    bool success = UpdateUserOnDevice(device.IpAddress, device.Port, userID, fullName, privilege, enabled, cardNumber, password);
                    if (!success)
                    {
                        MessageBox.Show($"Kullanıcı {userID} cihaza güncellenirken hata oluştu: {device.IpAddress}:{device.Port}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }




        // Cihaz bilgilerini temsil eden sınıf
        public class DeviceInfo
        {
            public int DeviceID { get; set; }
            public string IpAddress { get; set; }
            public int Port { get; set; }
        }
        public class UserInfo
        {
            public string UserID { get; set; }
            public string Name
            {
                get; set;
            }
            public int Privilege { get; set; }
            public bool Enabled { get; set; }
            public string CardNumber { get; set; }
            public string FingerPrintTemplates { get; set; }
            public string Password { get; set; }
        }
        public class PersonInfo
        {
            public int PersonID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Privilege { get; set; }
            public bool Enabled { get; set; }
            public string CardNumber { get; set; }
            public string FingerPrintTemplates { get; set; }
            public int? VardiyaID { get; set; } 
            public string Password { get; set; } 
        }
        public class Hareket
        {
            public string UserID { get; set; }
            public DateTime TimeStamp { get; set; }
            public int VerifyMode { get; set; }
            public int InOutMode { get; set; }
            public string DeviceSerial { get; set; } 
        }
    }
}
