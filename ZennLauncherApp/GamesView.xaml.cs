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
    /// Interaction logic for GamesView.xaml
    /// </summary>
    public partial class GamesView : UserControl
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = DBHandler.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        private MainWindow Main => Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        List<GameModel> allGamesList = new List<GameModel>();
        List<CategoryModel> allCategoriesList = new List<CategoryModel>();

        public GamesView()
        {            
            InitializeComponent();

            LoadGamesList();
            LoadCategoriesList();

            ComboSortBy.ItemsSource = GlobalFunctions.Instance.SortOptions;
            ComboSortBy.DisplayMemberPath = "DisplayName";
            ComboSortBy.SelectedIndex = 0;

            LoadDefaultSortOptions();
        }

        private void LoadDefaultSortOptions()
        {
            try
            {
                string defaultSortOption = ((App)Application.Current).CurrentProfileCfg?.General?.GamesDefaultSortOption;

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
            catch (Exception ex)
            {
                MessageBox.Show("Error: " +  ex, "GamesView Load Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadGamesList(string _sortBy = "title", bool _ascending = true)
        {

            int _gamesCount = 0;
            _gamesCount = db.ExecuteScalar<int>(db.ConnectionProfile, "SELECT COUNT(*) FROM games");

            if (_gamesCount > 0)
            {
                allGamesList = db.GetAllGames(_sortBy, _ascending);

                ContainerBtnApps.ItemsSource = null;
                ContainerBtnApps.ItemsSource = allGamesList;
            }
        }

        public void LoadCategoriesList()
        {
            int _categoriesCount = 0;
            _categoriesCount = db.ExecuteScalar<int>(db.ConnectionProfile, "SELECT COUNT(*) FROM categories WHERE id = 1");

            CategoryModel allCategory = new CategoryModel
            {
                CategoryId = 0,
                CategoryName = "All Games",
                CategoryDesc = "Display all games from your library"
            };

            if (_categoriesCount > 0)
            {
                allCategoriesList = db.GetAllCategories(1);

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

        private void UpdateAtLaunch(GameModel game)
        {
            string filePath = game.GameTarget;
            string args = game.GameLaunchOptions;

            func.LaunchFile(filePath, args);

            string query_fl = "UPDATE games SET first_launched = datetime('now', 'localtime') WHERE id = @id AND launch_times < 1";
            string query_ll = "UPDATE games SET last_launched = datetime('now', 'localtime') WHERE id = @id";

            var args_fl = new Dictionary<string, object>
                {
                    { "@id", game.GameId }
                };
            var args_ll = new Dictionary<string, object>
                {
                    { "@id", game.GameId }
                };

            db.ExecuteQuery(db.ConnectionProfile, query_fl, args_fl);
            db.ExecuteQuery(db.ConnectionProfile, query_ll, args_ll);

            string query__ = "UPDATE games SET launch_times = launch_times + 1 WHERE id = @id";

            var args__ = new Dictionary<string, object>
                {
                    { "@id", game.GameId }
                };

            db.ExecuteQuery(db.ConnectionProfile, query__, args__);
        }

        private void LaunchGame(GameModel launchedGame)
        {
            if (launchedGame != null)
            {
                UpdateAtLaunch(launchedGame);

                if (((App)Application.Current).CurrentProfileCfg.General.GamesLaunchAction
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
                PopupWindow popup = new PopupWindow("Game doesn't exist",
                                                     "No game found at this menu",
                                                     "Ok", false);
            }
        }

        private void BtnApps_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var game = btn.DataContext as GameModel;
            LaunchGame(game);
        }

        private void MenuItemLaunch_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var game = menuItem.DataContext as GameModel;
            LaunchGame(game);
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedGame = menuItem.Tag as GameModel;

            Main.EnableBlurAndOverlay();

            GamePropertiesWindow edit = new GamePropertiesWindow(selectedGame);
            if (edit.ShowDialog() == true)
            {
                LoadGamesList();
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
            Button btnGame = parentMenu.PlacementTarget as Button;
            if (btnGame == null) return;

            var game = btnGame.DataContext as GameModel;

            if (game != null && !string.IsNullOrEmpty(game.GameTarget))
            {
                try
                {
                    if (System.IO.File.Exists(game.GameTarget))
                    {
                        Process.Start("explorer.exe", $"/select,\"{game.GameTarget}\"");
                    }
                    else
                    {
                        string dir = System.IO.Path.GetDirectoryName(game.GameTarget);
                        if (System.IO.Directory.Exists(dir))
                        {
                            Process.Start("explorer.exe", dir);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedGame = menuItem.Tag as GameModel;

            Main.EnableBlurAndOverlay();
            if (selectedGame != null)
            {
                PopupWindow popup = new PopupWindow("Confirm Delete", 
                                                     $"Are you sure you want to remove \"{selectedGame.GameTitle}\"?",
                                                     "Delete Game", true);

                popup.ShowDialog();
                if (popup.DialogResult == true)
                {
                    DBHandler db = DBHandler.Instance;
                    db.DeleteGame(selectedGame.GameId);
                    Main.DisableBlurAndOverlay();

                    LoadGamesList();
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
            if (allGamesList == null) return;

            // --- 1. AMBIL INPUT ---
            var selectedCategory = ComboCategories.SelectedItem as CategoryModel;
            string searchQuery = SearchBar.Text.ToLower().Trim();

            var selectedSortItem = (SortCriterion)ComboSortBy.SelectedItem;
            if (selectedSortItem == null) return;

            string sortBy = selectedSortItem.SortByTag.ToString();
            bool asc = !(sortBy == "id" || sortBy == "last_launched" || sortBy == "launch_times");

            // --- 2. FILTERING (Kategori & Search) ---
            var query = allGamesList.Where(game =>
            {
                bool matchCategory = (selectedCategory == null || selectedCategory.CategoryId == 0)
                                     || game.GameCategoryId == selectedCategory.CategoryId;

                bool matchSearch = string.IsNullOrEmpty(searchQuery)
                                   || game.GameTitle.ToLower().Contains(searchQuery);

                return matchCategory && matchSearch;
            });

            // --- 3. SORTING (LINQ) ---
            IOrderedEnumerable<GameModel> sortedData;

            switch (sortBy)
            {
                case "last_launched":
                    sortedData = query.OrderByDescending(g => g.GameLastLaunched);
                    break;
                case "launch_times":
                    sortedData = query.OrderByDescending(g => g.GameLaunchTimes);
                    break;
                case "id":
                    sortedData = query.OrderByDescending(g => g.GameId);
                    break;
                default: // "title"
                    sortedData = query.OrderBy(g => g.GameTitle);
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

