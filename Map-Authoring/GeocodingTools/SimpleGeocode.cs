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
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace GeocodingTools
{
  /// <summary>
  /// A simple button illustrating how to use the Geocoding API to goeocde an address.  The results are displayed in a simple messagebox. 
  /// </summary>
  internal class SimpleGeocode : Button
  {
    protected override async void OnClick()
    {
      string text = "380 New York St, Redlands, CA, 92373, USA";

      // geocode
      IEnumerable<ArcGIS.Desktop.Mapping.Geocoding.GeocodeResult> results = await MapView.Active.LocatorManager.GeocodeAsync(text, false, false);

      // show results
      string msg = "results : " + results.Count().ToString() + "\r\n";
      foreach (var result in results)
        msg = msg + result.ToString() + "\r\n";

      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(msg, "Geocode Results");
    }
  }
}
