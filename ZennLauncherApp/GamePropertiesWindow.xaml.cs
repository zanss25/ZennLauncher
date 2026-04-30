using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for AddGameWindow.xaml
    /// </summary>
    public partial class GamePropertiesWindow : Window
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = DBHandler.Instance;

        string _gameIconPath;
        string _gameCoverPath;

        private readonly GameModel _existingGame;
        private readonly bool _isEditMode = false;

        public GamePropertiesWindow(GameModel game = null)
        {
            InitializeComponent();

            if (game != null)
            {
                _existingGame = game;
                _isEditMode = true;
                FillDataToFields();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FillDataToFields()
        {
            InputGameTitle.Text = _existingGame.GameTitle;
            InputGameSubtitle.Text = _existingGame.GameSubtitle;
            InputGameTarget.Text = _existingGame.GameTarget;
            InputGameStartIn.Text = System.IO.Path.GetDirectoryName(InputGameTarget.Text);
            InputGameLaunchOptions.Text = _existingGame.GameLaunchOptions;

            _gameIconPath = _existingGame.GameIconPath;
            _gameCoverPath = _existingGame.GameCoverPath;

            string extension = System.IO.Path.GetExtension(_gameIconPath).ToLower();
            if (extension == ".png" || extension == ".jpg")
            {
                BitmapImage bmp = new BitmapImage(new Uri(_gameIconPath));
                PreviewIcon.Background = new ImageBrush(bmp);
            }
            else
            {
                PreviewIcon.Background = new ImageBrush(func.GetIconNative(_gameIconPath));
            }

            if (!string.IsNullOrEmpty(_gameCoverPath))
            {
                try
                {
                    BitmapImage bmp = new BitmapImage(new Uri(_gameCoverPath));
                    PreviewCover.Background = new ImageBrush(bmp) { Stretch = Stretch.UniformToFill };
                }
                catch { }
            }
        }

        private void BtnBrowseTarget_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Supported Program Files (*.exe; *.lnk)|*.exe;*.lnk|All files (*.*)|*.*";
            openFile.Title = "Select Game Executable";

            if (Directory.Exists(((App)Application.Current).CurrentProfileCfg.Folders.DefaultGamesDir))
            {
                openFile.InitialDirectory = ((App)Application.Current).CurrentProfileCfg.Folders.DefaultGamesDir;
            }

            if (openFile.ShowDialog() == true)
            {
                InputGameTarget.Text = openFile.FileName;
                InputGameStartIn.Text = System.IO.Path.GetDirectoryName(openFile.FileName);

                if (string.IsNullOrWhiteSpace(InputGameTitle.Text))
                {
                    InputGameTitle.Text = System.IO.Path.GetFileNameWithoutExtension(openFile.FileName);
                }
            }
        }

        private void BtnBrowseStartIn_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cmnOpenFile = new CommonOpenFileDialog();
            cmnOpenFile.IsFolderPicker = true;
            cmnOpenFile.Title = "Select Game Folder";

            if (Directory.Exists(((App)Application.Current).CurrentProfileCfg.Folders.DefaultGamesDir))
            {
                cmnOpenFile.InitialDirectory = ((App)Application.Current).CurrentProfileCfg.Folders.DefaultGamesDir;
            }

            Window owner = Window.GetWindow(this);

            if (cmnOpenFile.ShowDialog(owner) == CommonFileDialogResult.Ok)
            {
                InputGameStartIn.Text = cmnOpenFile.FileName;
            }
        }

        private void BtnChangeIcon_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Supported Icon/Program Files (*.ico; *.exe; *.lnk)|*.ico; *.exe; *.lnk|Supported Image Files (*.png; *.jpg)|*.png; *.jpg|All files (*.*)|*.*";
            openFile.Title = "Choose Icon/Image/Exe/Shortcut File";

            if (Directory.Exists(((App)Application.Current).CurrentProfileCfg.Folders.DefaultIconsDir))
            {
                openFile.InitialDirectory = ((App)Application.Current).CurrentProfileCfg.Folders.DefaultIconsDir;
            }

            if (openFile.ShowDialog() == true)
            {
                string selectedIconPath = openFile.FileName;
                string extension = System.IO.Path.GetExtension(selectedIconPath).ToLower();
                ImageSource iconSource = null;

                if (extension == ".png" || extension == ".jpg")
                {
                    iconSource = new BitmapImage(new Uri(selectedIconPath));
                }
                else
                {
                    iconSource = func.GetIconNative(selectedIconPath);
                }

                if (iconSource != null)
                {
                    PreviewIcon.Background = new ImageBrush(iconSource);

                    _gameIconPath = selectedIconPath;
                }
            }
        }

        private void BtnChangeCover_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Supported Image Files (*.png; *.jpg)|*.png; *.jpg|All files (*.*)|*.*";
            openFile.Title = "Choose Image File";

            if (Directory.Exists(((App)Application.Current).CurrentProfileCfg.Folders.DefaultCoversDir))
            {
                openFile.InitialDirectory = ((App)Application.Current).CurrentProfileCfg.Folders.DefaultCoversDir;
            }

            if (openFile.ShowDialog() == true)
            {
                string selectedCoverPath = openFile.FileName;

                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(selectedCoverPath);

                bmp.DecodePixelWidth = 235;

                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();

                ImageBrush brush = new ImageBrush(bmp);
                brush.Stretch = Stretch.UniformToFill;
                PreviewCover.Background = brush;

                _gameCoverPath = selectedCoverPath;
            }
        }

        private void BtnResetCover_Click(object sender, RoutedEventArgs e)
        {
            _gameCoverPath = null;

            LinearGradientBrush defaultBgBrush = new LinearGradientBrush();
            var color1 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF342C49");
            var color2 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF403659");
            System.Windows.Point point1 = new System.Windows.Point(0, 0);
            System.Windows.Point point2 = new System.Windows.Point(1, 1);

            defaultBgBrush.StartPoint = point1;
            defaultBgBrush.EndPoint = point2;
            defaultBgBrush.GradientStops.Add(new GradientStop(color1, 0));
            defaultBgBrush.GradientStops.Add(new GradientStop(color2, 0.5));
            defaultBgBrush.GradientStops.Add(new GradientStop(color1, 1));

            PreviewCover.Background = defaultBgBrush;

        }

        private void BtnSaveGameInfo_Click(object sender, RoutedEventArgs e)
        {
            string _gameTitle = InputGameTitle.Text.Trim();
            string _gameSubtitle = InputGameSubtitle.Text.Trim();
            string _gameTarget = InputGameTarget.Text.Trim();
            string _gameStartIn = InputGameStartIn.Text.Trim();
            string _gameLaunchOptions = InputGameLaunchOptions.Text.Trim();

            string inputEmpty = "Input Empty";

            if (string.IsNullOrWhiteSpace(_gameTitle)) {
                PopupWindow popup = new PopupWindow(inputEmpty,
                                                    $"The game title cannot be blank",
                                                    "Ok", false);
                popup.ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(_gameTarget))
            {
                PopupWindow popup = new PopupWindow(inputEmpty,
                                                    $"The game target directory cannot be blank",
                                                    "Ok", false);
                popup.ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(_gameStartIn))
            {
                PopupWindow popup = new PopupWindow(inputEmpty,
                                                    $"The game starting directory cannot be blank",
                                                    "Ok", false);
                popup.ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(_gameSubtitle))
            {
                _gameSubtitle = null;
            }

            if (string.IsNullOrWhiteSpace(_gameLaunchOptions))
            {
                _gameLaunchOptions = null;
            }

            if (string.IsNullOrWhiteSpace(_gameIconPath) || 
                System.IO.Path.GetExtension(_gameIconPath) == ".exe" ||
                System.IO.Path.GetExtension(_gameIconPath) == ".lnk")
            {
                string iconPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(db.ConnectionProfile.Replace("Data Source=", "")), "Icons");

                if (!Directory.Exists(iconPath))
                {
                    Directory.CreateDirectory(iconPath);
                }

                var exeIcon = func.GetIconNative(_gameTarget);

                if (exeIcon != null)
                {
                    // Buat ikon di direktori Documents/Zenn Launcher/user_id/Icons/app.ico
                    string safeTitle = string.Concat(_gameTitle.Split(System.IO.Path.GetInvalidFileNameChars()));
                    string safeSubtitle = string.Concat((_gameSubtitle ?? "").Split(System.IO.Path.GetInvalidFileNameChars()));
                    string fileName = $"{safeTitle}_{safeSubtitle}.png";
                    string iconTarget = System.IO.Path.Combine(iconPath, fileName);

                    try
                    {
                        BitmapSource bitmapSource = exeIcon as BitmapSource;

                        // Jika ternyata exeIcon bukan BitmapSource (misal DrawingImage), lakukan konversi
                        if (bitmapSource == null)
                        {
                        }

                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                        using (var stream = File.Create(iconTarget))
                        {
                            encoder.Save(stream);
                        }

                        _gameIconPath = iconTarget;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed saving icon: " + ex.Message);
                    }
                }
            }

            try
            {
                string query;
                var args = new Dictionary<string, object>
                {
                    { "@title", InputGameTitle.Text.Trim() },
                    { "@subtitle", string.IsNullOrWhiteSpace(InputGameSubtitle.Text) ? null : InputGameSubtitle.Text.Trim() },
                    { "@target", InputGameTarget.Text.Trim() },
                    { "@start_in", InputGameStartIn.Text.Trim() },
                    { "@launch_options", string.IsNullOrWhiteSpace(InputGameLaunchOptions.Text) ? null : InputGameLaunchOptions.Text.Trim() },
                    { "@cover_path", _gameCoverPath },
                    { "@icon_path", _gameIconPath }
                };

                if (_isEditMode)
                {
                    PopupWindow popup = new PopupWindow("Confirm Edit",
                                                        $"Confirm editing \"{InputGameTitle.Text}\"?",
                                                        "Confirm", true);

                    popup.ShowDialog();
                    if (popup.DialogResult == true)
                    {
                        // Edit Mode
                        query = @"UPDATE games SET 
                                title=@title, subtitle=@subtitle, target=@target, 
                                start_in=@start_in, launch_options=@launch_options, 
                                cover_path=@cover_path, icon_path=@icon_path 
                                WHERE id=@id";
                        args.Add("@id", _existingGame.GameId);
                    } else
                    {
                        return;
                    }
                }
                else
                {
                    // Add Mode
                    query = @"INSERT INTO games (title, subtitle, target, start_in, launch_options, cover_path, icon_path) 
                            VALUES (@title, @subtitle, @target, @start_in, @launch_options, @cover_path, @icon_path)";
                }

                db.ExecuteQuery(db.ConnectionProfile, query, args);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error Saving Data", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
