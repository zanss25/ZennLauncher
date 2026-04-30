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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZennLauncherApp.Properties;
using static System.Resources.ResXFileRef;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = DBHandler.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            LoadProfileData();
            SidebarNavList.ItemsSource = func.SidebarTabs;
            LoadDefaultStartingTab();
        }

        public void LoadProfileData()
        {
            db.profileData = db.GetProfileData();
            this.DataContext = db.profileData;
            ((App)Application.Current).CurrentProfileCfg = cfg.LoadProfileConfig();
        }

        public void LoadDefaultStartingTab()
        {
            try
            {
                string defaultStartingTab = ((App)Application.Current)?.CurrentProfileCfg?.General?.StartingTab;

                if (string.IsNullOrEmpty(defaultStartingTab))
                {
                    defaultStartingTab = "games";
                }

                var targetTabObject = func.SidebarTabs
                                       .FirstOrDefault(s => s.ItemTag == defaultStartingTab);

                if (targetTabObject != null)
                {
                    SidebarNavList.SelectedItem = targetTabObject;
                }
                else
                {
                    SidebarNavList.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex, "MainWindow Load Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
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

        private void LinkProfileSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow("myprofile");

            EnableBlurAndOverlay();
            settings.ShowDialog();
            DisableBlurAndOverlay();
        }

        private void BtnProfileSettings_Click(object sender, RoutedEventArgs e)
        {
            
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
            int selectedTab = SidebarNavList.SelectedIndex;

            SettingsWindow settings = new SettingsWindow();
            settings.OnSettingsApplied = () => {
                RefreshCurrentTab();
            };

            EnableBlurAndOverlay();
            settings.ShowDialog();
            DisableBlurAndOverlay();
        }

        private void BtnAddNewGame_Click(object sender, RoutedEventArgs e)
        {
            EnableBlurAndOverlay();
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Supported Program Files (*.exe, *.lnk)|*.exe;*.lnk|All files (*.*)|*.*";
            openFile.Title = "Select Game Executable";

            if (Directory.Exists(((App)Application.Current).CurrentProfileCfg.Folders.DefaultGamesDir))
            {
                openFile.InitialDirectory = ((App)Application.Current).CurrentProfileCfg.Folders.DefaultGamesDir;
            }

            if (openFile.ShowDialog() == true)
            {
                string selectedFilePath = openFile.FileName;

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
                        gview.ComboSortByGames_Changed(gview.ComboSortByGames, null);
                    }
                }
                else
                {
                    if (ContentSlot.Content is GamesView gview)
                    {
                        gview.ComboSortByGames_Changed(gview.ComboSortByGames, null);
                    }
                }
            }

            DisableBlurAndOverlay();
        }

        private void BtnAddNewTool_Click(object sender, RoutedEventArgs e)
        {
            EnableBlurAndOverlay();
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Supported Program Files (*.exe, *.lnk)|*.exe;*.lnk|All files (*.*)|*.*";
            openFile.Title = "Select Tool Executable";

            if (Directory.Exists(((App)Application.Current).CurrentProfileCfg.Folders.DefaultToolsDir))
            {
                openFile.InitialDirectory = ((App)Application.Current).CurrentProfileCfg.Folders.DefaultToolsDir;
            }

            if (openFile.ShowDialog() == true)
            {
                string selectedFilePath = openFile.FileName;

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
                        gview.ComboSortByTools_Changed(gview.ComboSortByTools, null);
                    }
                }
                else
                {
                    if (ContentSlot.Content is ToolsView gview)
                    {
                        gview.ComboSortByTools_Changed(gview.ComboSortByTools, null);
                    }
                }
            }

            DisableBlurAndOverlay();
        }

        private void RefreshCurrentTab()
        {
            var selected = SidebarNavList.SelectedItem as TabModel;

            if (selected != null)
            {
                var newView = Activator.CreateInstance(selected.TargetView) as UserControl;
                ContentSlot.Content = newView;
            }
        }

        private void OnTabChanged(TabModel selectedTab)
        {
            if (selectedTab.TargetView != null)
            {
                var view = Activator.CreateInstance(selectedTab.TargetView) as UserControl;

                ContentSlot.Content = view;
            }
        }

        private void SidebarNavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = SidebarNavList.SelectedItem as TabModel;

            if (selected != null)
            {
                OnTabChanged(selected);

                StackPanelGamesBtns.Visibility = Visibility.Collapsed;
                StackPanelToolsBtns.Visibility = Visibility.Collapsed;

                if (selected.ItemTag == "games")
                {
                    StackPanelGamesBtns.Visibility = Visibility.Visible;
                }
                if (selected.ItemTag == "tools")
                {
                    StackPanelToolsBtns.Visibility = Visibility.Visible;
                }
            }
        }

        public void EnableBlurAndOverlay()
        {
            if (MainGrid.Effect is BlurEffect blur)
            {
                if (((App)Application.Current).CurrentProfileCfg.Interface.DialogOverlay
                  == ProfileInterfaceSettings.DialogOverlayModes.DimWithBlur)
                {
                    blur.Radius = 20;

                    var converter = new BrushConverter();
                    BlackOverlay.Background = (Brush)converter.ConvertFromString("#30080808");
                }

                if (((App)Application.Current).CurrentProfileCfg.Interface.DialogOverlay
                  == ProfileInterfaceSettings.DialogOverlayModes.DimOnly)
                {
                    blur.Radius = 0;

                    var converter = new BrushConverter();
                    BlackOverlay.Background = (Brush)converter.ConvertFromString("#60080808");
                }
                if (((App)Application.Current).CurrentProfileCfg.Interface.DialogOverlay
                  == ProfileInterfaceSettings.DialogOverlayModes.None)
                {
                    blur.Radius = 0;

                    var converter = new BrushConverter();
                    BlackOverlay.Background = (Brush)converter.ConvertFromString("#00000000");
                }
            }
        }

        public void DisableBlurAndOverlay()
        {
            if (MainGrid.Effect is BlurEffect blurAfter)
            {
                blurAfter.Radius = 0;

                var converter = new BrushConverter();
                BlackOverlay.Background = (Brush)converter.ConvertFromString("#00000000");
            }
        }

    }
}
