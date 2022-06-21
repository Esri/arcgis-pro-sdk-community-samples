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
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace MultipatchBuilderEx
{
  /// <summary>
  /// Creates a new triangle multipatch and stores it in a feature class. The multipatch has no materials and no textures. 
  /// </summary>
  internal class CreateTriangleMultipatch : Button
  {
    protected override async void OnClick()
    {
      if (MapView.Active?.Map == null)
        return;

      // find layer
      var member = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault(l => l.Name == "MultipatchWithTextureSimple") as FeatureLayer;
      if (member == null)
        return;

      // get the multipatch geometry
      var multipatch = MyMultipatchBuilder.CreateTriangleMultipatchGeometry();

      // create a new feature
      bool result = await QueuedTask.Run(() =>
      {
        var op = new EditOperation
        {
          Name = "Create multipatch feature",
          SelectNewFeatures = false
        };
        var rowToken = op.Create(member, multipatch);
        if (op.Execute())
        {
          // track the newly created objectID
          // save the oid in the module for other commands to use
          Module1.MultipatchOID = rowToken.ObjectID.Value;
          return true;
        }

        var msg = op.ErrorMessage;
        return false;
      });

    }
  }
}
