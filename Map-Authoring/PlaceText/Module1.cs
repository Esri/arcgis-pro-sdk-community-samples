/*

   Copyright 2018 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
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
using ArcGIS.Desktop.Mapping;

namespace PlaceText
{
  /// <summary>
  /// This sample shows how to place point markers with text inside the point marker.
  /// </summary>
  /// <remarks>
  /// 1. In Visual studio rebuild the solution.
  /// 1. Debug the add-in by clicking the "Start" button.
  /// 1. ArcGIS Pro opens, select any project with a map.
  /// 1. Open the Add-in Tab and click on the "Place Text Tool" button to active the "Place Text" Map tool.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Click on the map to place a point marker with text inside.
  /// 1. The sample code has been hardcoded to a circle (index 40) but developers can change the index to experiment with different shapes.
  /// 1. Click the 'Clear' button to remove all placed 'point marker' text.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;
    private List<IDisposable> _graphics = new List<IDisposable>();

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("PlaceText_Module"));
      }
    }

    public List<IDisposable> Graphics => _graphics;

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
