using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public class SettingsModel
    {
        public int OptionId { get; set; }
        public string OptionName { get; set; }
        public string OptionDisplay { get; set; }
        public string OptionDesc { get; set; }
        public string OptionValue { get; set; }
    }
}