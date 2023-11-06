/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Geoprocessing;
using System.Windows;
using System.IO;
using ArcGIS.Desktop.GeoProcessing;
using ArcGIS.Core.Events;

namespace GeoProcessingHistory
{
    internal class GetGPHistory : Button
    {

        protected override async void OnClick()
    {
      string tool1 = "management.GetCount";
      MapProjectItem mapProjItem = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("Map", StringComparison.CurrentCultureIgnoreCase));

      var map = await QueuedTask.Run(() => mapProjItem.GetMap());
      var ftrLayer = map.Layers[0] as FeatureLayer;
      //string tool1 = "management.GetCount";     
      var args1 = Geoprocessing.MakeValueArray(ftrLayer);
      var env = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
      GPExecuteToolFlags executeFlags = GPExecuteToolFlags.AddToHistory;

      // add gp event lisener
      ArcGIS.Desktop.Core.Events.GPExecuteToolEvent.Subscribe(e =>
      {
        string id = e.ID;                   // Same as history ID
        if (e.IsStarting == false)  // Execute completed
          _ = e.GPResult.ReturnValue;
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("event triggerred, do something.");
      });

      var t = await Geoprocessing.ExecuteToolAsync(tool1, args1, env, null, null, executeFlags);

      IEnumerable<IGPHistoryItem> hisItems = Project.Current.GetProjectItemContainer(Geoprocessing.HistoryContainerKey) as IEnumerable<IGPHistoryItem>;
      var counts = hisItems.Count();

      String hitemID = "";
      String hitemToolPath = "";
      IGPResult hitemGPResult = null;
      DateTime hitemTimeStamp;


      foreach (var hitem in hisItems)
      {
        // common IGPHistoryItem and Item properties
        hitemID = (hitem as Item).ID;
        hitemToolPath = hitem.ToolPath;
        hitemGPResult = hitem.GPResult;
        hitemTimeStamp = hitem.TimeStamp;
      }

      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("ID: " + hitemID + "\n" + "Tool Path: " + hitemToolPath + "\n" + "GPResults: " + hitemGPResult);

    }


  }
}



