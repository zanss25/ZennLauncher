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

            allGamesList = db.GetAllGames();
            allToolsList = db.GetAllTools();

            LoadCategoriesList(1);
            LoadCategoriesList(2);

            GamesCategoriesList.SelectedIndex = 0;
            CheckAndPreventAddNewToCategories();

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

        private void CheckAndPreventAddNewToCategories()
        {
            if (GamesCategoriesList.SelectedIndex == 0 ||
                ToolsCategoriesList.SelectedIndex == 0)
            {
                BtnAddNewToCategory.IsEnabled = false;
            }
            else
            {
                BtnAddNewToCategory.IsEnabled = true;
            }
        }

        private void SearchBarCategories_Changed(object sender, TextChangedEventArgs e)
        {
            
        }

        private void GamesCategoriesList_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (GamesCategoriesList.SelectedItem != null)
            {
                ToolsCategoriesList.SelectedIndex = -1;

                AppsDataGrid.ItemsSource = null;
                ApplyFilters(1);
            }

            CheckAndPreventAddNewToCategories();
        }

        private void ToolsCategoriesList_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ToolsCategoriesList.SelectedItem != null)
            {
                GamesCategoriesList.SelectedIndex = -1;

                AppsDataGrid.ItemsSource = null;
                ApplyFilters(2);
            }

            CheckAndPreventAddNewToCategories();
        }

        private void SearchBarApps_Changed(object sender, TextChangedEventArgs e)
        {
            int activeSection = (GamesCategoriesList.SelectedIndex > -1) ? 1 : 2;
            ApplyFilters(activeSection);
        }

        public void Combo_Changed(object sender, SelectionChangedEventArgs e)
        {

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

            // 2. Filtering & Sorting (Cukup tulis sekali!)
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
            NoAppsAvailable.Visibility = filteredData.Any() ? Visibility.Hidden : Visibility.Visible;
        }

        private void BtnAddNewCategoryGames_Click(object sender, RoutedEventArgs e)
        {
            AddNewCategory(1);
            LoadCategoriesList(1);
        }

        private void BtnAddNewCategoryTools_Click(object sender, RoutedEventArgs e)
        {
            AddNewCategory(2);
            LoadCategoriesList(2);
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
                newName = inputA.ResultText.Trim();
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

            if (!String.IsNullOrEmpty(newName) || !String.IsNullOrEmpty(newDesc))
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
            }
            else
            {
                Debug.WriteLine("EMPTY: The newName and newDesc are empty.");
                return;
            }
        }

        private void BtnAddNewToCategory_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnRemoveFromCategory_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
