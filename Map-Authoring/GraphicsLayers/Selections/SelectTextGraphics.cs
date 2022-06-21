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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace GraphicsLayers.Selections
{
    internal class SelectTextGraphics : MapTool
    {
        public SelectTextGraphics()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Map;
        }
        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }
    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {      
      return await QueuedTask.Run(() =>
      {
        var allSelElements = MapView.Active.SelectElements(geometry as Polygon);

        Dictionary<GraphicsLayer, List<Element>> glTextElements = new Dictionary<GraphicsLayer, List<Element>>();

        foreach (var selElement in allSelElements)
        {
          var graphicEl = selElement as GraphicElement;
          if (graphicEl == null) continue;
          if (!(graphicEl?.GetGraphic() is CIMTextGraphic))
            continue;
          //Get element's parent
          var elementContainer = selElement.GetParent() as GraphicsLayer;
          if (glTextElements.ContainsKey(elementContainer))
          {
            glTextElements[elementContainer].Add(graphicEl);
          }
          else
            glTextElements.Add(elementContainer, new List<Element> { graphicEl });
        }
        //No text graphics
        if (glTextElements.Values.ToList().Count == 0)
        {
          //Clear selection
          //Get all the graphics layers in the map
          var allGraphicsLayers = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<GraphicsLayer>();
          foreach (var gl in allGraphicsLayers)
          {
            gl.ClearSelection();
          }
          MessageBox.Show("No Text graphic elements selected");
          return true;
        }
        //Iterate through dictionary and select the elements
        foreach (var item in glTextElements)
        {
          item.Key.SelectElements(item.Value);
        }

        return true;
      });
    }

    protected override void OnToolMouseUp(MapViewMouseButtonEventArgs e)
    {
      switch (e.ChangedButton)
      {
        case System.Windows.Input.MouseButton.Right:
          e.Handled = true;
          var menu = FrameworkApplication.CreateContextMenu(
                 "esri_layouts_textElementCommonMenu");
          menu.DataContext = this;
          menu.IsOpen = true;
          break;
      }
    }



  }
}
