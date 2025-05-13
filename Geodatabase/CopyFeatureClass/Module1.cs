/*

   Copyright 2025 Esri

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
using System.Threading.Tasks;
using System.Windows.Input;
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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace CopyFeatureClass
{

  /// <summary>
  /// This sample shows how to implement a how to duplicate a feature layer schema in the local geodatabase and then copy all features from the feature layer into the local copy of the newly generated feature class.  This addin utilitzed the SchemaBuilder class and the EditOperation Class.
  /// </summary>
  /// <remarks>   
  /// 1. Open this solution in Visual Studio.  
  /// 1. Click the build menu and select Build Solution.
  /// 1. Launch the debugger to open ArCGIS Pro. 
  /// 1. Open any project with a map that contains as least one Point, Line or Polygon feature layer.
  /// 1. Click on the 'Add-in' tab and note the 'Copy Feature Layer' group.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. On the map's table of content click on any feature layer and then click the 'Copy Feature Layer' button.
  /// ![UI](Screenshots/Screen2.png)
  /// ![UI](Screenshots/Screen3.png)
  /// ![UI](Screenshots/Screen4.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("CopyFeatureClass_Module");

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
