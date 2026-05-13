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

namespace AnnotationRetrofit
{
	/// <summary>
	/// This sample illustrates how to implement an annotation feature construction tool that uses a tool to select a set of point features, and then creates a new annotation feature for each selected point by using the selected point's geometry and text.
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
	/// 1. Make sure that the Sample data is unzipped in c:\data 
	/// 1. The project used for this sample is 'C:\Data\SampleAnnoTwo\SampleAnno.aprx'
	/// 1. In Visual studio click the Build menu. Then select Build Solution.
	/// 1. Start the Debugger to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open, select the SampleAnno.aprx project
	/// 1. To use this tool you have to start editing by clicking the 'Create [features]' button.  Then use the Create Features dockpane to select the 'Retrofit Annotation for points' tool.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. When the tool has been activated, rubber band select a few cities to create the matching annotation strings and point geometry.
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Once the point feature selection (via the rubber banding) is complete, the tool creates an annotation feature for each selected point.  The tool is using the point's geometry to create the annotation and the 'CITY_NAME' attribute value as the annotation text.
	/// ![UI](Screenshots/Screen3.png)
  /// 1. You might have to zoom in, in order to view the newly created annotation features.
	/// </remarks>
	internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("AnnotationRetrofit_Module");

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
