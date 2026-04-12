using Microsoft.Data.Sqlite;
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
    public partial class GeneralSettingsView : UserControl, ISettingsTab
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = DBHandler.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        public Action OnSettingChanged { get; set; }

        public GeneralSettingsView()
        {
            InitializeComponent();
            LoadCfgToUI();
            
        }

        private void LoadCfgToUI()
        {
            // Default Starting Tab
            ComboDefaultStartingTab.ItemsSource = func.SidebarTabs;
            ComboDefaultStartingTab.DisplayMemberPath = "DisplayName";
            ComboDefaultStartingTab.SelectedValuePath = "ItemTag";

            ComboDefaultStartingTab.SelectedValue = ((App)Application.Current).CurrentProfileCfg.General.StartingTab;

            // Games Sort Option
            ComboGamesSortOption.ItemsSource = func.SortOptions;
            ComboGamesSortOption.DisplayMemberPath = "DisplayName";
            ComboGamesSortOption.SelectedValuePath = "SortByTag";

            ComboGamesSortOption.SelectedValue = ((App)Application.Current).CurrentProfileCfg.General.GamesDefaultSortOption;

            // Tools Sort Option
            ComboToolsSortOption.ItemsSource = func.SortOptions;
            ComboToolsSortOption.DisplayMemberPath = "DisplayName";
            ComboToolsSortOption.SelectedValuePath = "SortByTag";

            ComboToolsSortOption.SelectedValue = ((App)Application.Current).CurrentProfileCfg.General.ToolsDefaultSortOption;

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
            if (((App)Application.Current).CurrentProfileCfg.General.StartOnWindows == true)
            {
                CheckBoxStartupOnWindows.IsChecked = true;
            }
            else
            {
                CheckBoxStartupOnWindows.IsChecked = false;
            }
        }

        public void ComboDefaultStartingTab_Changed(object sender, EventArgs e)
        {
            var selectedItem = ComboDefaultStartingTab.SelectedItem as TabModel;

            if (selectedItem != null)
            {
                string selectedOption = Convert.ToString(selectedItem.ItemTag);

                ((App)Application.Current).CurrentProfileCfg.General.StartingTab = selectedOption;

                Debug.WriteLine("StartingTab: " + 
                                ((App)Application.Current).CurrentProfileCfg.General.StartingTab);
                OnSettingChanged?.Invoke();
            }
        }

        public void ComboGamesSortOption_Changed(object sender, EventArgs e)
        {
            var selectedItem = ComboGamesSortOption.SelectedItem as SortCriterion;

            if (selectedItem != null)
            {
                string selectedOption = Convert.ToString(selectedItem.SortByTag);

                ((App)Application.Current).CurrentProfileCfg.General.GamesDefaultSortOption = selectedOption;

                Debug.WriteLine("GamesDefaultSortOption: " + 
                                ((App)Application.Current).CurrentProfileCfg.General.GamesDefaultSortOption);
                OnSettingChanged?.Invoke();
            }
        }

        public void ComboToolsSortOption_Changed(object sender, EventArgs e)
        {
            var selectedItem = ComboToolsSortOption.SelectedItem as SortCriterion;

            if (selectedItem != null)
            {
                string selectedOption = Convert.ToString(selectedItem.SortByTag);

                ((App)Application.Current).CurrentProfileCfg.General.ToolsDefaultSortOption = selectedOption;

                Debug.WriteLine("ToolsDefaultSortOption: " + 
                                ((App)Application.Current).CurrentProfileCfg.General.ToolsDefaultSortOption);
                OnSettingChanged?.Invoke();
            }
        }

        public void ComboGamesLaunch_Changed(object sender, EventArgs e)
        {
            var selectedItem = ComboGamesLaunch.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                string selectedOption = Convert.ToString(selectedItem.Tag);

                var action = (ProfileGeneralSettings.LaunchAction)Enum.Parse(typeof(ProfileGeneralSettings.LaunchAction), selectedOption);
                ((App)Application.Current).CurrentProfileCfg.General.GamesLaunchAction = action;

                Debug.WriteLine("GamesLaunchAction: " + 
                                ((App)Application.Current).CurrentProfileCfg.General.GamesLaunchAction);
                OnSettingChanged?.Invoke();
            }
        }

        public void ComboToolsLaunch_Changed(object sender, EventArgs e)
        {
            var selectedItem = ComboToolsLaunch.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                string selectedOption = Convert.ToString(selectedItem.Tag);

                var action = (ProfileGeneralSettings.LaunchAction)Enum.Parse(typeof(ProfileGeneralSettings.LaunchAction), selectedOption);
                ((App)Application.Current).CurrentProfileCfg.General.ToolsLaunchAction = action;

                Debug.WriteLine("ToolsLaunchAction: " + 
                                ((App)Application.Current).CurrentProfileCfg.General.ToolsLaunchAction);
                OnSettingChanged?.Invoke();
            }
        }

        public void CheckBoxStartupOnWindows_Click(object sender, EventArgs e)
        {
            if (CheckBoxStartupOnWindows.IsChecked == true)
            {
                ((App)Application.Current).CurrentProfileCfg.General.StartOnWindows = true;
                
            }
            if (CheckBoxStartupOnWindows.IsChecked != true)
            {
                ((App)Application.Current).CurrentProfileCfg.General.StartOnWindows = false;
            }

            Debug.WriteLine("StartOnWIndows: " + 
                            ((App)Application.Current).CurrentProfileCfg.General.StartOnWindows);
            OnSettingChanged?.Invoke();
        }

    }
}
