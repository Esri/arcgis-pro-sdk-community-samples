/*

   Copyright 2017 Esri

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
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace EditingTemplates
{
  internal class CreateGroupTemplateWithExt : Button
  {
    protected override async void OnClick()
    {
      MapView mapvView = MapView.Active;
      if (mapvView == null)
        return;

      // get the layers
      FeatureLayer precinctLayer = mapvView.Map.GetLayersAsFlattenedList().
                              OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "Portland Precincts");
      FeatureLayer fireLayer = mapvView.Map.GetLayersAsFlattenedList().
                        OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "Fire Stations");
      FeatureLayer policeLayer = mapvView.Map.GetLayersAsFlattenedList().
                        OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "Police Stations");
      if ((precinctLayer == null) || (fireLayer == null) || (policeLayer == null))
        return;

      await QueuedTask.Run(() =>
      {
        // group templates are stored on the primary layer
        var layerDef = precinctLayer.GetDefinition() as CIMFeatureLayer;

        // create group template from primary layer
        var myGroupTemplateDef = precinctLayer.CreateGroupTemplateDefinition("My Group Template", "North Precinct", "some desc", new[] { "Group", "Polygon" });

        // set the default construction tool
        myGroupTemplateDef.SetDefaultToolDamlID("esri_editing_SketchPolygonTool");

        // remove construction tools from being available with this template
        List<string> filter = new List<string>();
        // guid = esri_editing_SketchFreehandPolygonTool
        filter.Add("0A7C16B9-1CFD-467f-8ECE-6BA376192431");
        // esri_editing_SketchAutoCompleteFreehandPolygonTool
        filter.Add("ACD53634-CBC7-44d5-BDE9-692FA8D45850");
        // esri_editing_SketchTracePolygonTool
        filter.Add("E22F7D98-007D-427C-8282-13704F7C84C3");
        myGroupTemplateDef.ToolFilter = filter.ToArray();

        // add component parts
        myGroupTemplateDef = myGroupTemplateDef.AddComponentTemplate(fireLayer, "Fire Stations", GroupTemplateBuilderMethods.builderPointAtPolygonCentroid);
        myGroupTemplateDef = myGroupTemplateDef.AddComponentTemplate(policeLayer, "Police Stations", GroupTemplateBuilderMethods.builderPointAtPolygonStart);

        // add group template to layer
        var template = precinctLayer.CreateTemplate(myGroupTemplateDef);
      });
    }
  }
}
