/*

   Copyright 2025 Esri

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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace TabularDataOptions
{
  internal class GeometryControlViewModel : DockPane
  {
    private const string _dockPaneID = "TabularDataOptions_GeometryControl";

    private MapMember _selectedMapMember;
    private string _currentObjectId;
    private int _currentObjectIdIndex = -1;
    private bool _IsMemberSelected;
    private readonly List<Int64> _ObjectIds = [];

    /// <summary>
    /// used to lock collections for use by multiple threads
    /// </summary>
    private readonly object _lockCollections = new();

    /// <summary>
    /// UI lists, read-only collections
    /// </summary>
    private readonly ObservableCollection<MapMember> _mapMembers = [];

    protected GeometryControlViewModel()
    {
      BindingOperations.EnableCollectionSynchronization(_mapMembers, _lockCollections);

      // subscribe to the map view changed event... that's when we update the list of feature layers
      ActiveMapViewChangedEvent.Subscribe((args) =>
      {
        if (args.IncomingView == null) return;
        GetMapMembers();
      });
      // in case we have a map already open
      GetMapMembers(true);
    }

    #region Geometry Control Properties

    private Geometry _geometry;

		/// <summary>
		/// The geometry to display
		/// GeometryControl: Geometry is data bound to the Geometry property of the GeometryControl
		/// </summary>
		public Geometry Geometry
    {
      get => _geometry;
      set => SetProperty(ref _geometry, value);
    }

    #endregion Geometry Control Properties

    #region Properties

    /// <summary>
    /// List of the current active map's map-members
    /// </summary>
    public ObservableCollection<MapMember> MapMembers => _mapMembers;

    /// <summary>
    /// The selected map member
    /// </summary>
    public MapMember SelectedMapMember
    {
      get => _selectedMapMember;
      set
      {
        SetProperty(ref _selectedMapMember, value);

        CurrentObjectID = string.Empty;
        if (_selectedMapMember == null)
        {
          _ObjectIds.Clear();
        }
        else
        {
          // fill the list of object ids
          _ = QueuedTask.Run(() =>
          {
            if (_selectedMapMember is BasicFeatureLayer featureLayer)
            {
              featureLayer.Select(new QueryFilter() { WhereClause = "1=1" });
              _ObjectIds.AddRange(featureLayer.GetSelection().GetObjectIDs());
              if (_ObjectIds.Count > 0)
              {
                _currentObjectIdIndex = 0;
                RunOnUiThread(() => CurrentObjectID = _ObjectIds[_currentObjectIdIndex].ToString());
              }
            }
          });
        }
        // enable the Row selection
        IsMemberSelected = _selectedMapMember != null;
      }
    }

    /// <summary>
    /// IsMemberSelected is true is a SelectedMapMember is not null
    /// </summary>
    public bool IsMemberSelected
    {
      get => _IsMemberSelected;
      set
      {
        SetProperty(ref _IsMemberSelected, value);
      }
    }

    /// <summary>
    /// The selected row (object id)
    /// </summary>
    public string CurrentObjectID
    {
      get => _currentObjectId;
      set
      {
        SetProperty(ref _currentObjectId, value);
        // make enable the up/down buttons
        NotifyPropertyChanged(() => IsPreviousEnabled);
        NotifyPropertyChanged(() => IsNextEnabled);
        // get the geometry
        if (SelectedMapMember == null || string.IsNullOrEmpty(value))
        {
          Geometry = null;
          return;
        }
        else
        {
          _= SetGeometry(SelectedMapMember as BasicFeatureLayer, value);
        }
      }
    }

    private async Task<Geometry> SetGeometry(BasicFeatureLayer selectedFeatureLayer, string soid)
    {
      var geometryQueryResult = await QueuedTask.Run<Geometry>(() =>
      {
        var oid = Convert.ToInt64(soid);
        var qf = new QueryFilter() { ObjectIDs = [oid] };
        // search the selected feature layer using the object id and get the geometry
        using var featureCursor = selectedFeatureLayer.Search(qf);
        if (!featureCursor.MoveNext())
        {
          // No feature found with the given ObjectID
          return null;
        }
        using var feature = featureCursor.Current as Feature;
        return feature.GetShape().Clone();
      });
			// GeometryControl: Query the geometry for a feature with the given ObjectID
			// the use the data bound property to update the GeometryControl
			Geometry = geometryQueryResult;
      return geometryQueryResult;
    }

    #endregion Properties

    #region Properties for ObjectId handling

    public ICommand CmdPreviousObjectId
    {
      get
      {
        return new RelayCommand(() => 
        {
          if (_currentObjectIdIndex > 0)
          {
            _currentObjectIdIndex--;
          };
          CurrentObjectID = _ObjectIds[_currentObjectIdIndex].ToString();
        });
      }
    }

    public ICommand CmdNextObjectId
    {
      get
      {
        return new RelayCommand(() => 
        {
          if (_currentObjectIdIndex < _ObjectIds.Count - 2)
          {
            _currentObjectIdIndex++;
          };
          CurrentObjectID = _ObjectIds[_currentObjectIdIndex].ToString();
        });
      }
    }

    public bool IsPreviousEnabled
    {
      get
      {
        return _currentObjectIdIndex > 0;
      }
    }

    public bool IsNextEnabled
    {
      get
      {
        return _currentObjectIdIndex < _ObjectIds.Count - 1;
      }
    }

    #endregion Properties for ObjectId handling

    #region Helper Methods

    /// <summary>
    /// This method is called to use the current active mapview and retrieve all 
    /// MapMembers that are part of the map in the current map view.
    /// </summary>
    private void GetMapMembers(bool startUp = false)
    {
      QueuedTask.Run(() =>
      {
        var map = MapView.Active?.Map;
        if (map == null)
        {
          // no active map ... use the first visible map instead
          var firstMapPane = ProApp.Panes.OfType<IMapPane>().FirstOrDefault();
          map = firstMapPane?.MapView?.Map;
        }
        if (map == null)
        {
          if (!startUp) MessageBox.Show("Can't find a MapView");
          return;
        }
        MapMembers.Clear();
        MapMembers.AddRange(map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>());
      });
    }

    /// <summary>
    /// utility function to enable an action to run on the UI thread (if not already)
    /// </summary>
    /// <param name="action">the action to execute</param>
    /// <returns></returns>
    internal static void RunOnUiThread(Action action)
    {
      try
      {
        if (IsOnUiThread)
          action();
        else
          System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
      }
      catch (Exception ex)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Error in RunOnUiThread: {ex.Message}");
      }
    }

    /// <summary>
    /// Determines whether the calling thread is the thread associated with this 
    /// System.Windows.Threading.Dispatcher, the UI thread.
    /// 
    /// If called from a View model test it always returns true.
    /// </summary>
    public static bool IsOnUiThread => ArcGIS.Desktop.Framework.FrameworkApplication.TestMode || System.Windows.Application.Current.Dispatcher.CheckAccess();

    #endregion Helper Methods

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
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Dockpane with GeometryControl";

    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class GeometryControl_ShowButton : Button
  {
    protected override void OnClick()
    {
      GeometryControlViewModel.Show();
    }
  }
}
