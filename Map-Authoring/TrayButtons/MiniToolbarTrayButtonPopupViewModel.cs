/*

   Copyright 2022 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TrayButtons
{
  internal class MiniToolbarTrayButtonPopupViewModel : PropertyChangedBase
  {
    /// <summary>
    /// Text shown near the top Map Tray UI.
    /// </summary>
    private string _heading = "MapTray";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    private bool _isChecked;
    public bool IsChecked
    {
      get => _isChecked;
      set => SetProperty(ref _isChecked, value);
    }

    public bool IsToolbarLeft
    {
      get => ApplicationOptions.EditingOptions.ToolbarPosition.Equals(ToolbarPosition.Left);
      set
      {
        if (value != IsToolbarLeft)
          ApplicationOptions.EditingOptions.ToolbarPosition = ToolbarPosition.Left;
      }
    }
    public bool IsToolbarBottom
    {
      get => ApplicationOptions.EditingOptions.ToolbarPosition.Equals(ToolbarPosition.Bottom);
      set
      {
        if (value != IsToolbarBottom)
          ApplicationOptions.EditingOptions.ToolbarPosition = ToolbarPosition.Bottom;
      }
    }
    public bool IsToolbarRight
    {
      get => ApplicationOptions.EditingOptions.ToolbarPosition.Equals(ToolbarPosition.Right);
      set
      {
        if (value != IsToolbarRight)
          ApplicationOptions.EditingOptions.ToolbarPosition = ToolbarPosition.Right;
      }
    }
    public bool IsToolbarSmall
    {
      get => ApplicationOptions.EditingOptions.ToolbarSize.Equals(ToolbarSize.Small);
      set
      {
        if (value != IsToolbarSmall)
          ApplicationOptions.EditingOptions.ToolbarSize = ToolbarSize.Small;
      }
    }
    public bool IsToolbarMedium
    {
      get => ApplicationOptions.EditingOptions.ToolbarSize.Equals(ToolbarSize.Medium);
      set
      {
        if (value != IsToolbarMedium)
          ApplicationOptions.EditingOptions.ToolbarSize = ToolbarSize.Medium;
      }
    }
    public bool IsToolbarLarge
    {
      get => ApplicationOptions.EditingOptions.ToolbarSize.Equals(ToolbarSize.Large);
      set
      {
        if (value != IsToolbarLarge)
          ApplicationOptions.EditingOptions.ToolbarSize = ToolbarSize.Large;
      }
    }

    public bool MagnificationOn
    {
      get => ApplicationOptions.EditingOptions.MagnifyToolbar;
      set
      {
        if (value != MagnificationOn)
        {
          ApplicationOptions.EditingOptions.MagnifyToolbar = value;
        }
      }
    }
    internal void ShowCorrectStates()
    {
      NotifyPropertyChanged(nameof(IsToolbarLeft));
      NotifyPropertyChanged(nameof(IsToolbarBottom));
      NotifyPropertyChanged(nameof(IsToolbarRight));
      NotifyPropertyChanged(nameof(IsToolbarSmall));
      NotifyPropertyChanged(nameof(IsToolbarMedium));
      NotifyPropertyChanged(nameof(IsToolbarLarge));
      NotifyPropertyChanged(nameof(MagnificationOn));
    }
  }
}
