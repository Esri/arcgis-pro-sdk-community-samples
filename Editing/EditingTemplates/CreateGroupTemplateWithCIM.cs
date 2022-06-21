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
  internal class CreateGroupTemplateWithCIM : Button
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
        // get the templates that will make up the group template
        var precinctTemplate = precinctLayer.GetTemplate("North Precinct");
        var fireTemplate = fireLayer.GetTemplates().FirstOrDefault();
        var policeTemplate = policeLayer.GetTemplates().FirstOrDefault();

        if ((precinctTemplate == null) || (fireTemplate == null) || (policeLayer == null))
          return;

        // group templates are stored on the primary layer
        var layerDef = precinctLayer.GetDefinition() as CIMFeatureLayer;

				//set new template values
				var myGroupTemplateDef = new CIMGroupEditingTemplate
				{
					Name = "My Group Template",
					Description = "some desc"
				};
				myGroupTemplateDef.WriteTags(new[] { "Group", "Polygon"});

        // set the default construction tool
        myGroupTemplateDef.SetDefaultToolID("esri_editing_SketchPolygonTool");
        
        // remove construction tools from being available with this template
        List<string> filter = new List<string>();
        // guid = esri_editing_SketchFreehandPolygonTool
        filter.Add("0A7C16B9-1CFD-467f-8ECE-6BA376192431");
        // esri_editing_SketchAutoCompleteFreehandPolygonTool
        filter.Add("ACD53634-CBC7-44d5-BDE9-692FA8D45850");
        // esri_editing_SketchTracePolygonTool
        filter.Add("E22F7D98-007D-427C-8282-13704F7C84C3");
        myGroupTemplateDef.ExcludedToolGUIDs = filter.ToArray();

        // create the base part
        var basepart = new CIMGroupEditingTemplatePart();
        basepart.LayerURI = precinctLayer.URI;
        basepart.Name = precinctTemplate.Name;
        basepart.TransformationID = "esri_editing_transformation_polygonPrimaryIdentity";

        // assign the base part to the group template
        myGroupTemplateDef.BaseName = basepart.Name;
        myGroupTemplateDef.BasePart = basepart;


        // create the component parts
        var part = new CIMGroupEditingTemplatePart();
        part.LayerURI = fireLayer.URI;
        part.Name = fireTemplate.Name;
        part.TransformationID = "esri_editing_transformation_pointAtPolygonCentroid";

        var part2 = new CIMGroupEditingTemplatePart();
        part2.LayerURI = policeLayer.URI;
        part2.Name = policeTemplate.Name;
        part2.TransformationID = "esri_editing_transformation_pointAtPolygonStart";

        // build the list of component templates
        List<CIMGroupEditingTemplatePart> parts = new List<CIMGroupEditingTemplatePart>();
        parts.Add(part);
        parts.Add(part2);
        // assign to the group template
        myGroupTemplateDef.Parts = parts.ToArray();


        //get all templates on this layer
        // NOTE - layerDef.FeatureTemplates could be null 
        //    if Create Features window hasn't been opened
        var layerTemplates = layerDef.FeatureTemplates?.ToList();
        if (layerTemplates == null)
          layerTemplates = new List<CIMEditingTemplate>();

        //add the new template to the layer template list
        layerTemplates.Add(myGroupTemplateDef);

        //update the layerdefinition with the templates
        layerDef.FeatureTemplates = layerTemplates.ToArray();

        // check the AutoGenerateFeatureTemplates flag, 
        //     set to false so our changes will stick
        if (layerDef.AutoGenerateFeatureTemplates)
          layerDef.AutoGenerateFeatureTemplates = false;

        //and commit
        precinctLayer.SetDefinition(layerDef);
      });
    }
  }
}

//sample of xml group template CIM

//<CIMGroupEditingTemplate xsi:type="typens:CIMGroupEditingTemplate">
//  <Description>some desc</Description>
//  <Name>My Group Template</Name>
//  <Tags>Group; Polygon</Tags>
//  <ToolProgID>8f79967b-66a0-4a1c-b884-f44bc7e26921</ToolProgID>
//  <ToolFilter xsi:type="typens:ArrayOfString">
//    <String>0a7c16b9-1cfd-467f-8ece-6ba376192431</String>
//    <String>7a61439f-21e3-50a9-1f9e-54545d8f53e4</String>
//    <String>acd53634-cbc7-44d5-bde9-692fa8d45850</String>
//  </ToolFilter>
//  <BaseName>North Precinct</BaseName>
//  <BasePart xsi:type="typens:CIMGroupEditingTemplatePart">
//    <LayerURI>CIMPATH=portland_crimes/portland_pd_precincts.xml</LayerURI>
//    <Name>North Precinct</Name>
//    <TransformationID>esri_editing_transformation_polygonPrimaryIdentity</TransformationID>
//  </BasePart>
//  <Parts xsi:type="typens:ArrayOfCIMGroupEditingTemplatePart">
//    <CIMGroupEditingTemplatePart xsi:type="typens:CIMGroupEditingTemplatePart">
//      <LayerURI>CIMPATH=portland_2d/fire_stations.xml</LayerURI>
//      <Name>Fire Stations</Name>
//      <TransformationID>esri_editing_transformation_pointAtPolygonCentroid</TransformationID>
//    </CIMGroupEditingTemplatePart>
//    <CIMGroupEditingTemplatePart xsi:type="typens:CIMGroupEditingTemplatePart">
//      <LayerURI>CIMPATH=portland_2d/police_stations.xml</LayerURI>
//      <Name>Police Stations</Name>
//      <TransformationID>esri_editing_transformation_pointAtPolygonStart</TransformationID>
//    </CIMGroupEditingTemplatePart>
//  </Parts>
//</CIMGroupEditingTemplate>