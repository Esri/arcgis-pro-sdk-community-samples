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

using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttributesCustomDockpane
{
  internal class ShowAttributesViewModel : DockPane
  {
    private const string _dockPaneID = "AttributesCustomDockpane_ShowAttributes";

    protected ShowAttributesViewModel() { }

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
      CreateInspectorEmbeddableControl();
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
      _featureInspector?.ClearAsync();
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

    private string _heading = "Custom Attributes";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    private Map _map;
    public Map Map
    {
      get => _map;
      set => SetProperty(ref _map, value);
    }


    #region Inspector
    private Inspector _featureInspector = null;
    private EmbeddableControl _inspectorViewModel = null;
    private System.Windows.Controls.UserControl _inspectorView = null;

    internal void CreateInspectorEmbeddableControl()
    {
      if (_featureInspector != null)
        return;

      // create a new instance for the inspector
      _featureInspector = new Inspector();
      // create an embeddable control from the inspector class to display on the pane
      var icontrol = _featureInspector.CreateEmbeddableControl();

      // get view and viewmodel from the inspector
      InspectorViewModel = icontrol.Item1;
      InspectorView = icontrol.Item2;
    }

    public Inspector AttributeInspector
    {
      get => _featureInspector;
    }

    /// <summary>
    /// Property for the inspector UI.
    /// </summary>
    public System.Windows.Controls.UserControl InspectorView
    {
      get => _inspectorView; 
      set => SetProperty(ref _inspectorView, value);
    }

    /// <summary>
    /// Access to the view model of the inspector
    /// </summary>
    public EmbeddableControl InspectorViewModel
    {
      get => _inspectorViewModel; 
      set
      {
        if (value != null)
        {
          _inspectorViewModel = value;
          _inspectorViewModel.OpenAsync();

        }
        else if (_inspectorViewModel != null)
        {
          _inspectorViewModel.CloseAsync();
          _inspectorViewModel = value;
        }
        NotifyPropertyChanged(() => InspectorViewModel);
      }
    }


    #endregion
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class ShowAttributes_ShowButton : Button
  {
    protected override void OnClick()
    {
      ShowAttributesViewModel.Show();
    }
  }
}
