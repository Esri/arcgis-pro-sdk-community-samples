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

namespace EditingTemplates
{
  /// <summary>
  /// This sample demonstrates creating and modifying editing templates largely using the Cartographic Information Model (CIM). 
  /// In this example we will create and modify templates including group templates.  
  /// We will also show how to retrieve and use templates to create features; overriding default attribute values. 
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open.  
  /// 1. Open the Interacting with Map project.
  /// 1. Open the Create Features dockpane. 
  /// 1. Click on the ADD-IN TAB.  
  /// 1. Click on the *Create Template with CIM* button.   
  /// 1. A new template 'My CIM Template' will be created.
  /// ![UI](screenshots/Templates_NewCIMTemplate.png) 
  /// 1. Click on the *Create Template with Extension* button.   
  /// 1. A new template 'My extension Template' will be created.
  /// ![UI](screenshots/Templates_NewExtensionTemplate.png) 
  /// 1. Click on the *Modify Template with CIM* button.   
  /// 1. Activate the 'North Precinct' template and see the default construction tool is now the Right Angle Polygon tool.
  /// 1. Click on the *Create Features* button.   
  /// 1. See 3 new Fire Station features created all with different City attribute values.
  /// 1. Click on the *Create Group Template with CIM* button.   
  /// 1. A new template 'My Group Template' will be created.
  /// ![UI](screenshots/Templates_GroupTemplate.png) 
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("EditingTemplates_Module"));
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
