//   Copyright 2019 Esri
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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace CameraNavigation
{
  /// <summary>
  /// This sample provides a new tab and controls that allow you to get the camera of an existing view 
  /// and zoom to or pan to a new camera position.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 2. Click Start button to open ArcGIS Pro.
  /// 3. ArcGIS Pro will open. 
  /// 4. Open a map view. Click on the new Camera tab on the ribbon.
  /// 5. Within this tab there is a Camera Properties button that when clicked will open the Camera Properties dockable window.
  /// ![UI](Screenshots/Screenshot1.png)  
  /// 6. The dock pane will show each property of the camera for the active map view and its corresponding value.
  /// 7. Change one or more of the values and use the zoom to or pan to commands to go to the new camera position. 
  /// </remarks>
  internal class CameraModule : Module
  {
    private static CameraModule _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static CameraModule Current
    {
      get
      {
        return _this ?? (_this = (CameraModule)FrameworkApplication.FindModule("Navigating_camera_with_ArcGIS_Pro_Module"));
      }
    }
  }
}
