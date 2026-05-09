using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public class ToolModel : IAppItem
    {
        public int ToolId { get; set; }
        public string ToolTitle { get; set; }
        public string ToolDesc { get; set; }
        public int ToolCategoryId { get; set; }
        public string ToolCategoryName { get; set; }
        public int ToolUserRating { get; set; }
        public string ToolFirstLaunched { get; set; }
        public string ToolLastLaunched { get; set; }
        public int ToolLaunchTimes { get; set; }
        public string ToolTarget { get; set; }
        public string ToolLaunchOptions { get; set; }
        public string ToolIconPath { get; set; }
        public string ToolCoverPath { get; set; }

        // -- Interface Mapping --
        public int AppId => ToolId;
        public string AppIconPath => ToolIconPath;
        public string AppTitle => ToolTitle;
        public int AppCategoryId => ToolCategoryId;
        public string AppCategoryName => ToolCategoryName;
        public string AppLastLaunched => ToolLastLaunched;
        public string AppTarget => ToolTarget;
        public string AppLaunchOptions => ToolLaunchOptions;
        public string TargetTable => "tools";
    }
}
