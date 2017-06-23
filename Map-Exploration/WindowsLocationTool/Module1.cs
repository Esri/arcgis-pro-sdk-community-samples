/*

   Copyright 2017 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace WindowsLocationTool
{
    /// <summary>
    /// This sample implements a tool that uses the System.Device.Location namespace to get the current location in order to zoom the current map view to that location.
    /// Note: In Windows 10 the accuracy of the location information depends on the source. The latitude and longitude may vary within the following ranges:
    /// * GPS : within approximately 10 meters
    /// * Wi-Fi : between approximately 30 meters and 500 meters
    /// * Cell towers : between approximately 300 meters and 3,000 meters
    /// * IP address : between approximately 1,000 meters and 5,000 meters In addition to latitude and longitude, GPS also provides information about heading, speed, and altitude.This additional information is optional when the location information comes from other sources.
    /// The user sets the privacy of their location data with the location privacy settings in the Settings app.Your app can access the user's location only when:
    /// Location for this device... is turned on (not applicable to Windows 10 Mobile)
    /// The location services setting, Location, is turned on
    /// Under Choose apps that can use your location, your app is set to on
    /// </summary>
    /// <remarks>
    /// Note: The GeoCoordinateWatcher class supplies coordinate-based location data from the current location provider. The current location provider is prioritized as the highest on the computer, based on a number of factors, such as the age and accuracy of the data from all providers, the accuracy requested by location applications, and the power consumption and performance impact associated with the location provider. The current location provider might change over time, for instance, when a GPS device loses its satellite signal indoors and a Wi-Fi triangulation provider becomes the most accurate provider on the computer.
    /// To begin accessing location data, create a GeoCoordinateWatcher and call Start or TryStart to initiate the acquisition of data from the current location provider.
    /// The Status property can be checked to determine if data is available.If data is available, you can get the location one time from the Position property, or receive continuous location updates by handling the PositionChanged event.
    /// The Permission, Status, and Position properties support INotifyPropertyChanged, so that an application can data-bind to these properties.
    /// In Windows 7, all the System.Device.Location classes are fully functional if a location provider is installed and able to resolve the computer's location
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Create a new map using the Map template. Click on the 'Add-in' tab on the ArcGIS Pro ribbon and notice the "Windows Location" group.  
    /// ![UI](Screenshots/Screen1.png)  
    /// 1. Click on the 'Zoom To Windows Location' button to zoom to the location given by windows location services.  
    /// ![UI](Screenshots/Screen2.png)  
    /// 1. Note that 'Location Privacy settings' have to allow the location to be used on your machine.  Use the control panel to configure location settings.
    /// ![UI](Screenshots/Screen3.png)  
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("WindowsLocationTool_Module"));
            }
        }

        /// <summary>
        /// Create a point symbol
        /// </summary>
        /// <returns></returns>
        internal static Task<CIMPointSymbol> CreatePointSymbolAsync()
        {
            return QueuedTask.Run(() => {
                return SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 18, SimpleMarkerStyle.Pushpin);
            });
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
