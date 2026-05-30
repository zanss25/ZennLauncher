using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZennLauncherApp
{
    public interface IAppItem
    {
        int AppId { get; }
        string AppIconPath { get; }
        string AppTitle { get; }
        string AppDesc { get; }
        int AppCategoryId { get; }
        string AppCategoryName { get; }
        string AppLastLaunched { get; }
        string AppTarget {  get; }
        string AppLaunchOptions { get; }
        string TargetTable { get; }
    }
}
