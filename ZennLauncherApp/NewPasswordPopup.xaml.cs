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

            this.Title = title;
            TitleTextBlock.Text = this.Title.ToUpper();
            PassTextBlock.Text = passText.ToUpper();
            ConfirmTextBlock.Text = confirmText.ToUpper();
            BtnOk.Content = btn;

            InputPassBox.Focus();

            if (!enableCheckboxDontUsePass)
            {
                CheckBoxDontUsePass.Visibility = Visibility.Collapsed;
                this.Height = 300;
            }

            if (!enableBtnClose)
            {
                BtnClose.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxDontUsePass.IsChecked == true)
            {
                IsPasswordDisabled = true;
                this.DialogResult = true;
            }
            else
            {
                ResultPass = InputPassBox.Password;
                ResultConfirm = InputConfirmBox.Password;

                if (String.IsNullOrEmpty(ResultPass))
                {
                    MessageBox.Show("The password cannot be empty");
                    return;
                }

                if (String.IsNullOrEmpty(ResultConfirm))
                {
                    MessageBox.Show("Please retype your password");
                    return;
                }

                if (ResultConfirm == ResultPass)
                {
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Password didn't match");
                    return;
                }
            }

        }
    }
}
