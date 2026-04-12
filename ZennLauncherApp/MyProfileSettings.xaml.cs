using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for MyProfileSettings.xaml
    /// </summary>
    public partial class MyProfileSettingsView : UserControl
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = new DBHandler();
        private SettingsWindow Settings => Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();

        public MyProfileSettingsView()
        {
            InitializeComponent();            
            LoadProfileData();

            if (string.IsNullOrEmpty(db.profileData.ProfileHash))
            {
                ResetPasswordBorder.Visibility = Visibility.Collapsed;
                AddPasswordBorder.Visibility = Visibility.Visible;
            }
        }

        private void LoadProfileData()
        {
            db.profileData = db.GetProfileData();

            this.DataContext = db.profileData;
        }

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileCreateWindow create = new ProfileCreateWindow(db.profileData);

            Settings.EnableBlurAndOverlay();
            create.ShowDialog();
            Settings.DisableBlurAndOverlay();
        }

        private void BtnResetPass_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault(); 
            var currentProfile = main.DataContext as ProfileDataModel;

            int currentId = currentProfile.ProfileId;
            string passwordInput;

            bool isPasswordCorrect = false;

            Settings.EnableBlurAndOverlay();
            while (!isPasswordCorrect)
            {
                InputBoxPopup currentpass = new InputBoxPopup("Reset Password", "Enter Your Current Password",
                                                                "Confirm", true, true);

                if (currentpass.ShowDialog() == true)
                {
                    passwordInput = currentpass.ResultText;
                    string currentHash = currentProfile.ProfileHash;

                    isPasswordCorrect = BCrypt.Net.BCrypt.Verify(passwordInput, currentHash);

                    if (!isPasswordCorrect)
                    {
                        PopupWindow popup = new PopupWindow("Password Incorrect",
                                                            $"The password you typed is incorrect",
                                                            "Ok", false);

                        popup.ShowDialog();
                    }
                    else
                    {
                        string _newPasswordInput = "";
                        string _hashResult = "";

                        NewPasswordPopup newpass = new NewPasswordPopup("Add New Password", "Enter Your New Password",
                                                                            "Confirm New Password", "Confirm", true, true);
                        if (newpass.ShowDialog() == true)
                        {
                            string query_np;

                            var args_np = new Dictionary<string, object>
                            {
                                ["@profile_id"] = currentId,
                                ["@hash"] = null
                            };

                            if (newpass.IsPasswordDisabled)
                            {
                                query_np = "UPDATE profiles SET hash = @hash WHERE id = @profile_id";
                            }
                            else
                            {
                                _newPasswordInput = newpass.ResultPass;
                                _hashResult = BCrypt.Net.BCrypt.HashPassword(_newPasswordInput);

                                query_np = "UPDATE profiles SET hash = @hash WHERE id = @profile_id";

                                args_np["@hash"] = _hashResult;
                            }
                            try
                            {
                                db.ExecuteScalar<int>(db.ConnectionGlobal, query_np, args_np);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error: " + ex.Message, "Reset Password Error", 
                                                MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            PopupWindow popup = new PopupWindow("Add New Password",
                                                                $"Your password has been changed successfully",
                                                                "Ok", false);

                            popup.ShowDialog();

                            ProfileSelectWindow profiles = new ProfileSelectWindow();
                            profiles.Show();
                            main.Close();
                            var settings = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
                            Settings.DisableBlurAndOverlay();
                            settings?.Close();
                        }
                        else
                        {
                            Settings.DisableBlurAndOverlay();
                            break;
                        }    
                    }
                }
                else
                {
                    Settings.DisableBlurAndOverlay();
                    break;
                }

            }
        }

        private void BtnAddPass_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            var currentProfile = main.DataContext as ProfileDataModel;

            string _newPasswordInput = "";
            string _hashResult = "";

            bool isPasswordAdded = false;

            Settings.EnableBlurAndOverlay();
            NewPasswordPopup newpass = new NewPasswordPopup("Add New Password", "Enter Your New Password",
                                                            "Confirm New Password", "Confirm", false, true);
            if (newpass.ShowDialog() == true)
            {
                string query_np;
                int currentId = currentProfile.ProfileId;

                var args_np = new Dictionary<string, object>
                {
                    ["@profile_id"] = currentId,
                    ["@hash"] = null
                };

                if (newpass.IsPasswordDisabled)
                {
                    query_np = "UPDATE profiles SET hash = @hash WHERE id = @profile_id";
                }
                else
                {
                    _newPasswordInput = newpass.ResultPass;
                    _hashResult = BCrypt.Net.BCrypt.HashPassword(_newPasswordInput);

                    query_np = "UPDATE profiles SET hash = @hash WHERE id = @profile_id";

                    args_np["@hash"] = _hashResult;
                }
                try
                {
                    db.ExecuteScalar<int>(db.ConnectionGlobal, query_np, args_np);
                    isPasswordAdded = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, " Add Password Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (isPasswordAdded)
            {
                PopupWindow popup = new PopupWindow("Add New Password",
                                                    $"The new password has been added successfully",
                                                    "Ok", false);

                popup.ShowDialog();

                ProfileSelectWindow profiles = new ProfileSelectWindow();
                profiles.Show();
                main.Close();
                var settings = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
                Settings.DisableBlurAndOverlay();
                settings?.Close();
            }
            else
            {
                Settings.DisableBlurAndOverlay();
                return;
            }
        }

        private void BtnExportBackup_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow as MainWindow;
            var currentProfile = main.DataContext as ProfileDataModel;

            string profileDataPath = System.IO.Path.Combine(func.dataPath, currentProfile.ProfileId.ToString());
            string backupPath = System.IO.Path.Combine(func.dataPath, "BACKUP", $"{currentProfile.ProfileId.ToString()}_{currentProfile.ProfileName}_bak.zip");

            if (File.Exists(backupPath)) File.Delete(backupPath);
            try
            {
                db.CloseAllConnections();

                ZipFile.CreateFromDirectory(profileDataPath, backupPath);
                PopupWindow popup = new PopupWindow("Profile Backup",
                                                    $"Profile successfully backed up to \"{backupPath}\"",
                                                    "Ok", false);

                popup.ShowDialog();
            } 
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Backup Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileDataModel profile = db.GetProfileData();

            if (profile.ProfileId >= 0)
            {
                PopupWindow popup = new PopupWindow("Confirm Profile Delete",
                                                    $"Are you sure you want to remove this profile? Removed profiles cannot be restored",
                                                    "Delete Profile", true);

                popup.ShowDialog();
                if (popup.DialogResult == true)
                {
                    DBHandler db = new DBHandler();
                    db.DeleteProfile(profile.ProfileId);

                    ProfileSelectWindow profiles = new ProfileSelectWindow();

                    PopupWindow popupDeleted = new PopupWindow("Delete Profile",
                                                               $"This profile has been deleted successfully",
                                                               "Ok", false);

                    popupDeleted.ShowDialog();

                    profiles.Show();
                    Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault().Close();
                    Application.Current.Windows.OfType<MainWindow>().FirstOrDefault().Close();
                }
                else
                {

                }
            }
        }

    }
}
