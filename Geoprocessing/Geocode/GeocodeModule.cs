//Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace Geocode
{
    /// <summary>
    /// This sample provides a new tab and controls that launches a Geocode dialog window. 
    /// This dialog window will allow you to enter an address or place name and will geocode 
    /// the results and then zoom to and place a graphic at the location on the map. 
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open.
    /// 4. Open a map and add a base map.
    /// 5. Click the ADD-IN tab group and click the Geocode Address button.
    /// 6. In the Geocode dialog window, enter and address or place name.
    /// 7. Click the Go button.
    /// ![UI](Screenshots/Screen.png)
    /// 8. The application will geocode the entered value, zoom to the 
    ///    location, and place a graphic symbol at the location.
    /// ![UI](Screenshots/Screen2.png)
    /// </remarks>
    internal class GeocodeModule : Module
    {
        private static GeocodeModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static GeocodeModule Current
        {
            get
            {
                return _this ?? (_this = (GeocodeModule)FrameworkApplication.FindModule("Geocode_Module"));
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
