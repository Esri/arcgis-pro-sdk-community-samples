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

namespace GraphicsLayers.ArrangeGraphics
{
    internal class SelectGraphicsToGroup : MapTool
    {
        public SelectGraphicsToGroup()
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
            return await QueuedTask.Run( () => {
              //Select elements in geometry  
              var selectedElements = MapView.Active.SelectElements(geometry, SelectionCombinationMethod.New);

              Dictionary<GraphicsLayer, List<Element>> glElements = new Dictionary<GraphicsLayer, List<Element>>();

              foreach (var selElement in selectedElements)
              {                
                //Get element's parent
                var elementContainer = selElement.GetParent() as GraphicsLayer;
                if (glElements.ContainsKey(elementContainer))
                {
                  glElements[elementContainer].Add(selElement);
                }
                else
                  glElements.Add(elementContainer, new List<Element> { selElement});
              }
             
              //The dictionary is now populated
              if (glElements.Count == 0)
              {
                MessageBox.Show($"No elements selected.");
                return true;
              }
              //elements selected in multiple Graphic layers, no op.
              if (glElements.Count > 1)
              {
                MessageBox.Show($"Select elements in the same graphics layer to group them.");
                return true;
              } 
              //get graphics layer
              var gl = glElements.FirstOrDefault().Key;
              //Get selected elements in graphics layer
              var elements = glElements.FirstOrDefault().Value;
              if (elements.Count == 0) return true;
              if (elements.Count == 1) return true;
              gl.GroupElements(elements);
              return true;
            });
        }
    }
}
