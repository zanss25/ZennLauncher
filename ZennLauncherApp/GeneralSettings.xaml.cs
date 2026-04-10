using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for GeneralSettings.xaml
    /// </summary>
    public partial class GeneralSettingsView : UserControl
    {
        GlobalFunctions func = new GlobalFunctions();
        DBHandler db = new DBHandler();
        ConfigHandler cfg = new ConfigHandler();

        public GeneralSettingsView()
        {
            InitializeComponent();
            LoadCfgToUI();
            
        }

        private void LoadCfgToUI()
        {
            //Starting Tab
            if (((App)Application.Current).CurrentProfileCfg.General.StartingTab == "games")
            {
                ComboDefaultStartView.SelectedIndex = 0;
            }
            if (((App)Application.Current).CurrentProfileCfg.General.StartingTab == "tools")
            {
                ComboDefaultStartView.SelectedIndex = 1;
            }

            // Games Sort Option
            ComboGamesSortOption.ItemsSource = func.SortOptions;
            ComboGamesSortOption.DisplayMemberPath = "DisplayName";
            ComboGamesSortOption.SelectedIndex = 0;

            string defaultGamesSortOption = ((App)Application.Current).CurrentProfileCfg.General.GamesDefaultSortOption;

            if (string.IsNullOrEmpty(defaultGamesSortOption))
            {
                defaultGamesSortOption = "title";
            }

            var targetGamesSortObject = func.SortOptions
                                   .FirstOrDefault(s => s.SortBy == defaultGamesSortOption);

            if (targetGamesSortObject != null)
            {
                ComboGamesSortOption.SelectedItem = targetGamesSortObject;
            }
            else
            {
                ComboGamesSortOption.SelectedIndex = 0;
            }

            // Tools Sort Option
            ComboToolsSortOption.ItemsSource = func.SortOptions;
            ComboToolsSortOption.DisplayMemberPath = "DisplayName";
            ComboToolsSortOption.SelectedIndex = 0;

            string defaultToolsSortOption = ((App)Application.Current).CurrentProfileCfg.General.ToolsDefaultSortOption;

            if (string.IsNullOrEmpty(defaultToolsSortOption))
            {
                defaultToolsSortOption = "title";
            }

            var targetToolsSortObject = func.SortOptions
                                   .FirstOrDefault(s => s.SortBy == defaultToolsSortOption);

            if (targetToolsSortObject != null)
            {
                ComboToolsSortOption.SelectedItem = targetToolsSortObject;
            }
            else
            {
                ComboToolsSortOption.SelectedIndex = 0;
            }

            // Games Launch Action
            if (((App)Application.Current).CurrentProfileCfg.General.GamesLaunchAction == ProfileGeneralSettings.LaunchAction.HideLauncher)
            {
                ComboGamesLaunch.SelectedIndex = 0;
            }
            else if (((App)Application.Current).CurrentProfileCfg.General.GamesLaunchAction == ProfileGeneralSettings.LaunchAction.CloseLauncher)
            {
                ComboGamesLaunch.SelectedIndex = 1;
            }
            else
            {
                ComboGamesLaunch.SelectedIndex = 2;
            }

            // Tools Launch Action
            if (((App)Application.Current).CurrentProfileCfg.General.ToolsLaunchAction == ProfileGeneralSettings.LaunchAction.HideLauncher)
            {
                ComboToolsLaunch.SelectedIndex = 0;
            }
            else if (((App)Application.Current).CurrentProfileCfg.General.ToolsLaunchAction == ProfileGeneralSettings.LaunchAction.CloseLauncher)
            {
                ComboToolsLaunch.SelectedIndex = 1;
            }
            else
            {
                ComboToolsLaunch.SelectedIndex = 2;
            }

            // Start on Windows Login
            if (((App)Application.Current).CurrentProfileCfg.General.StartOnWindows == 1)
            {
                CheckBoxStartupOnWindows.IsChecked = true;
            }
            else
            {
                CheckBoxStartupOnWindows.IsChecked = false;
            }
        }

        public void ComboDefaultStartView_Changed(object sender, EventArgs e)
        {

        }

        public void ComboSortOption_Changed(object sender, EventArgs e)
        {

        }

        public void ComboLaunch_Changed(object sender, EventArgs e)
        {

        }

    }
}
