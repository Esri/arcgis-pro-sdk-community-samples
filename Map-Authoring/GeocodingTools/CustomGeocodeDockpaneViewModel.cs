/*

   Copyright 2017 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace GeocodingTools
{
  /// <summary>
  /// Provide geocoding functionality using the API functions from the ArcGIS.Desktop.Mapping.Geocoding namespace with a custom UI.  The 
  /// UI contains a search box, and a listbox to display the geocode results. When a specific listbox item is selected, zoom to the 
  /// extent of the result and add a symbol to the mapView overlay at it's location. 
  /// </summary>
  internal class CustomGeocodeDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "GeocodingTools_CustomGeocodeDockpane";

    protected CustomGeocodeDockpaneViewModel() { }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    /// <summary>
    /// Dispose of any overlay objects.
    /// </summary>
    protected override void OnHidden()
    {
      base.OnHidden();
      if (disposable != null)
      {
        disposable.Dispose();
        disposable = null;
      }
    }

    /// <summary>
    /// Binds the geocode results to the ListBox.
    /// </summary>
    private List<ArcGIS.Desktop.Mapping.Geocoding.GeocodeResult> _results;
    public IReadOnlyList<ArcGIS.Desktop.Mapping.Geocoding.GeocodeResult> Results
    {
      get { return _results; }
    }

    private IDisposable disposable;

    /// <summary>
    /// The selected geocode result in the listbox.
    /// </summary>
    private ArcGIS.Desktop.Mapping.Geocoding.GeocodeResult _selectedResult;
    public ArcGIS.Desktop.Mapping.Geocoding.GeocodeResult SelectedResult
    {
      get { return _selectedResult; }
      set
      {
        SetProperty(ref _selectedResult, value, () => SelectedResult);

        // remove any existing item from the overlay
        if (disposable != null)
        {
          disposable.Dispose();
          disposable = null;
        }

        if (_selectedResult == null)
          return;

        // if the result has a location
        if (_selectedResult.DisplayLocation != null)
        {
          QueuedTask.Run(() =>
          {
            // zoom to the extent
            if (_selectedResult.Extent != null)
              MapView.Active.ZoomTo(_selectedResult.Extent);

            // add to the overlay
            disposable = MapView.Active.AddOverlay(_selectedResult.DisplayLocation,
                SymbolFactory.Instance.ConstructPointSymbol(
                        ColorFactory.Instance.RedRGB, 15.0, SimpleMarkerStyle.Star).MakeSymbolReference());
          });

        }
      }
    }

    /// <summary>
    /// Performs a geocode operation using the static GeocodeAsync method.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    internal async Task Search(string text)
    {
      // clean the overlay
      if (disposable != null)
      {
        disposable.Dispose();
        disposable = null;
      }

      // check the text
      if (string.IsNullOrEmpty(text))
        return;

      // do the geocode  - pass false and false because we want to control the display and zoom ourselves
      IEnumerable<ArcGIS.Desktop.Mapping.Geocoding.GeocodeResult> results = await MapView.Active.LocatorManager.GeocodeAsync(text, false, false);

      // check result count - if no results, most likely no locators
      if ((results == null) || (results.Count() == 0))
        _results = new List<ArcGIS.Desktop.Mapping.Geocoding.GeocodeResult>();
      else
        _results = results.ToList();

      // update UI
      NotifyPropertyChanged(() => Results);
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class CustomGeocodeDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      CustomGeocodeDockpaneViewModel.Show();
    }
  }
}
