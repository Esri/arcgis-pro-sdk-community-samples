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
  internal class CreateTemplateWithCIM : Button
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

      await QueuedTask.Run(() =>
      {
        //get the CIM layer definition
        var layerDef = layer.GetDefinition() as CIMFeatureLayer;

        //set new template values
        var myTemplateDef = new CIMRowTemplate();
        myTemplateDef.Name = "My CIM template";
        myTemplateDef.Description = "some description";
        myTemplateDef.WriteTags(new[] { "Point", "TesterTag" });

        // set some default attributes
        myTemplateDef.DefaultValues = new Dictionary<string, object>();
        myTemplateDef.DefaultValues.Add("City", "Portland");

        // set the default construction tool
        myTemplateDef.SetDefaultToolID("esri_editing_SketchPointTool");

        // remove construction tools from being available with this template
        List<string> filter = new List<string>();
        // guid = esri_editing_ConstructPointsAlongLineCommand
        filter.Add("BCCF295A-9C64-4ADC-903E-62D827C10EF7");   
        myTemplateDef.ExcludedToolGUIDs = filter.ToArray();

        //get all templates on this layer
        // NOTE - layerDef.FeatureTemplates could be null 
        //    if Create Features window hasn't been opened
        var layerTemplates = layerDef.FeatureTemplates?.ToList();
        if (layerTemplates == null)
          layerTemplates = new List<CIMEditingTemplate>();

        //add the new template to the layer template list
        layerTemplates.Add(myTemplateDef);

        //update the layerdefinition with the templates
        layerDef.FeatureTemplates = layerTemplates.ToArray();

        // check the AutoGenerateFeatureTemplates flag, 
        //     set to false so our changes will stick
        if (layerDef.AutoGenerateFeatureTemplates)
          layerDef.AutoGenerateFeatureTemplates = false;

        //and commit
        layer.SetDefinition(layerDef);
      });
    }
  }
}
