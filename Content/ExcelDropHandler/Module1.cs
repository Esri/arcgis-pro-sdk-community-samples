//   Copyright 2015 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ExcelDropHandler
{
    /// <summary>
    /// ArcGIS Pro Addin allows to Drag and drop *.xlsx file on ArcGIS Pro and execute the necessary Geoprocessing Tools automatically. 
    /// </summary>
    /// <remarks> 
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
    /// 1. Make sure that the Sample data is unzipped in c:\data 
    /// 1. Before you run the sample verify that the project C:\data\SDK\SDK 1.1.aprx is present since this is required to run the sample.        
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open the project 'SDK 1.1.aprx' which can now be found in the C:\Data\SDK folder.  
    /// 1. ArcGIS Pro will display a map view.  
    /// 1. Look at two eventhandler methods
    /// 1. OnDragOver – The Pro framework calls this method when an Excel file is dragged onto the Map holding down the left mouse button. 
    /// 1. OnDrop – The Pro framework calls this method when the user releases the left mouse button and the Excel file is dropped on Pro. 
    /// 1. Take a closer look at the OnDrop logic where the code for the execution of the Geoprocessing Tool can be found.  
    /// 1. Drag and Drop Meteorites_UK.xls onto Pro.  
    /// 1. View the results of the Meteorite strikes layer loaded into Pro with symbology applied  
    /// ![UI](Screenshots/2dScreen.png)
    /// 1. Switch to the 3D scene view  
    /// 1. Drag and Drop EarthquakeDamage.xls on to ArcGIS Pro  
    /// 1. View those results.  
    /// ![UI](Screenshots/3dScreen.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ProSDKDemo_Module"));
            }
        }

        internal static string GetUniqueLayerName(string fileName)
        {
            string baseName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            return baseName;
        }

        internal static string GetUniqueStandaloneTableName(string fileName)
        {
            string baseName = string.Format("{0}$", System.IO.Path.GetFileNameWithoutExtension(fileName));
            return baseName;
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
