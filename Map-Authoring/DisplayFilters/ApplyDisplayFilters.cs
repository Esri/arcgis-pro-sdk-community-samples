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

namespace DisplayFilters
{
  internal class ApplyDisplayFilters : Button
  {
    protected async override void OnClick()
    {
      //Get the Hydrology layer from the TOC
      var hydrologyLyr = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(f => f.Name == "Hydrology");
      if (hydrologyLyr == null)
      {
        MessageBox.Show("This renderer works with the Hydrology feature layer available with the ArcGIS Pro SDK Sample data", "Data missing");
        return;
      }
      await QueuedTask.Run(() => {

        //Get the CIM Definition
        var cimDefn = hydrologyLyr.GetDefinition() as CIMFeatureLayer;

        //Create a list of Display Filters
        var arrayDisplayFilters = new List<CIMDisplayFilter>()
                {
                    new CIMDisplayFilter{ Name = "StreamOrder > 6", WhereClause = "StreamOrder > 6", MinScale= 0, MaxScale=50000000},
                    new CIMDisplayFilter{ Name = "StreamOrder > 5", WhereClause = "StreamOrder > 5", MinScale= 50000000, MaxScale=20000000},
                    new CIMDisplayFilter{ Name = "StreamOrder > 4", WhereClause = "StreamOrder > 4", MinScale= 20000000, MaxScale=5000000},
                    new CIMDisplayFilter{ Name = "StreamOrder > 3", WhereClause = "StreamOrder > 3", MinScale= 5000000, MaxScale=1000000},
                    new CIMDisplayFilter{ Name = "StreamOrder > 2", WhereClause = "StreamOrder > 2", MinScale= 1000000, MaxScale=100000},
                    new CIMDisplayFilter{ Name = "StreamOrder > 1", WhereClause = "StreamOrder > 1", MinScale= 100000, MaxScale=24000},
                };

        cimDefn.DisplayFilters = arrayDisplayFilters.ToArray();
        cimDefn.EnableDisplayFilters = true;

        //Turn off the original layer visibility
        hydrologyLyr.SetDefinition(cimDefn);

      });
    }
  }
}
