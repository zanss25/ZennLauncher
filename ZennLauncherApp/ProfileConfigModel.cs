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

        public bool StartOnWindows { get; set; } = false;

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

    }

    public partial class ProfileFoldersSettings
    {
        public string DefaultGamesDir { get; set; } = String.Empty;
        public string DefaultToolsDir { get; set; } = String.Empty;
        public string DefaultIconsDir { get; set; } = String.Empty;
        public string DefaultCoversDir { get; set; } = String.Empty;
    }

}
