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
    /// Interaction logic for InputBoxPopup.xaml
    /// </summary>
    public partial class InputBoxPopup : Window
    {
        public string ResultText { get; private set; }
        private readonly bool _isPasswordMode;

        public InputBoxPopup(string title = "InputBoxPopup", string subtext = "InputTextBox", 
                             string btn = "Ok", bool isPassword = false, bool enableBtnClose = true)
        {
            InitializeComponent();

            Title = title;
            TitleTextBlock.Text = Title.ToUpper();
            InputTextBlock.Text = subtext.ToUpper();
            BtnOk.Content = btn;
            _isPasswordMode = isPassword;

            if (isPassword)
            {
                InputTextBox.Visibility = Visibility.Collapsed;
                InputPassBox.Visibility = Visibility.Visible;
                InputPassBox.Focus();
            }
            else
            {
                InputTextBox.Visibility = Visibility.Visible;
                InputPassBox.Visibility = Visibility.Collapsed;
                InputTextBox.Focus();
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
            ResultText = _isPasswordMode ? InputPassBox.Password : InputTextBox.Text;

            if (_isPasswordMode && String.IsNullOrEmpty(ResultText))
            {
                PopupWindow popup = new PopupWindow("Password Empty",
                                                    $"The password cannot be blank",
                                                    "Ok", false);

                popup.ShowDialog();
                return;
            }

            if (!_isPasswordMode && String.IsNullOrEmpty(ResultText))
            {
                PopupWindow popup = new PopupWindow("Input Empty",
                                                    $"The text input cannot be blank",
                                                    "Ok", false);

                popup.ShowDialog();
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
