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
using ArcGIS.Desktop.Editing.Controls;
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
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TabularDataOptions
{
  internal class TableControlViewModel : DockPane
  {
    private const string _dockPaneID = "TabularDataOptions_TableControl";

    private MapMember _selectedMapMember;

    /// <summary>
    /// used to lock collections for use by multiple threads
    /// </summary>
    private readonly object _lockCollections = new();

    /// <summary>
    /// UI lists, read-only collections, and theDictionary
    /// </summary>
    private readonly ObservableCollection<MapMember> _mapMembers = [];

    protected TableControlViewModel()
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


    #region Properties

    /// <summary>
    /// List of the current active map's mapmembers
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
        // populate the TableControl with the selected map member
        if (_selectedMapMember == null)
          TableContent = null;
        else
          TableContent = TableControlContentFactory.Create(_selectedMapMember);
        _isTableControlOpen = _selectedMapMember != null;
        NotifyPropertyChanged(() => TableControlOpenCaption);
        NotifyPropertyChanged(() => IsTableControlOpen);
      }
    }

    #endregion Properties

    #region TableControl properties

    private TableControlContent _tableControl;

		/// <summary>
		/// The TableControl's TableControlContent property
		/// TableControl: property is data bound to the TableControl in the view
		/// </summary>
		public TableControlContent TableContent
    {
      get => _tableControl;
      set => SetProperty(ref _tableControl, value);
    }

    private TableControl TableControl
    {
      get
      {
        // use the Content property of the TableControlContent to get the TableControl
        var visualTreeRoot = this.Content as DependencyObject;
        return visualTreeRoot.GetChildOfType<TableControl>();
      }
    }

    #endregion TableControl properties

    #region TableControl Command Properties

    public string TableControlOpenCaption
    {
      get
      {
        return _isTableControlOpen ? "Close TableControl" : "Open TableControl";
      }
    }

    private bool _isTableControlOpen = false;

    public ICommand CmdTableControlOpen
    {
      get
      {
        return new RelayCommand(() =>
        {
          try
          {
            if (_isTableControlOpen)
            {
              // close the table control content
              TableContent = null;
            }
            else
            {
							// open the table control
							// TableControl: create the TableControlContent for the selected map member
							TableContent = TableControlContentFactory.Create(_selectedMapMember);
            }
            _isTableControlOpen = !_isTableControlOpen;
            NotifyPropertyChanged(() => TableControlOpenCaption);
            NotifyPropertyChanged(() => IsTableControlOpen);
          }
          catch (Exception ex)
          {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Error in TableControlOpen: {ex.Message}");
          }
        }, () => SelectedMapMember != null);
      }
    }

    public bool IsTableControlOpen => _isTableControlOpen;

    public string TableControlHideFieldsCaption
    {
      get
      {
        return _isTableControlHideFields ? "Show all fields" : "Hide some fields";
      }
    }

    private bool _isTableControlHideFields = false;

    public ICommand CmdTableControlHideFields
    {
      get
      {
        return new RelayCommand(async () =>
        {
          try
          {
            if (_isTableControlHideFields)
            {
              // show all fields
              TableControl?.ShowAllFields();
            }
            else
            {
              // hide the object id field
              var oidName = await QueuedTask.Run<string>(() =>
              {
                if (_selectedMapMember is BasicFeatureLayer featureLayer)
                {
                  return featureLayer.GetTable().GetDefinition().GetObjectIDField();
                }
                if (_selectedMapMember is StandaloneTable standaloneTable)
                {
                  return standaloneTable.GetTable().GetDefinition().GetObjectIDField();
                }
                return string.Empty;
              });
              if (!string.IsNullOrEmpty(oidName))
                TableControl?.SetHiddenFields(new List<string> { oidName });
            }
            //}
            _isTableControlHideFields = !_isTableControlHideFields;
            NotifyPropertyChanged(() => TableControlHideFieldsCaption);
          }
          catch (Exception ex)
          {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Error in TableControlHideFields: {ex.Message}");
          }
        }, () => SelectedMapMember != null);
      }
    }

    public ICommand CmdToggleValueIDs
    {
      get
      {
        return new RelayCommand(() =>
        {
          try
          {
						// TableControl: toggle the column headers between field names and field aliases
						TableControl?.ToggleFieldAlias();
          }
          catch (Exception ex)
          {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Error in TableControlHideFields: {ex.Message}");
          }
        }, () => SelectedMapMember != null);
      }
    }

    public ICommand CmdSwitchSelected
    {
      get
      {
        return new RelayCommand(() =>
        {
          try
          {
            // toggle ViewMode              
            TableControl?.SetViewMode(TableControl?.ViewMode == TableViewMode.eSelectedRecords
                ? TableViewMode.eAllRecords : TableViewMode.eSelectedRecords);
          }
          catch (Exception ex)
          {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Error in TableControlHideFields: {ex.Message}");
          }
        }, () => SelectedMapMember != null);
      }
    }
		public ICommand CmdTableControlFind
		{
			get
			{
				return new RelayCommand(() =>
				{
					try
					{
						// Find/search           
						TableControl?.Find();
					}
					catch (Exception ex)
					{
						ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($@"Error in TableControlHideFields: {ex.Message}");
					}
				}, () => SelectedMapMember != null);
			}
		}

		#endregion TableControl Command Properties

		#region ImageSource Properties

		public ImageSource ImgTableControlOpen
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["InteractiveTable32"] as ImageSource;
        return imageSource;
      }
    }

    public ImageSource ImgTableControlHideFields
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["TableHideField32"] as ImageSource;
        return imageSource;
      }
    }

    public ImageSource ImgToggleValueIDs
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["ToggleValueIDs32"] as ImageSource;
        return imageSource;
      }
    }

    public ImageSource ImgSwitchSelected
    {
      get
      {
        var imageSource = System.Windows.Application.Current.Resources["TableSwitchHighlight32"] as ImageSource;
        return imageSource;
      }
		}

		public ImageSource ImgTableControlFind
		{
			get
			{
				var imageSource = System.Windows.Application.Current.Resources["Search32"] as ImageSource;
				return imageSource;
			}
		}

		#endregion ImageSource Properties

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
          if (!startUp) ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Can't find a MapView");
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
    private string _heading = "Dockpane with TableControl";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class TableControl_ShowButton : Button
  {
    protected override void OnClick()
    {
      TableControlViewModel.Show();
    }
  }
}
