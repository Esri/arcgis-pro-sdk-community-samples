/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Layouts.Events;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Button = ArcGIS.Desktop.Framework.Contracts.Button;

namespace StyleElements
{
  internal class StyleElementsDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "StyleElements_StyleElementsDockpane";
    private Element _selectedElement;
    private List<Element> _selectedElements = new List<Element>();
    protected StyleElementsDockpaneViewModel() {
      //Subscribe to element selection changed event. When graphic elements are selected or un-selected, this event will trigger.
      //The style elements dockpane will update the symbols displayed based on the type.
      ArcGIS.Desktop.Layouts.Events.ElementEvent.Subscribe(OnElementSelectionChanged);
    }

    private void OnElementSelectionChanged(ElementEventArgs args)
    {
      if (args.Hint == ElementEventHint.SelectionChanged )
      {
        UpdateSymbolCollection();
      }
    }

    protected override void OnShow(bool isVisible)
    {
      if (LayoutView.Active == null || MapView.Active.Map == null) return;
      if (isVisible)
      {
        UpdateSymbolCollection();
      }
      

      //base.OnShow(isVisible);
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
    private string _heading = "Select style";
    public string Heading
    {
      get => _heading;
      set {
        SetProperty(ref _heading, value);
      } 
    }

    private string _selectedElementType = string.Empty;
    public string SelectedElementType
    {
      get => _selectedElementType;
      set
      {
        SetProperty(ref _selectedElementType, value);
        Heading = $"Select a {SelectedElementType} style";
      }
    }

    private ObservableCollection<GeometrySymbolItem> _symbolStyleItemArrowCollection= new ObservableCollection<GeometrySymbolItem>();

    public ObservableCollection<GeometrySymbolItem> SymbolStyleItemCollection
    {
      get => _symbolStyleItemArrowCollection;
    }

    private GeometrySymbolItem _selectedItem;
    public GeometrySymbolItem SelectedItem
    {
      get => _selectedItem;
      set {
        SetProperty(ref _selectedItem, value);
        ApplySelectedStyle(); //Magic happens here. When the user selects a style, we apply it to the selected elements
      } 
    }

    private void ApplySelectedStyle()
    {
      if (SelectedItem != null && _selectedElements.Count > 0)
      {
        SelectedItem.Execute(_selectedElements);
      }      
    }
    /// <summary>
    /// This method is triggered when the Selected elements event is fired. It checks if the active pane is a layout. If not a layout, then it checks if it is a map view.
    /// The selected elements in these views checked if they are of the same type. Are they all point elements, for example? The first element determines "THE type" to compare against.
    /// If they are the same type, they get added to the _selectedElements member variable. SymbolStyleItemCollection is the MVVM Binding variable that gets updated with Point symbols,
    /// if _SelectedElements are all points.
    /// </summary>
    private async void UpdateSymbolCollection()
    {
      if (LayoutView.Active?.Layout.GetSelectedElements().Count == 0 && MapView.Active?.Map.TargetGraphicsLayer.GetSelectedElements().Count == 0)
        return;
      _selectedElements.Clear();

      List<Element> selectedElements = new List<Element>();

      //Get all the seleted elements in the Active Layout first
      if (LayoutView.Active != null)
      {
        foreach (var selElm in LayoutView.Active.GetSelectedElements())
        {
          selectedElements.Add(selElm);
        }
      }
      //If no elements are selected in the Active Layout, get all the selected elements in the Active Map
      else
      {
        if (MapView.Active != null && LayoutView.Active == null)
        {
          foreach (var selElm in MapView.Active.Map.TargetGraphicsLayer.GetSelectedElements())
          {
            selectedElements.Add(selElm);
          }
        }
      }
      if (selectedElements.Count == 0) return;
      //Get the first selected element and its type
      var firstItemSelected = selectedElements.FirstOrDefault();
      var firstItemType = selectedElements.FirstOrDefault().GetType();

      //iterate through all the selected elements, match each element to the first one found.
      //If the element type matches the first element type, add it to _selectedElements (member variable)
      //This is the collection that will be used to apply the style
      foreach (var item in selectedElements)
      {
        if (item.GetType() == firstItemType)
        {
          _selectedElements.Add(item);
        }
      }

      if (_selectedElements.Count == 0) return;

      //Now we populate the listbox with the appropriate style items 
      //If the _selectedElements collection contains a NorthArrow, we populate the listbox with NorthArrowStyleItems, etc
      SymbolStyleItemCollection.Clear();
      if (firstItemSelected is NorthArrow)
      {
        System.Diagnostics.Debug.WriteLine("North arrow element selected");
        SelectedElementType = "North Arrow";
        foreach (var item in Module1.Current.NorthArrowsStyleItems)
        {
          SymbolStyleItemCollection.Add(item);
        }
      }
      if (firstItemSelected is ScaleBar)
      {
        System.Diagnostics.Debug.WriteLine("Scale bar element selected");
        SelectedElementType = "Scale Bar";
        foreach (var item in Module1.Current.ScaleBarsStyleItems)
        {
          SymbolStyleItemCollection.Add(item);
        }
      }
      if (firstItemSelected is Legend)
      {
        System.Diagnostics.Debug.WriteLine("Legend element selected");
        SelectedElementType = "Legend";
        foreach (var item in Module1.Current.LegendStyleItems)
        {
          SymbolStyleItemCollection.Add(item);
        }
      }
      if (firstItemSelected is TableFrame)
      {
        System.Diagnostics.Debug.WriteLine("Table Frame element selected");
        SelectedElementType = "Table Frame";
        foreach (var item in Module1.Current.TableFrameStyleItems)
        {
          SymbolStyleItemCollection.Add(item);
        }
      }
      if (firstItemSelected is MapFrame mapFrame)
      {
        //valid 2D maps only
        if (mapFrame.Map == null || mapFrame.Map.DefaultViewingMode != MapViewingMode.Map)
          return;
        System.Diagnostics.Debug.WriteLine("Map Frame element selected");
        SelectedElementType = "Grid";
        foreach (var item in Module1.Current.GridStyleItems)
        {
          SymbolStyleItemCollection.Add(item);
        }
      }
      if (firstItemSelected is TextElement)
      {
        System.Diagnostics.Debug.WriteLine("Text element selected");
        SelectedElementType = "Text";
        foreach (var item in Module1.Current.TextStyleItems)
        {
          SymbolStyleItemCollection.Add(item);
        }
      }
      //For point, line and polygon elements, we need to get the CIMGraphic to determine the geometry type
      if (firstItemSelected is GraphicElement)
      {

        var selEl = firstItemSelected as GraphicElement;
        CIMGraphic cimGraphic = null;
        await QueuedTask.Run(() =>
        {
          cimGraphic = selEl.GetGraphic();
        });
        if (cimGraphic is CIMPointGraphic pointGraphic)
        {
          System.Diagnostics.Debug.WriteLine("Point element selected");
          SelectedElementType = "Point";
          foreach (var item in Module1.Current.PointStyleItems)
          {
            SymbolStyleItemCollection.Add(item);
          }
        }

        if (cimGraphic is CIMLineGraphic lineGraphic)
        {
          System.Diagnostics.Debug.WriteLine("Line element selected");
          SelectedElementType = "Line";
          foreach (var item in Module1.Current.LineStyleItems)
          {
            SymbolStyleItemCollection.Add(item);
          }
        }
        if (cimGraphic is CIMPolygonGraphic polyGraphic)
        {
          System.Diagnostics.Debug.WriteLine("Polygon element selected");
          SelectedElementType = "Polygon";
          foreach (var item in Module1.Current.PolygonStyleItems)
          {
            SymbolStyleItemCollection.Add(item);
          }
        }
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class StyleElementsDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      StyleElementsDockpaneViewModel.Show();
    }
  }
}
