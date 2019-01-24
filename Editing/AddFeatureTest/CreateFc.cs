/*

   Copyright 2019 Esri

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

namespace AddFeatureTest
{
  internal class CreateFc : Button
  {
    protected override void OnClick()
    {
      if (MapView.Active == null) return;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      CreateFcAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    private async Task CreateFcAsync()
    {
      try
      {
        if (await Module1.FeatureClassExistsAsync (Module1.PointFcName))
          throw new Exception($@"You must delete the {Module1.PointFcName} feature class first");
        if (await Module1.FeatureClassExistsAsync(Module1.PolyFcName))
          throw new Exception($@"You must delete the {Module1.PolyFcName} feature class first");

        await Module1.CreateFcWithAttributesAsync(Module1.PointFcName, Module1.EnumFeatureClassType.POINT);
        await Module1.CreateFcWithAttributesAsync(Module1.PolyFcName, Module1.EnumFeatureClassType.POLYGON);
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.ToString()}");
      }
    }
  }
}
