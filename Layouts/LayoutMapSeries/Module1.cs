/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace LayoutMapSeries
{  /// <summary>
   /// LayoutMapSeries exercises a collection of programmatic interactions with the layout API including the application of map series
   /// </summary>
   /// <remarks>
   /// 1. This solution is using the **Newtonsoft.Json NuGet**.  If needed, you can install the NuGet from the "NuGet Package Manager Console" by using this script: "Install-Package Newtonsoft.Json".
   /// 1. In Visual Studio click the Build menu. Then select Build Solution.
   /// 1. Click Start button to open ArcGIS Pro.
   /// 1. ArcGIS Pro will open. 
   /// 1. Open the "C:\Data\LayoutMapSeries\LayoutMapSeries.aprx" project file. Click on the Add-in tab on the ribbon and then on the Show "Generate Map Series" button.
   /// 1. This "Create Map Series" dockpane will open.
   /// 1. Select a "Map Series Layout" from the drop down and the click the "Generate Series" button.
   /// ![UI](Screenshots/Screenshot1.png)  
   /// 1. Once the new Layout has been generated, the layout is displayed and the map series items are displayed in the Map Series item list.
   /// 1. Now the map series items can be selected by clicking on the map series items list or the VCR buttons on the bottom.
   /// 1. The layout contains various layout elements like tables, dynamic text, layout, scale bar, north arrow, and finally the map frame.
   /// ![UI](Screenshots/Screenshot2.png)  
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("LayoutMapSeries_Module"));
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
