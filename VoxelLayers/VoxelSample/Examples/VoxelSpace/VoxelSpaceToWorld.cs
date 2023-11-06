/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Mapping.Voxel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelSample.Examples.VoxelSpace
{
  /// <summary>
  /// In this example, a voxel slice is created using 
  /// the center of the voxel volume as the position with 45 degrees orientation
  /// and convert the position and normal to world space
  /// </summary>
  internal class VoxelSpaceToWorld : Button
  {
    protected override void OnClick()
    {
      var voxelLayer = MapView.Active.GetSelectedLayers().OfType<VoxelLayer>().FirstOrDefault();
      if (voxelLayer == null)
      {
        //just get the first one if there is a voxel layer in the TOC
        voxelLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<VoxelLayer>().FirstOrDefault();
        if (voxelLayer == null)
          return;
      }

      QueuedTask.Run(() =>
      {
        if (voxelLayer.Visualization != VoxelVisualization.Volume)
          voxelLayer.SetVisualization(VoxelVisualization.Volume);
        voxelLayer.SetSliceContainerExpanded(true);
        voxelLayer.SetSliceContainerVisibility(true);

        var volume = voxelLayer.SelectedVariableProfile.Volume;
        var volumeSize = volume.GetVolumeSize();
        var sb = new StringBuilder();

        //delete all sections on the current volume
        foreach (var slices in volume.GetSlices())
          volume.DeleteSlice(slices);

        //compute for the center of a voxel layer
        //and get the voxel normal for 45deg
        var voxelCenterCoordinate = new Coordinate3D(volumeSize.X / 2, volumeSize.Y / 2, volumeSize.Z / 2);
        var voxelNormal = voxelLayer.GetNormal(45, 0.0);

        //create a slice from the coordinates
        volume.CreateSlice(new SliceDefinition()
        {
          Name = "Slice in 45 deg",
          VoxelPosition = voxelCenterCoordinate,
          Normal = voxelNormal,
          IsVisible = true
        });

        //convert the voxel coordinate and normal to world coordinate and normal
        var worldCoordinate = volume.VoxelCoordinateToWorld(voxelCenterCoordinate);
        var worldNormal = volume.VoxelNormalToWorld(voxelNormal);

        sb.AppendLine(string.Format("Voxel slice position: {0}, {1}, {2}", voxelCenterCoordinate.X, voxelCenterCoordinate.Y, voxelCenterCoordinate.Z));
        sb.AppendLine(string.Format("Converted to world coordinate: {0}, {1}, {2}", worldCoordinate.X, worldCoordinate.Y, worldCoordinate.Z));
        sb.AppendLine(string.Format("Voxel slice normal: {0}, {1}, {2}", voxelNormal.X, voxelNormal.Y, voxelNormal.Z));
        sb.AppendLine(string.Format("Converted to world normal: {0}, {1}, {2}", worldNormal.X, worldNormal.Y, worldNormal.Z));

        MessageBox.Show(sb.ToString());
      });
    }
  }
}
