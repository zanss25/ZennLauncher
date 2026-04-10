using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public partial class ProfileConfigModel
    {
        public ProfileGeneralSettings General { get; set; } = new ProfileGeneralSettings();
    }

    public partial class ProfileGeneralSettings
    {
        public string StartingTab { get; set; } = "games";
        public string GamesDefaultSortOption { get; set; } = "title";

        public string ToolsDefaultSortOption { get; set; } = "title";
        public enum LaunchAction
        {
            HideLauncher,
            CloseLauncher,
            DoNothing
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LaunchAction GamesLaunchAction { get; set; } = LaunchAction.HideLauncher;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LaunchAction ToolsLaunchAction { get; set; } = LaunchAction.DoNothing;

        public int StartOnWindows { get; set; } = 0;

    }
}
