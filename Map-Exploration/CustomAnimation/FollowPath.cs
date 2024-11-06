//   Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Editing;

namespace CustomAnimation
{
  /// <summary>
  /// Button used to construct keyframes from a selected 3D line feature.
  /// </summary>
  internal class FollowPath : Button
  {
    protected override async void OnClick()
    {
      var result = await QueuedTask.Run(() =>
      {
        var mapView = MapView.Active;
        if (mapView == null)
          return false;

        //Get the collection of line layers that have at least one feature selected.
        var selection = mapView.Map.GetSelection();
        var keyValuePairs = selection.ToDictionary().Where(kvp => (kvp.Key is BasicFeatureLayer) 
          && (kvp.Key as BasicFeatureLayer).ShapeType == esriGeometryType.esriGeometryPolyline);

				foreach (var kvp in keyValuePairs)
				{
					var layer = kvp.Key as BasicFeatureLayer;
					var oid = kvp.Value.First();

					//Get a cursor for the layer using the OID of the first selected feature.
					var oidField = layer.GetTable().GetDefinition().GetObjectIDField();
					var qf = new ArcGIS.Core.Data.QueryFilter() { WhereClause = string.Format("{0} = {1}", oidField, oid) };
					using var cursor = layer.Search(qf);

					if (cursor.MoveNext())
					{
						using var row = cursor.Current as Feature;

						if (row == null)
							continue;

						//If the feature doesn't have Z values in the geometry continue to the next layer.
						var polyline = row.GetShape();
						if (!polyline.HasZ)
							continue;

						//If the layer doesn't have 3D properties set continue to the next layer.
						var layerDef = layer.GetDefinition();
						var layer3DProperties = layerDef.Layer3DProperties;
						if (layer3DProperties == null)
							continue;

						//Get the vertical unit set on the layer and send it and the line feature to the module method to construct the keyframes.
						var verticalUnit = layer3DProperties.VerticalUnit;
						Animation.Current.CreateKeyframesAlongPath(polyline as Polyline, verticalUnit);
						return true;
					}
				}
				return false;
      });

      //If at least 1 selected 3D feature was not found show a message box.
      if (!result)
        MessageBox.Show("Select at least one 3D line feature.", "No 3D line features selected.");
    }
  }
}
