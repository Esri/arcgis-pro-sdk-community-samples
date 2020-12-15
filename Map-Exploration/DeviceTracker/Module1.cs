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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace DeviceTracker
{
  /// <summary>
  /// This sample illustrates functionality associated with GNSS devices and snapshot location data. In order to use this sample you must either have a GNSS device or have configured an appropriate simulator. 
  /// </summary>
  /// <remarks>
  /// 1. Start your GNSS device or simulator. 
  /// 1. In Visual Studio, build the solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. In Pro start with a new map.  Add a graphics layer to your map. (Map tab, Add Graphics Layer button). 
  /// 1. Open the Add-In tab. And click the "Show GNSS Properties" button. The GNSS Properties dockpane appears. 
  /// 1. If your device is not currently connected, the Current Properties section should be blank. 
  /// 1. Under "Input Parameters", enter the communicating comPort for your device. The comPort is the mandatory property required to connect. Other parameters such as baud rate, antenna height, accuracy are optional.  
  /// 1. Click the "Open" button. The "Current Parameters" should be populated and your device should be connected.
  /// 1. You can update your connection parameters (such as baud rate or antenna height) by entering values in the Input Properties and clicking the "Update" button.
  /// 1. You can close the connection by clicking the 'Close' button. 
  /// 1. With your device connected, enable the device by clicking the "Enable/Disable DeviceSource" button. 
  /// 1. Move to the GNSS Location tab. 
  /// 1. Click the "Zoom/Pan to Location" button.  You should zoom to where your device is tracking. 
  /// 1. Click "Show Location" and other Location Options then "Update Options" button to configure the device location map options. 
  /// 1. To add the most recent location to the graphics layer, click the "Add Last Location" button. 
  /// 1. To connect to the snapshot events, click the "Connect" button under Snapshot Events.  The event and location data will be logged to the dockPane. 
  /// 1. Wait until a number of snapshot events have been received.  Click the "Add Polyline" button.  Recent snapshot data will be added to the graphics layer as a polyline. 
  /// 1. Disconnect from the snapshot events by clicking the "Disconnect" button. 
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("DeviceTracker_Module"));
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
