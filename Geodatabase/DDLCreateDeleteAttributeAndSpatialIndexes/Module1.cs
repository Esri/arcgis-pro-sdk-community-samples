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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DDLCreateDeleteAttributeAndSpatialIndexes
{
  /// <summary>
  /// This sample shows how to use the DDL APIs to generate and delete Spatial Indexes and Attribute Indexes. 
  /// </summary>
  /// <remarks>
  /// Using the sample:
  /// 1. Open this solution in Visual Studio.  
  /// 1. Click the build menu and select Build Solution.  
  /// 1. Click the Start button to open ArCGIS Pro. ArcGIS Pro will open.    
  /// 1. Open any project.
  /// 1. Click on the Add-in tab and verify that a "Feature Indexing" group was added.
  /// 1. Notice the buttons in the FeatureIndexing group.
  /// 1. Tap the "Create Feature With Index" button.
  /// ![UI](Screenshots/Screen0.png)
  /// 1. Add the new Database into the Catalog pane.
  /// ![UI](Screenshots/Screen1.png)  
  /// 1. Expand the newly added database.
  /// 1. Right click on the "Buildings" Feature class.
  /// 1. Open the table
  /// ![UI](Screenshots/Screen2.png)  
  /// 1. Notice the "*" denotes indexing which can be seen on the name and address name fields.
  /// ![UI](Screenshots/Screen3.png)
  /// 1. Tap the "Add Indexs To An Existing Data Set" button.
  /// ![UI](Screenshots/Screen4.png)  
  /// 1. Refresh the database
  /// 1. Right click on the "Building" Feature class.
  /// 1. Open the table.
  /// ![UI](Screenshots/Screen2.png) 
  /// 1. Notice the "*" can be seen on buildingUsage and buildingColor name fields.
  /// ![UI] (Screenshots/Screen5.png)  
  /// 1. Notice when hovering over a field name it displays information regarding whether or not a field is indexed.
  /// ![UI](Screenshots/Screen6.png)
  /// 1. In order to delete the SpatialIndex Tap the "Remove Spatial Index" button.
  /// ![UI](Screenshots/Screen8.png)
  /// 1. This will delete the indexing on the Shape field.
  /// ![UI](Screenshots/Screen7.png)
  /// 1. In order to delete the Attribute index Tap the "Remove Attribute Index" button.
  /// ![UI](Screenshots/Screen9.png)
  /// 1. Notice the attribute "name" and "address" are no longer indexed.
  /// ![UI](Screenshots/Screen10.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("DDLCreateDeleteAttributeAndSpatialIndexes_Module");

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
