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
using ActiproSoftware.Windows.Extensions;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace EVChargers
{
  /// <summary>
  /// Represents the dockpane that holds the results of the Filter search for charging station.
  /// </summary>
  internal class DisplayResultsViewModel : DockPane
  {
    private const string _dockPaneID = "EVChargers_DisplayResults";

    protected DisplayResultsViewModel() {
      Module1.DisplayResultsVM = this;
    }

    protected override void OnShow(bool isVisible)
    {
    }

    /// <summary>
    /// UpdateResultsDockpane the DockPane.
    /// This is called when you apply the filter.
    /// </summary>
    internal static void UpdateResultsDockpane()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;
      pane.Activate();
      //Clear the results in the dockpane
      Module1.DisplayResultsVM?.SearchLocationResults.Clear();
      //Iterate through the results from the filter and update the dockpane display collection
      foreach (var chargerItem in Module1.EVChargerLocationItems)
      {
        Module1.DisplayResultsVM?.SearchLocationResults.Add(chargerItem);
      }
      if (Module1.DisplayResultsVM?.SearchLocationResults.Count == 0) return;
      //Select the first item in the dockpane
      Module1.DisplayResultsVM.SelectedLocationResult = Module1.DisplayResultsVM?.SearchLocationResults[0];
      //Get the OID of the selected item
      var oidOfSelectedResult = Module1.DisplayResultsVM.SelectedLocationResult.OID;
      //Select this one in the map also
      var selectionDictionary = new Dictionary<FeatureLayer, List<long>>();
      selectionDictionary.Add(Module1.Current.EVChargersFeatureLayer, new List<long> { oidOfSelectedResult });
      QueuedTask.Run(() =>
      {
        MapView.Active.Map.SetSelection(SelectionSet.FromDictionary(selectionDictionary), SelectionCombinationMethod.New);
      });
    }
    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Search Results";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
    private ObservableCollection<EVChargerLocationItem> _searchLocationResults = new ObservableCollection<EVChargerLocationItem>();
    public ObservableCollection<EVChargerLocationItem> SearchLocationResults
    {
      get => _searchLocationResults;
      set
      {
        SetProperty(ref _searchLocationResults, value);
        SelectedLocationResult = SearchLocationResults[0];
      }
    }
    private EVChargerLocationItem _selectedLocationResult;
    /// <summary>
    /// In the list of items in the list box, if you select an item, flash and zoom to the item.
    /// </summary>
    public EVChargerLocationItem SelectedLocationResult
    {
      get => _selectedLocationResult;
      set
      {
        SetProperty(ref _selectedLocationResult, value);
        SelectedLocationResult?.FlashAndZoomToFeature();
      }
    }

    private string _searchString;
    //Search the displayed results. 
    public string SearchString
    {
      get => _searchString;
      set
      {
        SetProperty(ref _searchString, value);
        if (!string.IsNullOrEmpty(value)) //character was entered in the search box
        {
          //Iterate through the dockpane items and find items that match the station names
          var searchList = new List<EVChargerLocationItem>();
          foreach (var result in SearchLocationResults)
          {
            if (result.StationName.StartsWith(SearchString, StringComparison.CurrentCultureIgnoreCase))
            {
              searchList.Add(result);
            }
          }
          //Clear the items in the dockpane
          SearchLocationResults.Clear();
          //Populate with the subset that contains the search text in the station name
          SearchLocationResults.AddRange(searchList);
        }
        else  //No character was entered or the search string was cleared.
        //repopulate with the original contents.
        if (string.IsNullOrEmpty(value))
        {
          SearchLocationResults.Clear();
          foreach (var result in Module1.EVChargerLocationItems) //restore the contents
          {
              SearchLocationResults.Add(result); 
            
          }
        }

      }
    }

  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class DisplayResults_ShowButton : Button
  {
    protected override void OnClick()
    {
      DisplayResultsViewModel.UpdateResultsDockpane();
    }
  }
}
