using Microsoft.Data.Sqlite;
using Microsoft.WindowsAPICodePack.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace ZennLauncherApp
{
    public class UpdateHandler
    {
        GlobalFunctions func = new GlobalFunctions();
        DBHandler db = new DBHandler();
        ConfigHandler cfg = new ConfigHandler();

        public void BackupData()
        {
            try
            {
                SqliteConnection.ClearAllPools();

                string sourceFile = System.IO.Path.Combine(func.dataPath, "launcher_data.db");

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
                string backupFile = System.IO.Path.Combine(func.dataPath, "BACKUP", $"launcher_data_backup_{timestamp}.db");

                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, backupFile, true);

                    Debug.WriteLine("Backup succesful: " + backupFile);
                }
                else
                {
                    throw new FileNotFoundException("Global database file cannot be found!");
                }
            }
            catch (Exception ex)
            {
                HandleFatalError("Update Error: " + ex.Message);
            }
        }

        public void VersionChecker()
        {
            int result = Convert.ToInt32(db.IsTableExists("global_config"));

            if (result > 0)
            {
                string query_dc = "DELETE FROM global_config;";
                string query_ds = "DELETE FROM sqlite_sequence WHERE name = 'global_config'";

                db.ExecuteQuery(db.ConnectionGlobal, query_dc);
                db.ExecuteQuery(db.ConnectionGlobal, query_ds);
            }
            
            var currentConfig = cfg.LoadGlobalConfig();

            if (currentConfig.Build != func.build)
            {
                currentConfig.Build = func.build;
                currentConfig.Version = func.version;

                try
                {
                    cfg.SaveGlobalConfig(currentConfig);
                }
                catch (Exception ex)
                {
                    HandleFatalError("Error: " + ex.Message);
                }
            }
        }

        public void SessionTokenUpdate()
        {
            bool columnExists = db.IsColumnExists(db.ConnectionGlobal, "profiles", "session_token");

            if (!columnExists)
            {
                string query_as = "ALTER TABLE profiles ADD COLUMN session_token TEXT DEFAULT ''";

                try
                {
                    db.ExecuteQuery(db.ConnectionGlobal, query_as);
                }
                catch (Exception ex)
                {
                    HandleFatalError("Error: " + ex.Message);
                }

            }
        } 

        private void HandleFatalError(string message)
        {
            var main = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            System.Windows.MessageBox.Show(main, message, "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Environment.Exit(1);
        }

    }
}
