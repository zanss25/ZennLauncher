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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using static System.Collections.Specialized.BitVector32;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for CategoriesView.xaml
    /// </summary>
    public partial class CategoriesView : UserControl
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = DBHandler.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        private MainWindow Main => Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        List<GameModel> allGamesList = new List<GameModel>();
        List<ToolModel> allToolsList = new List<ToolModel>();

        List<CategoryModel> masterGamesCategories = new List<CategoryModel>();
        List<CategoryModel> masterToolsCategories = new List<CategoryModel>();

        private bool isSidebarCollapsed = true;
        public CategoriesView()
        {
            InitializeComponent();

            allGamesList = db.GetAllGames();
            allToolsList = db.GetAllTools();

            LoadDefaultCategories();

            LoadCategoriesList(1);
            LoadCategoriesList(2);

            GamesCategoriesList.SelectedIndex = 0;
            BtnAddNewToCategory.Checked += BtnAddNewToCategory_Checked;

            SidebarCategoriesList.Visibility = Visibility.Collapsed;
        }

        public void LoadDefaultCategories()
        {
            var checkSetup = db.ExecuteScalar<string>(db.ConnectionProfile,
                "SELECT value FROM db_metadata WHERE key = 'defaults_categories_installed'", null);

            if (checkSetup == null)
            {
                db.InsertDbDefaults("categories");
            }
        }

        public void LoadCategoriesList(int sectionId)
        {
            int _categoriesCount;

            string query_cc = "SELECT COUNT(*) FROM categories WHERE section_id = @section_id";

            var args_cc = new Dictionary<string, object>()
            {
                { "@section_id", sectionId }
            };

            _categoriesCount = db.ExecuteScalar<int>(db.ConnectionProfile, query_cc, args_cc);

            string sectionName = db.ExecuteScalar<string>(db.ConnectionProfile, "SELECT name FROM sections WHERE id = @section_id", args_cc);

            CategoryModel allCategory = new CategoryModel
            {
                CategoryId = 0,
                CategoryName = "All " + sectionName,
                CategoryDesc = "Display all " + sectionName.ToLower() + " from your library"
            };

            List<CategoryModel> categories = new List<CategoryModel>();
            if (_categoriesCount > 0)
            {
                var dbResult = db.GetAllCategories(sectionId);           
                if (dbResult != null)
                {
                    categories = dbResult;
                }
            }

            categories.Insert(0, allCategory);
            if (sectionId == 1)
            {
                masterGamesCategories = categories;
                GamesCategoriesList.ItemsSource = null;
                GamesCategoriesList.ItemsSource = masterGamesCategories;

                if (GamesCategoriesList.Items.Count > 0)
                {
                    GamesCategoriesList.SelectedIndex = 0;
                }
            }
            else if (sectionId == 2)
            {
                masterToolsCategories = categories;
                ToolsCategoriesList.ItemsSource = null;
                ToolsCategoriesList.ItemsSource = masterToolsCategories;

                if (ToolsCategoriesList.Items.Count > 0)
                {
                    ToolsCategoriesList.SelectedIndex = 0;
                }
            }

            Debug.WriteLine($"ListBox Games Count: {GamesCategoriesList.Items.Count}");
            Debug.WriteLine($"ListBox Tools Count: {ToolsCategoriesList.Items.Count}");
        }

        private void SearchBarCategories_Changed(object sender, TextChangedEventArgs e)
        {
            string searchText = (sender as TextBox).Text.ToLower().Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                GamesCategoriesList.ItemsSource = masterGamesCategories;
                ToolsCategoriesList.ItemsSource = masterToolsCategories;
                return;
            }

            // Filter Games Categories
            // Tetap pertahankan "All Games" (Index 0) agar selalu muncul di atas
            var filteredGames = masterGamesCategories
                .Where(c => c.CategoryId == 0 || c.CategoryName.ToLower().Contains(searchText))
                .ToList();

            // Filter Tools Categories
            var filteredTools = masterToolsCategories
                .Where(c => c.CategoryId == 0 || c.CategoryName.ToLower().Contains(searchText))
                .ToList();

            // Update UI secara paralel
            GamesCategoriesList.ItemsSource = filteredGames;
            ToolsCategoriesList.ItemsSource = filteredTools;
        }

        private void GamesCategoriesList_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (GamesCategoriesList.SelectedItem != null)
            {
                ToolsCategoriesList.SelectedIndex = -1;

                AppsDataGrid.ItemsSource = null;
                ApplyFilters(1);
            }
        }

        private void ToolsCategoriesList_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ToolsCategoriesList.SelectedItem != null)
            {
                GamesCategoriesList.SelectedIndex = -1;

                AppsDataGrid.ItemsSource = null;
                ApplyFilters(2);
            }
        }

        private void SearchBarApps_Changed(object sender, TextChangedEventArgs e)
        {
            int activeSection = (GamesCategoriesList.SelectedIndex > -1) ? 1 : 2;
            ApplyFilters(activeSection);
        }

        public void ApplyFilters(int sectionId)
        {
            CategoryModel selectedCategory = (sectionId == 1)
                ? GamesCategoriesList.SelectedItem as CategoryModel
                : ToolsCategoriesList.SelectedItem as CategoryModel;

            string searchQuery = SearchBarApps.Text.ToLower().Trim();

            // 1. Pilih sumber data
            IEnumerable<IAppItem> source = (sectionId == 1)
                ? allGamesList?.Cast<IAppItem>()
                : allToolsList?.Cast<IAppItem>();

            if (source == null) return;

            // 2. Filtering & Sorting
            var filteredData = source.Where(item =>
            {
                // Langsung panggil item.CategoryId tanpa bantuan fungsi lain
                bool matchCategory = (selectedCategory == null || selectedCategory.CategoryId == 0)
                                     || item.AppCategoryId == selectedCategory.CategoryId;

                bool matchSearch = string.IsNullOrEmpty(searchQuery)
                                   || item.AppTitle.ToLower().Contains(searchQuery);

                return matchCategory && matchSearch;
            }).OrderBy(item => item.AppTitle).ToList();

            // 3. Update UI
            AppsDataGrid.ItemsSource = filteredData;
            //NoAppsAvailable.Visibility = filteredData.Any() ? Visibility.Hidden : Visibility.Visible;
        }

        private void BtnAddNewCategoryGames_Click(object sender, RoutedEventArgs e)
        {
            AddNewCategory(1);
        }

        private void BtnAddNewCategoryTools_Click(object sender, RoutedEventArgs e)
        {
            AddNewCategory(2);
        }

        private void AddNewCategory(int currentSectionId)
        {
            InputBoxPopup inputA = new InputBoxPopup("Enter Category Name", 
                                                    "Enter your new category name", 
                                                    "Confirm");

            string newName;
            string newDesc;

            if (inputA.ShowDialog() == true)
            {
                string inputName = inputA.ResultText.Trim();

                string query_cc = $"SELECT COUNT(*) FROM categories WHERE name = @name AND section_id = @section_id";

                var args_cc = new Dictionary<string, object>()
                {
                    { "@name", inputName },
                    { "@section_id", currentSectionId }
                };

                int result = db.ExecuteScalar<int>(db.ConnectionProfile, query_cc, args_cc);

                if (result > 0)
                {
                    new PopupWindow("Enter Category Name",
                                    "Category name has already been taken. Use a different name for your new category",
                                    "Ok", false).ShowDialog();
                    return;
                }
                else
                {
                    newName = inputName;
                }
            }
            else
            {
                return;
            }

            InputBoxPopup inputB = new InputBoxPopup("Enter Category Description",
                                                    "Enter your new category description",
                                                    "Confirm");

            if (inputB.ShowDialog() == true)
            {
                newDesc = inputB.ResultText.Trim();
            }
            else
            {
                return;
            }

            if (!String.IsNullOrEmpty(newName) && !String.IsNullOrEmpty(newDesc))
            {
                string query_sc = @"INSERT INTO categories (name, desc, section_id) 
                                    VALUES (@new_name, @new_desc, @section_id)";

                var args_sc = new Dictionary<string, object>()
                {
                    { "@new_name", newName },
                    { "@new_desc", newDesc },
                    { "@section_id", currentSectionId }
                };

                try
                {
                    db.ExecuteQuery(db.ConnectionProfile, query_sc, args_sc);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error Saving Category",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }

                AppsDataGrid.ItemsSource = null;
                LoadCategoriesList(currentSectionId);
                ApplyFilters(currentSectionId);
            }
            else
            {
                Debug.WriteLine("EMPTY: The newName and newDesc are empty.");
                return;
            }
        }

        private void RemoveCategory(int categoryId, int currentSectionId, string targetTable)
        {
            Debug.WriteLine("categoryId / currentSectionId / targetTable: " 
                            + categoryId + " " + currentSectionId + " " + targetTable);

            string query_dm = $"UPDATE {targetTable} SET category_id = NULL WHERE category_id = @category_id";

            var args_dm = new Dictionary<string, object>()
            {
                { "@category_id", categoryId }
            };

            string query_ds = $"DELETE FROM categories WHERE id = @category_id AND section_id = @section_id";

            var args_ds = new Dictionary<string, object>()
            {
                { "@category_id", categoryId },
                { "@section_id", currentSectionId }
            };
            
            try
            {
                db.ExecuteQuery(db.ConnectionProfile, query_dm, args_dm);
                db.ExecuteQuery(db.ConnectionProfile, query_ds, args_ds);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemoveCategory_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var selectedCategory = btn.Tag as CategoryModel;
            int selectedSectionId = selectedCategory.CategorySectionId;

            if (selectedCategory != null && selectedCategory.CategoryId != 0)
            {
                Main.EnableBlurAndOverlay();

                PopupWindow confirm = new PopupWindow("Delete Category",
                                                      $"Are you sure you want to delete \"{selectedCategory.CategoryName}\" from categories?",
                                                      "Delete", true);

                confirm.ShowDialog();
                if (confirm.DialogResult == true)
                {
                    RemoveCategory(selectedCategory.CategoryId, selectedSectionId,
                                   selectedCategory.CategorySectionName.ToLower());

                    LoadCategoriesList(selectedSectionId);

                    allGamesList = db.GetAllGames();
                    allToolsList = db.GetAllTools();

                    AppsDataGrid.ItemsSource = null;
                    ApplyFilters(selectedSectionId);
                }

            }

            Main.DisableBlurAndOverlay();
        }

        private void BtnAddNewToCategory_Checked(object sender, RoutedEventArgs e)
        {
            var selectedApps = AppsDataGrid.SelectedItems.Cast<IAppItem>().ToList();
            if (selectedApps.Count == 0) return;

            int sectionId = AppCurrentSectionChecker();

            var availableCategories = db.GetAllCategories(sectionId)
                                        .Where(c => c.CategoryId != 0).ToList();

            if (selectedApps.Count == 1)
            {
                int currentCatId = selectedApps[0].AppCategoryId;

                if (currentCatId > 0)
                {
                    TextPopupHeader.Text = "Change Category to:";
                    availableCategories = availableCategories.Where(c => c.CategoryId != currentCatId).ToList();
                }
                else
                {
                    TextPopupHeader.Text = "Add to Category:";
                }
            }
            else
            {
                TextPopupHeader.Text = $"Move {selectedApps.Count} items to:";
            }

            ListPopupCategories.ItemsSource = availableCategories;
        }

        private void BtnCategoryItem_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int targetCategoryId = (int)btn.Tag;
            var selectedApps = AppsDataGrid.SelectedItems.Cast<IAppItem>().ToList();

            if (selectedApps.Count > 0)
            {
                foreach (var app in selectedApps)
                {
                    db.UpdateAppCategory(app.AppId, app.TargetTable, targetCategoryId);

                    // Update di List Internal (agar UI langsung berubah)
                    if (app.TargetTable == "games")
                        allGamesList.FirstOrDefault(g => g.GameId == app.AppId).GameCategoryId = targetCategoryId;
                    else
                        allToolsList.FirstOrDefault(t => t.ToolId == app.AppId).ToolCategoryId = targetCategoryId;
                }

                allGamesList = db.GetAllGames();
                allToolsList = db.GetAllTools();

                int sectionId = AppCurrentSectionChecker();
                ApplyFilters(sectionId);

                BtnAddNewToCategory.IsChecked = false;
            }
        }

        private void BtnRemoveFromCategory_Click(object sender, RoutedEventArgs e)
        {
            var selectedApps = AppsDataGrid.SelectedItems.Cast<IAppItem>().ToList();

            if (selectedApps.Count == 0) return;

            var appsWithCategory = selectedApps.Where(a => a.AppCategoryId != 0).ToList();

            if (appsWithCategory.Count == 0)
            {
                return;
            }

            
             foreach (var app in appsWithCategory)
             {
                db.UpdateAppCategory(app.AppId, app.TargetTable, 0);

                if (app.TargetTable == "games")
                    allGamesList.FirstOrDefault(g => g.GameId == app.AppId).GameCategoryId = 0;
                else
                    allToolsList.FirstOrDefault(t => t.ToolId == app.AppId).ToolCategoryId = 0;
             }

             allGamesList = db.GetAllGames();
             allToolsList = db.GetAllTools();

            int sectionId = AppCurrentSectionChecker();
            ApplyFilters(sectionId);
        }

        private void AppsDataGrid_Changed(object sender, SelectionChangedEventArgs e)
        {
            var selectedApps = AppsDataGrid.SelectedItems.Cast<IAppItem>().ToList();
            var appsWithCategory = selectedApps.Where(a => a.AppCategoryId != 0).ToList();

            BtnAddNewToCategory.IsEnabled = AppsDataGrid.SelectedItems.Count > 0;
            BtnRemoveFromCategory.IsEnabled = AppsDataGrid.SelectedItems.Count > 0 && appsWithCategory.Count > 0;

            if (AppsDataGrid.SelectedItems.Count > 0 && appsWithCategory.Count > 0)
            {
                BtnAddNewToCategory.Content = "🗘  Change Category to";
            }
            else
            {
                BtnAddNewToCategory.Content = "✚  Add to Category";
            }

            int currentSectionId = AppCurrentSectionChecker();
            string appType = (currentSectionId == 1) ? "Game" : "Tool";

            if (AppsDataGrid.SelectedItems.Count > 1)
            {
                MenuItemLaunch.Visibility = Visibility.Collapsed;
                MenuItemOpenDir.Visibility = Visibility.Collapsed;
                MenuItemDelete.Header = $"Remove Selected {appType}s ({AppsDataGrid.SelectedItems.Count})";
            }
            else
            {
                MenuItemLaunch.Visibility = Visibility.Visible;
                MenuItemOpenDir.Visibility = Visibility.Visible;
                MenuItemDelete.Header = $"Remove {appType}";
            }
        }

        private int AppCurrentSectionChecker()
        {
            int currentSectionId = 0;
            var app = AppsDataGrid.SelectedItem as IAppItem;

            if (app != null)
            {
                if (app.TargetTable == "games")
                {
                    currentSectionId = 1;
                }
                if (app.TargetTable == "tools")
                {
                    currentSectionId = 2;
                }
            }

            return currentSectionId;
        }

        private void LaunchApp(IAppItem launchedApp, int currentSectionId)
        {
            if (launchedApp != null)
            {
                db.UpdateAppAtLaunch(launchedApp, currentSectionId);
                func.LaunchFile(launchedApp.AppTarget, launchedApp.AppLaunchOptions);

                ProfileGeneralSettings.LaunchActions AppCurrentLaunchOption 
                    = ProfileGeneralSettings.LaunchActions.DoNothing;
                if (currentSectionId == 1)
                {
                    AppCurrentLaunchOption 
                        = ((App)Application.Current).CurrentProfileCfg.General.GamesLaunchAction;
                }
                if (currentSectionId == 2)
                {
                    AppCurrentLaunchOption 
                        = ((App)Application.Current).CurrentProfileCfg.General.ToolsLaunchAction;
                }

                if (AppCurrentLaunchOption
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
                else if (((App)Application.Current).CurrentProfileCfg.General.GamesLaunchAction
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
                PopupWindow popup = new PopupWindow("App doesn't exist",
                                                     "No game found at this menu",
                                                     "Ok", false);
            }
        }

        private void AppsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element != null && element.DataContext is IAppItem app)
            {
                int currentSectionId = AppCurrentSectionChecker();
                LaunchApp(app, currentSectionId);
            }
        }

        private void MenuItemLaunch_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var app = menuItem.DataContext as IAppItem;

            int currentSectionId = AppCurrentSectionChecker();
            LaunchApp(app, currentSectionId);
        }

        private void MenuOpenDir_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var app = menuItem.DataContext as IAppItem;
            if (app == null)
            {
                var contextMenu = menuItem.Parent as ContextMenu;
                var target = contextMenu?.PlacementTarget as FrameworkElement;
                app = target?.DataContext as IAppItem;
            }

            if (app != null && !string.IsNullOrEmpty(app.AppTarget))
            {
                try
                {
                    if (System.IO.File.Exists(app.AppTarget))
                    {
                        Process.Start("explorer.exe", $"/select,\"{app.AppTarget}\"");
                    }
                    else
                    {
                        string dir = System.IO.Path.GetDirectoryName(app.AppTarget);
                        if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir))
                        {
                            Process.Start("explorer.exe", dir);
                        }
                        else
                        {
                            MessageBox.Show("Directory or file not found.", "Error",
                                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not open directory: " + ex.Message, "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            int currentSectionId = AppCurrentSectionChecker();
            string appSectionName = (currentSectionId == 1) ? "Game" : "Tool";

            var selectedApps = AppsDataGrid.SelectedItems.Cast<IAppItem>().ToList();
            if (selectedApps.Count == 0) return;

            Main.EnableBlurAndOverlay();

            string message = selectedApps.Count > 1
                ? $"Are you sure you want to remove {selectedApps.Count} selected {appSectionName.ToLower()}s from your library?"
                : $"Are you sure you want to remove \"{selectedApps[0].AppTitle}\" from your library?";

            PopupWindow popup = new PopupWindow($"Remove {appSectionName}", message, "Remove", true);

            if (popup.ShowDialog() == true)
            {
                foreach (var app in selectedApps)
                {
                    db.DeleteApp(app.AppId, app.TargetTable);

                    if (currentSectionId == 1) allGamesList.RemoveAll(g => g.GameId == app.AppId);
                    else allToolsList.RemoveAll(t => t.ToolId == app.AppId);
                }

                ApplyFilters(currentSectionId);
            }

            Main.DisableBlurAndOverlay();
        }

        private void AppsDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;

                // Mencari elemen induk yang bisa scroll dan mengirimkan event ke induk
                var parent = ((Control)sender).Parent as FrameworkElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void BtnSidebarCollapse_Click(object sender, RoutedEventArgs e)
        {
            if (!isSidebarCollapsed)
            {
                SidebarCategoriesList.Visibility = Visibility.Collapsed;
                isSidebarCollapsed = true;
            }
            else
            {
                SidebarCategoriesList.Visibility = Visibility.Visible;
                isSidebarCollapsed = false;
            }
        }
    }
}
