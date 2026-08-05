using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ZennLauncherApp
{
    /// <summary>
    /// Interaction logic for InterfaceSettings.xaml
    /// </summary>
    public partial class InterfaceSettingsView : UserControl, ISettingsTab
    {
        private readonly GlobalFunctions func = GlobalFunctions.Instance;
        private readonly DBHandler db = DBHandler.Instance;
        private readonly ConfigHandler cfg = ConfigHandler.Instance;

        public Action OnSettingChanged { get; set; }

        public InterfaceSettingsView()
        {
            InitializeComponent();
            LoadCfgToUI();
        }

        private void LoadCfgToUI()
        {
            var app = (App)Application.Current;

            // Backdrop Effects
            if (app.CurrentProfileCfg.Interface.DialogOverlay == ProfileInterfaceSettings.DialogOverlayModes.DimWithBlur)
            {
                ComboDialogOverlay.SelectedIndex = 0;
            }
            else if (app.CurrentProfileCfg.Interface.DialogOverlay == ProfileInterfaceSettings.DialogOverlayModes.DimOnly)
            {
                ComboDialogOverlay.SelectedIndex = 1;
            }
            else
            {
                ComboDialogOverlay.SelectedIndex = 2;
            }

            // Blur Intensity
            InputBlurIntensity.Text = Convert.ToString(app.CurrentProfileCfg.Interface.BlurIntensity);

            // Dim Color
            InputDimColor.Text = app.CurrentProfileCfg.Interface.DimColor;

            // Dim Intensity
            InputDimIntensity.Text = Convert.ToString(app.CurrentProfileCfg.Interface.DimIntensity);
        }

        public void ComboDialogOverlay_Changed(object sender, EventArgs e)
        {
            var selectedItem = ComboDialogOverlay.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
            {
                string selectedOption = Convert.ToString(selectedItem.Tag);

                var action = (ProfileInterfaceSettings.DialogOverlayModes)Enum.Parse(typeof(ProfileInterfaceSettings.DialogOverlayModes), selectedOption);
                ((App)Application.Current).CurrentProfileCfg.Interface.DialogOverlay = action;

                Debug.WriteLine("DialogOverlay: " +
                                ((App)Application.Current).CurrentProfileCfg.Interface.DialogOverlay);
                OnSettingChanged?.Invoke();
            }
        }

        private void InputBlurIntensity_LostFocus(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            int IntBlurIntensity = Convert.ToInt32(InputBlurIntensity.Text);
            
            if (IntBlurIntensity < 0)
            {
                IntBlurIntensity = 0;
                InputBlurIntensity.Text = Convert.ToString(IntBlurIntensity);
            }
            else if (IntBlurIntensity > 100)
            {
                IntBlurIntensity = 100;
                InputBlurIntensity.Text = Convert.ToString(IntBlurIntensity);
            }

            ((App)Application.Current).CurrentProfileCfg.Interface.BlurIntensity = IntBlurIntensity;

            OnSettingChanged?.Invoke();
        }

        private void InputDimColor_LostFocus(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            string input = InputDimColor.Text;
            string DimColor = "000000";
            if (input.Length == 6 && input.All(c => "0123456789abcdefABCDEF".Contains(c)))
            {
                DimColor = input;
            }
            else
            {
                InputDimColor.Text = app.CurrentProfileCfg.Interface.DimColor;
            }

            app.CurrentProfileCfg.Interface.DimColor = DimColor;

            OnSettingChanged?.Invoke();
        }

        private void InputDimIntensity_LostFocus(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            int IntDimIntensity = Convert.ToInt32(InputDimIntensity.Text);

            if (IntDimIntensity < 0)
            {
                IntDimIntensity = 0;
                InputDimIntensity.Text = Convert.ToString(IntDimIntensity);
            }
            else if (IntDimIntensity > 100)
            {
                IntDimIntensity = 100;
                InputDimIntensity.Text = Convert.ToString(IntDimIntensity);
            }

            app.CurrentProfileCfg.Interface.DimIntensity = IntDimIntensity;

            OnSettingChanged?.Invoke();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void NumberPasteValidationTextBox(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = e.DataObject.GetData(typeof(string)) as string;

                if (!text.All(char.IsDigit))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

    }
}
