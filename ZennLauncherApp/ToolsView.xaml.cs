using Microsoft.Data.Sqlite;
using System;
using System.Collections;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for ToolsView.xaml
    /// </summary>
    public partial class ToolsView : UserControl
    {
        GlobalFunctions func = new GlobalFunctions();
        DBHandler db = new DBHandler();
        ConfigHandler cfg = new ConfigHandler();


        List<ToolModel> allToolsList = new List<ToolModel>();
        public ToolsView()
        {
            InitializeComponent();
            LoadToolsList();

            ComboSortByTools.ItemsSource = func.SortOptions;
            ComboSortByTools.DisplayMemberPath = "DisplayName";
            ComboSortByTools.SelectedIndex = 0;

            LoadDefaultSortOptions();
        }

        private void LoadDefaultSortOptions()
        {
            string defaultSortOption = ((App)Application.Current).CurrentProfileCfg.General.ToolsDefaultSortOption;

            if (string.IsNullOrEmpty(defaultSortOption))
            {
                defaultSortOption = "title";
            }

            var targetSortObject = func.SortOptions
                                   .FirstOrDefault(s => s.SortByTag == defaultSortOption);

            if (targetSortObject != null)
            {
                ComboSortByTools.SelectedItem = targetSortObject;
            }
            else
            {
                ComboSortByTools.SelectedIndex = 0;
            }
        }

        public void LoadToolsList(string _sortBy = "title", bool _ascending = true)
        {

            int _toolsCount = 0;
            _toolsCount = db.ExecuteScalar<int>(db.ConnectionProfile, "SELECT COUNT(*) FROM tools");

            if (_toolsCount > 0)
            {
                allToolsList = db.GetAllTools(_sortBy, _ascending);

                ContainerBtnTools.ItemsSource = null;
                ContainerBtnTools.ItemsSource = allToolsList;
            }
        }

        private void BtnTools_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            var tool = btn.DataContext as ToolModel;

            if (tool != null)
            {
                string filePath = tool.ToolTarget;
                string args = tool.ToolLaunchOptions;

                func.LaunchFile(filePath, args);

                string query = "UPDATE tools SET last_launched = datetime('now', 'localtime') WHERE id = @id";

                var args_ = new Dictionary<string, object>
                {
                    { "@id", tool.ToolId }
                };

                db.ExecuteQuery(db.ConnectionProfile, query, args_);

                string query__ = "UPDATE tools SET launch_times = launch_times + 1 WHERE id = @id";

                var args__ = new Dictionary<string, object>
                {
                    { "@id", tool.ToolId }
                };

                db.ExecuteQuery(db.ConnectionProfile, query__, args__);

                if (((App)Application.Current).CurrentProfileCfg.General.ToolsLaunchAction 
                        == ProfileGeneralSettings.LaunchAction.HideLauncher 
                        && !Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Application.Current.MainWindow.Hide();
                    func.runningProcess.EnableRaisingEvents = true;
                    func.runningProcess.Exited += (s, ev) =>
                    {
                        // Ini berjalan saat proses awal tertutup
                        this.Dispatcher.Invoke(() => Application.Current.MainWindow.Show());
                    };
                }
                else if (((App)Application.Current).CurrentProfileCfg.General.ToolsLaunchAction 
                            == ProfileGeneralSettings.LaunchAction.CloseLauncher 
                            && !Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Application.Current.MainWindow.Close();
                }
            }
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedTool = menuItem.Tag as ToolModel;

            ToolPropertiesWindow edit = new ToolPropertiesWindow(selectedTool);
            if (edit.ShowDialog() == true)
            {
                LoadToolsList();
            }
            else
            {
                LoadToolsList();
            }
        }

        private void MenuOpenDir_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            ContextMenu parentMenu = menuItem.Parent as ContextMenu;
            if (parentMenu == null) return;

            Button btnTool = parentMenu.PlacementTarget as Button;
            if (btnTool == null) return;

            var tool = btnTool.DataContext as ToolModel;

            if (tool != null && !string.IsNullOrEmpty(tool.ToolTarget))
            {
                try
                {
                    if (System.IO.File.Exists(tool.ToolTarget))
                    {
                        Process.Start("explorer.exe", $"/select,\"{tool.ToolTarget}\"");
                    }
                    else
                    {
                        string dir = System.IO.Path.GetDirectoryName(tool.ToolTarget);
                        if (System.IO.Directory.Exists(dir))
                        {
                            Process.Start("explorer.exe", dir);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedTool = menuItem.Tag as ToolModel;

            if (selectedTool != null)
            {
                var result = MessageBox.Show($"Are you sure you want to remove '{selectedTool.ToolTitle}'?",
                                                "Confirm Delete",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DBHandler db = new DBHandler();
                    db.DeleteTool(selectedTool.ToolId);
                    LoadToolsList();
                }
            }
        }

        private void SearchBarTools_Changed(object sender, TextChangedEventArgs e)
        {
            string query = SearchBarTools.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(query))
            {
                ContainerBtnTools.ItemsSource = allToolsList;
                return;
            }

            var filteredList = allToolsList.Where(g =>
                g.ToolTitle.ToLower().Contains(query) ||
                (g.ToolDesc != null && g.ToolDesc.ToLower().Contains(query))
            ).ToList();

            ContainerBtnTools.ItemsSource = filteredList;
        }

        private void ComboSortByTools_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (db == null || ComboSortByTools.SelectedItem == null) return;

            var selectedItem = (SortCriterion)ComboSortByTools.SelectedItem;
            string sortBy = selectedItem.SortByTag.ToString();
            bool asc = true;

            if (sortBy == "id" || sortBy == "last_launched" || sortBy == "launch_times")
            {
                asc = false;
            }

            ContainerBtnTools.ItemsSource = db.GetAllTools(sortBy, asc);
        }
    }
}
