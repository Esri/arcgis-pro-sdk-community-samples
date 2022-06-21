//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Core.Geometry;


namespace AnimationFromPath
{
  internal class SetTarget : MapTool
  {

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected async override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
    {
      base.OnToolMouseDown(e);

      await QueuedTask.Run(() =>
      {
        CreateAnimationFromPath.TargetPoint = MapView.Active.ClientToMap(e.ClientPoint);
      });
    }

    protected override void OnToolMouseUp(MapViewMouseButtonEventArgs e)
    {
      base.OnToolMouseUp(e);
      this.IsChecked = false;
    }
  }
}
