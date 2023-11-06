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
using ActiproSoftware.Windows.Extensions;
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
  /// In this example, world space is converted to voxel's space
  /// by selecting a point in a map 
  /// the normal unit vector of 45degrees is converted to voxel's normal
  /// </summary>
  internal class WorldSpaceToVoxel : MapTool
  {
    public WorldSpaceToVoxel() : base()
    {
      this.OverlayControlID = "VoxelSample_VoxelEmbeddableControl";
      OverlayControlCanResize = true;
      OverlayControlPositionRatio = new System.Windows.Point(0, 0);
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
    {
      //On mouse down check if the mouse button pressed is the left mouse button. If it is handle the event.
      if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        e.Handled = true;
    }


    protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
    {
      //overlay control to show the coordinate selected on the map
      var vm = OverlayEmbeddableControl as VoxelEmbeddableControlViewModel;
      if (vm == null)
        return Task.FromResult(0);

      var voxelLayer = MapView.Active.GetSelectedLayers().OfType<VoxelLayer>().FirstOrDefault();
      if (voxelLayer == null)
      {
        //just get the first one if there is a voxel layer in the TOC
        voxelLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<VoxelLayer>().FirstOrDefault();
        if (voxelLayer == null)
          return Task.FromResult(0);
      }

      return QueuedTask.Run(() =>
      {
        //get the point and convert it to world coordinate
        var mapPoint = MapView.Active.ClientToMap(e.ClientPoint);
        Coordinate3D worldCoordinate = new Coordinate3D(mapPoint);

        var volume = voxelLayer.SelectedVariableProfile.Volume;
        var sb = new StringBuilder();

        //a normal unit vector of 45degrees in all dimension and normalized
        var normalVector = new Coordinate3D(0.5773502691896257, 0.5773502691896257, 0.5773502691896257);

        //converts the coordinates and normal vector to voxel space
        //voxel position alignment affects the coordinate calculation
        //if the alignment is set to center, it might show less than zero values,
        //when the selected world coordinate is at the edge of the voxel's coordinate
        var voxelCoordinate = volume.WorldCoordinateToVoxel(worldCoordinate);
        var voxelNormal = volume.WorldNormalToVoxel(normalVector);

        sb.AppendLine(string.Format("Selected world coordinate: {0}, {1}, {2}", worldCoordinate.X, worldCoordinate.Y, worldCoordinate.Z));
        sb.AppendLine(string.Format("Converted to voxel coordinate: {0}, {1}, {2}", voxelCoordinate.X, voxelCoordinate.Y, voxelCoordinate.Z));
        sb.Append(string.Format("Converted normal vector of 45 degrees to voxel normal: {0}, {1}, {2}", voxelNormal.X, voxelNormal.Y, voxelNormal.Z));
        vm.Text = sb.ToString();
      });
    }
  }
}
