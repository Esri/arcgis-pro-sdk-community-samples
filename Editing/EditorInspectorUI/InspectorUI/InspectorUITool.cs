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
using ArcGIS.Core.Internal.CIM;
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
using Geometry = ArcGIS.Core.Geometry.Geometry;
using QueryFilter = ArcGIS.Core.Data.QueryFilter;

namespace EditorInspectorUI.InspectorUI
{
  internal class InspectorUITool : MapTool
  {   
    public InspectorUITool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Screen;
      //sets the DAML ID of the embeddable control that hosts the Inspector UI
      //to show in the dock pane when the tool is active.
      ControlID = "EditorInspectorUI_InspectorUI_AttributeControl";
    }
    private AttributeControlViewModel _attributeControlVM = null;
    protected override async Task OnToolActivateAsync(bool active)
    {
      //get a reference to the attribute control view model
      if (_attributeControlVM == null)
      {
        _attributeControlVM = this.EmbeddableControl as AttributeControlViewModel;
      }
      //Get the building permits layer
      var buildingPermitsLayer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().
                FirstOrDefault(n => n.Name == "NYC building permits");
      await QueuedTask.Run( () => { 
         //get the selected features
         var selectionInMap = ActiveMapView.Map.GetSelection();

        if (buildingPermitsLayer == null) return;
        if (selectionInMap.ToDictionary().ContainsKey(buildingPermitsLayer))
        {
          //get the selected features oid list
          var selectionDictionary = new Dictionary<MapMember, List<long>>();
          selectionDictionary.Add(buildingPermitsLayer, selectionInMap.ToDictionary()[buildingPermitsLayer]);
          //load the first feature into the inspector
          var firstOid = selectionDictionary[buildingPermitsLayer][0];
          _attributeControlVM.AttributeInspector.Load(buildingPermitsLayer, firstOid);
          
          // set up a dictionary to "PermitRecord" objects for each selected feature
          //This is used to populate the tree view in the dock pane
          var selectionPermitRecordsDictionary = new Dictionary<MapMember, List<PermitRecord>>();
          var permitRecordList = new List<PermitRecord>();
          #region Create PermitRecords from the selection
          var queryFilter = new QueryFilter
          {
            ObjectIDs = selectionInMap.ToDictionary()[buildingPermitsLayer].ToArray()
          };
          var cursor = buildingPermitsLayer.Search(queryFilter);
          
          while (cursor.MoveNext())
          {
            var feature = cursor.Current as Feature;
            if (feature != null)
            {
              var oid = feature.GetObjectID();
              var jobNo = feature["Job_Number"].ToString();
              var jobType = feature["Job_Type"].ToString();
              var address = feature["Address"].ToString();
              var permitRecord = new PermitRecord(oid, jobNo, address, jobType);
              permitRecordList.Add(permitRecord);
            }
          }
          #endregion
          selectionPermitRecordsDictionary.Add(buildingPermitsLayer as MapMember, permitRecordList);

          // assign the dictionary to the view model - notifies the tree view to update
          _attributeControlVM.SelectedMapFeatures = selectionPermitRecordsDictionary;         
        }
      });
    }
    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      var buildingPermitsLayer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(n => n.Name == "NYC building permits");
      if (buildingPermitsLayer == null) return true;
      await QueuedTask.Run(() =>
      {
        //gather the selected features, using the selection geometry
        var selection = ActiveMapView.SelectFeatures(geometry, SelectionCombinationMethod.New).ToDictionary();
        if (!selection.ContainsKey(buildingPermitsLayer)) return;
        List<long> oidList = selection[buildingPermitsLayer];
        var selectionDictionary = new Dictionary<MapMember, List<long>>();
        //Selection dictionary is used to populate the inspector
        selectionDictionary.Add(buildingPermitsLayer as MapMember, oidList);

        //populate the inspector in the view model
        if (oidList.Count == 0)
          _attributeControlVM.AttributeInspector.LoadSchema(buildingPermitsLayer);
        else
        {          
          _attributeControlVM.AttributeInspector.Load(buildingPermitsLayer, oidList);
        }
        #region Create PermitRecords from the selection
        //store the selected features in the view model
        _attributeControlVM.SelectedMapFeatures.Clear();
        var queryFilter = new ArcGIS.Core.Data.QueryFilter
        {
          ObjectIDs = oidList.ToArray()
        };
        var cursor = buildingPermitsLayer.Search(queryFilter);
        var permitRecordList = new List<PermitRecord>();
        while (cursor.MoveNext())
        {
          var feature = cursor.Current as Feature;
          if (feature != null)
          {
            var oid = feature.GetObjectID();
            var jobNo = feature["Job_Number"].ToString();
            var jobType = feature["Job_Type"].ToString();
            var address = feature["Address"].ToString();  
            var permitRecord = new PermitRecord(oid, jobNo, address, jobType);
            permitRecordList.Add(permitRecord);
          }
        }
        // set up a dictionary to store the layer and the object IDs of the selected features
        var permitSelctionDictionary = new Dictionary<MapMember, List<PermitRecord>>();
        permitSelctionDictionary.Add(buildingPermitsLayer as MapMember, permitRecordList);
        #endregion
        // assign the dictionary to the view model - notifies the tree view to update
        _attributeControlVM.SelectedMapFeatures = permitSelctionDictionary;

        //load the first feature
         _attributeControlVM.AttributeInspector.Load(buildingPermitsLayer, selectionDictionary[buildingPermitsLayer][0]);
      });
      return true;
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      //if we have a viewmodel
      if (_attributeControlVM != null)
      {
        //free the embeddable control
        _attributeControlVM.InspectorView = null;
        _attributeControlVM.InspectorViewModel.Dispose();
        _attributeControlVM = null;
      }
      return Task.FromResult(0);
    }
  }
}
