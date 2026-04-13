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
    /// Interaction logic for AddToolWindow.xaml
    /// </summary>
    public partial class ToolPropertiesWindow : Window
    {
        private readonly GlobalFunctions func = new GlobalFunctions();
        private readonly DBHandler db = DBHandler.Instance;

        string _toolIconPath;
        string _toolCoverPath;

        private ToolModel _existingTool;
        private bool _isEditMode = false;

        public ToolPropertiesWindow(ToolModel tool = null)
        {
            InitializeComponent();

            if (tool != null)
            {
                _existingTool = tool;
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
            InputToolTitle.Text = _existingTool.ToolTitle;
            InputToolDesc.Text = _existingTool.ToolDesc;
            InputToolTarget.Text = _existingTool.ToolTarget;
            InputToolStartIn.Text = System.IO.Path.GetDirectoryName(InputToolTarget.Text);
            InputToolLaunchOptions.Text = _existingTool.ToolLaunchOptions;

            _toolIconPath = _existingTool.ToolIconPath;
            _toolCoverPath = _existingTool.ToolCoverPath;

            if (!string.IsNullOrEmpty(_toolIconPath))
            {
                string extension = System.IO.Path.GetExtension(_toolIconPath).ToLower();
                if (extension == ".png" || extension == ".jpg")
                {
                    BitmapImage bmp = new BitmapImage(new Uri(_toolIconPath));
                    PreviewIcon.Background = new ImageBrush(bmp);
                }
                else
                {
                    PreviewIcon.Background = new ImageBrush(func.GetIconNative(_toolIconPath));
                }
                    
            }

            if (!string.IsNullOrEmpty(_toolCoverPath))
            {
                try
                {
                    BitmapImage bmp = new BitmapImage(new Uri(_toolCoverPath));
                    PreviewCover.Background = new ImageBrush(bmp) { Stretch = Stretch.UniformToFill };
                }
                catch { }
            }
        }

        private void BtnBrowseTarget_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Supported Program Files (*.exe; *.lnk)|*.exe;*.lnk|All files (*.*)|*.*";
            openFile.Title = "Select Tool Executable";

            if (openFile.ShowDialog() == true)
            {
                InputToolTarget.Text = openFile.FileName;
                InputToolStartIn.Text = System.IO.Path.GetDirectoryName(openFile.FileName);

                if (string.IsNullOrWhiteSpace(InputToolTitle.Text))
                {
                    InputToolTitle.Text = System.IO.Path.GetFileNameWithoutExtension(openFile.FileName);
                }
            }
        }

        private void BtnBrowseStartIn_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cmnOpenFile = new CommonOpenFileDialog();
            cmnOpenFile.IsFolderPicker = true;
            cmnOpenFile.Title = "Select Tool Folder";

            Window owner = Window.GetWindow(this);

            if (cmnOpenFile.ShowDialog(owner) == CommonFileDialogResult.Ok)
            {
                InputToolStartIn.Text = cmnOpenFile.FileName;
            }
        }

        private void BtnChangeIcon_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Supported Icon/Program Files (*.ico; *.exe; *.lnk)|*.ico; *.exe; *.lnk|Supported Image Files (*.png; *.jpg)|*.png; *.jpg|All files (*.*)|*.*";
            openFileDialog.Title = "Choose Icon/Exe/Shortcut File";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedIconPath = openFileDialog.FileName;
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

                    _toolIconPath = selectedIconPath;
                }
            }
        }

        private void BtnChangeCover_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Supported Image Files (*.png; *.jpg)|*.png; *.jpg|All files (*.*)|*.*";
            openFileDialog.Title = "Choose Image File";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedCoverPath = openFileDialog.FileName;

                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(selectedCoverPath);

                bmp.DecodePixelWidth = 235;

                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();

                ImageBrush brush = new ImageBrush(bmp);
                brush.Stretch = Stretch.UniformToFill;
                PreviewCover.Background = brush;

                _toolCoverPath = selectedCoverPath;
            }
        }

        private void BtnResetCover_Click(object sender, RoutedEventArgs e)
        {
            _toolCoverPath = null;

            LinearGradientBrush defaultBgBrush = new LinearGradientBrush();
            var color1 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF342C49");
            var color2 = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF403659");
            System.Windows.Point point1 = new System.Windows.Point(0.5, 0);
            System.Windows.Point point2 = new System.Windows.Point(0.5, 1);

            defaultBgBrush.StartPoint = point1;
            defaultBgBrush.EndPoint = point2;
            defaultBgBrush.GradientStops.Add(new GradientStop(color1, 0));
            defaultBgBrush.GradientStops.Add(new GradientStop(color2, 0.5));
            defaultBgBrush.GradientStops.Add(new GradientStop(color1, 1));

            PreviewCover.Background = defaultBgBrush;

        }

        private void BtnSaveToolInfo_Click(object sender, RoutedEventArgs e)
        {
            string _toolTitle = InputToolTitle.Text.Trim();
            string _toolDesc = InputToolDesc.Text.Trim();
            string _toolTarget = InputToolTarget.Text.Trim();
            string _toolStartIn = InputToolStartIn.Text.Trim();
            string _toolLaunchOptions = InputToolLaunchOptions.Text.Trim();

            string inputEmpty = "Input Rmpty";

            if (string.IsNullOrWhiteSpace(_toolTitle))
            {
                PopupWindow popup = new PopupWindow(inputEmpty,
                                                    $"The tool title cannot be blank",
                                                    "Ok", false);
                popup.ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(_toolTarget))
            {
                PopupWindow popup = new PopupWindow(inputEmpty,
                                                    $"The game target directory cannot be blank",
                                                    "Ok", false);
                popup.ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(_toolStartIn))
            {
                PopupWindow popup = new PopupWindow(inputEmpty,
                                                    $"The game starting directory cannot be blank",
                                                    "Ok", false);
                popup.ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(_toolDesc))
            {
                _toolDesc = null;
            }

            if (string.IsNullOrWhiteSpace(_toolLaunchOptions))
            {
                _toolLaunchOptions = null;
            }

            if (string.IsNullOrWhiteSpace(_toolIconPath) || 
                System.IO.Path.GetExtension(_toolIconPath) == ".exe" ||
                System.IO.Path.GetExtension(_toolIconPath) == ".lnk")
            {
                string iconPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(db.ConnectionProfile.Replace("Data Source=", "")), "Icons");

                if (!Directory.Exists(iconPath))
                {
                    Directory.CreateDirectory(iconPath);
                }

                var exeIcon = func.GetIconNative(_toolTarget);

                if (exeIcon != null)
                {
                    // Buat ikon di direktori Documents/Zenn Launcher/user_id/Icons/app.ico
                    string safeTitle = string.Concat(_toolTitle.Split(System.IO.Path.GetInvalidFileNameChars()));
                    string fileName = $"{safeTitle}.png";
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

                        _toolIconPath = iconTarget;
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
                    { "@title", InputToolTitle.Text.Trim() },
                    { "@desc", string.IsNullOrWhiteSpace(InputToolDesc.Text) ? null : InputToolDesc.Text.Trim() },
                    { "@target", InputToolTarget.Text.Trim() },
                    { "@start_in", InputToolStartIn.Text.Trim() },
                    { "@launch_options", string.IsNullOrWhiteSpace(InputToolLaunchOptions.Text) ? null : InputToolLaunchOptions.Text.Trim() },
                    { "@cover_path", _toolCoverPath },
                    { "@icon_path", _toolIconPath }
                };

                if (_isEditMode)
                {
                    PopupWindow popup = new PopupWindow("Confirm Edit",
                                                        $"Confirm editing \"{InputToolTitle.Text}\"?",
                                                        "Confirm", true);

                    popup.ShowDialog();
                    if (popup.DialogResult == true)
                    {
                        // Edit Mode
                        query = @"UPDATE tools SET 
                            title=@title, desc=@desc, target=@target, 
                            start_in=@start_in, launch_options=@launch_options, 
                            cover_path=@cover_path, icon_path=@icon_path 
                            WHERE id=@id";

                        args.Add("@id", _existingTool.ToolId);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    // Add Mode
                    query = @"INSERT INTO tools (title, desc, target, start_in, launch_options, cover_path, icon_path) 
                            VALUES (@title, @desc, @target, @start_in, @launch_options, @cover_path, @icon_path)";
                }

                db.ExecuteQuery(db.ConnectionProfile, query, args);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error Saving Data", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
