/*

   Copyright 2024 Esri

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
using ArcGIS.Core.Data.UtilityNetwork.Trace;
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CreateLineOfSight
{
  internal class Reset : Button
  {
    protected override async void OnClick()
    {
      var mapView = MapView.Active;
      if (mapView == null)
        return;
      var lineOfSightLayer = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(layer => layer.Name.Equals("LineOfSights"));
      var obstructionPtsLayer = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(layer => layer.Name.Equals("Obstructions"));
      var targetPointLayer = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(layer => layer.Name.Equals("TargetPoints"));

      var deleteFeatures = new EditOperation() { Name = "Delete Features" };
      await QueuedTask.Run(() =>
      {
        var LOS = mapView.Map.GetLayersAsFlattenedList().OfType<GroupLayer>()
                  .First(gl => gl.Name == "LoS");
        LOS.SetVisibility(false);
        mapView.Map.ClearSelection();

        var lineOfSightLayerCursor = lineOfSightLayer.Search();
        while (lineOfSightLayerCursor.MoveNext())
        {
          var feature = lineOfSightLayerCursor.Current;
          deleteFeatures.Delete(feature);
        }

        var obstructionPtsLayerCursor = obstructionPtsLayer.Search();
        while (obstructionPtsLayerCursor.MoveNext())
        {
          var feature = obstructionPtsLayerCursor.Current;
          deleteFeatures.Delete(feature);
        }

        targetPointLayer.SetLabelVisibility(false);
      }); 
     
      if (!deleteFeatures.IsEmpty)
      {
        var result = deleteFeatures.ExecuteAsync();
        await Project.Current.SaveEditsAsync();
      }
    }
  }
}
