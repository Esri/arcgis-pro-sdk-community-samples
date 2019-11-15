/*

   Copyright 2019 Esri

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

namespace Inspector_AddAttributeAsync
{
  /// <summary>
  /// This sample demonstrates how to use the Inspector.AddAttributeAsync method. The sample contains a single map tool which displays a grid requesting attribute values
  /// from the user. After entering values into the grid, the user sketches a rectangle to identify features.  The attributes will be applied to the features that are 
  /// identified by the tool.
  /// 
  /// In this example we are using Inspector.AddAttributeAsync to only add a few fields from a single specific layer.  It is possible to add fields from multiple layers to 
  /// display in the grid. 
  /// </summary>
  /// 
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in C:\Data
  /// 1. Before you run the sample verify that the project "C:\Data\Interacting with Maps\Interacting with Maps.aprx" is present since this is required to run the sample.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open.
  /// 1. Open the "C:\Data\Interacting with Maps\Interacting with Maps" project.
  /// 1. Click on the ADD-IN TAB.
  /// ![UI](Screenshots/ribbon.png)
  /// 1. Click on the "Apply Attrributes" button.
  /// 1. A dockpane will be displayed showing the fields requiring user input. In this example we have chosen 2 fields from the Police Stations layer. 
  /// Each field is shown as invalid (outlined in a red box) indicating that validation of the values has occurred.
  /// ![UI](Screenshots/ApplyAttributes_1.png)
  /// 1. Enter a value into the first field.  The value is validated. A valid value is outlined with a green box.  The user can hover over the side bar of any invalid value to see any message you wish to display.
  /// ![UI](Screenshots/ApplyAttributes_2.png)
  /// 1. Enter a value into the second field.  The value is validated.  
  /// ![UI](Screenshots/ApplyAttributes_3.png)
  /// 1. Now draw a rectangle on the map around one of more Police Station features. 
  /// 1. The values entered will be applied to the identified features. 
  /// ![UI](Screenshots/ApplyAttributes_Results.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Inspector_AddAttributeAsync_Module"));
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
