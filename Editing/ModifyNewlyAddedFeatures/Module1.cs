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

namespace ModifyNewlyAddedFeatures
{
  /// <summary>
  /// This sample shows how to:
  /// * Update shape and attribute columns for a newly created or modified polygon feature
  /// * Create a logging/tracking entry for each change in a second feature class
  /// The workflow implemented in this sample examines every newly created or modified polygon and assure that it doesn't overlap any existing polygons in the same feature layer.  If the new polygon geometry overlaps any existing polygons, the geometry is corrected so that it doesn't overlap and a 'Description' attribute column is updated to show the action taken on the geometry.
  /// Furthermore to demonstrate a sample of change tracking or logging, the center point of the polygon is taken and in addition with a 'Description' attribute stored in a separate point feature class.
  /// The corresponding ProConcept document can be found here: [ProConcepts Editing / Row Events](https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Editing#row-events)
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data 
  /// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select the FeatureTest.aprx project
  /// 1. Click on the 'Post Edit Mods' tab and click the 'Show Modify Monitor' button to show the 'Modify New Added Row Monitor' dockpane  
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. On the 'Modify New Added Row Monitor' dockpane check the 'Enable Modification' checkbox to enable the polygon geometry correction feature   
  /// ![UI](Screenshots/Screenshot2.png)
  /// 1. Now the click on the 'Create Features' button and select the first 'polygon creation template' for the 'TestPolygons' layer  
  /// ![UI](Screenshots/Screenshot3.png)
  /// 1. Digitize a new polygon that partially overlaps an existing polygon
  /// ![UI](Screenshots/Screenshot4.png)
  /// 1. After you complete your new polygon sketch notice that your polygon's shape was adjusted and a new point at the center of your polygon was added to the map as well.  The 'Modify Monitor' dockpane shows the actions taken on the newly created feature.  Add a second overlapping polygon.    
  /// ![UI](Screenshots/Screenshot5.png)
  /// 1. Complete the second new polygon's sketch.
  /// ![UI](Screenshots/Screenshot6.png)
  /// 1. Note the corrected polygon shape, the 'Modify Monitor' dockpane status, and the newly created TestPoints features.  
  /// 1. Open the TestPoints and TestPolygons attribute tables and verify the updates to the 'Description' column values.
  /// ![UI](Screenshots/Screenshot7.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ModifyNewlyAddedFeatures_Module"));
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
