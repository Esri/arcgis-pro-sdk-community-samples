//   Copyright 2020 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Events;
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


namespace GeometryControl
{
  internal class GeometryViewDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "GeometryControl_GeometryViewDockpane";

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

    private SubscriptionToken _selectionChangedEventToken;
    protected GeometryViewDockpaneViewModel() 
    {
      // subscribe to map selection changed event
      if (_selectionChangedEventToken == null)
        _selectionChangedEventToken = ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEvent.Subscribe(OnSelectionChanged);
    }

    // when the selection changes, obtain the first feature's geometry and assign to the GeometryControl
    private async void OnSelectionChanged(ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEventArgs args)
    {
      if (args == null)
        return;

      var sel = args.Selection;

      // get the first BasicFeatureLayer
      var member = sel.ToDictionary().Keys.FirstOrDefault(mm => mm is BasicFeatureLayer);
      if (member == null)
      {
        Clear();
        return;
      }

      // get the first oid
      var oids = sel[member];
      var oid = oids[0];

      // get the geometry of that feature
      var geom = await QueuedTask.Run(() =>
      {
        var insp = new ArcGIS.Desktop.Editing.Attributes.Inspector();
        insp.Load(member, oid);
        return insp.Shape;
      });

      // bind to the view
      Geometry = geom;
    }

    private Geometry _geometry;
    public Geometry Geometry
    {
      get => _geometry;
      set => SetProperty(ref _geometry, value);
    }

    private void Clear()
      => Geometry = null;

  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class GeometryViewDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      GeometryViewDockpaneViewModel.Show();
    }
  }
}
