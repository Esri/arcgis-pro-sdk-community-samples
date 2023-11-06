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

namespace AddRemoveFields
{
  /// <summary>
  /// This sample shows how to use the DDL APIs to add and remove fields in a FeatureClass.
  /// </summary>
  /// <remarks>
  /// 1. Open this solution in Visual Studio.  
  /// 1. Click the build menu and select Build Solution.  
  /// 1. Click the Start button to open ArcGIS Pro. ArcGIS Pro will open.    
  /// 1. Open any project.
  /// 1. Click on the Add-in tab and verify that a "Add/Remove Fields" group was added.
  /// 1. Notice the buttons in the "Add/Remove Fields" group.
  /// 1. Tap the "Create Emtpy Geodatabase" button.
  /// ![UI](Screenshots/Screen0.png)
  /// 1. Add the new Database located in the "C:\temp\mySampleGeoDatabase.gdb" directory into the Catalog pane.
  /// 1. Add a Feature Class to the Geodatabase and name it "Parcels".
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Tap the finish button to finish adding the new Feature Class
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Tap the "Add Fields in Feature Class" button.
  /// ![UI](Screenshots/Screen3.png)
  /// 1. Open the Table for the Parcels Feature Class.
  /// 1. Notice the newly added "Tax_Code" , "Parcel_ID" , "Global_ID" and "Parcel_Address" fields.
  /// ![UI](Screenshots/Screen4.png)
  /// 1. Tap the "Remove Field Table"
  /// ![UI](Screenshots/Screen8.png)
  /// 1. Open the Table for the Parcels Feature Class.
  /// 1. Notice the "Parcel_Address" Field has been deleted.
  /// ![UI](Screenshots/Screen5.png)
  /// 1. Add a new Feature Class named "Pipes",
  /// 1. Tap the "Add Field with Domain" button.
  /// ![UI](Screenshots/Screen6.png) 
  /// 1. Open the table for the "Pipes" Feature Class.
  /// ![UI](Screenshots/Screen7.png)
  /// 1. Notice the domains of the "Pipe Type" field.
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;


        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("AddRemoveFields_Module");

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
