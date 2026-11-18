/*

   Copyright 2026 Esri

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
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.GeoProcessing;
using ArcGIS.Desktop.Internal.Mapping.Locate;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ArcGIS.Desktop.Internal.Framework.EsriPBf.Tile.Types;

namespace SolarSuitability
{
    internal class ResetDemo : Button
    {
    protected override async void OnClick()
    {
      var mapItem = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(x => x.Name == "Analysis");
      if (mapItem == null) return;
      Map map = null;
      //check if a MapPane with the caption "Analysis" already exists
      string mapPaneCaption = "Analysis";
      IMapPane mapViewPane = FrameworkApplication.Panes.OfType<IMapPane>().FirstOrDefault((p) => p.Caption == mapPaneCaption);
      if (mapViewPane != null)
      {
        // activate the MapPane
        (mapViewPane as Pane).Activate(); 
        map = MapView.Active?.Map;
      }
      else //no MapPane with the caption "Analysis" exists, open a new one
      {
        map = await QueuedTask.Run(() =>
        {
          var m = mapItem.GetMap();
          return m;
        });
        if (map == null) return;
        await map.OpenViewAsync();
      }
      var featureLayerToSymbolize = map.Layers.FirstOrDefault(n => n.Name == "RPHDBuildingFootprints") as FeatureLayer;
      if (featureLayerToSymbolize == null)
        return;
    await QueuedTask.Run(async () => {
      #region Set the renderer to a simple renderer with a default symbol
      var simpleRenderer = new SimpleRendererDefinition();
      var cimRenderer = featureLayerToSymbolize.CreateRenderer(simpleRenderer);
      featureLayerToSymbolize?.SetRenderer(cimRenderer);
      #endregion Set the renderer to a simple renderer with a default symbol
      #region Remove Layers and tables
      var layers = map.GetLayersAsFlattenedList();
      List<string> layersToLeave = new List<string>()
      {
        "RPHDBuildingFootprints", "DSM", "World Topographic Map", "World Hillshade"
      };
      foreach (var layer in layers)
      {
        if (!layersToLeave.Contains(layer.Name))
        {
          map.RemoveLayer(layer);
        }
      }
      //Remove Solar_Rad_Table from the map
      var solarRadtable = map.GetStandaloneTablesAsFlattenedList().FirstOrDefault(t => t.Name == "Step4_Solar_Rad_Table");
      if (solarRadtable != null)
        map.RemoveStandaloneTable(solarRadtable);
      #endregion Remove Layers and tables
      #region Turn on the visibility for RPHDBuildingFootprints
      featureLayerToSymbolize.SetVisibility(true); 
      #endregion
      #region Delete the "Mean" field from the RPHDBuildingFootprints layer
      //Get the table for the RPHDBuildingFootprints layer
      var inputTable =  featureLayerToSymbolize.GetTable();
      var parametersBuildingFootprintUpdate = Geoprocessing.MakeValueArray(
          inputTable, // Input Table
          "MEAN" //Field to delete
          );
      var progDlgBuildingFootprintUpdate = new ProgressDialog($@"Reset Demo: Deleting the Mean field", "Canceled", false);
      var progsrcBuildingFootprintUpdate = new CancelableProgressorSource(progDlgBuildingFootprintUpdate);
      var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
      // Execute DeleteField tool   
      var gpResultBuildingFootprintUpdate = await Geoprocessing.ExecuteToolAsync("management.DeleteField", parametersBuildingFootprintUpdate, environments, progsrcBuildingFootprintUpdate.Progressor);

      if (gpResultBuildingFootprintUpdate.IsFailed)
      {
        // display error messages if the tool fails, otherwise shows the default messages
        if (gpResultBuildingFootprintUpdate.Messages.Count() != 0)
        {
          Geoprocessing.ShowMessageBox(gpResultBuildingFootprintUpdate.Messages, progsrcBuildingFootprintUpdate.Message,
                                          gpResultBuildingFootprintUpdate.IsFailed ?
                                          GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
        else
        {
          MessageBox.Show($@"{progsrcBuildingFootprintUpdate.Message} failed with errorcode, check parameters.");
        }
      }
      if (progsrcBuildingFootprintUpdate.CancellationTokenSource.IsCancellationRequested)
      {
        MessageBox.Show("The operation was canceled.");
      }
      #endregion Delete the "Mean" field from the RPHDBuildingFootprints layer
      #region Turn on the DSM layer
      var dsmLayer = map.GetLayersAsFlattenedList().FirstOrDefault(l => l.Name == "DSM");
      if (dsmLayer != null)
          dsmLayer.SetVisibility(true);
      #endregion
      await MapView.Active?.ZoomToAsync(dsmLayer);
      #region Reset content of the Local Scene
      //Get the local scene
      var sceneItem = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(x => x.Name == "Redlands Packing house district");
      if (sceneItem != null)
      {
        var scene = sceneItem.GetMap();
        if (scene != null)
        {
          var layerToSymbolize = scene.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(n => n.Name == "RPHDBuildingFootprints");
          if (layerToSymbolize != null)
          {
            //Update the feature layer renderer
            layerToSymbolize.SetRenderer(Module1._buildingFootprintSimpleRenderer);
          }
        }
      }
      #endregion
     
    });
        }
        
    }
}
