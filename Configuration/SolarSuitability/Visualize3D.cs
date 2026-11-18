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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping.Ribbon;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSuitability
{
  internal class Visualize3D : Button
  {
    protected override async void OnClick()
    {
      var packHouseScene = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(x => x.Name == "Redlands Packing house district");
      if (packHouseScene == null) return;

      //check if a MapPane with the caption "Redlands Packing house district" already exists
      string mapPaneCaption = "Redlands Packing house district";
      IMapPane mapViewPane = FrameworkApplication.Panes.OfType<IMapPane>().FirstOrDefault((p) => p.Caption == mapPaneCaption);
      Map map = null;
      if (mapViewPane != null)
      {
        // activate the MapPane
        (mapViewPane as Pane).Activate();
        map = mapViewPane.MapView.Map; //map is null here. Need to investigate why.
        if (map == null)
        {
         // MessageBox.Show("No active map found in the MapPane.");
          return;
        }

      }
      else //no MapPane with the caption "Analysis" exists, open a new one
      {
        map = await QueuedTask.Run(() =>
        {
          var m = packHouseScene.GetMap();
          return m;
        });
        if (map == null) return;
        await map.OpenViewAsync();
      }

      ////Scene is open,lets add the Building Footprints layer to the scene.
      //await QueuedTask.Run(() =>
      //{
      //  //Add the Redlands packing house footprint layer to the scene.
      //  //var pathToLyrx = Path.Combine(Project.Current.HomeFolderPath, @"Suitable_Buildings-New.lyrx");
      //  var homePath = Project.Current.HomeFolderPath;
      //  var uri = new Uri($@"{homePath}\SolarEstimation.gdb\RPHDBuildingFootprints");
      //  var createParams = new LayerCreationParams(uri)
      //  {
      //    Name = "RPHDBuildingFootprints",
      //    IsVisible = true,
      //  };
      //  //Just in case map is null, lets get it again.
      //  map = MapView.Active?.Map;
      //  if (map !=null)
      //    LayerFactory.Instance.CreateLayer<FeatureLayer>(createParams, map);
      //});
    }
  }
}
