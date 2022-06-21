/*

   Copyright 2020 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace GraphicsLayers.Order
{
  internal class SendBackward : Button
  {
    protected async override void OnClick()
    {
      await QueuedTask.Run(() =>
      {
        //Only one Graphic layer should be there in the dictionary. This is because of the state logic.
        var graphicLayer = Module1.Current.GraphicsLayerSelectedElements.FirstOrDefault().Key;
        var selectedElements = Module1.Current.GraphicsLayerSelectedElements[graphicLayer];
        var elementMoveBackward = graphicLayer.CanSendBackward(selectedElements) ? true : false;
        if (elementMoveBackward)
        {
          Module1.Current.GraphicsLayerSelectedElements.FirstOrDefault().Key.SendBackward(selectedElements);
        }
      });
      //need to evaluate state again, back and front.
      var canMoveBackward = await Module1.Current.CanSendToBack();
      Module1.SetState("can_send_backward_state", canMoveBackward);
      var canBringFwd = await Module1.Current.CanBringForward();
      Module1.SetState("can_bring_forward_state", canBringFwd);
    }
  
  }
}
