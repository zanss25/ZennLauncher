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
        List<CategoryModel> allCategoriesList = new List<CategoryModel>();

        public CategoriesView()
        {
            InitializeComponent();

            LoadCategoriesList(1);
            LoadCategoriesList(2);
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

            allCategoriesList = new List<CategoryModel>();

            if (_categoriesCount > 0)
            {
                allCategoriesList = db.GetAllCategories(sectionId);
            }

            allCategoriesList.Insert(0, allCategory);

            if (sectionId == 1)
            {
                GamesCategoriesList.ItemsSource = allCategoriesList;
                GamesCategoriesList.DisplayMemberPath = "CategoryName";
            }

            if (sectionId == 2)
            {
                ToolsCategoriesList.ItemsSource = allCategoriesList;
                ToolsCategoriesList.DisplayMemberPath = "CategoryName";
            }
        }

        private void SearchBar_Changed(object sender, TextChangedEventArgs e)
        {
            
        }

        private void GamesCategoriesList_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (GamesCategoriesList.SelectedItem != null)
            {
                ToolsCategoriesList.SelectedIndex = -1;
                ApplyFilters(2);
            }
        }

        private void ToolsCategoriesList_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ToolsCategoriesList.SelectedItem != null)
            {
                GamesCategoriesList.SelectedIndex = -1;
                ApplyFilters(1);
            }
        }

        public void ApplyFilters(int sectionId)
        {
            // --- 1. AMBIL INPUT ---
            var selectedCategory = GamesCategoriesList.SelectedItem as CategoryModel;
            string searchQuery = SearchBar.Text.ToLower().Trim();

            if (sectionId == 1)
            {
                if (allGamesList == null) return;

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
                sortedData = query.OrderBy(g => g.GameTitle);

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
            else if (sectionId == 2)
            {
                if (allToolsList == null) return;

                // --- 2. FILTERING (Kategori & Search) ---
                var query = allToolsList.Where(game =>
                {
                    bool matchCategory = (selectedCategory == null || selectedCategory.CategoryId == 0)
                                         || game.ToolCategoryId == selectedCategory.CategoryId;

                    bool matchSearch = string.IsNullOrEmpty(searchQuery)
                                       || game.ToolTitle.ToLower().Contains(searchQuery);

                    return matchCategory && matchSearch;
                });

                // --- 3. SORTING (LINQ) ---
                IOrderedEnumerable<ToolModel> sortedData;
                sortedData = query.OrderBy(t => t.ToolTitle);

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
}
