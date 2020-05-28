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
using System.Windows.Media;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace RubiksCube
{
  internal class ApplyMaterials : Button
  {
    protected override async void OnClick()
    {
      // make sure there's an OID from a created feature
      if (Module1.CubeMultipatchObjectID == -1)
        return;

      //get the multipatch layer from the map
      var localSceneLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>()
                          .FirstOrDefault(l => l.Name == "Cube" && l.ShapeType == esriGeometryType.esriGeometryMultiPatch);
      if (localSceneLayer == null)
        return;

      // get the multipatch shape using the Inspector
      var insp = new Inspector();
      await insp.LoadAsync(localSceneLayer, Module1.CubeMultipatchObjectID);
      var multipatchFromScene = insp.Shape as Multipatch;

      // apply materials to the multipatch
      var multipatchWithMaterials = ApplyMaterialsToMultipatch(multipatchFromScene);


      // modify the multipatch geometry
      string msg = await QueuedTask.Run(() =>
      {
        var op = new EditOperation();
        op.Name = "Apply materials to multipatch";

        // queue feature modification
        op.Modify(localSceneLayer, Module1.CubeMultipatchObjectID, multipatchWithMaterials);
        // execute
        bool result = op.Execute();
        // if successful
        if (result)
        {
          return "";
        }

        // not successful, return any error message from the EditOperation
        return op.ErrorMessage;
      });

      // if there's an error, show it
      if (!string.IsNullOrEmpty(msg))
        MessageBox.Show($@"Multipatch update failed: " + msg);
    }

    private Multipatch ApplyMaterialsToMultipatch(Multipatch source)
    {
      // create material for the top face patch
      BasicMaterial topFaceMaterial = new BasicMaterial();
      topFaceMaterial.Color = Color.FromRgb(203, 65, 84);
      topFaceMaterial.Shininess = 150;
      topFaceMaterial.TransparencyPercent = 50;
      topFaceMaterial.EdgeWidth = 20;

      // create material for the bottom face patch
      BasicMaterial bottomFaceMaterial = new BasicMaterial();
      bottomFaceMaterial.Color = Color.FromRgb(203, 65, 84);
      bottomFaceMaterial.EdgeWidth = 20;

      // create material for the sides face
      BasicMaterial sidesFaceMaterial = new BasicMaterial();
      sidesFaceMaterial.Color = Color.FromRgb(133, 94, 66);
      sidesFaceMaterial.Shininess = 0;
      sidesFaceMaterial.EdgeWidth = 20;


      // create a builder using the source multipatch
      var cubeMultipatchBuilderEx = new MultipatchBuilderEx(source);

      // set material to the top face patch
      var topFacePatch = cubeMultipatchBuilderEx.Patches[0];
      topFacePatch.Material = topFaceMaterial;

      // set material to the bottom face patch
      var bottomFacePatch = cubeMultipatchBuilderEx.Patches[1];
      bottomFacePatch.Material = bottomFaceMaterial;

      // set material to the sides face patch
      var sidesFacePatch = cubeMultipatchBuilderEx.Patches[2];
      sidesFacePatch.Material = sidesFaceMaterial;

      // create the geometry
      Multipatch cubeMultipatchWithMaterials = cubeMultipatchBuilderEx.ToGeometry() as Multipatch;
      return cubeMultipatchWithMaterials;
    }
  }
}
