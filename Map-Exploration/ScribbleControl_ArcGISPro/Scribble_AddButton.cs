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
using System.Windows.Media;
using System.Windows;

namespace ScribbleControl_ArcGISPro
{
  /// <summary>
  /// Button to add scribble control to the current map/scene view
  /// </summary>
  internal class Scribble_AddButton : Button
  {
    protected override void OnClick()
    {
      Scribble_ControlView scribbleControl = null;
      MapViewOverlayControl overlayControl = null;

      string mapURI = MapView.Active.Map.URI;

      if (Scribble_Module.projectControls.Contains(mapURI))
      {
        overlayControl = (MapViewOverlayControl) Scribble_Module.projectControls[mapURI];
        MapView.Active.RemoveOverlayControl(overlayControl);
        Scribble_Module.projectControls.Remove(mapURI);
        overlayControl = null;
      }
      scribbleControl = new Scribble_ControlView();
      scribbleControl.Name = "Pro_ScribbleControl";
      scribbleControl.Width = MapView.Active.GetViewSize().Width;
      scribbleControl.Height = 40;
      scribbleControl.cvs.DefaultDrawingAttributes.Width = 20;
      scribbleControl.cvs.DefaultDrawingAttributes.Height = 20;
      scribbleControl.cvs.DefaultDrawingAttributes.Color = Colors.Tomato;
      overlayControl = new MapViewOverlayControl(scribbleControl, false);

      //Add the overlay control to active map view
      MapView.Active.AddOverlayControl(overlayControl);

      //Add to dictionary
      Scribble_Module.projectControls.Add(mapURI, overlayControl);
    }
  }
}
