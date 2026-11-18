/*

   Copyright 2026 Esri

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

using ArcGIS.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionControls
{
  internal class MapSelectionDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "SelectionControls_MapSelectionDockpane";

    protected MapSelectionDockpaneViewModel() { }

    protected override void OnShow(bool isVisible)
    {
      // when the dockpane is visible, subscribe to events
      if (isVisible)
      {
        SubscribeToEvents();
      }
      else
      {
        // not visible, unsubscribe from events
        UnsubscribeEvents();
      }

      base.OnShow(isVisible);
    }

    protected override void OnSetContent()
    {
      base.OnSetContent();
      Map = MapView.Active?.Map;
    }

    private SubscriptionToken _activeMapViewChanged = null;
    private SubscriptionToken _mapPaneClosed = null;
    private void SubscribeToEvents()
    {
      // subscribe to ActiveMapViewChangedEvent and MapClosedEvent
      if (_activeMapViewChanged == null)
        _activeMapViewChanged = ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      if (_mapPaneClosed == null)
        _mapPaneClosed = MapClosedEvent.Subscribe(OnMapPaneClosed);
    }

    private void UnsubscribeEvents()
    {
      // unsubscribe from events
      if (_activeMapViewChanged != null)
        ActiveMapViewChangedEvent.Unsubscribe(_activeMapViewChanged);
      _activeMapViewChanged = null;
      if (_mapPaneClosed != null)
        MapClosedEvent.Unsubscribe(_mapPaneClosed);
      _mapPaneClosed = null;
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
    {
      var mv = args.IncomingView;
      var map = mv?.Map;

      // update bound Map property with active map
      Map = map;
    }

    private void OnMapPaneClosed(MapClosedEventArgs obj)
    {
      var map = obj?.MapPane?.MapView?.Map;
      if (map == null)
        return;

      // check if any other mapPanes for the same map still open
      var mapPanes = FrameworkApplication.Panes.OfType<IMapPane>();
      var count = mapPanes?.Count() ?? 0;
      if (count > 0)
        return;

      // none open, set bound Map property to null
      this.Map = null;
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


    private Map _map;
    public Map Map
    {
      get => _map;
      set => SetProperty(ref _map, value);
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class MapSelectionDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      MapSelectionDockpaneViewModel.Show();
    }
  }
}
