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
using System.Windows.Input;
using TableViewerTest.Helper;

namespace TableViewerTest
{
  internal class CAMAShow : Button
  {
    private bool _tablePaneOn = false;

    protected override void OnClick()
    {
      try
      {
        var tableName = "Changes to CAMA";

        if (MapView.Active?.Map == null) throw new Exception($@"Error in [{DiagnosticHelper.GetMethodName()}]: No Active Map");

        // activate the CAMA table view as the active view
        var camaTable = MapView.Active.Map.GetStandaloneTablesAsFlattenedList().OfType<StandaloneTable>().Where(fl => fl.Name.Equals(tableName)).FirstOrDefault();
        if (camaTable == null) return;

        _tablePaneOn = !_tablePaneOn;
        if (_tablePaneOn)
        {
          // set the query definition to show non-posted data only
          SetCamaDefinitionQuery(camaTable, "posted = 0", "Queued Non-Posted records only");

          // run on UI thread
          var camaPane = Module1.OpenAndActivateTablePane(camaTable);
          if (camaPane.TablePane == null) throw new Exception($@"Unable to activate {camaTable.Name}");

          // override the default title of the TablePane to show Queued status
          camaPane.TablePaneEx.Caption = $@"Queued CAMA Changes";

          CustomizeCamaTableView(TableView.Active);
        }
        else // _tablePaneOn == false
        {
          var camaTablePanes = Module1.ActivateTablePane(camaTable);
          if (camaTablePanes.TablePane == null)
            throw new Exception($@"Unable to activate {camaTable.Name}");

          Module1.ResetTableView(camaTablePanes.TablePaneEx.TableView);

          Module1.CloseTablePane(camaTable);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"{ex.Message}");
      }
    }

    /// <summary>
    /// Update the query defintion for the given standalone table using the given whereclause and description
    /// </summary>
    /// <param name="standaloneTable">table for which to update the expression</param>
    /// <param name="whereClause"></param>
    /// <param name="description"></param>
    private static async void SetCamaDefinitionQuery(StandaloneTable standaloneTable, string whereClause, string description)
    {
      await QueuedTask.Run(() =>
      {
        // change the definition query
        standaloneTable.RemoveAllDefinitionQueries();
        standaloneTable.InsertDefinitionQuery(new DefinitionQuery() { Name = description, WhereClause = whereClause }, true);
      });
    }

    /// <summary>
    /// Shows the CAMA queue, only the non-posted CAMA records: where Posted = 0
    /// </summary>
    /// <param name="tableView">Cama table View</param>
    private static async void CustomizeCamaTableView(TableView tableView)
    {
      // change the camaTableView as well
      if (tableView == null || tableView.IsReady == false)
      {
        DiagnosticHelper.WriteWarning(tableView == null ? "Active TableView is null" : "Active TableView is not ready");
        throw new Exception(tableView == null ? "Active TableView is null" : "Active TableView is not ready");
      }
      // set visible and hidden columns
      // hide PostedDate and Posted Columns
      tableView.ShowAllFields();
      tableView.SetHiddenFields(new List<string> { "PostedDate", "Posted", "PostedBy" });

      // clear and reset frozen fields
      var frozenFields = tableView.GetFrozenFields();
      if (!frozenFields.Contains("TypeOfChange"))
      {
        await tableView.ClearAllFrozenFieldsAsync();
        await tableView.SetFrozenFieldsAsync(new List<string> { "ObjectId", "ParcelNo", "TypeOfChange", "DateSubmitted" });
      }
      await tableView.SetViewMode(TableViewMode.eAllRecords);
      tableView.SetZoomLevel(150);
    }
  }
}
