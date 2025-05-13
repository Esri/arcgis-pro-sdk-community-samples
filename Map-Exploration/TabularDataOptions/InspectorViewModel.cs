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
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Controls;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace TabularDataOptions
{
  internal class InspectorViewModel : DockPane
  {
    private const string _dockPaneID = "TabularDataOptions_Inspector";

    private MapMember _selectedMapMember;
    private string _currentObjectId;
    private int _currentObjectIdIndex = -1;
    private bool _IsMemberSelected;
    private readonly List<Int64> _ObjectIds = [];

    private static InspectorViewModel _dockPane = null;

    /// <summary>
    /// used to lock collections for use by multiple threads
    /// </summary>
    private readonly object _lockCollections = new();

    /// <summary>
    /// UI lists, read-only collections
    /// </summary>
    private readonly ObservableCollection<MapMember> _mapMembers = [];

    protected InspectorViewModel()
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
      _dockPane = this;
    }

    #region Inspector Properties

    private System.Windows.Controls.UserControl _InspectorView;

		/// <summary>
		/// Inspector: InspectorView property is data bound to ContentPresenter.Content
		/// </summary>
		public System.Windows.Controls.UserControl InspectorView
    {
      get => _InspectorView;
      set => SetProperty(ref _InspectorView, value, () => InspectorView);
    }

    private EmbeddableControl _inspectorViewModel;

    private Inspector _inspector;

    public Inspector Inspector
    {
      get
      {
        if (_inspector == null)
        {
          // close the inspector if it is already open
          if (_inspectorViewModel != null)
          {
            //Occurs when the control is closed
            _inspectorViewModel.CloseAsync();
          }

					// initialize the inspector
					// Inspector: Create an instance of the Inspector class
					//            Create an embeddable control from the inspector class
					_inspector = new Inspector();

          // create an embeddable control from the inspector class
          // returns a tuple (view-model, view)
          (EmbeddableControl inspectorViewModel, 
            System.Windows.Controls.UserControl inspectorView) 
            = _inspector.CreateEmbeddableControl();

					// get view and view-model from the inspector
					// Inspector: call view-model OpenAsync and publish the inspector view
					_inspectorViewModel = inspectorViewModel;
          _inspectorViewModel.OpenAsync();
          InspectorView = inspectorView;
        }
        return _inspector;
      }
    }

    #endregion Inspector Properties

    #region Properties

    /// <summary>
    /// List of the current active map's map-members
    /// </summary>
    public ObservableCollection<MapMember> MapMembers
    {
      get { return _mapMembers; }
    }

    /// <summary>
    /// The selected map member
    /// </summary>
    public MapMember SelectedMapMember
    {
      get { return _selectedMapMember; }
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
            if (_selectedMapMember is StandaloneTable standaloneTable)
            {
              standaloneTable.Select(new QueryFilter() { WhereClause = "1=1" });
              _ObjectIds.AddRange(standaloneTable.GetSelection().GetObjectIDs());
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
      set => SetProperty(ref _IsMemberSelected, value);
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
				// load inspector with data
				// Inspector: Load new data into the InspectorView
				//            Inspector.LoadAsync is called when the object id is changed
				if (!string.IsNullOrEmpty(_currentObjectId))
          LoadDataIntoInspector(SelectedMapMember, Convert.ToInt64(_currentObjectId));
      }
    }

    private async void LoadDataIntoInspector(MapMember selectedMapMember, long oid)
    {
      if (Inspector != null && !string.IsNullOrEmpty(_currentObjectId))
      {
        await Inspector.LoadAsync(selectedMapMember, oid);
      }
    }

    #endregion Properties

    #region Propeties for ObjectId handling

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
        MapMembers.AddRange(map.GetStandaloneTablesAsFlattenedList().OfType<MapMember>());
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
    internal static  void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;
      pane.Activate();
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Inspector: row-level attributes";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class Inspector_ShowButton : Button
  {
    protected override void OnClick()
    {
      InspectorViewModel.Show();
    }
  }
}
