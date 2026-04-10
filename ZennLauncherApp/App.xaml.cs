using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        GlobalFunctions func = new GlobalFunctions();
        UpdateHandler up = new UpdateHandler();
        DBHandler db = new DBHandler();
        ConfigHandler cfg = new ConfigHandler();

        public GlobalConfigModel CurrentGlobalCfg;
        public ProfileConfigModel CurrentProfileCfg;

        public App()
        {
            db.GlobalDBInitialize();
            db.ProfileDBInitialize();
            up.VersionChecker();
            up.SessionTokenUpdate();

            cfg.CreateDefaultCfg();
            CurrentGlobalCfg = cfg.LoadGlobalConfig();

            bool isKeepLoggedIn = CheckIsKeepLoggedIn();

            if (isKeepLoggedIn)
            {
                LoadProfileData();
            }

        }

        public bool CheckIsKeepLoggedIn()
        {
            int _activeProfileId = CurrentGlobalCfg.ActiveProfile;

            if (_activeProfileId <= 0)
            {
                if (!Current.Windows.OfType<ProfileSelectWindow>().Any())
                {
                    ProfileSelectWindow profile = new ProfileSelectWindow();
                    profile.Show();
                }

                return false;
            }

            string query_ck = "SELECT keep_logged_in FROM profiles WHERE id = @profile_id";

            var args_ck = new Dictionary<string, object>
            {
                { "@profile_id", _activeProfileId }
            };

            int isKeepLoggedIn = db.ExecuteScalar<int>(db.ConnectionGlobal, query_ck, args_ck);

            if (isKeepLoggedIn <= 0)
            {
                CurrentGlobalCfg.SessionToken = "";

                try
                {
                    cfg.SaveGlobalConfig(CurrentGlobalCfg);

                    string query_rs = "UPDATE profiles SET session_token = '' WHERE id = @profile_id";

                    var args_rs = new Dictionary<string, object>()
                    {
                        { "@profile_id", CurrentGlobalCfg.ActiveProfile }
                    };

                    db.ExecuteQuery(db.ConnectionGlobal, query_rs, args_rs);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error Saving Config to JSON File: " + ex.Message);
                }

                CurrentGlobalCfg.ActiveProfile = -1;
                cfg.SaveGlobalConfig(CurrentGlobalCfg);

                System.Windows.Forms.Application.Restart();
                Current.Shutdown();
            }
            return true;
        }

        public void LoadProfileData()
        {
            int _activeProfileId = CurrentGlobalCfg.ActiveProfile;
            string _encryptedTokenFromConfig = CurrentGlobalCfg.SessionToken;

            string query_cs = "SELECT session_token FROM profiles WHERE id = @profile_id";

            var args_cs = new Dictionary<string, object>
            {
                { "@profile_id", _activeProfileId }
            };

            string tokenFromDb = db.ExecuteScalar<string>(db.ConnectionGlobal, query_cs, args_cs);

            string decryptedSessionToken = GlobalFunctions.SecurityHelper.Decrypt(_encryptedTokenFromConfig);

            Debug.WriteLine($"DEBUG: ID={_activeProfileId}");
            Debug.WriteLine($"DEBUG: DB Token='{tokenFromDb}'");
            Debug.WriteLine($"DEBUG: Config Decrypted='{decryptedSessionToken}'");

            bool isTokenMatch = !string.IsNullOrEmpty(tokenFromDb) && tokenFromDb.Trim() == decryptedSessionToken.Trim();

            if (isTokenMatch)
            {
                CurrentProfileCfg = cfg.LoadProfileConfig();
                MainWindow main = new MainWindow();
                main.Show();
            }
            else
            {
                Debug.WriteLine("DEBUG: Validasi Gagal, Lempar ke Login...");

                if (!Current.Windows.OfType<ProfileSelectWindow>().Any())
                {
                    ProfileSelectWindow profile = new ProfileSelectWindow();
                    profile.Show();
                }
            }
        }
    }
}
