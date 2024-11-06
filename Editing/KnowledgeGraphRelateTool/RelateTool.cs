//
// Copyright 2024 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgeGraphRelateTool
{
  /// <summary>
  /// A tool to create KnowledgeGraph relate records for a specified pair of entities types.
  /// 
  /// The UI requires the user to specify the source and destination entity types along with the relate type and 
  /// the relate direction.  
  /// The tool uses the SketchTip property to direct the user to identify a single source entity. Once a source entity
  /// has been found, the SketchTip is updated to direct the user to identify destination entities. 
  /// Once destination entities have been found, a KnowledgeGraphRelationshipDescription is created
  /// for each source and destination entity pairs. 
  /// </summary>
  internal class RelateTool : MapTool
  {
    public RelateTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Map;

      OverlayControlID = "KnowledgeGraphRelateTool_RelateEmbeddableControl";
    }

    // state of the tool.  0 = identify source entity, 1 = identify destination entities
    private int _state = 0;
    // the oid of the source entity
    private long _sourceOID = -1;

    // reset the state of the tool
    private void Reset()
    {
      _state = 0;
      _sourceOID = -1;
      SketchTip = "Identify a single source entity";
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      // initialize the overlay control with a KnowledgeGraphLayer
      var kgLayer = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<KnowledgeGraphLayer>().FirstOrDefault();
      var control = OverlayEmbeddableControl as RelateEmbeddableControlViewModel;
      control.Init(kgLayer);

      // reset the state of the tool
      Reset();

      return Task.CompletedTask;
    }

    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (geometry == null)
        return false;

      var control = OverlayEmbeddableControl as RelateEmbeddableControlViewModel;
      var sourceLayerName = control.SourceEntity;
      var destinationLayerName = control.DestinationEntity;
      var relateLayerName = control.Relate;
      var isRelateForward = control.IsDirectionForward;

      // check we have values
      if (string.IsNullOrEmpty(sourceLayerName) || string.IsNullOrEmpty(destinationLayerName) || string.IsNullOrEmpty(relateLayerName))
      {
        MessageBox.Show("Please choose a source entity, destination entity and a relate.", "Relate");
        return false;
      }

      // check source is not the same as destination
      if (sourceLayerName.CompareTo(destinationLayerName) == 0)
      {
        MessageBox.Show("Please choose a destination entity different than the source entity.", "Relate");
        return false;
      }

      // get the layers
      var sourceLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where(l => l.Name == sourceLayerName).FirstOrDefault();
      var destinationLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where(l => l.Name == destinationLayerName).FirstOrDefault();
      var relateLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where(l => l.Name == relateLayerName).FirstOrDefault();

      (bool success, string msg) = await QueuedTask.Run(() =>
      {
        IList<long> destinationOIDs = null;

        // get the set of features intersection the sketch
        var set = MapView.Active.GetFeatures(geometry);
        if (_state == 0)
        {
          // determine source feature
          if (!set.Contains(sourceLayer))
            return (false, "No features from the source entity were identified");

          _sourceOID = set[sourceLayer].FirstOrDefault();
        }
        else
        {
          // determine destination features
          if (!set.Contains(destinationLayer))
            return (false, "No features from the destination entity were identified");

          destinationOIDs = set[destinationLayer];
        }

        // increment the state of the tool
        _state++;

        if (_state == 1)
        {
          // update the sketch tip
          SketchTip = "Identify destination entities";
          // start another sketch to get the destination ids
          this.StartSketchAsync();
        }
        else
        {
          // we have source and destination entities.  
          // now build and execute the editOperation
          try
          {
            var op = new EditOperation();
            op.Name = "Create " + relateLayerName + " relationships";

            // build a RowHandle for the source feature
            var sourceRowHandle = new RowHandle(sourceLayer, _sourceOID);
            // iterate through the destination features
            foreach (var destOID in destinationOIDs)
            {
              // build a rowHandle
              var destinationRowHandle = new RowHandle(destinationLayer, destOID);

              // build the KnowledgeGraphRelationshipDescription
              KnowledgeGraphRelationshipDescription rd = null;
              if (isRelateForward)
                rd = new KnowledgeGraphRelationshipDescription(sourceRowHandle, destinationRowHandle);
              else
                rd = new KnowledgeGraphRelationshipDescription(destinationRowHandle, sourceRowHandle);

              // queue the create
              op.Create(relateLayer, rd);
            }
            // execute the edit
            var success = op.Execute();
            if (!success)
              return (false, op.ErrorMessage);
          }
          catch (Exception ex)
          {
            return (false, ex.Message);
          }
          finally
          {
            // reset the state of the tool
            Reset();
          }
        }
        
        return (true, "");
      });

      if (!success)
        MessageBox.Show(msg, "Relate");

      return true;
    }
  }
}
