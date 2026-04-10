using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for ProfileSelectWindow.xaml
    /// </summary>
    public partial class ProfileSelectWindow : Window
    {
        GlobalFunctions func = new GlobalFunctions();
        UpdateHandler up = new UpdateHandler();
        DBHandler db = new DBHandler();
        ConfigHandler cfg = new ConfigHandler();

        List<ProfileDataModel> allProfilesList = new List<ProfileDataModel>();

        public ProfileSelectWindow()
        {
            InitializeComponent();
            up.SessionTokenUpdate();

            LoadProfilesList("id", true);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void LoadProfilesList(string _sortBy = "id", bool _ascending = true)
        {

            int _profilesCount = 0;
            _profilesCount = db.ExecuteScalar<int>(db.ConnectionGlobal, "SELECT COUNT(*) FROM profiles");

            if (_profilesCount > 0)
            {
                allProfilesList = db.GetAllProfiles(_sortBy, _ascending);

                SidebarProfilesList.ItemsSource = null;
                SidebarProfilesList.ItemsSource = allProfilesList;

            }
        }

        int selectedProfileId;
        string selectedProfileHash;

        private void SidebarProfilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedProfile = SidebarProfilesList.SelectedItem as ProfileDataModel;

            if (selectedProfile != null)
            {
                selectedProfileId = selectedProfile.ProfileId;
                selectedProfileHash = selectedProfile.ProfileHash;
                InputProfilePass.Focus();
            }
            else
            {
                selectedProfileId = 0;
                selectedProfileHash = null;
            }

            Console.WriteLine(selectedProfileId);
        }

        public bool newProfileSaved = false;

        private void BtnCreateNewProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileCreateWindow create = new ProfileCreateWindow();

            try
            {
                create.ShowDialog();

                if (newProfileSaved == true)
                {
                    LoadProfilesList();
                    newProfileSaved = false;
                }
                
            } catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void BtnLogIn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProfileId <= 0) return;

            if (!string.IsNullOrEmpty(selectedProfileHash))
            {
                if (!BCrypt.Net.BCrypt.Verify(InputProfilePass.Password, selectedProfileHash))
                {
                    MessageBox.Show("Password incorrect");
                    return;
                }
            }

            try
            {
                string newToken = Guid.NewGuid().ToString();
                int isKeepLogin = (CheckBoxKeepLoggedIn.IsChecked == true) ? 1 : 0;

                string query_update = "UPDATE profiles SET keep_logged_in = @is_keep, session_token = @new_token WHERE id = @current_id";
                var args_update = new Dictionary<string, object>
                {
                    { "@is_keep", isKeepLogin },
                    { "@new_token", newToken },
                    { "@current_id", selectedProfileId }
                };
                db.ExecuteQuery(db.ConnectionGlobal, query_update, args_update);

                var currentConfig = cfg.LoadGlobalConfig();
                currentConfig.ActiveProfile = selectedProfileId;

                currentConfig.SessionToken = GlobalFunctions.SecurityHelper.Encrypt(newToken);

                cfg.SaveGlobalConfig(currentConfig);

                cfg.LoadProfileConfig();

                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Log In Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
