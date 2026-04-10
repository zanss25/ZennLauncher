using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public class ProfileDataModel
    {
        public int ProfileId { get; set; } = 0;
        public string ProfileName { get; set; } = "";
        public string ProfileHash { get; set; } = "";
        public string ProfileFirstActive { get; set; } = "";
        public int ProfileKeepLoggedIn { get; set; } = 0;
        public string ProfileAvatarPath { get; set; } = "";
    }
}
