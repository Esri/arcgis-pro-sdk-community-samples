using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

using ArcGIS.Desktop.Mapping.Controls;

namespace SymbolSearcherControl
{
  internal class VerySimpleSymbolSearcherViewModel : DockPane
  {
    private const string _dockPaneID = "SymbolSearcherControl_VerySimpleSymbolSearcher";

    protected VerySimpleSymbolSearcherViewModel() { }

    private StyleItem _selectedPickerStyleItem;
    public StyleItem SelectedPickerStyleItem
    {
      get
      {
        return _selectedPickerStyleItem;
      }
      set
      {
        SetProperty(ref _selectedPickerStyleItem, value, () => SelectedPickerStyleItem);
        MessageBox.Show($@"SelectedPickerStyleItem: {_selectedPickerStyleItem?.Name}");
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

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Symbol Searcher with ListBox";
    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class VerySimpleSymbolSearcher_ShowButton : Button
  {
    protected override void OnClick()
    {
      VerySimpleSymbolSearcherViewModel.Show();
    }
  }
}
