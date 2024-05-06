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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using EditorInspectorUI.InspectorUI;

namespace EditorInspectorUI.InspectorUIProvider
{
	internal class InspectorProviderTool : MapTool
	{
    private AttributeControlProviderViewModel _attributeControlProviderVM = null;
    public InspectorProviderTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Screen;

      ControlID = "EditorInspectorUI_InspectorUIProvider_AttributeControlProvider";
    }

    protected override async Task OnToolActivateAsync(bool active)
    {
      //get a reference to the attribute control view model
      if (_attributeControlProviderVM == null)
      {
        _attributeControlProviderVM = this.EmbeddableControl as AttributeControlProviderViewModel;
      }

      await QueuedTask.Run(() => {
        var selectionInMap = ActiveMapView.Map.GetSelection();
        var layer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(n => n.Name == "NYC building permits");
        if (layer == null) return;
        if (selectionInMap.ToDictionary().ContainsKey(layer))
        {
          //get the selected features oid list
          var selectionDictionary = new Dictionary<MapMember, List<long>>();
          selectionDictionary.Add(layer, selectionInMap.ToDictionary()[layer]);
          //Now store this in the ViewModel, this property populates the tree view
          _attributeControlProviderVM.SelectedMapFeatures = selectionDictionary;
          //load the first feature
          _attributeControlProviderVM.AttributeInspector.Load(layer, selectionDictionary[layer][0]);
        }
      });
    }

    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      var layer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(n => n.Name == "NYC building permits");
      if (layer == null) return true;
      await QueuedTask.Run(() =>
      {
        //define the spatial filter using the selection rectangle
        var spatialFilter = new SpatialQueryFilter
        {
          FilterGeometry = geometry,

          SpatialRelationship = SpatialRelationship.Contains
        };
        //gather the selected features
        var selection = layer.Select(spatialFilter, SelectionCombinationMethod.New);
        var selection1 = ActiveMapView.SelectFeatures(geometry, SelectionCombinationMethod.New).ToDictionary();
        //List<long> oidList = selection.GetObjectIDs().ToList();
        List<long> oidList = selection1[layer];

        //populate the inspector in the view model
        if (oidList.Count == 0)
          _attributeControlProviderVM.AttributeInspector.LoadSchema(layer);
        else
          _attributeControlProviderVM.AttributeInspector.Load(layer, oidList);

        //store the selected features in the view model
        var selectionDictionary = new Dictionary<MapMember, List<long>>();
        selectionDictionary.Add(layer, oidList);
        _attributeControlProviderVM.SelectedMapFeatures = selectionDictionary;

      });
      return true;
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      //if we have a viewmodel
      if (_attributeControlProviderVM != null)
      {
        //free the embeddable control
        _attributeControlProviderVM.InspectorView = null;
        _attributeControlProviderVM.InspectorViewModel.Dispose();
        _attributeControlProviderVM = null;
      }
      return Task.FromResult(0);
    }
  }
}
