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
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using TableViewerTest.Helper;

namespace TableViewerTest
{
  /// <summary>
  /// This sample exercises TableView functionality.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data) specifically the CommunitySampleData-ParcelFabric data.  Make sure that the Sample data is unzipped in c:\data\ParcelFabric.
  /// 1. Open this solution in Visual Studio.
  /// 1. Click the build menu and select Build Solution.
  /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
  /// 1. Open the project "ParcelIsland.aprx" in the "C:\Data\ParcelFabric\Island" folder.
  /// 1. After the map view opens click on the 'TableView Tester' tab. 
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Click on the 'Tsunami/Hurricane' button to change the Attribute TableView's layout
  /// ![UI](Screenshots/Screen2.png)
  /// </remarks>
  internal class Module1 : Module
  {

    internal static List<string> RegisteredFeatureLayers = new();
    internal static readonly string TaxParcelPolygonLayerName = "Tax";
    internal static readonly string TaxParcelLineLayerName = "Tax_Lines";
    internal static readonly string MisclosurePrefix = "Misclose distance: ";
    internal static readonly double MisclosureMinDistance = 0.10;
    internal static readonly double DistanceDiscrepanyAlert = 0.10; // change cell color for discrepancy to red if discrepany is more
    internal static readonly List<string> TaxTableHiddenFields = new() { "Shape", "CreatedByRecord", "RetiredByRecord", "CalculatedArea", "IsSeed", "created_user", "created_date", 
      "last_edited_user", "last_edited_date", "Shape_Length", "Shape_Area", "GlobalID","VALIDATIONSTATUS"};
    internal static readonly List<string> TaxTableFrozenFields = new() { "Name" };
    internal static readonly List<string> TaxLineTableHiddenFields = new() { "Shape", "CreatedByRecord", "RetiredByRecord", "ParentLineID", "Radius2", "COGOType", "IsCOGOGround", "Rotation", "Scale", "DirectionAccuracy", "DistanceAccuracy", "created_user", "created_date", "last_edited_user", "last_edited_date", "GlobalID", "VALIDATIONSTATUS", "LabelPosition" };
    internal static readonly List<string> TaxLineTableFrozenFields = new() { "OBJECTID" };
    internal static readonly string MisclosedTaxParcelTableCaption = "Tax Parcels";
    internal static readonly string TaxLinesTableCaption = "Boundary Lines";
    internal static IReadOnlyList<long> SampleTaxParceelOids = new List<long>() { 13543 };  //  13543, 13544 
    internal static readonly string PlatOnOffLayerName = "Plat Scans";
    internal static readonly Dictionary<string, int> TaxLineSignificantDigits = new Dictionary<string, int>() { { "Distance", 2 }, { "Direction", 2 }, { "Radius", 2 }, { "ArcLength", 2 }, { "Shape_Length", 2 } };
    internal static readonly Dictionary<string, int> TaxSignificantDigits = new Dictionary<string, int>() { { "StatedArea", 0 }, { "CalculatedArea", 0 }, { "MiscloseRatio", 0 }, { "MiscloseDistance", 2 }, { "Shape_Area", 0 } };

    private static Module1 _this = null;
    
    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("TableViewerTest_Module");

    protected override bool Initialize()
    {
      // subscribe to map view initialized in order to add required table views
      MapViewInitializedEvent.Subscribe(new Action<MapViewEventArgs>((mapViewEventArgs) =>
      {
        var map = mapViewEventArgs.MapView?.Map;
        if (map != null)
        {
          var showTableView4MapMembers = new List<MapMember>();
          var featLayers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList();
          foreach (var featLayer in featLayers)
          {
            if (featLayer.Name.Equals (Module1.TaxParcelPolygonLayerName))
              showTableView4MapMembers.Add(featLayer);
          }
          // We can only open the table views we need after the mapview has been initialized
          if (showTableView4MapMembers.Count > 0)
          {
            // the only way to get this to work is to delay 
            // otherwise opening the TablePane will through an error
            Task.Delay(new TimeSpan(0, 0, 3)).ContinueWith(o =>
            {
              System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () =>
                {
                    foreach (var showTableView in showTableView4MapMembers)
                      OpenAndActivateTablePane(showTableView);
                });
            });
          }
        }
      }));
      return base.Initialize();
    }

    #region Utility Functions

