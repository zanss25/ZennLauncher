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
        GlobalFunctions func = new GlobalFunctions();
        DBHandler db = new DBHandler();
        ConfigHandler cfg = new ConfigHandler();

        List<GameModel> allGamesList = new List<GameModel>();

        public GamesView()
        {
            
            InitializeComponent();
            LoadGamesList();

            ComboSortByGames.ItemsSource = func.SortOptions;
            ComboSortByGames.DisplayMemberPath = "DisplayName";
            ComboSortByGames.SelectedIndex = 0;

            LoadDefaultSortOptions();
        }

        private void LoadDefaultSortOptions()
        {
            string defaultSortOption = ((App)Application.Current).CurrentProfileCfg.General.GamesDefaultSortOption;

            if (string.IsNullOrEmpty(defaultSortOption))
            {
                defaultSortOption = "title";
            }

            var targetSortObject = func.SortOptions
                                   .FirstOrDefault(s => s.SortByTag == defaultSortOption);

            if (targetSortObject != null)
            {
                ComboSortByGames.SelectedItem = targetSortObject;
            }
            else
            {
                ComboSortByGames.SelectedIndex = 0;
            }
        }

        public void LoadGamesList(string _sortBy = "title", bool _ascending = true)
        {

            int _gamesCount = 0;
            _gamesCount = db.ExecuteScalar<int>(db.ConnectionProfile, "SELECT COUNT(*) FROM games");

            if (_gamesCount > 0)
            {
                allGamesList = db.GetAllGames(_sortBy, _ascending);

                ContainerBtnGames.ItemsSource = null;
                ContainerBtnGames.ItemsSource = allGamesList;

            }
        }

        private void BtnGames_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            var game = btn.DataContext as GameModel;

            if (game != null)
            {
                string filePath = game.GameTarget;
                string args = game.GameLaunchOptions;

                func.LaunchFile(filePath, args);

                string query_ = "UPDATE games SET last_launched = datetime('now', 'localtime') WHERE id = @id";

                var args_ = new Dictionary<string, object>
                {
                    { "@id", game.GameId }
                };

                db.ExecuteQuery(db.ConnectionProfile, query_, args_);

                string query__ = "UPDATE games SET launch_times = launch_times + 1 WHERE id = @id";

                var args__ = new Dictionary<string, object>
                {
                    { "@id", game.GameId }
                };

                db.ExecuteQuery(db.ConnectionProfile, query__, args__);

                if (((App)Application.Current).CurrentProfileCfg.General.GamesLaunchAction 
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
                else if (((App)Application.Current).CurrentProfileCfg.General.GamesLaunchAction 
                            == ProfileGeneralSettings.LaunchAction.CloseLauncher 
                            && !Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Application.Current.MainWindow.Close();
                }
                else
                {

                }
            }
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedGame = menuItem.Tag as GameModel;

            GamePropertiesWindow edit = new GamePropertiesWindow(selectedGame);
            if (edit.ShowDialog() == true)
            {
                ComboSortByGames_Changed(ComboSortByGames, null);
            }
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
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedGame = menuItem.Tag as GameModel;

            if (selectedGame != null)
            {
                var result = MessageBox.Show($"Are you sure you want to remove '{selectedGame.GameTitle}'?",
                                                "Confirm Delete",
                                                MessageBoxButton.YesNo, 
                                                MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DBHandler db = new DBHandler();
                    db.DeleteGame(selectedGame.GameId);
                    LoadGamesList();
                    LoadDefaultSortOptions();
                }
            }
        }

        private void SearchBarGames_Changed(object sender, TextChangedEventArgs e)
        {
            string query = SearchBarGames.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(query))
            {
                ContainerBtnGames.ItemsSource = allGamesList;
                return;
            }

            var filteredList = allGamesList.Where(g =>
                g.GameTitle.ToLower().Contains(query) ||
                (g.GameSubtitle != null && g.GameSubtitle.ToLower().Contains(query))
            ).ToList();

            ContainerBtnGames.ItemsSource = filteredList;
        }

        private void ComboSortByGames_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (db == null || ComboSortByGames.SelectedItem == null) return;

            var selectedItem = (SortCriterion)ComboSortByGames.SelectedItem;
            string sortBy = selectedItem.SortByTag.ToString();
            bool asc = true;

            if (sortBy == "id" || sortBy == "last_launched" || sortBy == "launch_times")
            {
                asc = false;
            }

            ContainerBtnGames.ItemsSource = db.GetAllGames(sortBy, asc);

        }
    }
}
