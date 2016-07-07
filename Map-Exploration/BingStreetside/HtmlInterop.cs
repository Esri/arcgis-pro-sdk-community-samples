/*

   Copyright 2016 Esri

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
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Dialogs;

namespace BingStreetside
{
    /// <summary>
    /// Interop with javascript calls into this methods here
    /// </summary>
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class HtmlInterop
    {
        /// <summary>
        /// Update Big Map using new coords and heading
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="heading"></param>
        public void ShowLatLngHeading(double lat, double lng, int heading)
        {
            System.Diagnostics.Debug.WriteLine($@"ShowLatLngHeading from html Lat: {Math.Round(lat, 5)} Long: {Math.Round(lng, 5)} Heading: {heading}");
            BingStreetsideModule.SetMapLocationFromBing(lng, lat, heading);
        }
    }
}
