/*

   Copyright 2022 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DockpaneWithListCheckbox
{
  internal class TestListboxViewModel : DockPane
  {
    private const string _dockPaneID = "DockpaneWithListCheckbox_TestListbox";
    private ObservableCollection<MyLayer> _myLayers = new ObservableCollection<MyLayer>();
    private MyLayer? _myLayersSelection;

    protected TestListboxViewModel() { }

    /// <summary>
    /// This is the list of MyLayer objects
    /// </summary>
    public ObservableCollection<MyLayer> MyLayers
    {
      get { return _myLayers; }
      set
      {
        SetProperty(ref _myLayers, value, () => MyLayers);
      }
    }

    /// <summary>
    /// This is called on selection change of MyLayers
    /// </summary>
    public MyLayer MyLayersSelection
    {
      get { return _myLayersSelection; }
      set
      {
        SetProperty(ref _myLayersSelection, value, () => MyLayersSelection);
        if (_myLayersSelection != null)
          MessageBox.Show($@"Selection changed: {_myLayersSelection.Name} {_myLayersSelection.IsChecked}");
      }
    }

    private int _addedLayers = 5;

    /// <summary>
    /// Populate the Listbox
    /// </summary>
    public ICommand CmdPopulate
    {
      get
      {
        return new RelayCommand(() =>
        {
          MyLayers.Clear();
          for (int i = 1; i <= _addedLayers; i++)
          {
            MyLayers.Add(new MyLayer($@"Layer_{i}"));
          }
          _addedLayers++;
        }, true);
      }
    }
    /// <summary>
    /// Read the Listbox
    /// </summary>
    public ICommand CmdRead
    {
      get
      {
        return new RelayCommand(() =>
        {
          StringBuilder sb = new StringBuilder();
          foreach (var layer in MyLayers)
          {
            if (layer.IsChecked)
            {
              if (sb.Length > 0) sb.Append(", ");
              sb.Append(layer.Name);
            }
          }
          MessageBox.Show($@"Selected items: {sb.ToString()}");
        }, true);
      }
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
  internal class TestListbox_ShowButton : Button
  {
    protected override void OnClick()
    {
      TestListboxViewModel.Show();
    }
  }
}
