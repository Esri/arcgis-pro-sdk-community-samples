/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace GeocodingTools
{
    /// <summary>
    /// This sample demonstrates adding geocoding functionalities to your application.  3 different methods of geocoding are presented.
    /// - simple geocoding using an API method and no custom UI
    /// - using the LocatorControl on a dockpane
    /// - more advanced geocoding using API methods with a custom UI providing search capabilities and viewing of results. 
    /// <para></para>
    /// The LocatorControl provides similar functionality to that of the Locate dockpane. 
    /// The geocoding API methods allow you tighter control over how to display geocoding results within a UI and on the map. 
    /// You can also use methods within the ArcGIS.Desktop.Mapping.Geocoding.LocatorManager to add, remove, enable, reorder geocoding locators. 
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Launch the debugger to open ArcGIS Pro.
    /// 1. Open any project.
    /// 1. Click on the Add-In Tab.
    /// 1. Click on the *Simple Geocode* button. 
    /// 1. A Messagebox will be displayed with the geocode results. 
    /// ![UI](screenshots/SimpleGeocode.png)  
    /// ![UI](screenshots/SimpleGeocodeResults.png)  
    /// 1. Click on the *Show Geocode Dockpane* button. 
    /// 1. The Geocode dock pane will open up.  Enter a location and see the results display in the dockpane as well as on the map.
    /// ![UI](screenshots/LocatorControl.png)  
    /// ![UI](screenshots/LocatorControlResults.png)  
    /// 1. Click on the *Show Custom Geocode Dockpane* button. 
    /// 1. The Custom Geocode dock pane will open up.  
    /// 1. Enter a location and see the results display in the dockpane.
    /// 1. Highlight a result and see the map zoom and a symbol be added to the map at the result location. 
    /// ![UI](screenshots/Geocode_CustomUI.png)  
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("GeocodingTools_Module"));
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
