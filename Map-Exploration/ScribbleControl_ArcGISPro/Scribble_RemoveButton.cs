//   Copyright 2014 Esri
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
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace ScribbleControl_ArcGISPro
{
  /// <summary>
  /// Button to remove scribble control from the current map/scene view
  /// When the control is removed, the list of controls in the project is also updated
  /// </summary>
  internal class Scribble_RemoveButton : Button
  {
    protected override void OnClick()
    {
      string mapURI = MapView.Active.Map.URI;
      if (Scribble_Module.projectControls.Contains(mapURI))
      {
        var overlayControl = (MapViewOverlayControl)Scribble_Module.projectControls[mapURI];
        MapView.Active.RemoveOverlayControl(overlayControl);
        Scribble_Module.projectControls.Remove(mapURI);
        overlayControl = null;
      }
    }
  }
}
