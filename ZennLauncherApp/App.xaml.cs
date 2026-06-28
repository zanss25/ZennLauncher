using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly UpdateHandler up = new UpdateHandler();
        private readonly DBHandler db = DBHandler.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        public GlobalConfigModel CurrentGlobalCfg;
        public ProfileConfigModel CurrentProfileCfg;

        public SystemTrayManager trayManager { get; set; }

        private static Mutex _mutex = null;

        private const string appName = "ZennLauncherApp-29484735857748";
        private const string PipeName = "ZennLauncherPipe-29484735857748";

        // --- WIN32 API TO CALL EXISTING APP INSTANCE ---
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        // Konstanta perintah untuk ShowWindow utk memulihkan window dari minimize/tray
        private const int SW_RESTORE = 9;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool createdNew;

            // Trying to take Mutex ownership
            _mutex = new Mutex(true, appName, out createdNew);

            string targetCommand = "WAKE_UP";
            if (e.Args.Length > 0)
            {
                string rawArg = e.Args[0];
                string cleanArg = rawArg.TrimStart('-');

                targetCommand = $"OPEN_TAB:{e.Args[0]}";
            }

            if (!createdNew)
            {
                SendWakeUpCommand(targetCommand);

                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            StartPipeServer();

            db.GlobalDBInitialize();
            db.ProfileDBInitialize();
            up.VersionChecker();
            up.SessionTokenUpdate();

            cfg.CreateDefaultCfg();
            CurrentGlobalCfg = cfg.LoadGlobalConfig();

            if (CheckIsKeepLoggedIn())
            {
                LoadProfileData();
            }
        }

        // --- INSTANCE BARU: Memberikan informasi + perintah ---
        private void SendWakeUpCommand(string command)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    client.Connect(1000);
                    using (var writer = new StreamWriter(client))
                    {
                        writer.Write(command);
                        writer.Flush();
                    }
                }
            }
            catch { /* Instance pertama tidak merespon */ }
        }

        // --- INSTANCE LAMA: Mendengarkan informasi dari instance baru ---
        private async void StartPipeServer()
        {
            while (true)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                    {
                        // Tunggu sampai ada instansi kedua yang mengetuk
                        await server.WaitForConnectionAsync();

                        using (var reader = new StreamReader(server))
                        {
                            string message = await reader.ReadToEndAsync();
                            if (message == "WAKE_UP")
                            {
                                // Karena event ini berjalan di Thread latar belakang, 
                                // kita wajib melempar perintah bangun ke UI Thread utama WPF
                                Current.Dispatcher.Invoke(() =>
                                {
                                    trayManager.MaximizeFromTray();
                                });
                            }
                        }
                    }
                }
                catch
                {
                    // Antisipasi error crash pada server pipe, loop akan me-restart server otomatis
                    await Task.Delay(1000);
                }
            }
        }

        private void ProcessIncomingCommand(string message)
        {
            trayManager.MaximizeFromTray();

            MainWindow targetWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (targetWindow != null)
            {
                if (message.StartsWith("OPEN_TAB:"))
                {
                    // Ambil argumen setelah tanda titik dua (misal: "Tools" atau "Boards")
                    string tabName = message.Substring("OPEN_TAB:".Length);

                    // 3. Panggil fungsi publik yang ada di dalam MainWindow.xaml.cs kamu
                    targetWindow.NavigateToTab(tabName);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Lepaskan Mutex saat aplikasi benar-benar ditutup
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
            base.OnExit(e);
        }

        public bool CheckIsKeepLoggedIn()
        {
            int _activeProfileId = CurrentGlobalCfg.ActiveProfile;

            if (_activeProfileId <= 0)
            {
                if (!Current.Windows.OfType<ProfileSelectWindow>().Any())
                {
                    AppStateManager.Instance.TriggerLogout();

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

            AppStateManager.Instance.isLoggedIn = true;
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
                AppStateManager.Instance.LoadAllData();
                AppStateManager.Instance.isLoggedIn = true;        

                MainWindow main = new MainWindow();

                main.Loaded += (s, e) =>
                {
                    trayManager = new SystemTrayManager();
                };

                main.Show();
            }
            else
            {
                Debug.WriteLine("DEBUG: Validasi Gagal, Lempar ke Login...");

                AppStateManager.Instance.isLoggedIn = false;
                AppStateManager.Instance.HideTrayAction?.Invoke();

                if (!Current.Windows.OfType<ProfileSelectWindow>().Any())
                {
                    ProfileSelectWindow profile = new ProfileSelectWindow();
                    profile.Show();
                }
            }
        }

    }
}
