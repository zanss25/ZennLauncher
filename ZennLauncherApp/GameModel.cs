using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public class GameModel : IAppItem
    {
        public int GameId { get; set; }
        public string GameTitle { get; set; }
        public string GameSubtitle { get; set; }
        public int GameCategoryId { get; set; }
        public string GameCategoryName { get; set; }
        public int GameGenreId { get; set; }
        public int GameUserRating { get; set; }

        public string GameFirstLaunched { get; set; }
        public string GameLastLaunched { get; set; }
        public int GameLaunchTimes { get; set; }
        public string GameTarget { get; set; }
        public string GameLaunchOptions { get; set; }
        public string GameIconPath { get; set; }
        public string GameCoverPath { get; set; }

        // -- Interface Mapping --
        public int AppId => GameId;
        public string AppIconPath => GameIconPath;
        public string AppTitle => GameTitle;
        public string AppDesc => GameSubtitle;
        public int AppCategoryId => GameCategoryId;
        public string AppCategoryName => GameCategoryName;
        public string AppLastLaunched => GameLastLaunched;
        public string AppTarget => GameTarget;
        public string AppLaunchOptions => GameLaunchOptions;
        public string TargetTable => "games";
    }

}
