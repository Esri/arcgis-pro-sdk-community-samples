/*

   Copyright 2018 Esri

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

namespace Overlay3D
{
  /// <summary>
  /// This sample shows how to add overlay graphics to 3D map view.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains required data for this sample add-in.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Open the "C:\Data\Interacting with Maps\Interacting with Maps.aprx" project which contains the required data needed for this sample.
  /// 1. From the "Catalog dockpane" open the "Portland 3D City" map.
  /// ![UI](Screenshots/Screen0.png) 
  /// 1. Click on the Add-In tab on the ribbon and open the "Overlay 3D Dockpane" using the "Show Overlay 3D Dockpane" button.
  /// 1. On the 'Overlay 3D Dockpane' click the refresh button, this will update the 'Trees Name field selection" list.
  /// 1. Select one of the "Tree Names" (i.e. Ponderosa Pine) in order to add these records to the graphic overlay.
  /// ![UI](Screenshots/Screen01.png) 
  /// 1. Once a "Tree Name" is selected the "Symbol for Overlay Graphic" list is updated using one of two '3D symbol style' categories.
  /// 1. Uncheck the 'Trees' layer on the content dockpane to better view the added overlay graphics.
  /// 1. Select a "Symbol for 'overlay graphics'" and observe how the symbols are added to the mapview.
  /// ![UI](Screenshots/Screen02.png) 
  /// 1. Add more symbols to the mapview and optionally 'Clear the graphics overlay'/
  /// ![UI](Screenshots/Screen1.png)  
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Overlay3D_Module"));
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
