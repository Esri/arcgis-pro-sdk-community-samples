/*

   Copyright 2018 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatasetCompatibility.UI;
using DatasetCompatibility.Extensions;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace DatasetCompatibility.Buttons
{
  internal class DatasetCompatibilityButton : Button
  {
    protected async override void OnClick()
    {
      var featLayers = MapView.Active?.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();

     var str = await QueuedTask.Run(() => {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Format("{0,-28} {1,-16} {2,-17} {3,-10}","Name", "GeodatabaseType","RegistrationType","EditOpType"));
        sb.AppendLine(string.Format("{0}", new string('_', 75)));

        foreach (var flayer in featLayers)
        {
          var name = flayer.Name.PadRight(28);
         //Plugins do not have a GeodatabaseType or EditOperationType
         //they are read-only
         var gdb = flayer.GetGeodatabaseType()?.ToString().PadRight(16) ?? "";
         var regtype = flayer.GetRegistrationType()?.ToString().PadRight(17) ?? "";
         var editoptype = flayer.GetEditOperationType()?.ToString().PadRight(10) ?? "";
         sb.AppendLine($"{name} {gdb} {regtype} {editoptype}");
        }
        return sb.ToString();
      });
      MessageBox.Show(str, "Dataset Compatibility");
    }
  }
}
