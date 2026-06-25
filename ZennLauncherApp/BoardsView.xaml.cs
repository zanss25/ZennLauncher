using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for RelicsHistoryView.xaml
    /// </summary>
    public partial class BoardsView : UserControl
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = DBHandler.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        List<GameModel> allGamesList = new List<GameModel>();
        List<ToolModel> allToolsList = new List<ToolModel>();

        public List<StatsItem> statsList { get; set; }

        private MainWindow Main => Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

        public BoardsView()
        {
            statsList = new List<StatsItem>()
            {
                new StatsItem
                {
                    StatName = "PlayedGames",
                    StatDisplay = "Played Games",
                    CalculateValue = () => allGamesList.Count(g => g.GameLaunchTimes > 0).ToString()
                },
                new StatsItem
                {
                    StatName = "UnplayedGames",
                    StatDisplay = "Unplayed Games",
                    CalculateValue = () => allGamesList.Count(g => g.GameLaunchTimes <= 0).ToString()
                },  
                new StatsItem
                {
                    StatName = "TotalGames",
                    StatDisplay = "Total Games",
                    CalculateValue = () => allGamesList.Count().ToString()
                },
                new StatsItem
                {
                    StatName = "TotalTools",
                    StatDisplay = "Total Tools",
                    CalculateValue = () => allToolsList.Count().ToString()
                },
                new StatsItem
                {
                    StatName = "GamesTotalLaunch",
                    StatDisplay = "Total Launch (Game)",
                    CalculateValue = () => allGamesList.Sum(g => g.GameLaunchTimes).ToString()
                },
                new StatsItem
                {
                    StatName = "ToolsTotalLaunch",
                    StatDisplay = "Total Launch (Tool)",
                    CalculateValue = () => allToolsList.Sum(t => t.ToolLaunchTimes).ToString()
                }
            };

            InitializeComponent();

            LoadGamesData();
            LoadMostPlayedData();
            LoadMostRatedData();
            LoadToolsData();
            UpdateHeaderStats();

            StatsDataListBox.ItemsSource = statsList;
        }

        public void RefreshStats()
        {
            this.DataContext = null;
            this.DataContext = this;
        }

        private void LoadGamesData()
        {
            allGamesList = db.GetAllGames();

            if (allGamesList == null)
            {
                allGamesList = new List<GameModel>();
            }
        }

        private void LoadMostPlayedData()
        {
            if (allGamesList == null) return;

            var firstMostPlayed = allGamesList
                                   .Where(game => game.GameLaunchTimes > 0)
                                   .OrderByDescending(game => game.GameLaunchTimes)
                                   .FirstOrDefault();

            var mostPlayedGames = allGamesList
                                 .Where(game => game.GameLaunchTimes > 0)
                                 .OrderByDescending(game => game.GameLaunchTimes)
                                 .Skip(1)
                                 .Take(4)
                                 .ToList();

            if (firstMostPlayed != null)
            {
                var imageConverter = new PathToImageConverter();

                if (!String.IsNullOrEmpty(firstMostPlayed.GameCoverPath))
                {
                    var bmp = imageConverter.Convert(firstMostPlayed.GameCoverPath, 
                                                     typeof(BitmapImage), 235, null) as BitmapImage;
                    if (bmp != null)
                    {
                        ImageBrush brush = new ImageBrush(bmp);
                        brush.Stretch = Stretch.UniformToFill;
                        MostPlayedCover.Background = brush;
                    }
                }
                else
                {
                    var bmp = imageConverter.Convert(firstMostPlayed.GameIconPath,
                                                     typeof(BitmapImage), 235, null) as BitmapImage;
                    if (bmp != null)
                    {
                        MostPlayedIcon.Source = bmp;
                    }
                }

                MostPlayedTitle.Text = firstMostPlayed.GameTitle;
                MostPlayedCount.Text = "Play count: " + firstMostPlayed.GameLaunchTimes;
            }

            List<GameRankingItem> rankingList = new List<GameRankingItem>();

            int rank = 2;
            foreach (var game in mostPlayedGames)
            {
                rankingList.Add(new GameRankingItem
                {
                    RankNumber = rank++,
                    AppTitle = game.AppTitle,
                    RankValue = game.GameLaunchTimes
                });
            }

            MostPlayedRanks.ItemsSource = rankingList;
        }

        private void LoadMostRatedData()
        {
            if (allGamesList == null) return;

            var firstTopRated = allGamesList
                                   .Where(game => game.GameUserRating > 0)
                                   .OrderByDescending(game => game.GameUserRating)
                                   .FirstOrDefault();

            var topRatedGames = allGamesList
                                 .Where(game => game.GameUserRating > 0)
                                 .OrderByDescending(game => game.GameUserRating)
                                 .Skip(1)
                                 .Take(4)
                                 .ToList();

            if (firstTopRated != null)
            {
                var imageConverter = new PathToImageConverter();

                if (!String.IsNullOrEmpty(firstTopRated.GameCoverPath))
                {
                    var bmp = imageConverter.Convert(firstTopRated.GameCoverPath,
                                                     typeof(BitmapImage), 235, null) as BitmapImage;
                    if (bmp != null)
                    {
                        ImageBrush brush = new ImageBrush(bmp);
                        brush.Stretch = Stretch.UniformToFill;
                        MostFavoriteCover.Background = brush;
                    }
                }
                else
                {
                    var bmp = imageConverter.Convert(firstTopRated.GameIconPath,
                                                     typeof(BitmapImage), 235, null) as BitmapImage;
                    if (bmp != null)
                    {
                        MostFavoriteIcon.Source = bmp;
                    }
                }

                MostFavoriteTitle.Text = firstTopRated.GameTitle;
                MostFavoriteRating.Text = "User Rating: " + firstTopRated.GameUserRating;
            }

            List<GameRankingItem> rankingList = new List<GameRankingItem>();

            int rank = 2;
            foreach (var game in topRatedGames)
            {
                rankingList.Add(new GameRankingItem
                {
                    RankNumber = rank++,
                    AppTitle = game.AppTitle,
                    RankValue = game.GameUserRating
                });
            }

            FavoritesRanks.ItemsSource = rankingList;
        }

        private void LoadToolsData()
        {
            allToolsList = db.GetAllTools();

            if (allToolsList == null)
            {
                allToolsList = new List<ToolModel>();
            }
        }

        private void UpdateHeaderStats()
        {
            if (allGamesList == null || !allGamesList.Any()) return;

            int playedGamesCount = allGamesList.Count(g => g.GameLaunchTimes > 0);

            var oldestLaunchGame = allGamesList
                .Where(g => !string.IsNullOrEmpty(g.GameLastLaunched))
                .Select(g => {
                    bool success = DateTime.TryParse(g.GameLastLaunched, out DateTime parsedDate);
                    return new { Success = success, Date = parsedDate };
                })
                .Where(x => x.Success)
                .OrderBy(x => x.Date)
                .FirstOrDefault();

            string timeAgoText = "recently";

            if (oldestLaunchGame != null)
            {
                DateTime oldestDate = oldestLaunchGame.Date;
                DateTime now = DateTime.Now;

                TimeSpan diff = now - oldestDate;

                int totalMonths = (now.Year - oldestDate.Year) * 12 + now.Month - oldestDate.Month;

                if (totalMonths >= 1)
                {
                    timeAgoText = $"{totalMonths} month{(totalMonths > 1 ? "s" : "")}";
                }
                else
                {
                    int totalDays = (int)diff.TotalDays;
                    if (totalDays >= 1)
                    {
                        timeAgoText = $"{totalDays} day{(totalDays > 1 ? "s" : "")}";
                    }
                    else
                    {
                        timeAgoText = "a few hours";
                    }
                }
            }

            HeaderStatsText.Text = $"Played {playedGamesCount} games since the last {timeAgoText}";
        }

        private void GamesCategoriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SearchBarCategories_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ToolsCategoriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

}
