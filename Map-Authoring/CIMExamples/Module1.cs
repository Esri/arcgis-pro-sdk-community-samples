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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace CIMExamples
{
    /// <summary>
    /// Shows the following CIM capabilities:
    /// 1. Provide sample to create CIMUniqueValueRenderer from scratch
    /// 1. Same as above but using the UniqueValueRendererDefinition class and the layer to configure the underlying Renderer
    /// 1. How to create the equivalent of symbol levels in Pro.
    /// 1. How to change out the Data Connection (equivalent to changing "DataSource" in ArcObjects)
    /// 1. Change the selection color for the given feature layer
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data). The sample data contains an ArcGIS Pro project and data to be used for this sample. Make sure that the Sample data is unzipped in c:\data and c:\data\Admin is available.
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Launch the debugger to open ArcGIS Pro.
    /// 1. Open the project 'C:\Data\Admin\AdminSample.aprx'.  Please note that layer names and other specific data is required for this sample, hence this specific project is required.
    /// 1. Click on the 'CIM Examples' tab.
    /// ![CIMExamples](Screenshots/Screenshot1.png)
    /// 1. Click the 'Renderer From Scratch' button to create a new render for the States layer:
    /// ![CIMExamples](Screenshots/Screenshot2.png)
    /// 1. Click the 'Renderer via Definition' button to create a new UniqueValueRendererDefinition for States using the 'TOTPOP2010' field:
    /// ![CIMExamples](Screenshots/Screenshot3.png) 
    /// 1. Click the 'Create Symbol Levels' button to create the equivalent of symbol levels in Pro:
    /// ![CIMExamples](Screenshots/Screenshot4.png)
    /// 1. Click the 'Layer DataSource' button to change out the Data Connection to 'C:\Data\Admin\AdminSample.gdb':
    /// ![CIMExamples](Screenshots/Screenshot5.png)
    /// 1. Click the 'Layer Selection Color' button to change the selection color for the given feature layer:
    /// ![CIMExamples](Screenshots/Screenshot6.png)  
    /// 1. Add a raster layer to your map.  For example: find and add 'CharlotteLAS' from the 'All Portal' connection, add the layer to your map, and then zoom to the newly added layer's extent.
    /// 1. Click the 'Raster Stretch' button to create a CIMRasterStretchColorizer:
    /// ![CIMExamples](Screenshots/Screenshot7.png)  
    /// </remarks>
    internal class Module1 : Module {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current {
            get {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CIMExamples_Module"));
            }
        }
        /// <summary>
        /// Are we on the UI thread?
        /// </summary>
        internal static bool OnUIThread
        {
            get
            {
                return System.Windows.Application.Current.Dispatcher.CheckAccess();
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
