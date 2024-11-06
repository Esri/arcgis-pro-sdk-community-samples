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
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;

namespace SketchToolWithHalos
{
  internal class SketchToolHalo : MapTool
  {
    public SketchToolHalo()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Line;
      SketchOutputMode = SketchOutputMode.Map;

      SketchTipID = "SketchToolWithHalos_HaloEmbeddableControl";
      SketchTipControlPosition = SketchTipControlPosition.Center;
      IsSketchTipControlTransparent = true;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      return base.OnSketchCompleteAsync(geometry);
    }
  }
}
