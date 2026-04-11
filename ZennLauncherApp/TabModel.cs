using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public class TabModel
    {
        public string ItemTag { get; set; } = String.Empty;
        public string DisplayName { get; set; } = String.Empty;
        public Type TargetView { get; set; }
    }
}
