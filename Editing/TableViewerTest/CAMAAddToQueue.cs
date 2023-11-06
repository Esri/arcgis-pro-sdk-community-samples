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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TableViewerTest
{
  internal class CAMAAddToQueue : Button
  {
    protected override async void OnClick()
    {
      try
      {        
        // activate the CAMA table view as the active view
        var camaTable = MapView.Active.Map.GetStandaloneTablesAsFlattenedList().OfType<StandaloneTable>().Where(fl => fl.Name.Equals("Changes to CAMA")).FirstOrDefault();
        if (camaTable == null) return;

        // Ask if the selection should be queued
        var taxTable = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Equals(Module1.TaxParcelPolygonLayerName)).FirstOrDefault();
        if (taxTable == null)
        {
          MessageBox.Show("Unable to find Tax Feature class in Table of Content");
          return;
        }
        var selectionCount = await QueuedTask.Run<long> (() => taxTable.GetSelection().GetCount());
        if (selectionCount == 0)
        {
          MessageBox.Show("No Tax parcels have been selected to add to the CAMA queue");
          return;
        }
        var result = MessageBox.Show($@"{selectionCount} Tax parcels have been selected. Add them to the CAMA Queue?", "Continue to submit to Queue", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
        if (result != System.Windows.MessageBoxResult.Yes) return;

        // post to CAMA
        var submissionDate = DateTime.Now;
        await QueuedTask.Run(() =>
        {
          var editOp = new EditOperation
          {
            Name = "Add to CAMA Queue"
          };
          var qf = new QueryFilter() { ObjectIDs = taxTable.GetSelection().GetObjectIDs() };
          var rowCursorParcels = taxTable.Search(qf);
          var attribs = new Dictionary<string, object>();
          while (rowCursorParcels.MoveNext())
          {
            using var parcelRow = rowCursorParcels.Current as Row;
            attribs.Clear();
            attribs.Add("TypeOfChange", "Disaster Tax Relief");
            attribs.Add("ParcelNo", parcelRow["TMK"]);
            attribs.Add("DateSubmitted", submissionDate);
            attribs.Add("AssessmentValue", "$0.00");
            attribs.Add("Note", "2021 Tonga Tsunami");
            attribs.Add("Posted", 0);
            editOp.Create(camaTable, attribs);
          }
          if (!editOp.Execute())
          {
            MessageBox.Show ($@"Unable to add to CAMA queue: {editOp.ErrorMessage}");
          }
          else
          {
            Project.Current.SaveEditsAsync();
          }
        });
        Module1.ChangeConditionState("state_AddToCAMA", false);
        Module1.ChangeConditionState("state_CamaPostReady", true);

        // Show CAMA (in case it's not currently shown)
        var command = FrameworkApplication.GetPlugInWrapper("TableViewerTest_ShowCAMA") as ICommand;
        if (command.CanExecute(null))
          command.Execute(null);
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.Message}");
      }
    }
  }
}
