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

namespace DDLCreateDeleteRelationshipClass
{
  /// <summary>
  /// This sample illustrates how to use the DDL APIs to create and delete relationship classes and move a relationship class in and out of a feature data set.
  /// </summary>
  /// <remarks>
  /// 1. Open this solution in Visual Studio.  
  /// 1. Click the build menu and select Build Solution.
  /// 1. Click the Start button to open ArCGIS Pro. ArcGIS Pro will open.    
  /// 1. Open any project.
  /// 1. Click on the Add-in tab and verify that a "Operations" group was added.
  /// 1. Notice the buttons in the "Operations" group.
  /// 1. Tap the "Create Relationship with Relationship Rules" button.
  /// ![UI](Screenshots/Screen0.png)
  /// 1. Add the new Database into the Catalog pane.
  /// ![UI](Screenshots/Screen1.png)  
  /// 1. Notice the structure of the data.
  /// 1. In order to move the "BuildingToBuildingType" RelationshipClass out of the FeatureDataSet ,"Parcel_Information", and into the root we tap on the "Remove RelationshipClass out of feature dataset" button
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Notice the "BuildingToBuildingType" is in the root of the "mySampleGeoDatabase.gdb".
  /// ![UI](Screenshots/Screen3.png)  
  /// 1. Delete the "mySampleGeoDatabase.gdb"
  /// 1. Tap on the "Create Atrributed Relationship" button 
  /// ![UI](Screenshots/Screen4.png) 
  /// 1. Add the new Database into the Catalog pane.
  /// 1. Right click on "BuildingToBuildingType", select properties, then select Rules.Notice the Min and Max values are blank.
  /// ![UI](Screenshots/Screen5.png)
  /// 1. Tap the "Modify RelationshipClass" button
  /// ![UI](Screenshots/Screen7.png)  
  /// 1.Right click on "BuildingToBuildingType", select properties, then select Rules.Notice the Min and Max values are no longer blank.
  /// 1.Tap the "Delete RelationshipClass" button
  /// ![UI](Screenshots/Screen8.png)
  /// 1.Refresh the Database.
  /// 1. Notice "BuildingToBuildingType" has been deleted.
  /// </remarks>   
  internal class Module1 : Module
    {
        private static Module1 _this = null;
        
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("DDLCreateDeleteRelationshipClass_Module");

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
