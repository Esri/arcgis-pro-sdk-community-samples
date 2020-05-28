/*

   Copyright 2020 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

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
using ArcGIS.Desktop.Mapping;

namespace RubiksCube
{
  internal class CreateCube : Button
  {
    protected override async void OnClick()
    {
      //get the multipatch layer from the map
      var localSceneLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                          .FirstOrDefault(l => l.Name == "Cube" && l.ShapeType == esriGeometryType.esriGeometryMultiPatch);
      if (localSceneLayer == null)
        return;

      // create the multipatch geometry
      var cubeMultipatch = CreateCubeMultipatch();

      // add the multipatch geometry to the layer
      string msg = await QueuedTask.Run(() =>
      {
        long newObjectID = -1;

        var op = new EditOperation();
        op.Name = "Create multipatch feature";
        op.SelectNewFeatures = false;

        // queue feature creation and track the newly created objectID
        op.Create(localSceneLayer, cubeMultipatch, oid => newObjectID = oid);
        // execute
        bool result = op.Execute();
        // if successful
        if (result)
        {
          // save the objectID in the module for other commands to use
          Module1.CubeMultipatchObjectID = newObjectID;

          // zoom to it
          MapView.Active.ZoomTo(localSceneLayer);

          return "";
        }

        // not successful, return any error message from the EditOperation
        return op.ErrorMessage;
      });

      // if there's an error, show it
      if (!string.IsNullOrEmpty(msg)) 
        MessageBox.Show($@"Multipatch creation failed: " +  msg);
    }

    private Multipatch CreateCubeMultipatch()
    { 
      double side = 5.0;

      // create the multipatch builder
      MultipatchBuilderEx cubeMultipatchBuilderEx = new MultipatchBuilderEx();

      // make the top face patch
      Patch topFacePatch = cubeMultipatchBuilderEx.MakePatch(esriPatchType.FirstRing);
      topFacePatch.Coords = new List<Coordinate3D>
      {
        new Coordinate3D(0, 0, side),
        new Coordinate3D(0, side, side),
        new Coordinate3D(side, side, side),
        new Coordinate3D(side, 0, side),
      };

      // make the bottom face patch
      Patch bottomFacePatch = cubeMultipatchBuilderEx.MakePatch(esriPatchType.FirstRing);
      bottomFacePatch.Coords = new List<Coordinate3D>
      {
        new Coordinate3D(0, 0, 0),
        new Coordinate3D(0, side, 0),
        new Coordinate3D(side, side, 0),
        new Coordinate3D(side, 0, 0),
      };

      // make the sides face patch
      Patch sidesFacePatch = cubeMultipatchBuilderEx.MakePatch(esriPatchType.TriangleStrip);
      sidesFacePatch.Coords = new List<Coordinate3D>
      {
        new Coordinate3D(0, 0, 0),
        new Coordinate3D(0, 0, side),
        new Coordinate3D(side, 0, 0),
        new Coordinate3D(side, 0, side),
        new Coordinate3D(side, side, 0),
        new Coordinate3D(side, side, side),
        new Coordinate3D(0, side, 0),
        new Coordinate3D(0, side, side),
        new Coordinate3D(0, 0, 0),
        new Coordinate3D(0, 0, side),
      };

      // add to the Patches collection on the builder
      cubeMultipatchBuilderEx.Patches.Add(topFacePatch);
      cubeMultipatchBuilderEx.Patches.Add(bottomFacePatch);
      cubeMultipatchBuilderEx.Patches.Add(sidesFacePatch);

      // create the geometry
      Multipatch cubeMultipatch = cubeMultipatchBuilderEx.ToGeometry() as Multipatch;
      return cubeMultipatch;
    }
  }
}
