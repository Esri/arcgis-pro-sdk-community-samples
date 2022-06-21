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

using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Editing.Templates;

namespace DivideLines
{
  /// <summary>Tool with an embeded control for dividing a polyline into number of parts of equal length or parts of a certain length.</summary>
  internal class DivideLinesTool : MapTool
  {
    /// <summary>Set properties for the tool.</summary>
    /// <remarks>Defines the embeded control the tool is associated with. See config.daml. Set as maptool.</remarks>
    public DivideLinesTool() : base()
    {
      ControlID = "DivideLines_DivideLines";
      IsSketchTool = false;
    }

    protected override async Task OnToolActivateAsync(bool hasMapViewChanged)
    {
      //jump into selection mode when the tool is activated.
      await ActivateSelectAsync(true);
    }
  }
}
