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
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Events;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace EditOperationRowEvent
{
  internal class Initialize : Button
  {
    private SubscriptionToken _rowChangedToken, _rowCreatedToken, _rowDeletedToken;
    private StandaloneTable _ehTable;
    protected override void OnClick()
    {
      QueuedTask.Run(async() =>
        {
          //find layer and derive geodatabase
          var cpLayer = MapView.Active.Map.FindLayers("CrowdPlanning").FirstOrDefault() as FeatureLayer;
          var geodatabase = cpLayer.GetFeatureClass().GetDatastore() as Geodatabase;

          //Advise if the project has edits. Need to clear edits to make schema changes.
          if (Project.Current.HasEdits)
          {
            MessageBox.Show("Please save or discard edits", "Pending Edits");
            return;
          }

          //Delete and Create the editlog table
          //For the purpose of this sample, start with a fresh table
          var mva = Geoprocessing.MakeValueArray(geodatabase.GetPath().AbsolutePath,"EditLog");
          var cts = new System.Threading.CancellationTokenSource();
          await Geoprocessing.ExecuteToolAsync("Delete_management", mva);
          await Geoprocessing.ExecuteToolAsync("CreateTable_management", mva);

          //add fields to editlog
          var tablePath = geodatabase.GetPath().AbsolutePath + @"\EditLog";
          mva = Geoprocessing.MakeValueArray(tablePath, "Layer", "STRING");
          await Geoprocessing.ExecuteToolAsync("AddField_management", mva);
          mva = Geoprocessing.MakeValueArray(tablePath,"OID","LONG");
          await Geoprocessing.ExecuteToolAsync("AddField_management", mva);
          mva = Geoprocessing.MakeValueArray(tablePath, "Date", "DATE");
          await Geoprocessing.ExecuteToolAsync("AddField_management", mva);
          mva = Geoprocessing.MakeValueArray(tablePath, "EditType", "STRING");
          await Geoprocessing.ExecuteToolAsync("AddField_management", mva);

          _ehTable = MapView.Active.Map.FindStandaloneTables("EditLog").FirstOrDefault();

          //setup row events for layer
          if (_rowChangedToken == null)
            _rowChangedToken = RowChangedEvent.Subscribe(OnRowEvent,cpLayer.GetTable());
          if (_rowCreatedToken == null)
            _rowCreatedToken = RowCreatedEvent.Subscribe(OnRowEvent, cpLayer.GetTable());
          if (_rowDeletedToken == null)
            _rowDeletedToken = RowDeletedEvent.Subscribe(OnRowEvent, cpLayer.GetTable());
        });
    }

    private void OnRowEvent(RowChangedEventArgs obj)
    {
      //get the parent operation
      var parentEditOp = obj.Operation;

      //create values for the editlog
      var atts = new Dictionary<string, object>();
      atts.Add("Layer", obj.Row.GetTable().GetName());
      atts.Add("OID", obj.Row.GetObjectID());
      atts.Add("Date", DateTime.Now.ToShortTimeString());
      atts.Add("EditType", obj.EditType.ToString());

      //add a row to the editlog table
      //as part of the current running edit operation
      parentEditOp.Create(_ehTable, atts);
    }
  }
}
