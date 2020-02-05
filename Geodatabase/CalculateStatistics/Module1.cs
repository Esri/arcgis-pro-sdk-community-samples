/*

   Copyright 2020 Esri

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

namespace CalculateStatistics
{   /// <summary>
    /// This sample illustrates the use of the CalculateStatistics class.
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a dataset called 'FeatureTest' with sample data for use by this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\FeatureTest" is available.
    /// 1. In Visual studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. In ArcGIS Pro open the 'C:\Data\FeatureTest\FeatureTest.aprx' project
	/// 1. Click on the ADD-IN tab which contains the 'Calculate Statistics' group.  
	/// ![UI](Screenshots/Screen1.png)
    /// 1. Click on the "Calculate Length" button to calculate the sum of all length field values for all features in the testlines feature layer.
    /// ![UI](Screenshots/Screen2.png)
	/// Connect to a SQL Server database and add both TestLines and TestPolygons from the file geodatabase to SQL Server.  
	/// Add a new Map and add the two SQL Server feature layers to the new map
	/// ![UI](Screenshots/Screen3.png)
    /// 1. Click on the "Calculate Area" button to calculate the sum of all area field values for all features in the testPolygon feature layer.
    /// ![UI](Screenshots/Screen4.png)
	/// The calculate statistics function is not working properly with release 2.5 and early, so this version contains a workaround that can be used until ArcGIS Pro 2.6 is released.
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CalculateStatistics_Module"));
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
