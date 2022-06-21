/*

   Copyright 2019 Esri

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

namespace EditingTemplates
{
  internal class ModifyTemplateCIM : Button
  {
    protected override async void OnClick()
    {
      MapView mapvView = MapView.Active;
      if (mapvView == null)
        return;

      // get the layer
      FeatureLayer layer = mapvView.Map.GetLayersAsFlattenedList().
                              OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "Portland Precincts");
      if (layer == null)
        return;

      await QueuedTask.Run(() =>
      {
        // get the template
        var template = layer.GetTemplate("North Precinct");
        if (template != null)
        {
          // get the definition
          var templateDef = template.GetDefinition() as CIMRowTemplate;
          // change the default tool        
          templateDef.SetDefaultToolID("esri_editing_SketchRightPolygonTool");  
          // commit  the definition 
          template.SetDefinition(templateDef);
        }
      });
    }
  }
}

