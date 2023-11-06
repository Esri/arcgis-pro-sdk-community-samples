/*

   Copyright 2023 Esri

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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace ProStartPageConfig.UI
{
  /// <summary>
  /// Business logic for the StartPage
  /// </summary>
  internal class ProStartPageSampleViewModel : PropertyChangedBase
  {
    private Dictionary<StartPageNavPages, UserControl> _pages =
      new Dictionary<StartPageNavPages, UserControl>();

    public ProStartPageSampleViewModel()
    {
      _pages[StartPageNavPages.Home] = new ProStartPageBody();
      _pages[StartPageNavPages.Resources] = new ProStartPageResources();
      this.ControlContent = _pages[StartPageNavPages.Home];
    }

    private ICommand _navigateCmd = null;
    public ICommand NavigationCommand
    {
      get
      {
        if (_navigateCmd == null)
        {
          _navigateCmd = new RelayCommand(
                           (param) => Navigate(param.ToString()), () => true, true);
        }
        return _navigateCmd;
      }
    }

    private System.Windows.FrameworkElement _controlContent = null;
    public System.Windows.FrameworkElement ControlContent
    {
      get { return _controlContent; }
      set
      {
        SetProperty(ref _controlContent, value, () => ControlContent);
      }
    }

    private StartPageNavPages _currentNavPage = StartPageNavPages.Home;

    public StartPageNavPages CurrentNavPage
    {
      get { return _currentNavPage; }
      set { SetProperty(ref _currentNavPage, value, () => CurrentNavPage); }
    }

    internal void Navigate(string navParam)
    {
      object result;
      Enum.TryParse(typeof(StartPageNavPages), navParam, out result);
      var navPage = (StartPageNavPages)result;

      if (CurrentNavPage != navPage)
      {
        if (navPage == StartPageNavPages.Settings)
        {
          //show backstage settings
          FrameworkApplication.OpenBackstage("esri_core_aboutTab");
          return;
        }
        CurrentNavPage = navPage;//for the toggle button
        this.ControlContent = _pages[navPage];
      }
    }     
  }
}
