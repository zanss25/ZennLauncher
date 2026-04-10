using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GlobalFunctions func = new GlobalFunctions();
        UpdateHandler up = new UpdateHandler();
        DBHandler db = new DBHandler();
        ConfigHandler cfg = new ConfigHandler();

        public MainWindow()
        {
            InitializeComponent();

            LoadProfileData();

            string defaultTab = ((App)System.Windows.Application.Current).CurrentProfileCfg.General.StartingTab;

            if (!String.IsNullOrEmpty(defaultTab))
            {
                var targetItem = SidebarNavList.Items.OfType<ListBoxItem>()
                 .FirstOrDefault(item => item.Tag?.ToString() == defaultTab);

                SidebarNavList.SelectedItem = targetItem;
            }
            else
            {
                var targetItem = SidebarNavList.Items.OfType<ListBoxItem>()
                 .FirstOrDefault(item => item.Tag?.ToString() == "games");

                SidebarNavList.SelectedItem = targetItem;
            }
        }

        public void LoadProfileData()
        {
            db.profileData = db.GetProfileData();
            this.DataContext = db.profileData;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            var currentProfile = this.DataContext as ProfileDataModel;

            if (currentProfile.ProfileKeepLoggedIn <= 0)
            {
                var currentConfig = cfg.LoadGlobalConfig();

                currentConfig.ActiveProfile = -1;
                currentConfig.SessionToken = "";

                try
                {
                    cfg.SaveGlobalConfig(currentConfig);

                    string query_rs = "UPDATE profiles SET session_token = '' WHERE id = @profile_id";

                    var args_rs = new Dictionary<string, object>()
                    {
                        { "@profile_id", currentProfile.ProfileId }
                    };

                    db.ExecuteQuery(db.ConnectionGlobal, query_rs, args_rs);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error Saving Config to JSON File: " + ex.Message);
                }
            }

            Close();
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
            {
                this.WindowState = WindowState.Maximized;
            } 
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void LinkProfileInfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnProfileSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();

            settings.ShowDialog();
        }

        private void BtnSwitchProfile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLogOut_Click(object sender, RoutedEventArgs e)
        {
            var currentProfile = this.DataContext as ProfileDataModel;

            var currentConfig = cfg.LoadGlobalConfig();

            currentConfig.ActiveProfile = -1;
            currentConfig.SessionToken = "";

            try
            {
                cfg.SaveGlobalConfig(currentConfig);

                string query_rs = "UPDATE profiles SET session_token = '' WHERE id = @profile_id";

                var args_rs = new Dictionary<string, object>()
                    {
                        { "@profile_id", currentProfile.ProfileId }
                    };

                db.ExecuteQuery(db.ConnectionGlobal, query_rs, args_rs);

                ProfileSelectWindow profiles = new ProfileSelectWindow();
                profiles.Show();
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error Saving Config to JSON File: " + ex.Message);
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.ShowDialog();
        }

        private void BtnAddNewGame_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Supported Program Files (*.exe, *.lnk)|*.exe;*.lnk|All files (*.*)|*.*";
            openFileDialog.Title = "Select Game Executable";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                GamePropertiesWindow add = new GamePropertiesWindow();

                ImageSource iconSource = func.GetIconNative(selectedFilePath);

                add.InputGameTarget.Text = selectedFilePath;
                add.InputGameStartIn.Text = System.IO.Path.GetDirectoryName(selectedFilePath);
                add.InputGameTitle.Text = System.IO.Path.GetFileNameWithoutExtension(selectedFilePath);

                if (iconSource != null)
                {
                    add.PreviewIcon.Background = new ImageBrush(iconSource);
                }

                if (add.ShowDialog() == true)
                {
                    if (ContentSlot.Content is GamesView gview)
                    {
                        gview.LoadGamesList();
                    }
                }
                else
                {
                    if (ContentSlot.Content is GamesView gview)
                    {
                        gview.LoadGamesList();
                    }
                }
            }
        }

        private void BtnAddNewTool_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Supported Program Files (*.exe, *.lnk)|*.exe;*.lnk|All files (*.*)|*.*";
            openFileDialog.Title = "Select Tool Executable";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                ToolPropertiesWindow add = new ToolPropertiesWindow();

                ImageSource iconSource = func.GetIconNative(selectedFilePath);

                add.InputToolTarget.Text = selectedFilePath;
                add.InputToolStartIn.Text = System.IO.Path.GetDirectoryName(selectedFilePath);
                add.InputToolTitle.Text = System.IO.Path.GetFileNameWithoutExtension(selectedFilePath);

                if (iconSource != null)
                {
                    add.PreviewIcon.Background = new ImageBrush(iconSource);
                }

                if (add.ShowDialog() == true)
                {
                    if (ContentSlot.Content is ToolsView gview)
                    {
                        gview.LoadToolsList();
                    }
                }
                else
                {
                    if (ContentSlot.Content is ToolsView gview)
                    {
                        gview.LoadToolsList();
                    }
                }
            }
        }

        private void SidebarNavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SidebarNavList.SelectedItem is FrameworkElement element)
            {
                if (element.Tag == null)
                {
                    Debug.WriteLine("!! Error: tag is NULL bruh");
                    return;
                }

                string tag = element.Tag.ToString();

                switch (tag)
                {
                    case "games":
                        ContentSlot.Content = new GamesView();
                        break;
                    case "tools":
                        ContentSlot.Content = new ToolsView();
                        break;
                    case "categories":
                        ContentSlot.Content = new CategoriesView();
                        break;
                    case "relics":
                        ContentSlot.Content = new RelicsHistoryView();
                        break;
                    default:
                        System.Windows.MessageBox.Show($"Error: Tag '{tag}' is not listed in the switch-case");
                        break;
                }
            }
        }
    }
}