    /// <summary>
    /// utility function to open and activate the TablePane for a given MapMember
    /// </summary>
    /// <param name="mapMember">table to have the table view activated</param>
    internal static (ITablePane TablePane, ITablePaneEx TablePaneEx) OpenAndActivateTablePane(MapMember mapMember)
    {
      try
      {
        // check the open panes to see if it's open but just needs activating
        IEnumerable<ITablePane> tablePanes = FrameworkApplication.Panes.OfType<ITablePane>();
        foreach (var tablePane in tablePanes)
        {
          if (tablePane.MapMember != mapMember) continue;
          var pane = tablePane as Pane;
          pane?.Activate();
          return (pane as ITablePane, pane as ITablePaneEx);
        }
        // it's not currently open... so open it
        if (FrameworkApplication.Panes.CanOpenTablePane(mapMember))
        {
          var iTablePane = FrameworkApplication.Panes.OpenTablePane(mapMember);
          return (iTablePane as ITablePane, iTablePane as ITablePaneEx);
        }
        return (null, null);
      }
      catch (Exception ex)
      {
        throw new Exception($@"Error in [{DiagnosticHelper.GetMethodName()}]: {ex.Message}");
      }
    }

    /// <summary>
    /// Make the given TablePane the active TableView
    /// </summary>
    /// <param name="mapMember"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal static (ITablePane TablePane, ITablePaneEx TablePaneEx) ActivateTablePane(MapMember mapMember)
    {
      try
      {
        // check the open panes to see if it's open but just needs activating
        IEnumerable<ITablePane> tablePanes = FrameworkApplication.Panes.OfType<ITablePane>();
        foreach (var tablePane in tablePanes)
        {
          if (tablePane.MapMember != mapMember) continue;
          var pane = tablePane as Pane;
          pane?.Activate();
          return (pane as ITablePane, pane as ITablePaneEx);
        }
        return (null, null);
      }
      catch (Exception ex)
      {
        throw new Exception($@"Error in [{DiagnosticHelper.GetMethodName()}]: {ex.Message}");
      }
    }

    /// <summary>
    /// utility function to find the TablePane for a given MapMember
    /// </summary>
    /// <param name="mapMember">table to have the table view activated</param>
    internal static ITablePane GetTablePaneForMapMember(MapMember mapMember)
    {
      try
      {
        // check the open panes to see if it's open but just needs activating
        IEnumerable<ITablePane> tablePanes = FrameworkApplication.Panes.OfType<ITablePane>();
        foreach (var tablePane in tablePanes)
        {
          if (tablePane.MapMember != mapMember) continue;
          var pane = tablePane as Pane;
          pane?.Activate();
          return pane as ITablePane;
        }
        // it's not currently open... so open it
        if (FrameworkApplication.Panes.CanOpenTablePane(mapMember))
        {
          return FrameworkApplication.Panes.OpenTablePane(mapMember);
        }
        return null;
      }
      catch (Exception ex)
      {
        throw new Exception($@"Error in [{DiagnosticHelper.GetMethodName()}]: {ex.Message}");
      }
    }

    internal static void CloseTablePane(MapMember mapMember)
    {
      // check the open panes to see if it's open but just needs activating
      IEnumerable<ITablePane> tablePanes = FrameworkApplication.Panes.OfType<ITablePane>();
      foreach (var tablePane in tablePanes)
      {
        if (mapMember != null && tablePane.MapMember != mapMember) continue;
        var pane = tablePane as Pane;
        pane?.Close();
      }
    }

    internal static void ChangeConditionState (string condition, bool state)
    {
      if (state)
      {
        // set state to true => activate it
        if (!FrameworkApplication.State.Contains(condition))
        {
          FrameworkApplication.State.Activate(condition); //activates the state
        }
      }
      else
      {
        // set state to false => deactivate it
        if (FrameworkApplication.State.Contains(condition))
        {
          FrameworkApplication.State.Deactivate(condition); //activates the state
        }
      }
    }

    #endregion

    #region TableView Utilities

    /// <summary>
    /// Reset the table views
    /// </summary>
    /// <param name="tableView">table View</param>
    internal static async void ResetTableView(TableView tableView)
    {
      try
      {
        if (tableView == null || tableView.IsReady == false)
        {
          DiagnosticHelper.WriteWarning(tableView == null ? "Active TableView is null" : "Active TableView is not ready");
          return;
        }
        // set visible and hidden columns
        tableView.ShowAllFields();

        // clear and reset frozen fields
        await tableView.ClearAllFrozenFieldsAsync();

        await tableView.SetViewMode(TableViewMode.eAllRecords);
        tableView.SetZoomLevel(100);
      }
      catch (Exception ex)
      {
        MessageBox.Show ($@"Error in [{DiagnosticHelper.GetMethodName()}]: {ex.Message}");
      }
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

  }
}
