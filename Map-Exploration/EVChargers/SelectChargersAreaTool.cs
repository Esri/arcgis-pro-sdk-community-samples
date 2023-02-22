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
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace EVChargers
{
  /// <summary>
  /// Represents the tool that displays the embedded control with the Search filter for charger locations./// 
  /// </summary>
  /// <remarks>
  /// After filtereing, you can use the tool to select specific charger station to view the details.
  /// </remarks>
  internal class SelectChargersAreaTool : MapTool
  {
    public SelectChargersAreaTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Rectangle;
      SketchOutputMode = SketchOutputMode.Map;
      OverlayControlID = "EVChargers_EVChargersUI";
      OverlayControlCanResize = true;
      
    }
    /// <summary>
    /// Activating the tool queries the active map view to get the height and width neeeded to display the overlay.
    /// </summary>
    /// <param name="active"></param>
    /// <returns></returns>
    protected override Task OnToolActivateAsync(bool active)
    {
      //Set the User control size
      var viewSize = MapView.Active.GetViewSize();
      Module1.Current.EVViewModel.ControlWidth = viewSize.Width;
      Module1.Current.EVViewModel.ControlHeight = viewSize.Height / 6;      
      return base.OnToolActivateAsync(active);
    }
    /// <summary>
    /// When the Display Result dockpane displays the search results, you can sketch on the map view to select specific results.
    /// This will also highlight the selected item on the dockpane - two way binding at work here.
    /// </summary>
    /// <param name="geometry"></param>
    /// <returns></returns>
    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (Module1.EVChargerLocationItems.Count == 0)
        MessageBox.Show("Search for the charger location first.");
      var oid = await QueuedTask.Run(() =>
      {
        Dictionary<MapMember, List<long>> selectionDictionary = new Dictionary<MapMember, List<long>>();
        var selectionPoly = geometry as Polygon;
        if (selectionPoly != null)
        {
          //Select the features
          selectionDictionary = ActiveMapView.SelectFeatures(geometry, SelectionCombinationMethod.New).ToDictionary();
        }
         return selectionDictionary[Module1.Current.EVChargersFeatureLayer][0];     //returns the OID of the first selected item
      });
      if (Module1.DisplayResultsVM.SearchLocationResults.Any(e => e.OID == oid)) //Does the dockpane items contain the selected item?
      {
        Module1.DisplayResultsVM.SelectedLocationResult = Module1.DisplayResultsVM.SearchLocationResults.FirstOrDefault(f => f.OID == oid); //set the selection in the dockpane
        int index = Module1.DisplayResultsVM.SearchLocationResults.IndexOf(Module1.DisplayResultsVM.SelectedLocationResult); //move the item to the top
        Module1.DisplayResultsVM.SearchLocationResults.Move(index, 0);
      }

      return true;
    }   
  }
}
