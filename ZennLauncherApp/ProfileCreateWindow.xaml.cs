using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for ProfileCreateWindow.xaml
    /// </summary>
    public partial class ProfileCreateWindow : Window
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = DBHandler.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        string _avatarPath = "";

        private readonly ProfileDataModel activeProfile;
        private readonly bool _isEditMode = false;

        public ProfileCreateWindow(ProfileDataModel profile = null)
        {
            InitializeComponent();

            if (profile != null)
            {
                activeProfile = profile;
                _isEditMode = true;

                Debug.WriteLine("PROFILE CREATE WINDOW - EDIT MODE ON");
                Debug.WriteLine("Profile ID: " + profile.ProfileId);
                Debug.WriteLine("Profile Name: " + profile.ProfileName);

                LoadExistingAvatar();

                this.Title = "Edit Profile";
                this.Height = 325;
                TitleTextBlock.Text = "EDIT PROFILE";
                BtnCreateProfile.Content = "Save";
                PasswordStackPanel.Visibility = Visibility.Collapsed;
                ConfirmPasswordStackPanel.Visibility = Visibility.Collapsed;
                CheckBoxDontUsePass.Visibility = Visibility.Collapsed;

                InputProfileName.Text = profile.ProfileName;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void LoadExistingAvatar()
        {
            string profileAvatarPath = System.IO.Path.Combine(func.dataPath, Convert.ToString(activeProfile.ProfileId),
                                                            "avatar.png");
            Debug.WriteLine("Image Path Set: " + profileAvatarPath);

            ImageSource avatarSource = null;

            if (File.Exists(profileAvatarPath))
            {
                avatarSource = new BitmapImage(new Uri(profileAvatarPath));

                if (avatarSource != null)
                {
                    Debug.WriteLine("Loading Image from " + profileAvatarPath + "...");
                    PreviewAvatar.Background = new ImageBrush(avatarSource);

                    _avatarPath = profileAvatarPath;
                }
            }
        }

        private void BtnCreateProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditMode)
            {
                bool isEditSuccessful = false;

                string _profileName = InputProfileName.Text.Trim();

                if (string.IsNullOrWhiteSpace(_profileName))
                {
                    PopupWindow popup = new PopupWindow("Enter Profile Name",
                                                        $"Your new profile name cannot be blank",
                                                        "Ok", false);
                    popup.ShowDialog();
                    return;
                }

                string query_ch = "SELECT username FROM profiles WHERE username = @username && id != @profile_id";

                var args_ch = new Dictionary<string, object>
                {
                    { "@username", _profileName },
                    { "@profile_id", activeProfile.ProfileId }
                };

                string result = db.ExecuteScalar<string>(db.ConnectionGlobal, query_ch, args_ch);

                if (result == _profileName)
                {
                    PopupWindow popup = new PopupWindow("Profile Name Occupied",
                                                        $"This profile name isn't available. Choose another name for your new profile",
                                                        "Ok", false);
                    popup.ShowDialog();
                    return;
                }

                string query_se = "UPDATE profiles SET username = @profile_name WHERE id = @profile_id";

                var args_se = new Dictionary<string, object>
                {
                    { "@profile_name", _profileName },
                    { "@profile_id", activeProfile.ProfileId }
                };

                try
                {
                    db.ExecuteQuery(db.ConnectionGlobal, query_se, args_se);

                    isEditSuccessful = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Edit Profile Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string profileAvatarPath = System.IO.Path.Combine(func.dataPath, Convert.ToString(activeProfile.ProfileId),
                                                                    "avatar.png");

                if (!string.IsNullOrEmpty(_avatarPath) && _avatarPath != profileAvatarPath)
                {
                    string fileExtension = System.IO.Path.GetExtension(_avatarPath);

                    File.Copy(_avatarPath, profileAvatarPath, true);
                }
                else if (string.IsNullOrEmpty(_avatarPath))
                {
                    try
                    {
                        File.Delete(profileAvatarPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Avatar Change Error", 
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (isEditSuccessful)
                {
                    string appPath = Process.GetCurrentProcess().MainModule.FileName;
                    Process.Start(appPath);
                    System.Environment.Exit(0);
                }
            }
            else
            {
                string _profileName = InputProfileName.Text.Trim();
                string _profilePass = "";
                string _confirmPass = "";

                int lastId;

                if (string.IsNullOrWhiteSpace(_profileName))
                {
                    PopupWindow popup = new PopupWindow("Enter Profile Name",
                                                        $"Your new profile name cannot be blank",
                                                        "Ok", false);
                    popup.ShowDialog();
                    return;
                }

                string query_ch = "SELECT username FROM profiles WHERE username = @username";

                var args_ch = new Dictionary<string, object>
                    {
                        { "@username", _profileName }
                    };

                string result = db.ExecuteScalar<string>(db.ConnectionGlobal, query_ch, args_ch);

                if (result == _profileName)
                {
                    PopupWindow popup = new PopupWindow("Profile Name Occupied",
                                                        $"This profile name isn't available. Choose another name for your new profile",
                                                        "Ok", false);
                    popup.ShowDialog();
                    return;
                }

                if (CheckBoxDontUsePass.IsChecked == true)
                {
                    string query_sv = "INSERT INTO profiles (username, first_active, keep_logged_in) VALUES (@profile_name, datetime('now', 'localtime'), @keep_logged_in); SELECT last_insert_rowid();";

                    var args_sv = new Dictionary<string, object>
                    {
                        { "@profile_name", _profileName },
                        { "@keep_logged_in", 0 }
                    };

                    try
                    {
                        lastId = db.ExecuteScalar<int>(db.ConnectionGlobal, query_sv, args_sv);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Create Profile Error", 
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    _profilePass = InputProfilePass.Password;
                    _confirmPass = InputConfirmPass.Password;

                    if (string.IsNullOrEmpty(_profilePass))
                    {
                        PopupWindow popup = new PopupWindow("Password is Empty",
                                                            $"The password cannot be blank",
                                                            "Ok", false);
                        popup.ShowDialog();
                        return;
                    }

                    if (string.IsNullOrEmpty(_confirmPass))
                    {
                        PopupWindow popup = new PopupWindow("Confirm Password",
                                                            $"Please retype your new password",
                                                            "Ok", false);
                        popup.ShowDialog();
                        return;
                    }

                    if (_profilePass != _confirmPass)
                    {
                        PopupWindow popup = new PopupWindow("Password Didn't Match",
                                                            $"The password didn't match. Please check your password confirmation",
                                                            "Ok", false);
                        popup.ShowDialog();
                        return;
                    }

                    string _hashResult = BCrypt.Net.BCrypt.HashPassword(_profilePass);

                    string query_sv = "INSERT INTO profiles (username, hash, first_active, keep_logged_in) VALUES (@profile_name, @hash, datetime('now', 'localtime'), @keep_logged_in); SELECT last_insert_rowid();";

                    var args_sv = new Dictionary<string, object>
                    {
                        { "@profile_name", _profileName },
                        { "@hash", _hashResult },
                        { "@keep_logged_in", 0 }
                    };

                    try
                    {
                        lastId = db.ExecuteScalar<int>(db.ConnectionGlobal, query_sv, args_sv);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Create Profile Error", 
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                try
                {
                    string targetPath = System.IO.Path.Combine(func.dataPath, lastId.ToString());
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    db.ProfileDBInitialize();

                    var currentConfig = cfg.LoadGlobalConfig();

                    currentConfig.ActiveProfile = lastId;

                    if (!string.IsNullOrEmpty(_avatarPath))
                    {
                        string fileExtension = System.IO.Path.GetExtension(_avatarPath);
                        string targetFile = System.IO.Path.Combine(targetPath, "avatar" + fileExtension);

                        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
                        File.Copy(_avatarPath, targetFile, true);
                    }

                    var parentWindow = Application.Current.Windows.OfType<ProfileSelectWindow>().FirstOrDefault();
                    if (parentWindow != null) parentWindow.newProfileSaved = true;

                    PopupWindow popup = new PopupWindow("Add New Profile",
                                                        $"Profile \"{_profileName}\" created successfully",
                                                        "Ok", false);
                    popup.ShowDialog();
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Initialize Profile Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Supported Image Files (*.png; *.jpg)|*.png; *.jpg|All files (*.*)|*.*";
            openFileDialog.Title = "Choose Image File";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedAvatarPath = openFileDialog.FileName;
                string extension = System.IO.Path.GetExtension(selectedAvatarPath).ToLower();
                ImageSource avatarSource = null;

                if (extension == ".png" || extension == ".jpg")
                {
                    avatarSource = new BitmapImage(new Uri(selectedAvatarPath));
                }

                if (avatarSource != null)
                {
                    PreviewAvatar.Background = new ImageBrush(avatarSource);

                    _avatarPath = selectedAvatarPath;
                }
            }
        }

        private void BtnResetAvatar_Click(object sender, RoutedEventArgs e)
        {
            _avatarPath = "";

            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri("pack://application:,,,/default_avatar.jpg"));

                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = bitmap;
                imageBrush.Stretch = Stretch.UniformToFill;

                PreviewAvatar.Background = imageBrush;
            }
            catch (Exception ex)
            {
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

                PreviewAvatar.Background = defaultBgBrush;
            }
        }

    }
}

