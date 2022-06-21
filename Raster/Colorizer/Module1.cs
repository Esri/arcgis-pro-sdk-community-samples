/*

   Copyright 2020 Esri

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

namespace Colorizer
{
    /// <summary>
    /// This sample demonstrates how to change colors for rasters that have attribute tables.  The colorizer has to contain the attribute field name that is then used on the TOC and is also used to define the colors.
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a folder called 'C:\Data\Raster\Landuse' with sample data required for this solution.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Raster\Landuse" is available.
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. In ArcGIS Pro the 'C:\Data\Raster\Landuse\Landuse.aprx' project.  
    /// 1. Click on the Add-In tab and then click the "New Color by 'Value'" button to change the raster image colors and break those colors up by using the 'Value' field in the raster's Attribute table.  
	/// 1. A message box will display: "In ArcGIS Pro 2.5 and older this method only works when using the 'value' field, unless you deploy the 'RecalculateColorizer' workaround."", click Ok.
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Open the map's content dockpane and select the raster in the TOC (table of content).
	/// 1. The add-in automatically selects the first column in the attribute table, in this case the ObjectID column, and renders the raster with a color ramp using the 'ObjectID' column.
	/// ![UI](Screenshots/Screen4.png)
	/// 1. On the 'Use this attribute' drop down select the 'LandType' attribute column and verify that the raster redraws with a color ramp using the 'LandType' column.
	/// ![UI](Screenshots/Screen5.png)
	/// 1. On the 'Use this attribute' drop down select 'Attribute driven RGB', which will use the RGB values that are defined in the raster's attribute table to define the colors.  Verify that the raster redraws with colors defined by the 'Red', 'Green', and 'Blue' columns.
	/// ![UI](Screenshots/Screen6.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Colorizer_Module"));
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
