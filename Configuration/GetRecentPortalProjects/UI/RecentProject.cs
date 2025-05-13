using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetRecentPortalProjects.UI
{
  internal class RecentProject : PropertyChangedBase
  {
    private string _path;
    private string _name;
    public string Path
    {
      get => _path;
      set
      {
        SetProperty(ref _path, value, () => Path);
      }
    }
    public string Name
    {
      get => _name;
      set
      {
        SetProperty(ref _name, value, () => Name);
      }
    }
  }
}
