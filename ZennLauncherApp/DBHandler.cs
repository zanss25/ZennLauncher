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

                var createTableMetadata = @"CREATE TABLE IF NOT EXISTS db_metadata (
                                            key TEXT PRIMARY KEY,
                                            value TEXT);";

                ExecuteNonQuery(connection, createTableMetadata);

                var createTableSqlSections = @"CREATE TABLE IF NOT EXISTS sections (
                                                 id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                 name TEXT UNIQUE);";

                var insertSectionsDefaults = @"INSERT OR IGNORE INTO sections (id, name)
                                                 VALUES ('1', 'Games'), ('2', 'Tools');";

                var createTableSqlCategories = @"CREATE TABLE IF NOT EXISTS categories (
                                                 id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                 name TEXT,
                                                 desc TEXT,
                                                 section_id INT REFERENCES sections(id));";

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
                                            category_id INT REFERENCES categories(id),
                                            user_rating INT(10),
                                            first_launched DATETIME,
                                            last_launched DATETIME,
                                            launch_times INT DEFAULT 0,
                                            target TEXT NOT NULL,
                                            start_in TEXT NOT NULL,
                                            launch_options TEXT,
                                            cover_path TEXT,
                                            icon_path TEXT);";

                ExecuteNonQuery(connection, createTableSqlSections);
                ExecuteNonQuery(connection, insertSectionsDefaults);
                ExecuteNonQuery(connection, createTableSqlCategories);
                ExecuteNonQuery(connection, createTableSqlGenre);
                ExecuteNonQuery(connection, createTableSqlGames);
                ExecuteNonQuery(connection, createTableSqlTools);

            }

            Debug.WriteLine("Current active profile data: " + profileData.ProfileId);
        }



        public void InsertDbDefaults(string targetTable)
        {
            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();

                if (targetTable == "categories")
                {
                    var defaults = @"INSERT OR IGNORE INTO categories (name, desc, section_id)
                                     VALUES ('Action', 'Fast-paced reflex-based games', '1'),
                                            ('Adventure', 'Story-driven exploration games', '1'),
                                            ('RPG', 'Games with deep character progression and narrative choices', '1'),
                                            ('Sandbox', 'Limitless creativity and free-form gameplay', '1'),
                                            ('Simulation', 'Realistic mimicry of real-world activities', '1'),
                                            ('Strategy', 'Games involving actical planning and resource management', '1'),
                                            ('Platformer', 'Jumping and navigating through levels.', '1'),
                                            ('Horror', 'Suspenseful gameplay designed to evoke fear', '1'),
                                            ('Puzzle', 'Logical challenges and brain-teasing tasks', '1'),
                                            ('Emulators', 'Run games from console platforms.', '2'),
                                            ('Game Configuration', 'Game configuration apps', '2'),
                                            ('Modding Tools', 'Tools to create, install, and manage game modifications', '2');";

                    ExecuteNonQuery(connection, defaults);

                    ExecuteNonQuery(connection, @"INSERT OR IGNORE INTO db_metadata (key, value) 
                                                  VALUES ('defaults_categories_installed', 'true')");
                }
            }

        }

        public void CloseAllConnections()
        {
            SqliteConnection.ClearAllPools();

            GC.Collect();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // Fungsi untuk query SQLite (Tanpa Return)
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

        // Fungsi untuk query SQLite dengan Type dan Return
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
                var command = new SqliteCommand($@"SELECT g.*, c.name as category_name 
                                                   FROM games g
                                                   LEFT JOIN categories c ON g.category_id = c.id 
                                                   ORDER BY [{sortBy}] {order}", connection);


                var reader = command.ExecuteReader();

                using (reader)
                {
                    while (reader.Read())
                    {
                        games.Add(new GameModel
                        {
                            GameId = reader.GetInt32(0),
                            GameTitle = reader.IsDBNull(reader.GetOrdinal("title")) ? "" : reader.GetString(reader.GetOrdinal("title")),
                            GameSubtitle = reader.IsDBNull(reader.GetOrdinal("subtitle")) ? "" : reader.GetString(reader.GetOrdinal("subtitle")),
                            GameCategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("category_id")),
                            GameCategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? "" : reader.GetString(reader.GetOrdinal("category_name")),
                            GameGenreId = reader.IsDBNull(reader.GetOrdinal("genre_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("genre_id")),
                            GameUserRating = reader.IsDBNull(reader.GetOrdinal("user_rating")) ? 0 : reader.GetInt32(reader.GetOrdinal("user_rating")),
                            GameFirstLaunched = reader.IsDBNull(reader.GetOrdinal("first_launched")) ? "" : reader.GetString(reader.GetOrdinal("first_launched")),
                            GameLastLaunched = reader.IsDBNull(reader.GetOrdinal("last_launched")) ? "" : reader.GetString(reader.GetOrdinal("last_launched") ),
                            GameLaunchTimes = reader.IsDBNull(reader.GetOrdinal("launch_times")) ? 0 : reader.GetInt32(reader.GetOrdinal("launch_times")),
                            GameTarget = reader.IsDBNull(reader.GetOrdinal("target")) ? "" : reader.GetString(reader.GetOrdinal("target")),
                            GameLaunchOptions = reader.IsDBNull(reader.GetOrdinal("launch_options")) ? "" : reader.GetString(reader.GetOrdinal("launch_options")),
                            GameIconPath = reader.IsDBNull(reader.GetOrdinal("icon_path")) ? "" : reader.GetString(reader.GetOrdinal("icon_path")),
                            GameCoverPath = reader.IsDBNull(reader.GetOrdinal("cover_path")) ? "" : reader.GetString(reader.GetOrdinal("cover_path"))
                        });
                    }
                }
            }
            return games;
        }

        public List<CategoryModel> GetAllCategories(int sectionId)
        {
            ProfileDBInitialize();
            GetCurrentProfileData();

            string query_cc = "SELECT COUNT(*) FROM categories WHERE section_id = @section_id;";

            var args_cc = new Dictionary<string, object>()
            {
                { "@section_id", sectionId }
            };

            int result = ExecuteScalar<int>(ConnectionProfile, query_cc, args_cc);

            if (result < 1)
            {
                return null;
            }

            List<CategoryModel> categories = new List<CategoryModel>();

            Debug.WriteLine("GET ALL CATEGORIES PATH: " + ConnectionProfile);

            if (ConnectionProfile.Contains(":memory:"))
            {
                return new List<CategoryModel>();
            }

            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();
                var command = new SqliteCommand($@"SELECT c.id, c.name, c.desc, c.section_id, s.name AS section_name
                                                   FROM categories c
                                                   INNER JOIN sections s ON c.section_id = s.id
                                                   WHERE c.section_id = @section_id", connection);

                var args = new Dictionary<string, object>()
                {
                    { "@section_id", sectionId }
                };

                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        command.Parameters.AddWithValue(arg.Key, arg.Value ?? DBNull.Value);
                    }
                }

                var reader = command.ExecuteReader();

                using (reader)
                {
                    while (reader.Read())
                    {
                        categories.Add(new CategoryModel
                        {
                            CategoryId = reader.GetInt32(0),
                            CategoryName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            CategoryDesc = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            CategorySectionId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            CategorySectionName = reader.IsDBNull(4) ? "" : reader.GetString(4)
                        });
                    }
                }
            }
            return categories;
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
                var command = new SqliteCommand($@"SELECT t.*, c.name as category_name 
                                                   FROM tools t
                                                   LEFT JOIN categories c ON t.category_id = c.id 
                                                   ORDER BY [{sortBy}] {order}", connection);

                using (var reader = command.ExecuteReader())
                {
                    // Looping selama masih ada baris data
                    while (reader.Read())
                    {
                        tools.Add(new ToolModel
                        {
                            ToolId = reader.GetInt32(0),
                            ToolTitle = reader.IsDBNull(reader.GetOrdinal("title")) ? "" : reader.GetString(reader.GetOrdinal("title")),
                            ToolDesc = reader.IsDBNull(reader.GetOrdinal("desc")) ? "" : reader.GetString(reader.GetOrdinal("desc")),
                            ToolCategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("category_id")),
                            ToolCategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? "" : reader.GetString(reader.GetOrdinal("category_name")),
                            ToolUserRating = reader.IsDBNull(reader.GetOrdinal("user_rating")) ? 0 : reader.GetInt32(reader.GetOrdinal("user_rating")),
                            ToolFirstLaunched = reader.IsDBNull(reader.GetOrdinal("first_launched")) ? "" : reader.GetString(reader.GetOrdinal("first_launched")),
                            ToolLastLaunched = reader.IsDBNull(reader.GetOrdinal("last_launched")) ? "" : reader.GetString(reader.GetOrdinal("last_launched")),
                            ToolLaunchTimes = reader.IsDBNull(reader.GetOrdinal("launch_times")) ? 0 : reader.GetInt32(reader.GetOrdinal("launch_times")),
                            ToolTarget = reader.IsDBNull(reader.GetOrdinal("target")) ? "" : reader.GetString(reader.GetOrdinal("target")),
                            ToolLaunchOptions = reader.IsDBNull(reader.GetOrdinal("launch_options")) ? "" : reader.GetString(reader.GetOrdinal("launch_options")),
                            ToolIconPath = reader.IsDBNull(reader.GetOrdinal("icon_path")) ? "" : reader.GetString(reader.GetOrdinal("icon_path")),
                            ToolCoverPath = reader.IsDBNull(reader.GetOrdinal("cover_path")) ? "" : reader.GetString(reader.GetOrdinal("cover_path"))
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

        public void DeleteApp(int id, string tableName)
        {
            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();
                var cmd = new SqliteCommand($"DELETE FROM {tableName} WHERE id = @id", connection);
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

        public void UpdateAppCategory(int appId, string tableName, int newCategoryId)
        {
            using (var connection = new SqliteConnection(ConnectionProfile))
            {
                connection.Open();

                string query = $"UPDATE {tableName} SET category_id = @catid WHERE id = @id";
                var cmd = new SqliteCommand(query, connection);
                if (newCategoryId <= 0)
                    cmd.Parameters.AddWithValue("@catid", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@catid", newCategoryId);

                cmd.Parameters.AddWithValue("@id", appId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateAppAtLaunch(IAppItem app, int sectionId)
        {
            string query_fl = $"UPDATE {app.TargetTable} SET first_launched = datetime('now', 'localtime') WHERE id = @id AND launch_times < 1";
            string query_ll = $"UPDATE {app.TargetTable} SET last_launched = datetime('now', 'localtime') WHERE id = @id";

            var args_fl = new Dictionary<string, object>
                {
                    { "@id", app.AppId }
                };
            var args_ll = new Dictionary<string, object>
                {
                    { "@id", app.AppId }
                };

            ExecuteQuery(ConnectionProfile, query_fl, args_fl);
            ExecuteQuery(ConnectionProfile, query_ll, args_ll);

            string query__ = $"UPDATE {app.TargetTable} SET launch_times = launch_times + 1 WHERE id = @id";

            var args__ = new Dictionary<string, object>
                {
                    { "@id", app.AppId }
                };

            ExecuteQuery(ConnectionProfile, query__, args__);
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
