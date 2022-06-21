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
using System.Windows.Input;
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

namespace GraphicsLayers.Align
{
  internal class AlignLeft : MapTool
  {
    public AlignLeft()
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
      return await QueuedTask.Run(() => {
        var selPoly = geometry as Polygon;
        //Select graphics
        var selectedElements = MapView.Active.SelectElements(selPoly);
        if (selectedElements.Count < 2)
        {
          MessageBox.Show("Select more than 1 graphic element to align.", "Align Left");
          return true;
        }
         //Get the ID of Pro's Align Left button and use it. 
        var alignLeftCmd = FrameworkApplication.GetPlugInWrapper("esri_layouts_alignLeft") as ICommand;
        if (alignLeftCmd != null)
        {
          if (alignLeftCmd.CanExecute(null))
          {
            alignLeftCmd.Execute(null);
          }
        }
        return true;

      });
    }
  }
}
