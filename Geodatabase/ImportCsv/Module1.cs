/*

   Copyright 2026 Esri

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

namespace ImportCsv
{
  /// <summary>
  /// This sample demonstrates how to import a CSV datasource and create a new Featureclass for the CSV data to be copied to.
  /// </summary>
  /// <remarks>
  /// Using the sample:
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. To run the add-in start with a new [empty] map template and open the 'FGB Importer' tab.
  /// 1. Click the 'Show FGB Importer' button to open the Importer dockpane.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. On the dockpane all paths etc. should be filled in.
  /// 1. Click the 'Import into F/C' button to start the CSV import.
  /// 1. This will create a new feature class in the given geodatabase and then adds the csv data to that feature class.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. No layers are added to the map, instead the data is added to the geodatabase and can be viewed using the Catalog dockpane.
  /// 1. Note: you might have to 'refresh' the file geodatabase in order to view the newly created featureclass.
  /// ![UI](Screenshots/Screen3.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ImportCsv_Module");

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
