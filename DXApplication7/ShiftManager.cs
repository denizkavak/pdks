using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

public class ShiftManager
{
    private string connectionString;

    public ShiftManager(string connString)
    {
        connectionString = connString;
    }

    public List<Vardiya> GetAllShifts()
    {
        List<Vardiya> shifts = new List<Vardiya>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = "SELECT * FROM pdks_vardiya";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Vardiya vardiya = new Vardiya
                        {
                            VardiyaID = (int)reader["vardiya_id"],
                            VardiyaAdi = reader["vardiya_adi"].ToString(),
                            BaslangicSaat = TimeSpan.Parse(reader["baslangic_saat"].ToString()),
                            BitisSaat = TimeSpan.Parse(reader["bitis_saat"].ToString()),
                            Gunler = reader["gunler"].ToString(),
                            Aciklama = reader["aciklama"]?.ToString()
                        };

                        shifts.Add(vardiya);
                    }
                }
            }
        }

        return shifts;
    }

    public bool IsWithinShift(Vardiya vardiya, DateTime time)
    {
        // Vardiya günlerini kontrol et
        if (!vardiya.Gunler.Contains(time.DayOfWeek.ToString()))
        {
            return false; // Tarih vardiya günleri arasında değil
        }

        TimeSpan shiftStart = vardiya.BaslangicSaat;
        TimeSpan shiftEnd = vardiya.BitisSaat;
        TimeSpan currentTime = time.TimeOfDay;

        if (shiftStart < shiftEnd)
        {
            // Vardiya aynı gün içerisinde tamamlanıyor
            return currentTime >= shiftStart && currentTime <= shiftEnd;
        }
        else
        {
            // Vardiya ertesi güne uzanıyor
            return currentTime >= shiftStart || currentTime <= shiftEnd;
        }
    }

    public bool IsEmployeeInShift(int employeeID, DateTime time)
    {
        List<Vardiya> shifts = GetAllShifts();

        foreach (Vardiya shift in shifts)
        {
            if (IsWithinShift(shift, time))
            {
                // Geçerli vardiyada çalışıyor
                return true;
            }
        }

        return false; // Hiçbir vardiya ile eşleşmedi
    }
}

public class Vardiya
{
    public int VardiyaID { get; set; }
    public string VardiyaAdi { get; set; }
    public TimeSpan BaslangicSaat { get; set; }
    public TimeSpan BitisSaat { get; set; }
    public string Gunler { get; set; }
    public string Aciklama { get; set; }
}
