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
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = DBHandler.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        private MainWindow Main => Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        List<ToolModel> allToolsList = new List<ToolModel>();
        List<CategoryModel> allCategoriesList = new List<CategoryModel>();

        public ToolsView()
        {
            InitializeComponent();

            LoadToolsList();
            LoadCategoriesList();

            ComboSortBy.ItemsSource = func.SortOptions;
            ComboSortBy.DisplayMemberPath = "DisplayName";
            ComboSortBy.SelectedIndex = 0;

            LoadDefaultSortOptions();
        }

        private void LoadDefaultSortOptions()
        {
            try
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex, "ToolsView Load Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

            string defaultSortOption = ((App)Application.Current).CurrentProfileCfg?.General?.ToolsDefaultSortOption;

            if (string.IsNullOrEmpty(defaultSortOption))
            {
                defaultSortOption = "title";
            }

            var targetSortObject = func.SortOptions
                                   .FirstOrDefault(s => s.SortByTag == defaultSortOption);

            if (targetSortObject != null)
            {
                ComboSortBy.SelectedItem = targetSortObject;
            }
            else
            {
                ComboSortBy.SelectedIndex = 0;
            }
        }

        public void LoadToolsList(string _sortBy = "title", bool _ascending = true)
        {

            int _toolsCount = 0;
            _toolsCount = db.ExecuteScalar<int>(db.ConnectionProfile, "SELECT COUNT(*) FROM tools");

            if (_toolsCount > 0)
            {
                allToolsList = db.GetAllTools(_sortBy, _ascending);

                ContainerBtnApps.ItemsSource = null;
                ContainerBtnApps.ItemsSource = allToolsList;
            }
        }

        public void LoadCategoriesList()
        {
            int _categoriesCount = 0;
            _categoriesCount = db.ExecuteScalar<int>(db.ConnectionProfile, "SELECT COUNT(*) FROM categories WHERE id = 2");

            CategoryModel allCategory = new CategoryModel
            {
                CategoryId = 0,
                CategoryName = "All Tools",
                CategoryDesc = "Display all tools from your library"
            };

            if (_categoriesCount > 0)
            {
                allCategoriesList = db.GetAllCategories(2);

                allCategoriesList.Insert(0, allCategory);

                ComboCategories.ItemsSource = allCategoriesList;
                ComboCategories.DisplayMemberPath = "CategoryName";

            }
            else
            {
                allCategoriesList.Insert(0, allCategory);

                ComboCategories.ItemsSource = allCategoriesList;
                ComboCategories.DisplayMemberPath = "CategoryName";
            }
        }

        private void UpdateAtLaunch(ToolModel tool)
        {
            string filePath = tool.ToolTarget;
            string args = tool.ToolLaunchOptions;

            func.LaunchFile(filePath, args);

            string query_fl = "UPDATE tools SET first_launched = datetime('now', 'localtime') WHERE id = @id AND launch_times < 1";
            string query_ll = "UPDATE tools SET last_launched = datetime('now', 'localtime') WHERE id = @id";

            var args_fl = new Dictionary<string, object>
                {
                    { "@id", tool.ToolId }
                };
            var args_ll = new Dictionary<string, object>
                {
                    { "@id", tool.ToolId }
                };

            db.ExecuteQuery(db.ConnectionProfile, query_fl, args_fl);
            db.ExecuteQuery(db.ConnectionProfile, query_ll, args_ll);

            string query__ = "UPDATE tools SET launch_times = launch_times + 1 WHERE id = @id";

            var args__ = new Dictionary<string, object>
                {
                    { "@id", tool.ToolId }
                };

            db.ExecuteQuery(db.ConnectionProfile, query__, args__);
        }

        private void LaunchApp(ToolModel launchedTool)
        {
            if (launchedTool != null)
            {
                UpdateAtLaunch(launchedTool);

                if (((App)Application.Current).CurrentProfileCfg.General.ToolsLaunchAction
                        == ProfileGeneralSettings.LaunchActions.HideLauncher
                        && !Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Application.Current.MainWindow.Hide();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    func.runningProcess.EnableRaisingEvents = true;
                    func.runningProcess.Exited += (s, ev) =>
                    {
                        this.Dispatcher.Invoke(() => Application.Current.MainWindow.Show());
                    };
                }
                else if (((App)Application.Current).CurrentProfileCfg.General.ToolsLaunchAction
                            == ProfileGeneralSettings.LaunchActions.CloseLauncher
                            && !Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Application.Current.MainWindow.Close();
                }
                else
                {

                }
            }
            else
            {
                PopupWindow popup = new PopupWindow("Tool doesn't exist",
                                                     "No tool found at this menu",
                                                     "Ok", false);
            }
        }

        private void BtnApps_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var tool = btn.DataContext as ToolModel;
            LaunchApp(tool);
        }

        private void MenuItemLaunch_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var game = menuItem.DataContext as ToolModel;
            LaunchApp(game);
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedTool = menuItem.Tag as ToolModel;

            Main.EnableBlurAndOverlay();
            ToolPropertiesWindow edit = new ToolPropertiesWindow(selectedTool);
            if (edit.ShowDialog() == true)
            {
                LoadToolsList();
                ApplyFilters();
            }

            Main.DisableBlurAndOverlay();
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
            
            Main.EnableBlurAndOverlay();
            if (selectedTool != null)
            {
                PopupWindow popup = new PopupWindow("Confirm Delete",
                                                     $"Are you sure you want to remove \"{selectedTool.ToolTitle}\"?",
                                                     "Delete Tool", true);

                popup.ShowDialog();
                if (popup.DialogResult == true)
                {
                    DBHandler db = DBHandler.Instance;
                    db.DeleteTool(selectedTool.ToolId);
                    
                    LoadToolsList();
                    ApplyFilters();
                }
            }

            Main.DisableBlurAndOverlay();
        }

        private void SearchBar_Changed(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        public void ComboCategories_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        public void ComboSortBy_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        public void ApplyFilters()
        {
            if (allToolsList == null) return;

            // --- 1. AMBIL INPUT ---
            var selectedCategory = ComboCategories.SelectedItem as CategoryModel;
            string searchQuery = SearchBar.Text.ToLower().Trim();

            var selectedSortItem = (SortCriterion)ComboSortBy.SelectedItem;
            if (selectedSortItem == null) return;

            string sortBy = selectedSortItem.SortByTag.ToString();
            bool asc = !(sortBy == "id" || sortBy == "last_launched" || sortBy == "launch_times");

            // --- 2. FILTERING (Kategori & Search) ---
            var query = allToolsList.Where(tools =>
            {
                bool matchCategory = (selectedCategory == null || selectedCategory.CategoryId == 0)
                                     || tools.ToolCategoryId == selectedCategory.CategoryId;

                bool matchSearch = string.IsNullOrEmpty(searchQuery)
                                   || tools.ToolTitle.ToLower().Contains(searchQuery);

                return matchCategory && matchSearch;
            });

            // --- 3. SORTING (LINQ) ---
            IOrderedEnumerable<ToolModel> sortedData;

            switch (sortBy)
            {
                case "last_launched":
                    sortedData = query.OrderByDescending(t => t.ToolLastLaunched);
                    break;
                case "launch_times":
                    sortedData = query.OrderByDescending(t => t.ToolLaunchTimes);
                    break;
                case "id":
                    sortedData = query.OrderByDescending(t => t.ToolId);
                    break;
                default: // "title"
                    sortedData = query.OrderBy(t => t.ToolTitle);
                    break;
            }

            // --- 4. UPDATE UI ---
            ContainerBtnApps.ItemsSource = sortedData.ToList();

            if (!sortedData.Any())
            {
                NoAppsAvailable.Visibility = Visibility.Visible;
            }
            else
            {
                NoAppsAvailable.Visibility = Visibility.Hidden;
            }
        }

    }
}
