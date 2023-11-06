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
using ProStartPageConfig.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ProStartPageConfig
{

  public enum StartPageNavPages
  {
    Home = 0,
    Resources,
    Settings
  }

  /// <summary>
  /// Provides an example Pro start page
  /// Shows how to replicate the Pro 3.0 start page. 
  /// This may be useful for Configurations which want to show a "stock" or "near stock" start page with a few (custom) modifications.
  /// Configurations would be responsible for making their own resources page.
  /// A placeholder is provided in <b>ProStartPageResources.xaml</b>
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to debug ArcGIS Pro.
  /// 1. ArcGIS Pro displays the custom splash screen.
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Pro will then show a "stock" or "near stock" start page with a few (custom) modifications
  /// ![UI](Screenshots/Screenshot2.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;
    private ProStartPageSampleViewModel _vm;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ProStartPageConfig_Module");

    public ProStartPageSampleViewModel ProStartPageViewModel
    {
      get
      {
        if (_vm == null)
          _vm = new ProStartPageSampleViewModel();
        return _vm;
      }
    }

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

  }
}