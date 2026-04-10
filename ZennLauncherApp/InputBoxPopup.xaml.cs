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
        private bool _isPasswordMode;

        public InputBoxPopup(string title = "InputBoxPopup", string subtext = "InputTextBox", 
                             string btn = "Ok", bool isPassword = false, bool enableBtnClose = true)
        {
            InitializeComponent();

            this.Title = title;
            TitleTextBlock.Text = this.Title.ToUpper();
            InputTextBlock.Text = subtext;
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
            this.DialogResult = false;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            ResultText = _isPasswordMode ? InputPassBox.Password : InputTextBox.Text;

            if (_isPasswordMode && String.IsNullOrEmpty(ResultText))
            {
                MessageBox.Show("The password cannot be empty");
                return;
            }

            if (!_isPasswordMode && String.IsNullOrEmpty(ResultText))
            {
                MessageBox.Show("The text input cannot be empty");
                return;
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}
