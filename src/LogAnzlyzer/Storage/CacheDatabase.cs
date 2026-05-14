using System;
using System.Data.SQLite;
using System.IO;

namespace LogAnzlyzer.Storage
{
    // SQLite cache at %APPDATA%\log-analyzer\cache.db.
    // Stores parsed analysis results so re-opening the same file is instant,
    // plus app settings (theme, accent color, etc.).
    public static class CacheDatabase
    {
        public static string DataDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "log-analyzer");
        public static string DbPath => Path.Combine(DataDir, "cache.db");

        public static SQLiteConnection Open()
        {
            var conn = new SQLiteConnection("Data Source=" + DbPath + ";Version=3;");
            conn.Open();
            return conn;
        }

        public static void Initialize()
        {
            Directory.CreateDirectory(DataDir);
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SchemaSql;
                cmd.ExecuteNonQuery();
            }
        }

        public static string GetSetting(string key, string fallback)
        {
            try
            {
                using (var conn = Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT value FROM app_settings WHERE key = @k LIMIT 1";
                    cmd.Parameters.AddWithValue("@k", key);
                    var v = cmd.ExecuteScalar();
                    return v == null || v == DBNull.Value ? fallback : v.ToString();
                }
            }
            catch
            {
                return fallback;
            }
        }

        public static void SetSetting(string key, string value)
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "INSERT INTO app_settings(key, value) VALUES(@k, @v) " +
                    "ON CONFLICT(key) DO UPDATE SET value=excluded.value";
                cmd.Parameters.AddWithValue("@k", key);
                cmd.Parameters.AddWithValue("@v", value);
                cmd.ExecuteNonQuery();
            }
        }

        public const string SchemaSql = @"
CREATE TABLE IF NOT EXISTS app_settings (
  key   TEXT PRIMARY KEY,
  value TEXT
);

CREATE TABLE IF NOT EXISTS analyzed_files (
  id              INTEGER PRIMARY KEY AUTOINCREMENT,
  file_path       TEXT NOT NULL,
  file_sha256     TEXT NOT NULL UNIQUE,
  parser_regex    TEXT,
  parsed_at       TEXT DEFAULT (datetime('now'))
);
CREATE INDEX IF NOT EXISTS idx_analyzed_files_path ON analyzed_files(file_path);

CREATE TABLE IF NOT EXISTS summary_stats (
  file_id   INTEGER PRIMARY KEY REFERENCES analyzed_files(id) ON DELETE CASCADE,
  count     INTEGER,
  p1        REAL,
  median    REAL,
  p95       REAL,
  p99       REAL,
  mean      REAL,
  min       REAL,
  max       REAL
);

CREATE TABLE IF NOT EXISTS recent_files (
  path       TEXT PRIMARY KEY,
  opened_at  TEXT DEFAULT (datetime('now'))
);
CREATE INDEX IF NOT EXISTS idx_recent_files_opened ON recent_files(opened_at DESC);
";

        public static void AddRecentFile(string path)
        {
            try
            {
                using (var conn = Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO recent_files(path, opened_at) VALUES(@p, datetime('now')) " +
                        "ON CONFLICT(path) DO UPDATE SET opened_at = datetime('now')";
                    cmd.Parameters.AddWithValue("@p", path);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* non-fatal */ }
        }

        public static System.Collections.Generic.List<string> GetRecentFiles(int limit = 10)
        {
            var list = new System.Collections.Generic.List<string>();
            try
            {
                using (var conn = Open())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT path FROM recent_files ORDER BY opened_at DESC LIMIT @n";
                    cmd.Parameters.AddWithValue("@n", limit);
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read()) list.Add(rdr.GetString(0));
                }
            }
            catch { /* return whatever we have */ }
            return list;
        }

        public static void ClearRecentFiles()
        {
            using (var conn = Open())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM recent_files";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
