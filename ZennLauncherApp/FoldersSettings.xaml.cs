using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
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

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for FoldersSettings.xaml
    /// </summary>
    public partial class FoldersSettingsView : UserControl, ISettingsTab
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = DBHandler.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        public Action OnSettingChanged { get; set; }

        public FoldersSettingsView()
        {
            InitializeComponent();
            LoadCfgToUI();
        }

        private void LoadCfgToUI()
        {
            // Games Default Directory
            InputGamesDefaultDir.Text = ((App)Application.Current).CurrentProfileCfg.Folders.DefaultGamesDir;

            // Tools Default Directory
            InputToolsDefaultDir.Text = ((App)Application.Current).CurrentProfileCfg.Folders.DefaultToolsDir;

            // Icons Default Directory
            InputIconsDefaultDir.Text = ((App)Application.Current).CurrentProfileCfg.Folders.DefaultIconsDir;

            // Grid Covers Default Directory
            InputCoversDefaultDir.Text = ((App)Application.Current).CurrentProfileCfg.Folders.DefaultCoversDir;

        }

        private void BtnBrowseGamesDefaultDir_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cmnOpenFile = new CommonOpenFileDialog();
            cmnOpenFile.IsFolderPicker = true;
            cmnOpenFile.Title = "Select Default Games Folder";

            Window owner = Window.GetWindow(this);

            if (cmnOpenFile.ShowDialog(owner) == CommonFileDialogResult.Ok)
            {
                InputGamesDefaultDir.Text = cmnOpenFile.FileName;
                ((App)Application.Current).CurrentProfileCfg.Folders.DefaultGamesDir = cmnOpenFile.FileName;

                OnSettingChanged?.Invoke();
            }
        }

        private void BtnResetGamesDefaultDir_Click(object sender, RoutedEventArgs e)
        {
            InputGamesDefaultDir.Clear();
            ((App)Application.Current).CurrentProfileCfg.Folders.DefaultGamesDir = "";

            OnSettingChanged?.Invoke();
        }

        private void BtnBrowseToolsDefaultDir_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cmnOpenFile = new CommonOpenFileDialog();
            cmnOpenFile.IsFolderPicker = true;
            cmnOpenFile.Title = "Select Default Tools Folder";

            Window owner = Window.GetWindow(this);

            if (cmnOpenFile.ShowDialog(owner) == CommonFileDialogResult.Ok)
            {
                InputToolsDefaultDir.Text = cmnOpenFile.FileName;
                ((App)Application.Current).CurrentProfileCfg.Folders.DefaultToolsDir = cmnOpenFile.FileName;

                OnSettingChanged?.Invoke();
            }
        }

        private void BtnResetToolsDefaultDir_Click(object sender, RoutedEventArgs e)
        {
            InputToolsDefaultDir.Clear();
            ((App)Application.Current).CurrentProfileCfg.Folders.DefaultToolsDir = "";

            OnSettingChanged?.Invoke();
        }

        private void BtnBrowseIconsDefaultDir_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cmnOpenFile = new CommonOpenFileDialog();
            cmnOpenFile.IsFolderPicker = true;
            cmnOpenFile.Title = "Select Default Icons Folder";

            Window owner = Window.GetWindow(this);

            if (cmnOpenFile.ShowDialog(owner) == CommonFileDialogResult.Ok)
            {
                InputIconsDefaultDir.Text = cmnOpenFile.FileName;
                ((App)Application.Current).CurrentProfileCfg.Folders.DefaultIconsDir = cmnOpenFile.FileName;

                OnSettingChanged?.Invoke();
            }
        }

        private void BtnResetIconsDefaultDir_Click(object sender, RoutedEventArgs e)
        {
            InputIconsDefaultDir.Clear();
            ((App)Application.Current).CurrentProfileCfg.Folders.DefaultIconsDir = "";

            OnSettingChanged?.Invoke();
        }

        private void BtnBrowseCoversDefaultDir_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cmnOpenFile = new CommonOpenFileDialog();
            cmnOpenFile.IsFolderPicker = true;
            cmnOpenFile.Title = "Select Default Cover Grids Folder";

            Window owner = Window.GetWindow(this);

            if (cmnOpenFile.ShowDialog(owner) == CommonFileDialogResult.Ok)
            {
                InputCoversDefaultDir.Text = cmnOpenFile.FileName;
                ((App)Application.Current).CurrentProfileCfg.Folders.DefaultCoversDir = cmnOpenFile.FileName;

                OnSettingChanged?.Invoke();
            }
        }

        private void BtnResetCoversDefaultDir_Click(object sender, RoutedEventArgs e)
        {
            InputCoversDefaultDir.Clear();
            ((App)Application.Current).CurrentProfileCfg.Folders.DefaultCoversDir = "";

            OnSettingChanged?.Invoke();
        }

        private void InputGamesDefaultDir_LostFocus(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).CurrentProfileCfg.Folders.DefaultGamesDir = InputGamesDefaultDir.Text;

            OnSettingChanged?.Invoke();
        }

        private void InputToolsDefaultDir_LostFocus(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).CurrentProfileCfg.Folders.DefaultToolsDir = InputToolsDefaultDir.Text;

            OnSettingChanged?.Invoke();
        }

        private void InputIconsDefaultDir_LostFocus(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).CurrentProfileCfg.Folders.DefaultIconsDir = InputIconsDefaultDir.Text;

            OnSettingChanged?.Invoke();
        }

        private void InputCoversDefaultDir_LostFocus(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).CurrentProfileCfg.Folders.DefaultCoversDir = InputCoversDefaultDir.Text;

            OnSettingChanged?.Invoke();
        }

    }
}
