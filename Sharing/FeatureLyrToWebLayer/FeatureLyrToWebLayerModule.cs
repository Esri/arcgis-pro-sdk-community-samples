//Copyright 2026 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific .cs governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace FeatureLyrToWebLayer
{
  /// <summary>
  /// This Button add-in, when clicked, uses the first feature layer in the Map and publishes it as a Web Layer using a custom Python script.
  /// </summary>
  /// <remarks>
  /// 1. This solution file includes an example python script named MySharing.pyt which is included as 'Content' (Build action) in the add-in.
  /// 1. The python script is stored in the .\Toolboxes\toolboxes folder and when this add-in is loaded in ArcGIS Pro the python script is available a script tool under the ArcGIS Pro Geoprocessing toolbox.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. You can run the python script from the Geoprocessing toolbox as shown here.
  /// ![UI](Screenshots/Screen2.png)
  /// ![UI](Screenshots/Screen3.png)
  /// ![UI](Screenshots/Screen4.png)
  /// ![UI](Screenshots/Screen5.png)
  /// ![UI](Screenshots/Screen6.png)
  /// 1. You can also run the python script from code as implemented in the 'Publish as Web Layer' button under the 'Custom Publishing' tab (RunPyScriptButton.cs).
  /// ![UI](Screenshots/Screen7.png)
  /// ![UI](Screenshots/Screen8.png)
  /// ![UI](Screenshots/Screen9.png)
  /// </remarks>
  internal class FeatureLyrToWebLayerModule : Module
  {
    private static FeatureLyrToWebLayerModule _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static FeatureLyrToWebLayerModule Current
    {
      get
      {
        return _this ??= (FeatureLyrToWebLayerModule)FrameworkApplication.FindModule("FeatureLyrToWebLayer_Module");
      }
    }

  }
}
