using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static ZennLauncherApp.ProfileGeneralSettings;

namespace ZennLauncherApp
{
    public partial class ProfileConfigModel
    {
        public ProfileGeneralSettings General { get; set; } = new ProfileGeneralSettings();
        public ProfileMyProfileSettings MyProfile { get; set; } = new ProfileMyProfileSettings();
        public ProfileInterfaceSettings Interface { get; set; } = new ProfileInterfaceSettings();
        public ProfileFoldersSettings Folders { get; set; } = new ProfileFoldersSettings();
    }

    public partial class ProfileGeneralSettings
    {
        public string StartingTab { get; set; } = "games";
        public string GamesDefaultSortOption { get; set; } = "title";
        public string ToolsDefaultSortOption { get; set; } = "title";
        public enum LaunchActions
        {
            HideLauncher,
            CloseLauncher,
            DoNothing
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LaunchActions GamesLaunchAction { get; set; } = LaunchActions.HideLauncher;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LaunchActions ToolsLaunchAction { get; set; } = LaunchActions.DoNothing;

        public bool CloseToTray { get; set; } = true;
        public string IgnoreLaunchActionsModifier { get; set; } = null;

    }

    public partial class ProfileMyProfileSettings
    {
        public int AutoBackupFrequency { get; set; } = 0;   // in days, '0' means disabled
    }

    public partial class ProfileInterfaceSettings
    {
        public enum DialogOverlayModes
        {
            DimWithBlur,
            DimOnly,
            None
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DialogOverlayModes DialogOverlay { get; set; } = DialogOverlayModes.DimWithBlur;

        public int BlurIntensity { get; set; } = 0;         // 0-100 percent
        public string DimColor { get; set; } = "000000";    // in hex color
        public int DimIntensity { get; set; } = 0;          // 0-100 percent
    }

    public partial class ProfileFoldersSettings
    {
        public string DefaultGamesDir { get; set; } = String.Empty;
        public string DefaultToolsDir { get; set; } = String.Empty;
        public string DefaultIconsDir { get; set; } = String.Empty;
        public string DefaultCoversDir { get; set; } = String.Empty;
    }

}
