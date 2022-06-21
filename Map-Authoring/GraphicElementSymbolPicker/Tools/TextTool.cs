/*

   Copyright 2022 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicElementSymbolPicker.TextTools
{
  internal class TextTool : LayoutTool
  {
    public TextTool()
    {
      SketchType = SketchGeometryType.Point;

    }

    protected override Task OnToolActivateAsync(bool active)
    {    
      return base.OnToolActivateAsync(active);
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (ActiveElementContainer == null)
        Task.FromResult(true);

      if (Module1.SelectedSymbol == null) return Task.FromResult(true);
      
      return QueuedTask.Run(() =>
      {

        var cimGraphic = new CIMTextGraphic()
        {
          Shape = geometry as MapPoint,
          Text = "Text",
          Symbol = Module1.SelectedSymbol.MakeSymbolReference()
        };
        ElementFactory.Instance.CreateGraphicElement(this.ActiveElementContainer, cimGraphic);
        //The new text graphic element has been created
        //We now switch to Pro's out of box "esri_layouts_inlineEditTool" 
        //This will allow inline editing of the text 
        //This tool will work on graphics on both map view and layouts
        FrameworkApplication.SetCurrentToolAsync("esri_layouts_inlineEditTool");
        return true;
      });
    }
  }
}
