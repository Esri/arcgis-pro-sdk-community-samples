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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace TableControl
{
  internal class TableControlDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "TableControl_TableControlDockpane";

    protected TableControlDockpaneViewModel()
    {
      ProjectWindowSelectedItemsChangedEvent.Subscribe(OnProjectWindowSelectedItem);
    }

    private Item _selectedItem;
    private async void OnProjectWindowSelectedItem(ProjectWindowSelectedItemsChangedEventArgs args)
    {
      if (args.IProjectWindow.SelectionCount > 0)
      {
        Module1.SelectedMapMember = null;

        // get the first selected item
        _selectedItem = args.IProjectWindow.SelectedItems.First();

        // check if it's supported by the TableControl
        if (!TableControlContentFactory.IsItemSupported(_selectedItem))
          return;

        // create the content
        var tableContent = TableControlContentFactory.Create(_selectedItem);

        // assign it
        if (tableContent != null)
        {
          this.TableContent = tableContent;
        }
      }
    }

    private TableControlContent _tableContent;
    public TableControlContent TableContent
    {
      get { return _tableContent; } 
      set { SetProperty(ref _tableContent, value); }
    }

    private ICommand _addToMapCommand = null;
    public ICommand AddToMapCommand
    {
      get
      {
        if (_addToMapCommand == null)
        {
          _addToMapCommand = new RelayCommand(() =>
          {
            var map = MapView.Active?.Map;
            if (map == null)
              return;

            QueuedTask.Run(() =>
            {
              // test if the selected Catalog item can create a layer
              if (LayerFactory.Instance.CanCreateLayerFrom(_selectedItem))
                Module1.SelectedMapMember = LayerFactory.Instance.CreateLayer(_selectedItem, map);

              // test if the selected Catalog item can create a table
              else if (StandaloneTableFactory.Instance.CanCreateStandaloneTableFrom(_selectedItem))
                Module1.SelectedMapMember = StandaloneTableFactory.Instance.CreateStandaloneTable(_selectedItem, map);

              else
                Module1.SelectedMapMember = null;
            });

          });
        }
        return _addToMapCommand;
      }
    }

    private ArcGIS.Desktop.Editing.TableControl _tableControl = null;
    internal void SetTable(ArcGIS.Desktop.Editing.TableControl tableControl)
    {
      _tableControl = tableControl;
      UpdateContextMenu();
    }

    private bool _contextmenu_item_added = false;
    private void UpdateContextMenu()
    {
      if (_contextmenu_item_added)
        return;

      if (_tableControl.RowContextMenu == null)
        return;
      var mnuItem_Zoom = new System.Windows.Controls.MenuItem() { Header = "Zoom to Row", Command = this.ZoomToRowCommand };
      _tableControl.RowContextMenu.Items.Add(mnuItem_Zoom);

      _contextmenu_item_added = true;
    }
  
    private ICommand _zoomToRowCommand = null;
    public ICommand ZoomToRowCommand
    {
      get
      {
        if (_zoomToRowCommand == null)
        {
          _zoomToRowCommand = new RelayCommand(() =>
          {
            // if we have some content, a map and our data is added to the map
            if (_tableControl?.TableContent != null && MapView.Active != null && Module1.SelectedMapMember is Layer)
            {
              // get the oid of the active row
              var oid = _tableControl.GetObjectIdAsync(_tableControl.ActiveRowIndex).Result;
              // load into an inspector to obtain the Shape
              var insp = new Inspector();
              insp.LoadAsync(Module1.SelectedMapMember, oid).ContinueWith((t) =>
              {
                // zoom
                MapView.Active.ZoomToAsync(insp.Shape.Extent, new TimeSpan(0, 0, 0, 1));
              });
            }
          });
        }
        return _zoomToRowCommand;
      }
    }

    // updates the table control to scroll to the record specified by oid
    internal Task MoveTo(long oid)
    {
      if (oid == -1)
        return Task.CompletedTask;

      if (_tableControl == null)
        return Task.CompletedTask;

      // return task
      return QueuedTask.Run(async () =>
      {
        // get the row index from the oid
        //  use false to search entire table
        var rowIndex = _tableControl.GetRowIndex(oid, false);
        if (rowIndex != -1)
        {
          // scroll to it
          await _tableControl.BringIntoView(rowIndex);

          // clear any existing selection
          _tableControl.ClearSelection();

          // toggle selection state of the row (ie select it)
          //   note the feature will not automatically select in the map
          //    because the tableControl was populated from the catalog item.
          _tableControl.ToggleRowSelection();
        }
      });
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
	internal class TableControlDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      TableControlDockpaneViewModel.Show();
    }
  }
}
