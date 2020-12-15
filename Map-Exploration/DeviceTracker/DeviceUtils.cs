//Copyright 2020 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using ArcGIS.Core.Geometry;

namespace DeviceTracker
{
  public class DeviceUtils
  {
    private static int MaxDeviceLocations = 10;
    private static List<MapPoint> _deviceLocations = new List<MapPoint>();

    public static IReadOnlyList<MapPoint> DeviceLocations => _deviceLocations.AsReadOnly();
    public static void ClearDeviceLocations()
    => _deviceLocations.Clear(); 

    public static void AddDeviceLocation(MapPoint pt)
    {
      if ((pt == null) || (pt.IsEmpty))
        return;

      // add it
      _deviceLocations.Add(pt);

      // now trim
      if (_deviceLocations.Count >= MaxDeviceLocations)
        _deviceLocations.RemoveAt(0);
    }

    public static MapPoint LastDeviceLocation
    {
      get
      {
        int count = _deviceLocations.Count;
        if (count == 0)
          return null;

        return _deviceLocations[count - 1];
      }
    }
  }
}
