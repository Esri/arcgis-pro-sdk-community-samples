//   Copyright 2015 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

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

using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace UpdateAttributesWithSketch
{
  internal class AttributeWithSketch : MapTool
  {
    public AttributeWithSketch()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Line;
      SketchOutputMode = ArcGIS.Desktop.Mapping.SketchOutputMode.Map;
    }

    protected override Task<bool> OnSketchCompleteAsync(ArcGIS.Core.Geometry.Geometry geometry)
    {
      //Simple check for selected layer
      if (MappingModule.ActiveTOC.SelectedLayers.Count == 0)
      {
        System.Windows.MessageBox.Show("Select a layer in the toc");
        return Task.FromResult(true);
      }

      //jump to CIM thread
      return QueuedTask.Run(async () =>
      {
        //Get the selected layer in toc
        var featLayer = MappingModule.ActiveTOC.SelectedLayers[0] as FeatureLayer;

        //find feature oids under the sketch for the selected layer
        var features = await MapView.Active.HitTestAsync(geometry, CancelableProgressor.None);
        var featOids = features.Where(x => x.Item1 == featLayer).Select(x => x.Item2).First();

        //update the attributes of those features
        var fi = new FeatureInspector(true);
        await fi.FillAsync(featLayer, featOids);
        await fi.Attributes.Where(a => a.FieldName == "PARCEL_ID").First().SetValueAsync(42);

        //create and execute the edit operation
        var op = new EditOperation();
        op.Name = "The ultimate answer";
        op.SelectModifiedFeatures = true;
        op.SelectNewFeatures = false;
        op.Modify(fi);
        return await op.ExecuteAsync();
      });
    }
  }
}
