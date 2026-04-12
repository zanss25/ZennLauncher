using System.Windows;

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        public PopupWindow(string title = "PopupWindow", string message = "PopupWindow: DefaultMessage",
                             string btn = "Ok", bool enableBtnClose = true)
        {
            InitializeComponent();

            Title = title;
            TitleTextBlock.Text = Title.ToUpper();
            MessageTextBlock.Text = message;
            BtnOk.Content = btn;

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
            DialogResult = true;
            Close();
        }
    }
}
