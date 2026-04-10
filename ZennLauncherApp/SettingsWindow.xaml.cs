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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        GlobalFunctions func = new GlobalFunctions();
        DBHandler db = new DBHandler();
        ConfigHandler cfg = new ConfigHandler();

        public SettingsWindow()
        {
            InitializeComponent();

            TextVersionDisplay.Text = $"Version {func.version}";
            SidebarSettingsList.SelectedIndex = 0;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                cfg.SaveProfileConfig(((App)Application.Current).CurrentProfileCfg);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error saving config", MessageBoxButton Yes, MessageBoxImage Error);
            }

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

        private void SidebarSettingsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SidebarSettingsList.SelectedItem is FrameworkElement element)
            {
                if (element.Tag == null)
                {
                    Debug.WriteLine("!! Error: tag is NULL bruh");
                    return;
                }

                string tag = element.Tag.ToString();

                switch (tag)
                {
                    case "general":
                        ContentSlot.Content = new GeneralSettingsView();
                        break;
                    case "myprofile":
                        ContentSlot.Content = new MyProfileSettingsView();
                        break;
                    case "interface":
                        ContentSlot.Content = new InterfaceSettingsView();
                        break;
                    case "categories":
                        ContentSlot.Content = new CategoriesSettings();
                        break;
                    case "folders":
                        ContentSlot.Content = new FoldersSettingsView();
                        break;
                    case "advanced":
                        ContentSlot.Content = new AdvancedSettingsView();
                        break;
                    default:
                        MessageBox.Show($"Error: Tag '{tag}' is not listed in the switch-case");
                        break;
                }
            }
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
