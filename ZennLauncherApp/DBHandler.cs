using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ZennLauncherApp
{
    public class DBHandler
    {
        public static DBHandler Instance { get; } = new DBHandler();

        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        public ProfileDataModel profileData = new ProfileDataModel();

        public string ConnectionGlobal => $"Data Source={func.dataPath}/launcher_data.db";
        public string ConnectionProfile
        {
            get
            {
                try
                {
                    var config = cfg.LoadGlobalConfig();

                    if (config == null || config.ActiveProfile <= 0)
                    {
                        return "Data Source=:memory:";
                    }

                    string path = System.IO.Path.Combine(func.dataPath, config.ActiveProfile.ToString(), "profile_data.db");

                    return $"Data Source={path}";
                }
                catch
                {
                    return "Data Source=:memory:";
                }
            }
        }

        private void ExecuteNonQuery(SqliteConnection conn, string sql)
        {
            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsTableExists(string tableName)
        {
            using (var connection = new SqliteConnection(ConnectionGlobal))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name=$name";
                command.Parameters.AddWithValue("$name", tableName);

                var result = Convert.ToInt32(command.ExecuteScalar());

                return result > 0;
            }
        }

        private bool IsTableEmpty(SqliteConnection connection, string tableName)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM {tableName}";
            return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
        }

        public void GlobalDBInitialize()
        {
            if (!Directory.Exists(func.dataPath))
            {
                Directory.CreateDirectory(func.dataPath);
            }

            using (var connection = new SqliteConnection(ConnectionGlobal))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();

                var createTableSqlProfiles = @"CREATE TABLE IF NOT EXISTS profiles (
                                               id INTEGER PRIMARY KEY AUTOINCREMENT,
                                               username TEXT NOT NULL,
                                               hash TEXT,
                                               first_active DATETIME,
                                               keep_logged_in INT(2) DEFAULT 0);";

                var createTableSqlLanguages = @"CREATE TABLE IF NOT EXISTS languages (
                                                id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                language TEXT NOT NULL);";

                ExecuteNonQuery(connection, createTableSqlProfiles);
                ExecuteNonQuery(connection, createTableSqlLanguages);

                if (IsTableEmpty(connection, "languages"))
                {
                    var insertLangs = @"INSERT INTO languages (language) VALUES 
                                      ('English'), ('Indonesia'), ('Arabic');";

                    ExecuteNonQuery(connection, insertLangs);
                }

            }
        }

        public ProfileDataModel GetProfileData()
        {
            ProfileDataModel currentProfileData = new ProfileDataModel { ProfileId = 0 };

            int _profileId = cfg.LoadGlobalConfig().ActiveProfile;

            Debug.WriteLine("Current active profile ID: " + _profileId);

            if (_profileId > 0)
            {
                string query_ = $@"SELECT * FROM profiles WHERE id = {_profileId}";

                using (var connection = new SqliteConnection(ConnectionGlobal))
                {
                    connection.Open();
                    var command = new SqliteCommand($@"SELECT id, username, hash, first_active, keep_logged_in
                                                    FROM profiles WHERE id = {_profileId}", connection);

                    var reader = command.ExecuteReader();

                    using (reader)
                    {
                        if (reader.Read())
                        {
                            currentProfileData = new ProfileDataModel
                            {
                                ProfileId = reader.GetInt32(0),
                                ProfileName = reader.GetString(1),
                                ProfileHash = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                ProfileFirstActive = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                ProfileKeepLoggedIn = reader.GetInt32(4)
                            };
                        }
                    }
                }
            }
            return currentProfileData;
        }

        public void GetCurrentProfileData()
        {
            profileData = GetProfileData();
        }

        public void ProfileDBInitialize()
        {
            if (ConnectionProfile.Contains(":memory:")) return;

            GetCurrentProfileData();
            Debug.WriteLine("PROFILE DATABASE PATH: " + ConnectionProfile);

            if (!Directory.Exists(func.dataPath))
            {
                Directory.CreateDirectory(func.dataPath);
            }

            // 2. Pastikan folder profile (untuk profile data) ada
            string profileFolder = Path.Combine(func.dataPath, profileData.ProfileId.ToString());
            if (!Directory.Exists(profileFolder))
            {
                Directory.CreateDirectory(profileFolder);
            }

            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();

                var createTableSqlCategories = @"CREATE TABLE IF NOT EXISTS categories (
                                                 id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                 name TEXT NOT NULL,
                                                 desc TEXT);";

                var createTableSqlGenre = @"CREATE TABLE IF NOT EXISTS genres (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            name TEXT NOT NULL,
                                            desc TEXT);";

                var createTableSqlGames = @"CREATE TABLE IF NOT EXISTS games (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            title TEXT NOT NULL,
                                            subtitle TEXT,
                                            category_id INT REFERENCES categories(id),
                                            genre_id INT REFERENCES genres(id),
                                            user_rating INT(10),
                                            first_launched DATETIME,
                                            last_launched DATETIME,
                                            launch_times INT DEFAULT 0,
                                            target TEXT NOT NULL,
                                            start_in TEXT NOT NULL,
                                            launch_options TEXT,
                                            cover_path TEXT,
                                            icon_path TEXT);";

                var createTableSqlTools = @"CREATE TABLE IF NOT EXISTS tools (
                                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            title TEXT NOT NULL,
                                            desc TEXT,
                                            first_launched DATETIME,
                                            last_launched DATETIME,
                                            launch_times INT DEFAULT 0,
                                            target TEXT NOT NULL,
                                            start_in TEXT NOT NULL,
                                            launch_options TEXT,
                                            cover_path TEXT,
                                            icon_path TEXT);";

                var createTableSqlRelics = @"CREATE TABLE IF NOT EXISTS relics (
                                             id INTEGER PRIMARY KEY AUTOINCREMENT,
                                             title TEXT NOT NULL,
                                             subtitle TEXT,
                                             category_id INT REFERENCES categories(id),
                                             genre_id INT REFERENCES genres(id),
                                             user_rating INT(10),
                                             user_commentary TEXT,
                                             first_launched DATETIME,
                                             last_launched DATETIME,
                                             launch_times INT,
                                             executable_path TEXT,
                                             cover_path TEXT,
                                             icon_path TEXT,
                                             profile_name TEXT);";

                var createTableSqlSettings = @"CREATE TABLE IF NOT EXISTS profile_settings (
                                             id INTEGER PRIMARY KEY AUTOINCREMENT,
                                             option_name TEXT NOT NULL,
                                             option_display TEXT,
                                             desc TEXT,
                                             value TEXT);";

                ExecuteNonQuery(connection, createTableSqlCategories);
                ExecuteNonQuery(connection, createTableSqlGenre);
                ExecuteNonQuery(connection, createTableSqlGames);
                ExecuteNonQuery(connection, createTableSqlTools);
                ExecuteNonQuery(connection, createTableSqlRelics);
                ExecuteNonQuery(connection, createTableSqlSettings);

            }
            Debug.WriteLine("Current active profile data: " + profileData.ProfileId);
        }

        public void CloseAllConnections()
        {
            SqliteConnection.ClearAllPools();

            GC.Collect();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // Fungsi untuk Query SQLite
        public void ExecuteQuery(string conn, string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqliteConnection(conn))
            {
                connection.Open();
                using (var command = new SqliteCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        // Fungsi untuk query SQLite dengan ExecuteScalar
        public T ExecuteScalar<T>(string conn, string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqliteConnection(conn))
            {
                try
                {
                    connection.Open();
                    var command = new SqliteCommand(query, connection);

                    // Tambahkan parameter jika ada
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        // Mengubah object hasil database ke tipe T yang diminta
                        return (T)Convert.ChangeType(result, typeof(T));
                    }

                    return default(T);
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
                {
                    return default(T);
                }
            }
        }

        public List<ProfileDataModel> GetAllProfiles(string sortBy = "username", bool ascending = true)
        {
            List<ProfileDataModel> profiles = new List<ProfileDataModel>();
            string order = ascending ? "ASC" : "DESC";

            using (var connection = new SqliteConnection(ConnectionGlobal))
            {
                connection.Open();
                var command = new SqliteCommand($@"SELECT id, username, hash, first_active, 
                                                keep_logged_in FROM profiles 
                                                ORDER BY [{sortBy}] {order}", connection);


                var reader = command.ExecuteReader();

                using (reader)
                {
                    while (reader.Read())
                    {
                        profiles.Add(new ProfileDataModel
                        {
                            ProfileId = reader.GetInt32(0),
                            ProfileName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            ProfileHash = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            ProfileFirstActive = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            ProfileKeepLoggedIn = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
                        });
                    }
                }
            }
            return profiles;
        }

        public List<GameModel> GetAllGames(string sortBy = "title", bool ascending = true)
        {
            ProfileDBInitialize();
            GetCurrentProfileData();

            List<GameModel> games = new List<GameModel>();
            string order = ascending ? "ASC" : "DESC";

            Debug.WriteLine("GET ALL GAMES PATH: " + ConnectionProfile);

            if (ConnectionProfile.Contains(":memory:"))
            {
                return new List<GameModel>();
            }

            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();
                var command = new SqliteCommand($@"SELECT id, title, subtitle, first_launched, 
                                                last_launched, launch_times, target, launch_options, 
                                                icon_path, cover_path FROM games 
                                                ORDER BY [{sortBy}] {order}", connection);


                var reader = command.ExecuteReader();

                using (reader)
                {
                    while (reader.Read())
                    {
                        games.Add(new GameModel
                        {
                            GameId = reader.GetInt32(0),
                            GameTitle = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            GameSubtitle = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            GameFirstLaunched = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            GameLastLaunched = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            GameLaunchTimes = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            GameTarget = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            GameLaunchOptions = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            GameIconPath = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            GameCoverPath = reader.IsDBNull(9) ? "" : reader.GetString(9)
                        });
                    }
                }
            }
            return games;
        }

        public List<ToolModel> GetAllTools(string sortBy = "title", bool ascending = true)
        {
            ProfileDBInitialize();
            GetCurrentProfileData();

            List<ToolModel> tools = new List<ToolModel>();
            string order = ascending ? "ASC" : "DESC";

            if (ConnectionProfile.Contains(":memory:"))
            {
                return new List<ToolModel>();
            }

            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();
                var command = new SqliteCommand($"SELECT id, title, desc, first_launched, " +
                                                $"last_launched, launch_times, target, launch_options, " +
                                                $"icon_path, cover_path FROM tools " +
                                                $"ORDER BY [{sortBy}] {order}", connection);

                using (var reader = command.ExecuteReader())
                {
                    // Looping selama masih ada baris data
                    while (reader.Read())
                    {
                        tools.Add(new ToolModel
                        {
                            ToolId = reader.GetInt32(0),
                            ToolTitle = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            ToolDesc = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            ToolFirstLaunched = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            ToolLastLaunched = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            ToolLaunchTimes = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            ToolTarget = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            ToolLaunchOptions = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            ToolIconPath = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            ToolCoverPath = reader.IsDBNull(9) ? "" : reader.GetString(9)
                        });
                    }
                }
            }
            return tools;
        }

        public string GetAppVersion()
        {
            string AppVersion = cfg.LoadGlobalConfig().Version;
            return AppVersion;
        }

        public void UpdateGame(GameModel game)
        {
            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();
                var cmd = new SqliteCommand(@"UPDATE games SET 
                                                title = @title, 
                                                target = @target,
                                                start_in = @start_in,
                                                launch_options = @options, 
                                                cover_path = @cover, 
                                                icon_path = @icon WHERE id = @id", connection);

                string gameStartIn = System.IO.Path.GetDirectoryName(game.GameTarget);

                cmd.Parameters.AddWithValue("@title", game.GameTitle);
                cmd.Parameters.AddWithValue("@target", game.GameTarget);
                cmd.Parameters.AddWithValue("@start_in", gameStartIn);
                cmd.Parameters.AddWithValue("@options", game.GameLaunchOptions);
                cmd.Parameters.AddWithValue("@cover", game.GameCoverPath);
                cmd.Parameters.AddWithValue("@icon", game.GameIconPath);
                cmd.Parameters.AddWithValue("@id", game.GameId);

                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateTool(ToolModel tool)
        {
            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();
                var cmd = new SqliteCommand(@"UPDATE tools SET 
                                                title = @title, 
                                                target = @target,
                                                start_in = @start_in,
                                                launch_options = @options, 
                                                cover_path = @cover, 
                                                icon_path = @icon 
                                                WHERE id = @id", connection);

                string toolStartIn = System.IO.Path.GetDirectoryName(tool.ToolTarget);

                cmd.Parameters.AddWithValue("@title", tool.ToolTitle);
                cmd.Parameters.AddWithValue("@target", tool.ToolTarget);
                cmd.Parameters.AddWithValue("@start_in", toolStartIn);
                cmd.Parameters.AddWithValue("@options", tool.ToolLaunchOptions);
                cmd.Parameters.AddWithValue("@cover", tool.ToolCoverPath);
                cmd.Parameters.AddWithValue("@icon", tool.ToolIconPath);
                cmd.Parameters.AddWithValue("@id", tool.ToolId);

                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteGame(int id)
        {
            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();
                var cmd = new SqliteCommand("DELETE FROM games WHERE id = @id", connection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteTool(int id)
        {
            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();
                var cmd = new SqliteCommand("DELETE FROM tools WHERE id = @id", connection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteProfile(int id)
        {
            using (var connection = new SqliteConnection(ConnectionGlobal))
            {
                connection.Open();
                var cmd = new SqliteCommand("DELETE FROM profiles WHERE id = @id", connection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            var currentConfig = cfg.LoadGlobalConfig();

            currentConfig.ActiveProfile = -1;

            cfg.SaveGlobalConfig(currentConfig);
            CloseAllConnections();
        }

        public List<SettingsModel> GetAllProfileSettings(string sortBy = "id", bool ascending = true)
        {
            GetCurrentProfileData();

            List<SettingsModel> options = new List<SettingsModel>();
            string order = ascending ? "ASC" : "DESC";

            Debug.WriteLine("GET ALL SETTINGS PATH: " + ConnectionProfile);

            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();
                var command = new SqliteCommand($@"SELECT id, option_name, option_display, desc, value FROM profile_settings
                                                ORDER BY [{sortBy}] {order}", connection);

                var reader = command.ExecuteReader();

                using (reader)
                {
                    while (reader.Read())
                    {
                        options.Add(new SettingsModel
                        {
                            OptionId = reader.GetInt32(0),
                            OptionName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            OptionDisplay = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            OptionDesc = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            OptionValue = reader.IsDBNull(3) ? "" : reader.GetString(3)
                        });
                    }
                }
            }
            return options;
        }

        public bool IsColumnExists(string conn, string tableName, string columnName)
        {
            bool exists = false;
            using (var connection = new SqliteConnection(conn))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info({tableName})";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string currentColumnName = reader.GetString(1);
                        if (currentColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            exists = true;
                            break;
                        }
                    }
                }
            }
            return exists;
        }
    }
}
