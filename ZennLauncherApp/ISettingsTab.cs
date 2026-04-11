using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public interface ISettingsTab
    {
        Action OnSettingChanged { get; set; }
    }
}
