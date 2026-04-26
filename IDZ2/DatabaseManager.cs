using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// Управление базой данных SQLite.
/// </summary>
public class DatabaseManager
{
    private readonly string _connectionString;

    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public void InitializeDatabase(string hspCsvPath, string docCsvPath)
    {
        CreateTables();
        if (GetAllHospitals().Count == 0 && File.Exists(hspCsvPath))
        {
            ImportHospitalsFromCsv(hspCsvPath);
            Console.WriteLine($"[OK] Загружены больницы из {hspCsvPath}");
        }
        if (GetAllDoctors().Count == 0 && File.Exists(docCsvPath))
        {
            ImportDoctorsFromCsv(docCsvPath);
            Console.WriteLine($"[OK] Загружены врачи из {docCsvPath}");
        }
    }

    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS hsp (
                hsp_id INTEGER PRIMARY KEY AUTOINCREMENT,
                hsp_name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS doc (
                doc_id INTEGER PRIMARY KEY AUTOINCREMENT,
                hsp_id INTEGER NOT NULL,
                doc_name TEXT NOT NULL,
                doc_experience INTEGER NOT NULL,
                FOREIGN KEY (hsp_id) REFERENCES hsp(hsp_id)
            );";
        cmd.ExecuteNonQuery();
    }

    private void ImportHospitalsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO hsp (hsp_id, hsp_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
        }
    }

    private void ImportDoctorsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO doc (doc_id, hsp_id, doc_name, doc_experience) 
                VALUES (@id, @hspId, @name, @experience)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@hspId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@experience", int.Parse(parts[3]));
            cmd.ExecuteNonQuery();
        }
    }

    public List<Hospital> GetAllHospitals()
    {
        var result = new List<Hospital>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT hsp_id, hsp_name FROM hsp ORDER BY hsp_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Hospital(reader.GetInt32(0), reader.GetString(1)));
        }
        return result;
    }

    public List<Doctor> GetAllDoctors()
    {
        var result = new List<Doctor>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT doc_id, hsp_id, doc_name, doc_experience FROM doc ORDER BY doc_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Doctor(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3)));
        }
        return result;
    }

    public Doctor GetDoctorById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT doc_id, hsp_id, doc_name, doc_experience FROM doc WHERE doc_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Doctor(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3));
        }
        return null;
    }

    public void AddDoctor(Doctor doc)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO doc (hsp_id, doc_name, doc_experience) 
            VALUES (@hspId, @name, @experience)";
        cmd.Parameters.AddWithValue("@hspId", doc.HospitalId);
        cmd.Parameters.AddWithValue("@name", doc.Name);
        cmd.Parameters.AddWithValue("@experience", doc.Experience);
        cmd.ExecuteNonQuery();
    }

    public void UpdateDoctor(Doctor doc)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE doc 
            SET hsp_id = @hspId, doc_name = @name, doc_experience = @experience 
            WHERE doc_id = @id";
        cmd.Parameters.AddWithValue("@id", doc.Id);
        cmd.Parameters.AddWithValue("@hspId", doc.HospitalId);
        cmd.Parameters.AddWithValue("@name", doc.Name);
        cmd.Parameters.AddWithValue("@experience", doc.Experience);
        cmd.ExecuteNonQuery();
    }

    public void DeleteDoctor(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM doc WHERE doc_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }
        return (columns, rows);
    }
}