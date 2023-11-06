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

namespace TableViewerTest
{
  internal class CAMAPostTo : Button
  {
    protected override async void OnClick()
    {
      try
      {
        // activate the CAMA table view as the active view
        var camaTable = MapView.Active.Map.GetStandaloneTablesAsFlattenedList().OfType<StandaloneTable>().Where(fl => fl.Name.Equals("Changes to CAMA")).FirstOrDefault();
        if (camaTable == null) return;

        // Select all non posted CAMA records and post them (if any)
        var selectionCount = await QueuedTask.Run<long>(() =>
          {
            camaTable.RemoveAllDefinitionQueries();
            camaTable.Select(new QueryFilter() { WhereClause = "posted = 0" });
            return camaTable.GetSelection().GetCount();
          });
        if (selectionCount == 0)
        {
          MessageBox.Show("There are no records in the CAMA queue that can be posted");
          return;
        }
        var result = MessageBox.Show($@"{selectionCount} changes can be posted to CAMA.  Do you want to post these records?", "Continue to submit to Queue", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
        if (result != System.Windows.MessageBoxResult.Yes) return;

        var postedDate = DateTime.Now;

        // post to CAMA
        await QueuedTask.Run(() =>
        {
          var editOp = new EditOperation
          {
            Name = "Post CAMA Queue"
          };
          var qf = new QueryFilter() { WhereClause = "Posted = 0" };
          var rowCursorCama = camaTable.Search(qf);
          var attribs = new Dictionary<string, object>();
          while (rowCursorCama.MoveNext())
          {
            using var parcelRow = rowCursorCama.Current as Row;
            attribs.Clear();
            attribs.Add("Posted", 1);
            attribs.Add("PostedDate", postedDate);
            attribs.Add("PostedBy", System.Security.Principal.WindowsIdentity.GetCurrent().Name);

            editOp.Modify(camaTable, parcelRow.GetObjectID(), attribs);
          }
          if (!editOp.Execute())
          {
            MessageBox.Show($@"Unable to add to CAMA queue: {editOp.ErrorMessage}");
          }
          else
          {
            Project.Current.SaveEditsAsync();
          }
        });
        Module1.ChangeConditionState("state_CamaPostReady", false);

        // Show CAMA (in case it's not currently shown)
        var command = FrameworkApplication.GetPlugInWrapper("TableViewerTest_ShowPostedCAMA") as ICommand;
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
