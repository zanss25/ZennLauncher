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
            // Backdrop Effects
            if (((App)Application.Current).CurrentProfileCfg.Interface.DialogOverlay == ProfileInterfaceSettings.DialogOverlayModes.DimWithBlur)
            {
                ComboDialogOverlay.SelectedIndex = 0;
            }
            else if (((App)Application.Current).CurrentProfileCfg.Interface.DialogOverlay == ProfileInterfaceSettings.DialogOverlayModes.DimOnly)
            {
                ComboDialogOverlay.SelectedIndex = 1;
            }
            else
            {
                ComboDialogOverlay.SelectedIndex = 2;
            }
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

    }
}
