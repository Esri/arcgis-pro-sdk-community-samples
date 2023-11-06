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
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitPolygon
{
  internal class SplitPolygon : Button
  {
    protected override void OnClick()
    {
try
{
  var mapView = MapView.Active;
  if (mapView == null) return;

  var polygonLayer = mapView.Map.GetLayersAsFlattenedList()
      .OfType<BasicFeatureLayer>().FirstOrDefault (lyr => lyr.Name == "TestPolygons");
  var lineLayer = mapView.Map.GetLayersAsFlattenedList()
      .OfType<BasicFeatureLayer>().FirstOrDefault(lyr => lyr.Name == "TestLines");

  if (polygonLayer == null || lineLayer == null)
  {
    throw new Exception($@"The split action requires both Layers: TestPolygons and TestLines");
  }
  QueuedTask.Run(() =>
  {
    ArcGIS.Core.Events.SubscriptionToken token = null;
    try
    {
      // create an edit operation
      EditOperation splitOperation = new()
      {
        Name = "Split polygons",
        ProgressMessage = "Working...",
        CancelMessage = "Operation canceled.",
        ErrorMessage = "Error splitting polygons",
        SelectModifiedFeatures = false,
        SelectNewFeatures = false
      };

      // initialize a list of ObjectIDs that to be split (selected TestPolygons)
      var splitTargetSelectionSet = SelectionSet.FromSelection(polygonLayer, polygonLayer.GetSelection());

      // initialize the list of ObjectIDs that are used for the splitting (selected TestLines)
      var splittingSelectionSet = SelectionSet.FromSelection(lineLayer, lineLayer.GetSelection());

      if (splitTargetSelectionSet.Count == 0 ||
          splittingSelectionSet.Count == 0)
        throw new Exception($@"You need to select at least one feature each in: {polygonLayer.Name} and {lineLayer.Name}");

      // perform the Split operation:
      // To be split (selected TestPolygons)
      // Used for the splitting (selected TestLines)
      splitOperation.Split(splitTargetSelectionSet, splittingSelectionSet);

      // start listening to RowCreate events in order to collect all new rows
      List<long> newOids = new();
      token = ArcGIS.Desktop.Editing.Events.RowCreatedEvent.Subscribe((ev) =>
      {
        newOids.Add (ev.Row.GetObjectID());
      }, polygonLayer.GetTable());

      if (splitOperation.IsEmpty)
        throw new Exception($@"Nothing to split");
      var result = splitOperation.Execute();
      if (result != true || splitOperation.IsSucceeded != true)
        throw new Exception($@"Cut failed: {splitOperation.ErrorMessage}");

      // create an edit operation
      splitOperation = new()
      {
        Name = "Update newly created split parts",
        ProgressMessage = "Working...",
        CancelMessage = "Operation canceled.",
        ErrorMessage = "Error splitting polygons",
        SelectModifiedFeatures = false,
        SelectNewFeatures = false
      };
      // update all newly created rows by the split operation
      foreach (long oid in newOids) 
      {
        var attributes = new Dictionary<string, object>()
        {
          { "Description", "Created by Split operation" }
        };
        splitOperation.Modify(polygonLayer, oid, attributes);
      }
      if (!splitOperation.IsEmpty)
      {
        result = splitOperation.Execute();
        if (result != true || splitOperation.IsSucceeded != true)
          throw new Exception($@"Edit of newly create split parts failed: {splitOperation.ErrorMessage}");
      }
      System.Diagnostics.Trace.WriteLine($@"Number of new rows: {newOids.Count}");
    }
    catch
    {
      throw;
    }
    finally
    {
      if (token != null)
        ArcGIS.Desktop.Editing.Events.RowCreatedEvent.Unsubscribe(token);
    }
  });
}
catch (Exception ex)
{
  MessageBox.Show(ex.ToString());
}
    }
  }
}
