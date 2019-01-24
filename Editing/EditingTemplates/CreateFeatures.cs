/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

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

namespace EditingTemplates
{
  internal class CreateFeatures : Button
  {
    protected override async void OnClick()
    {
      MapView mapvView = MapView.Active;
      if (mapvView == null)
        return;

      // get the Fire Stations layer
      FeatureLayer layer = mapvView.Map.GetLayersAsFlattenedList().
                              OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "Fire Stations");
      if (layer == null)
        return;

      await QueuedTask.Run(async () =>
      {
        // use the center of the mapextent as the geometry
        var extent = mapvView.Extent;
        var geometry = extent.Center;
        // set up 2 other geometries offset
        var geometry2 = GeometryEngine.Instance.Move(geometry, extent.Width / 4, extent.Height / 4);
        var geometry3 = GeometryEngine.Instance.Move(geometry, -extent.Width / 4, -extent.Height / 4);

        // get one of the templates
        var template = layer.GetTemplate("Fire Stations");
        if (template != null)
        {
          // activate the template - use the default tool
          await template.ActivateDefaultToolAsync();

          // you can also activate a template via a specific tool 
          // (assuming tool is available for the template)
          // await template.ActivateToolAsync("esri_editing_SketchPointTool");

          // perform the creation
          var op = new EditOperation();
          op.Name = "Create feature";
          // use template default field values
          op.Create(template, geometry);

          // to modify default template properties 
          var insp = template.Inspector;
          insp["City"] = "xxx";

          // create with the modified fields and a different geometry
          op.Create(template, geometry2);

          // reset the modified fields back to original defaults
          insp.Cancel();
          // change the field again
          insp["City"] = "yyy";
          // create with the modified fields and a different geometry
          op.Create(template, geometry3);

          // reset the modified fields back to original defaults
          insp.Cancel();

          // execute the operation
          bool result = op.Execute();
        }
      });
    }
  }
}
