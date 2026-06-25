using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public class GlobalConfigModel
    {
        public int Build { get; set; } = 10401;
        public string Version { get; set; } = "0.4.0";
        public int ActiveProfile { get; set; } = -1;
        public string SessionToken { get; set; } = "";
        public string DefaultLanguage { get; set; } = "English";

    }
}
