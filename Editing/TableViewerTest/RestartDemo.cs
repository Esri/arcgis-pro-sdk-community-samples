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
using System.Windows.Threading;
using TableViewerTest.Helper;

namespace TableViewerTest
{
  internal class RestartDemo : Button
  {
    protected override async void OnClick()
    {
      try
      {
        #region CAMA settings

        // close the CAMA table view as the active view
        var camaTable = MapView.Active.Map.GetStandaloneTablesAsFlattenedList().OfType<StandaloneTable>().Where(fl => fl.Name.Equals("Changes to CAMA")).FirstOrDefault();
        if (camaTable == null) return;
        await QueuedTask.Run(() =>
        {
          camaTable.GetTable().DeleteRows(new QueryFilter());
        });
        Module1.ChangeConditionState("state_CamaPostReady", false);
        Module1.ChangeConditionState("state_AddToCAMA", false);
        Module1.ChangeConditionState("state_DisasterOverlay", false);
        // close table panes
        var lstMapMembers = new List<MapMember>
        {
          camaTable
        };
        foreach (var mapMember in lstMapMembers)
          Module1.CloseTablePane(mapMember);

        #endregion

        // discard all edits
        _ = Project.Current.DiscardEditsAsync();

        // activate the TAX (parcel) table view as the active view
        var taxTable = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Equals(Module1.TaxParcelPolygonLayerName)).FirstOrDefault();
        if (taxTable == null) return;
        await QueuedTask.Run(() => taxTable.ClearSelection());

        // reset the caption to the default table name
        var taxTablePanes = Module1.OpenAndActivateTablePane(taxTable);
        if (taxTablePanes.TablePane == null) return;
        taxTablePanes.TablePaneEx.Caption = taxTable.Name;

        Module1.ResetTableView(taxTablePanes.TablePaneEx.TableView);

        Module1.CloseTablePane(null);
      }
      catch (Exception ex)
      {
        throw new Exception($@"Error: {ex.Message}");
      }
    }

  }
}
