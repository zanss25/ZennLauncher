using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public class GameModel
    {
        public int GameId { get; set; }
        public string GameTitle { get; set; }
        public string GameSubtitle { get; set; }
        public string GameFirstLaunched { get; set; }
        public string GameLastLaunched { get; set; }
        public int GameLaunchTimes { get; set; }
        public string GameTarget { get; set; }
        public string GameLaunchOptions { get; set; }
        public string GameIconPath { get; set; }
        public string GameCoverPath { get; set; }
    }
}
