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
using System.Windows.Shapes;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for NewPasswordPopup.xaml
    /// </summary>
    public partial class NewPasswordPopup : Window
    {
        public string ResultPass { get; private set; }
        public string ResultConfirm { get; private set; }
        public bool IsPasswordDisabled { get; set; } = false;

        public NewPasswordPopup(string title = "Add New Password", string passText = "Password",
                                string confirmText = "Confirm Password", string btn = "Confirm", 
                                bool enableCheckboxDontUsePass = true, bool enableBtnClose = true)
        {
            InitializeComponent();

            Title = title;
            TitleTextBlock.Text = Title.ToUpper();
            PassTextBlock.Text = passText.ToUpper();
            ConfirmTextBlock.Text = confirmText.ToUpper();
            BtnOk.Content = btn;

            InputPassBox.Focus();

            if (!enableCheckboxDontUsePass)
            {
                CheckBoxDontUsePass.Visibility = Visibility.Collapsed;
                Height = 300;
            }

            if (!enableBtnClose)
            {
                BtnClose.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxDontUsePass.IsChecked == true)
            {
                IsPasswordDisabled = true;
                DialogResult = true;
            }
            else
            {
                ResultPass = InputPassBox.Password;
                ResultConfirm = InputConfirmBox.Password;

                if (String.IsNullOrEmpty(ResultPass))
                {
                    PopupWindow popup = new PopupWindow("Password Empty",
                                                        $"The password cannot be blank",
                                                        "Ok", false);
                    popup.ShowDialog();
                    return;
                }

                if (String.IsNullOrEmpty(ResultConfirm))
                {
                    PopupWindow popup = new PopupWindow("Confirm Password",
                                                        $"Please retype your new password",
                                                        "Ok", false);
                    popup.ShowDialog();
                    return;
                }

                if (ResultConfirm == ResultPass)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    PopupWindow popup = new PopupWindow("Passwords Don't Match",
                                                        $"The passwords don't match. Please check your password confirmation",
                                                        "Ok", false);
                    popup.ShowDialog();
                    return;
                }
            }
        }

    }
}
