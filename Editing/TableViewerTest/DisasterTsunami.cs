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
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TableViewerTest.Helper;

namespace TableViewerTest
{
  internal class DisasterTsunami : Button
  {
    private bool _tablePaneOn = false;

    protected override async void OnClick()
    {
      try
      {
        if (MapView.Active == null) throw new Exception($@"No Active Map");

        // First make the tsunami layer visible
        var tsunamiLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Equals("SouthShoreImpact")).FirstOrDefault() 
          ?? throw new Exception("Can't find 'SouthShoreImpact' Layer");

        // activate the Tax table view as the active view
        var taxTable = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Equals(Module1.TaxParcelPolygonLayerName)).FirstOrDefault()
          ?? throw new Exception($@"Can't find '{Module1.TaxParcelPolygonLayerName}' Layer");

        // Set the Tax_Lines as un-selectable layer and taxtable as selectable
        var taxLine = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Equals(Module1.TaxParcelLineLayerName)).FirstOrDefault()
          ?? throw new Exception($@"Can't find '{Module1.TaxParcelLineLayerName}' Layer");

        _tablePaneOn = !_tablePaneOn;

        if (_tablePaneOn)
        {
          await QueuedTask.Run(() =>
            {
              taxLine.SetSelectable(false);
              taxLine.ClearSelection();
              taxTable.SetScaleSymbols(true);
              taxTable.ClearSelection();
            });

          var taxTablePanes = Module1.OpenAndActivateTablePane(taxTable);
          if (taxTablePanes.TablePane == null)
            throw new Exception($@"Unable to activate {taxTable.Name}");

          TableView tableView = taxTablePanes.TablePaneEx.TableView;
          if (tableView == null) throw new Exception($@"Can't get TableView for {taxTable.Name}");
          if (tableView.IsReady)
          {
            // set visible and hidden columns
            tableView.ShowAllFields();
            // adds to hidden
            tableView.SetHiddenFields(new List<string> { "GlobaID", "CREATEDBYRECORD", "RETIREDBYRECORD", "CalculatedArea", "MiscloseRatio", "MiscloseDistance", "IsSeed", "created_user", "created_date", "last_edited_user", "last_edited_date", "Shape_Length", "Shape_Area", "VALIDATIONSTATUS", "Parcel", "TMK", "qpub_link", "Zone", "Section", "Plat", "Island" });
            // clear and reset frozen fields
            var frozenFields = tableView.GetFrozenFields();
            if (!frozenFields.Contains("Name"))
            {
              await tableView.ClearAllFrozenFieldsAsync();
              await tableView.SetFrozenFieldsAsync(new List<string> { "OBJECTID", "Name" });
            }
            await tableView.SetViewMode(TableViewMode.eSelectedRecords);
            tableView.SetZoomLevel(150);
          }
          else
          {
            throw new Exception($@"Can't get IsReady == true for {taxTable.Name} TableView");
          }
          // activate condition so we can see CAMA buttons on the Ribbon
          Module1.ChangeConditionState("state_DisasterOverlay", true);
          Module1.ChangeConditionState("state_AddToCAMA", true);

          await QueuedTask.Run(() =>
          {
            tsunamiLayer.SetVisibility(!tsunamiLayer.IsVisible);

            // select all tax records that are within the SouthShoreImpact zone
            // first get the complete SouthShoreImpact geometry
            var rowCursorOverlayPoly = tsunamiLayer.Search();
            Geometry disasterArea = null;
            while (rowCursorOverlayPoly.MoveNext())
            {
              using var feature = rowCursorOverlayPoly.Current as Feature;
              if (disasterArea == null)
              {
                disasterArea = feature.GetShape().Clone();
                continue;
              }
              disasterArea = GeometryEngine.Instance.Union(disasterArea, feature.GetShape().Clone());
            }
            var description = string.Empty;
            if (disasterArea.IsEmpty) return;

            // define the spatial query filter
            // exclude roads
            var spatialQuery = new SpatialQueryFilter()
            {
              WhereClause = "not (name like '%-999')",
              FilterGeometry = disasterArea,
              SpatialRelationship = SpatialRelationship.Intersects
            };
            // which tax polygons are selected
            (taxTable as FeatureLayer).Select(spatialQuery);

            // zoom the map to the selection
            MapView.Active?.ZoomToSelected(); // .ZoomTo(disasterArea.Extent.Expand(1.1,1.1,true));
          });
        }
        else // _tablePaneOn == false
        {
          var taxTablePanes = Module1.ActivateTablePane(taxTable);
          if (taxTablePanes.TablePane == null)
            throw new Exception($@"Unable to activate {taxTable.Name}");

          Module1.ResetTableView(taxTablePanes.TablePaneEx.TableView);

          Module1.CloseTablePane(taxTable);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error in [{DiagnosticHelper.GetMethodName()}]: {ex.Message}");
      }
    }

  }
}
