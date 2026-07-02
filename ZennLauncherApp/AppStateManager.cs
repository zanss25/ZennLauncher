using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public class AppStateManager
    {
        private static AppStateManager _instance;
        
        private static readonly Lazy<AppStateManager> _lazyInstance =
            new Lazy<AppStateManager>(() => new AppStateManager());
        public static AppStateManager Instance => _lazyInstance.Value;

        // State values:
        public bool isLoggedIn = true;
        public bool enableTrayHide;

        public ObservableCollection<GameModel> AllGamesList { get; private set; } = new ObservableCollection<GameModel>();
        public ObservableCollection<ToolModel> AllToolsList { get; private set; } = new ObservableCollection<ToolModel>();

        private DBHandler db = new DBHandler(); 

        private AppStateManager() { }

        public void LoadAllData()
        {
            var games = db.GetAllGames();
            AllGamesList = new ObservableCollection<GameModel>(games);

            var tools = db.GetAllTools();
            AllToolsList = new ObservableCollection<ToolModel>(tools);
        }

        public void UpdateAppData(IAppItem launchedApp, int sectionId)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (launchedApp is GameModel game)
            {
                if (string.IsNullOrEmpty(game.GameFirstLaunched))
                {
                    game.GameFirstLaunched = now;
                }

                game.GameLastLaunched = now;
                game.GameLaunchTimes++;
            }
            else if (launchedApp is ToolModel tool)
            {
                if (string.IsNullOrEmpty(tool.ToolFirstLaunched))
                {
                    tool.ToolFirstLaunched = now;
                }

                tool.ToolLastLaunched = now;
                tool.ToolLaunchTimes++;
            }

            DBHandler.Instance.UpdateAppAtLaunch(launchedApp, sectionId);
        }

        public Action HideTrayAction { get; set; }

        public void TriggerLogout()
        {
            isLoggedIn = false;
            HideTrayAction?.Invoke();
            ResetAllData();
        }

        public void TriggerLogin()
        {
            isLoggedIn = true;
        }

        public void ResetAllData()
        {
            AllGamesList.Clear();
            AllToolsList.Clear();
        }

    }
}
