using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
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
using static ZennLauncherApp.GlobalFunctions;

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

        List<CategoryModel> allCategoriesList = new List<CategoryModel>();

        public ToolsView()
        {
            InitializeComponent();
            LoadCategoriesList();

            ComboSortBy.ItemsSource = func.SortOptions;
            ComboSortBy.DisplayMemberPath = "DisplayName";
            ComboSortBy.SelectedIndex = 0;

            this.Loaded += ToolsView_Loaded;
        }

        private async void ToolsView_Loaded(object sender, RoutedEventArgs e)
        {
            var categories = await Task.Run(() => db.GetAllCategories(1));

            CategoryModel allCategory = new CategoryModel
            {
                CategoryId = 0,
                CategoryName = "All Tools"
            };

            allCategoriesList = categories ?? new List<CategoryModel>();
            allCategoriesList.Insert(0, allCategory);

            ComboCategories.ItemsSource = allCategoriesList;
            ComboCategories.DisplayMemberPath = "CategoryName";
            ComboCategories.SelectedIndex = 0;

            ContainerBtnApps.ItemsSource = AppStateManager.Instance.AllGamesList;

            LoadDefaultSortOptions();
            AppStateManager.Instance.LoadAllData();
            ApplyFilters();
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

        public void LoadCategoriesList()
        {
            int _categoriesCount = 0;
            _categoriesCount = db.ExecuteScalar<int>(db.ConnectionProfile, "SELECT COUNT(*) FROM categories WHERE section_id = 2");

            CategoryModel allCategory = new CategoryModel
            {
                CategoryId = 0,
                CategoryName = "All Tools",
                CategoryDesc = "Display all tools from your library"
            };

            allCategoriesList = new List<CategoryModel>();

            if (_categoriesCount > 0)
            {
                var dbResult = db.GetAllCategories(2);
                if (dbResult != null)
                {
                    allCategoriesList = dbResult;
                }
            }

            allCategoriesList.Insert(0, allCategory);

            ComboCategories.ItemsSource = null; // Reset binding
            ComboCategories.ItemsSource = allCategoriesList;
            ComboCategories.DisplayMemberPath = "CategoryName";
            ComboCategories.SelectedIndex = 0;
        }

        private void GetChildProcesses(int parentId, List<Process> results)
        {
            // Menggunakan WMI untuk mencari proses yang ParentProcessId-nya cocok
            string query = $"SELECT ProcessId FROM Win32_Process WHERE ParentProcessId = {parentId}";
            using (var searcher = new ManagementObjectSearcher(query))
            using (var collection = searcher.Get())
            {
                foreach (var obj in collection)
                {
                    int childId = Convert.ToInt32(obj["ProcessId"]);
                    try
                    {
                        var childProc = Process.GetProcessById(childId);
                        results.Add(childProc);
                        // Rekursif: Cari anak dari anak ini juga (untuk launcher bertingkat)
                        GetChildProcesses(childId, results);
                    }
                    catch (ArgumentException)
                    {

                    }
                    finally
                    {
                        obj.Dispose();
                    }
                }
            }
        }

        private void LaunchApp(IAppItem launchedApp)
        {
            if (launchedApp == null)
            {
                PopupWindow popup = new PopupWindow("Tool doesn't exist",
                                                     "No tool found at this menu",
                                                     "Ok", false);
                return;
            }

            var result = func.LaunchFile(launchedApp.AppTarget, launchedApp.AppLaunchOptions);

            if (result != LaunchResult.Success)
                return;

            db.UpdateAppAtLaunch(launchedApp, 1);

            var launchAction = ((App)Application.Current).CurrentProfileCfg.General.ToolsLaunchAction;
            bool shiftHeld = Keyboard.IsKeyDown(Key.LeftShift);

            if (launchAction == ProfileGeneralSettings.LaunchActions.HideLauncher && !shiftHeld)
            {
                Application.Current.MainWindow.Hide();

                int rootId = func.runningProcess.Id;

                // Gunakan Task.Run agar tidak memblokir UI thread
                Task.Run(async () =>
                {
                    bool isGameRunning = true;

                    while (isGameRunning)
                    {
                        await Task.Delay(3000); // Cek setiap 3 detik agar hemat CPU

                        List<Process> familyTree = new List<Process>();

                        // 1. Cek apakah proses utama masih ada
                        try { familyTree.Add(Process.GetProcessById(rootId)); } catch { }

                        // 2. Cari semua anak-anaknya
                        try { GetChildProcesses(rootId, familyTree); } catch { }

                        // 3. Filter hanya proses yang masih aktif
                        if (familyTree.Count == 0)
                        {
                            isGameRunning = false;
                        }
                    }

                    this.Dispatcher.Invoke(() => Application.Current.MainWindow.Show());
                });
            }
            else if (launchAction == ProfileGeneralSettings.LaunchActions.CloseLauncher && !shiftHeld)
            {
                Application.Current.MainWindow.Close();
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
            var tool = menuItem.DataContext as ToolModel;
            LaunchApp(tool);
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var selectedTool = menuItem.Tag as ToolModel;

            Main.EnableBlurAndOverlay();
            ToolPropertiesWindow edit = new ToolPropertiesWindow(selectedTool);
            if (edit.ShowDialog() == true)
            {
                AppStateManager.Instance.LoadAllData();
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
                        string folderPath = System.IO.Path.GetDirectoryName(tool.ToolTarget);
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = folderPath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show("Directory or file not found.", "Error",
                                            MessageBoxButton.OK, MessageBoxImage.Warning);
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
                PopupWindow popup = new PopupWindow("Remove Tool",
                                                     $"Are you sure you want to remove \"{selectedTool.ToolTitle}\" from your library?",
                                                     "Remove", true);

                popup.ShowDialog();
                if (popup.DialogResult == true)
                {
                    DBHandler db = DBHandler.Instance;
                    db.DeleteApp(selectedTool.ToolId, selectedTool.TargetTable);

                    AppStateManager.Instance.LoadAllData();
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
            if (AppStateManager.Instance.AllToolsList == null) return;

            // --- 1. AMBIL INPUT ---
            var selectedCategory = ComboCategories.SelectedItem as CategoryModel;
            string searchQuery = SearchBar.Text.ToLower().Trim();

            var selectedSortItem = (SortCriterion)ComboSortBy.SelectedItem;
            if (selectedSortItem == null) return;

            string sortBy = selectedSortItem.SortByTag.ToString();
            bool asc = !(sortBy == "id" || sortBy == "last_launched" || sortBy == "launch_times");

            // --- 2. FILTERING (Kategori & Search) ---
            var query = AppStateManager.Instance.AllToolsList.Where(tools =>
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

        private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if (!comboBox.IsDropDownOpen)
            {
                if (e.Delta > 0)
                {
                    if (comboBox.SelectedIndex > 0)
                        comboBox.SelectedIndex--;
                }
                else if (e.Delta < 0)
                {
                    if (comboBox.SelectedIndex < comboBox.Items.Count - 1)
                        comboBox.SelectedIndex++;
                }

                // Tandai event sebagai handled agar tidak men-scroll halaman utama
                e.Handled = true;
            }
        }

        private void RefreshCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AppStateManager.Instance.LoadAllData();
            ApplyFilters();
        }

    }
}
