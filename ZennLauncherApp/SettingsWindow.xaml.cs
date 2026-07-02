using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Resources.ResXFileRef;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = new DBHandler();
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        public Action OnSettingsApplied { get; set; }

        private readonly string _targetTabTag;

        public SettingsWindow(string targetTab = "general")
        {
            InitializeComponent();

            TextVersionDisplay.Text = $"Version {func.version}";
            SidebarSettingsList.ItemsSource = func.SettingsTabs;
            
            _targetTabTag = targetTab;
            this.Loaded += LoadTargetTab;
        }

        private void LoadTargetTab(object sender, RoutedEventArgs e)
        {
            var target = SidebarSettingsList.Items.Cast<TabModel>()
                            .FirstOrDefault(x => x.ItemTag == _targetTabTag);

            if (target != null)
            {
                SidebarSettingsList.SelectedItem = target;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).CurrentProfileCfg = cfg.LoadProfileConfig();
            this.Close();
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

        private void OnTabChanged(TabModel selectedTab)
        {
            if (selectedTab.TargetView != null)
            {
                var view = Activator.CreateInstance(selectedTab.TargetView) as UserControl;

                if (view is ISettingsTab tab)
                {
                    tab.OnSettingChanged = CheckSettingsChanged;
                }

                ContentSlot.Content = view;
            }
        }

        private void SidebarSettingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = SidebarSettingsList.SelectedItem as TabModel;

            if (selected != null)
            {
                OnTabChanged(selected);
            }
        }

        public void CheckSettingsChanged()
        {
            bool isDifferent = false;

            var jsonProfileConfig = cfg.LoadProfileConfig();
            if (((App)Application.Current).CurrentProfileCfg != jsonProfileConfig)
            {
                isDifferent = true;
            }

            BtnApply.IsEnabled = isDifferent;
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cfg.SaveProfileConfig(((App)Application.Current).CurrentProfileCfg);
                ((App)Application.Current).CurrentProfileCfg = cfg.LoadProfileConfig();

                OnSettingsApplied?.Invoke();
                BtnApply.IsEnabled = false;

                var Main = Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w is MainWindow) as MainWindow;
                Main?.EnableBlurAndOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error Saving Config", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
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
