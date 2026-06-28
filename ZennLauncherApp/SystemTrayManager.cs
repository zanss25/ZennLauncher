using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace ZennLauncherApp
{
    public class SystemTrayManager
    {
        private TaskbarIcon _notifyIcon;
        private readonly GlobalFunctions func = GlobalFunctions.Instance;

        public SystemTrayManager()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    Uri iconUri = new Uri("pack://application:,,,/Icon.ico");
                    StreamResourceInfo streamInfo = Application.GetResourceStream(iconUri);

                    _notifyIcon = new TaskbarIcon
                    {
                        Icon = streamInfo != null ? new System.Drawing.Icon(streamInfo.Stream) : null,
                        ToolTipText = "Zenn Launcher",
                        Visibility = Visibility.Visible
                    };

                    _notifyIcon.TrayMouseDoubleClick += (s, e) => MaximizeFromTray();

                    BuildContextMenu();

                    AppStateManager.Instance.HideTrayAction = () =>
                    {
                        Destroy();
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Gagal membuat tray: " + ex.Message);
                }
            }));
        }

        // Styles declaration
        Style menuStyle = (Style)Application.Current.FindResource("ContextMenuStyle");
        Style itemStyle = (Style)Application.Current.FindResource("ContextMenuItemStyle_Common");
        Style sepStyle = (Style)Application.Current.FindResource("ContextMenuSeparatorStyle");

        private void BuildContextMenu()
        {
            ContextMenu trayMenu = new ContextMenu { Style = menuStyle };

            trayMenu.Opened += TrayContextMenu_Opened;

            // --- Bagian 1: Recent Games (Header Induk) ---
            MenuItem recentMarker = new MenuItem
            {
                Name = "RecentGamesMarker",
                Header = "no recent games",
                IsEnabled = false,
                Style = itemStyle
            };
            trayMenu.Items.Add(recentMarker);

            Separator recentSeparator = new Separator { Name = "RecentGamesSeparator", Style = sepStyle };
            trayMenu.Items.Add(recentSeparator);

            // --- Bagian 2: Tombol Navigasi Halaman Launcher ---
            trayMenu.Items.Add(CreateMenuItem("Games", "games", MenuTabs_Click, itemStyle));
            trayMenu.Items.Add(CreateMenuItem("Tools", "tools", MenuTabs_Click, itemStyle));
            trayMenu.Items.Add(CreateMenuItem("Library", "categories", MenuTabs_Click, itemStyle));
            trayMenu.Items.Add(CreateMenuItem("Boards", "boards", MenuTabs_Click, itemStyle));

            trayMenu.Items.Add(new Separator { Style = sepStyle });

            // --- Bagian 3: Settings & Exit ---
            trayMenu.Items.Add(CreateMenuItem("Settings", null, MenuSettings_Click, itemStyle));
            trayMenu.Items.Add(CreateMenuItem("Exit", null, MenuExit_Click, itemStyle));

            _notifyIcon.ContextMenu = trayMenu;
        }

        private MenuItem CreateMenuItem(string header, string commandParam, RoutedEventHandler clickEvent, Style style)
        {
            MenuItem item = new MenuItem { Header = header, Style = style };
            if (!string.IsNullOrEmpty(commandParam)) item.CommandParameter = commandParam;
            item.Click += clickEvent;
            return item;
        }

        private void TrayContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu trayMenu)
            {
                var separator = trayMenu.Items.OfType<Separator>()
                                .FirstOrDefault(item => item.Name == "RecentGamesSeparator");

                if (separator == null) return;

                int separatorIndex = trayMenu.Items.IndexOf(separator);

                while (separatorIndex > 0)
                {
                    trayMenu.Items.RemoveAt(0);
                    separatorIndex--; // Indeks bergeser maju setelah item pertama dihapus
                }

                var allGamesList = AppStateManager.Instance.AllGamesList;

                if (allGamesList == null || allGamesList.Count == 0)
                {
                    trayMenu.Items.Insert(0, new MenuItem { Header = "No recent games found", IsEnabled = false });
                    return;
                }

                string formatTanggalDb = "yyyy-MM-dd HH:mm:ss";
                var recentGames = allGamesList
                    .Where(g => !string.IsNullOrEmpty(g.GameLastLaunched) && DateTime.TryParseExact(g.GameLastLaunched, formatTanggalDb, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    .OrderByDescending(g => DateTime.ParseExact(g.GameLastLaunched, formatTanggalDb, CultureInfo.InvariantCulture))
                    .Take(5).ToList();

                if (recentGames.Count == 0)
                {
                    trayMenu.Items.Insert(0, new MenuItem { Header = "No recent games played", IsEnabled = false });
                    return;
                }

                int insertIndex = 0;
                foreach (var game in recentGames)
                {
                    MenuItem gameItem = new MenuItem
                    {
                        Header = game.GameTitle,
                        CommandParameter = game,
                        Style = itemStyle
                    };
                    gameItem.Click += RecentGameItem_Click;

                    if (!string.IsNullOrEmpty(game.GameIconPath) && System.IO.File.Exists(game.GameIconPath))
                    {
                        try
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(game.GameIconPath);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();

                            Image imgIcon = new Image
                            {
                                Source = bitmap,
                                Width = 16,     
                                Height = 16,
                                Margin = new Thickness(0, 0, 5, 0), 
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center
                            };

                            gameItem.Icon = imgIcon;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Gagal memuat ikon game \"{game.GameTitle}\": {ex.Message}");
                        }
                    }

                    trayMenu.Items.Insert(insertIndex, gameItem);
                    insertIndex++;
                }
            }
        }

        private void RecentGameItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem gameItem && gameItem.CommandParameter is GameModel game)
            {
                func.LaunchFile(game.GameTarget, game.GameLaunchOptions);
                AppStateManager.Instance.UpdateAppData(game, 1);
            }
        }

        private void MenuTabs_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter != null)
            {
                string targetTab = menuItem.CommandParameter.ToString();
                MaximizeFromTray();

                var targetWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                targetWindow?.NavigateToTab(targetTab);
            }
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            var targetWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            var existingSettingsWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
            if (targetWindow != null && existingSettingsWindow == null)
            {
                targetWindow.OpenSettings("general");
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Destroy();
            Application.Current.Shutdown();
        }

        public void MaximizeFromTray()
        {
            var main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (main != null)
            {
                if (!main.IsVisible) main.Show();
                if (main.WindowState == WindowState.Minimized) main.WindowState = WindowState.Normal;
                main.Activate();
                main.Topmost = true;
                main.Topmost = false;
            }
        }

        public void Destroy()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.ContextMenu = null;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }
    }
}