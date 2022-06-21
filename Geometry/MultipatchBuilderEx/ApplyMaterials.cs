/*

   Copyright 2019 Esri

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

using System.Linq;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace MultipatchBuilderEx
{
  /// <summary>
  /// Applies materials to the newly created multipatch feature
  /// </summary>
  internal class ApplyMaterials : Button
  {
    protected override async void OnClick()
    {
      // make sure there's an OID from a created feature
      if (Module1.MultipatchOID == -1)
        return;

      if (MapView.Active?.Map == null)
        return;

      // find layer
      var member = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault(l => l.Name == "MultipatchWithTextureSimple") as FeatureLayer;
      if (member == null)
        return;

      // create some materials
      var materialRed = new BasicMaterial();
      materialRed.Color = System.Windows.Media.Colors.Red;

      var materialTransparent = new BasicMaterial();
      materialTransparent.Color = System.Windows.Media.Colors.White;
      materialTransparent.TransparencyPercent = 80;

      var materialBlue = new BasicMaterial();
      materialBlue.Color = System.Windows.Media.Colors.SkyBlue;

      bool result = await QueuedTask.Run(() =>
      {
        // get the multipatch shape using the Inspector
        var insp = new Inspector();
        insp.Load(member, Module1.MultipatchOID);
        var origMultipatch = insp.Shape as Multipatch;

        // create a builder
        var mpb = new ArcGIS.Core.Geometry.MultipatchBuilderEx(origMultipatch);
        // apply the materials to the patches
        var patches = mpb.Patches;
        patches[0].Material = materialRed;
        patches[1].Material = materialTransparent;
        patches[2].Material = materialRed; 
        patches[3].Material = materialBlue; 
        patches[4].Material = materialRed;
        patches[5].Material = materialRed;

        // use the QueryPatchIndicesWithMaterial method to determine patches which contain the specified material
        //var red = mpb.QueryPatchIndicesWithMaterial(materialRed);
        //var blue = mpb.QueryPatchIndicesWithMaterial(materialBlue);

        // get the modified multipatch geometry
        var newMultipatch = mpb.ToGeometry() as Multipatch;

        // modify operation
        var modifyOp = new EditOperation();
        modifyOp.Name = "Apply materials to multipatch";
        modifyOp.Modify(member, Module1.MultipatchOID, newMultipatch);

        if (modifyOp.Execute())
          return true;

        return false;
      });
    }
  }
}
