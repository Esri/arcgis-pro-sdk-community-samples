/*

   Copyright 2019 Esri

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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Realtime;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using RealtimeAnalysis.JSON;
using Newtonsoft.Json;

namespace RealtimeAnalysis
{
  internal class BingSearchResultViewModel : DockPane
  {
    public static string _dockPaneID = "BingSearch_BingSearchResult";
    private ICommand _searchCmd = null;
    private string _results = "";
    private string _search = "Coffee";

    protected BingSearchResultViewModel() { }

    public string SearchResults
    {
      get
      {
        return _results;
      }
      set
      {
        SetProperty(ref _results, value, () => SearchResults);
      }
    }

    public string SearchText
    {
      get
      {
        return _search;
      }
      set
      {
        SetProperty(ref _search, value, () => SearchText);
      }
    }

    public ICommand SearchCommand
    {
      get
      {
        if (_searchCmd == null)
        {
          _searchCmd = new RelayCommand(() => DoSearch());
        }
        return _searchCmd;
      }
    }

    public void DoSearch()
    {
      //Do the Bing Search
      WebClient client = new WebClient();
      string json = "";
      StringBuilder sb = new StringBuilder();
      sb.AppendLine($"Results for: '{this.SearchText}'");
      sb.AppendLine("");

      var url = $"https://dev.virtualearth.net/REST/v1/LocalSearch/?q={this.SearchText}&userCircularMapView={Module1.Current.Lat},{Module1.Current.Long},100&key={Module1.BingKey}";
      //var url = $"https://dev.virtualearth.net/REST/v1/LocalSearch/?type={this.SearchText}&userLocation={Module1.Current.Lat},{Module1.Current.Long}&key={Module1.BingKey}";

      try
      {
        //https://dev.virtualearth.net/REST/v1/LocalSearch/?type=CoffeeAndTea&userLocation=47.602038,-122.333964&key={BingMapsAPIKey}
        json = client.DownloadString(url);
        FormatResultJson(json, sb);
      }
      catch(System.Net.WebException we)
      {
        sb.AppendLine($"status code: 401");
        sb.AppendLine($"description: Unauthorized");
        sb.AppendLine($"{we.Message}");
        sb.AppendLine("");
      }
      SearchResults = sb.ToString();
    }

    private string FormatResultJson(string jsonResults, StringBuilder sb)
    {
      var root = JsonConvert.DeserializeObject<RootObject>(jsonResults);
      
      
      foreach (var rs in root.resourceSets)
      {
        sb.AppendLine($"total: {rs.resources?.Count() ?? 0}");

        foreach (var r in rs.resources)
        {
          sb.AppendLine($"{r.name}");
          sb.AppendLine($"{r.entityType}");
          sb.AppendLine($"{r.address.formattedAddress}");
          if (r.point != null)
          {
            sb.AppendLine($"{r.point.coordinates[0] + " , " + r.point.coordinates[1]}");
          }
          sb.AppendLine("");
        }
      }
      return sb.ToString();
    }

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
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class BingSearchResult_ShowButton : Button
  {
    private RealtimeCursor _rtCursor = null;
    protected override void OnClick()
    {
      BingSearchResultViewModel.Show();
      StreamLayerSubscribe();
    }

    private async void StreamLayerSubscribe()
    {
      if (_rtCursor?.GetState() == RealtimeCursorState.Subscribed)
      {
        _rtCursor.Unsubscribe();
        return;
      }

      Map map = MapView.Active.Map;
      if (map == null) return;

      StreamLayer streamLayer = map.Layers[0] as StreamLayer;

      await QueuedTask.Run(async () => {
        var rtFC = streamLayer.GetFeatureClass();
        _rtCursor = rtFC.Subscribe(null, true);
        while (await _rtCursor.WaitForRowsAsync())
        {
          while (_rtCursor.MoveNext())
          {
            var _rtFeature = _rtCursor.Current as RealtimeFeature;
            switch (_rtFeature.GetRowSource())
            {
              case RealtimeRowSource.EventInsert:
                var point = _rtFeature.GetShape() as MapPoint;
                Module1.Current.Long = point.X;
                Module1.Current.Lat = point.Y;
                var pane = FrameworkApplication.DockPaneManager.Find(BingSearchResultViewModel._dockPaneID) as BingSearchResultViewModel;
                pane?.DoSearch();
                continue;
              default:
                continue;
            }
          } 
        };
      });
    }
  }
}
