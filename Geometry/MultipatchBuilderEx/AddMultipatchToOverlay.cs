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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace MultipatchBuilderEx
{
  /// <summary>
  /// Add a multipatch geometry to the overlay.
  /// </summary>
  internal class AddMultipatchToOverlay : Button
  {
    protected override async void OnClick()
    {
      if (MapView.Active == null)
        return;

      // find a 3d symbol
      var spi = Project.Current.GetItems<StyleProjectItem>().First(s => s.Name == "ArcGIS 3D");
      if (spi == null)
        return;

      bool result = await QueuedTask.Run(() =>
      {
        var style_item = spi.SearchSymbols(StyleItemType.MeshSymbol, "").FirstOrDefault(si => si.Key == "White (use textures) with Edges_Material Color_12");
        var symbol = style_item?.Symbol as CIMMeshSymbol;
        if (symbol == null)
          return false;
        
        // get the multipatch shape 
        var origMultipatch = MyMultipatchBuilder.CreateTriangleMultipatchGeometry();

        // move it in the z direction
        var newMultipatch = GeometryEngine.Instance.Move(origMultipatch, 0, 0, 10) as Multipatch;

        CIMMultiPatchGraphic graphic = new CIMMultiPatchGraphic()
        {
          MultiPatch = newMultipatch,
          Symbol = symbol.MakeSymbolReference()
        };

        MapView.Active.AddOverlay(graphic);

        // or just use 
        // MapView.Active.AddOverlay(newMultipatch, symbol.MakeSymbolReference());

        return true;
      });
    }
  }
}
