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
using System.Text;
using System.Threading.Tasks;

namespace CheckSelectionLayer
{
  internal class CheckSelectionLayer : Button
  {
    protected override async void OnClick()
    {
      try
      {
        //get selected layer in toc
        var featLayer = MapView.Active.GetSelectedLayers().First() as FeatureLayer;
        if (featLayer == null)
          return;
        CIMBaseLayer baseLayer = await QueuedTask.Run(() => featLayer.GetDefinition());
        var definitionSetURI = GetDefinitionSetURI(baseLayer);
        var isSelectionLayer = !string.IsNullOrEmpty(definitionSetURI);
        MessageBox.Show($@"The layer ""{featLayer.Name}"" was created using ""Make Layer From Selected Features"": {isSelectionLayer}");
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.Message}\n{ex.StackTrace}");
      }
    }

    internal static string GetDefinitionSetURI(CIMBaseLayer baseLayer)
    {
      if (baseLayer is CIMGeoFeatureLayerBase geoFeatureLayerBase)
        return (geoFeatureLayerBase.FeatureTable?.DefinitionSetURI);
      else if (baseLayer is CIMKMLLayer kmlLayer)
        return (null);
      else if (baseLayer is CIMAnnotationLayer annotationLayer)
        return (annotationLayer.FeatureTable?.DefinitionSetURI);
      else if (baseLayer is CIMDimensionLayer dimensionLayer)
        return (dimensionLayer.FeatureTable?.DefinitionSetURI);

      return (null);
    }
  }
}
