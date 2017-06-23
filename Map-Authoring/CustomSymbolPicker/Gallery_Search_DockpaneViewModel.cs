//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Windows.Input;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;
using System.Windows.Data;
using ArcGIS.Desktop.Core;

namespace CustomSymbolPicker
{
  /// <summary>
  /// Custom Symbol Picker dock pane view model
  /// </summary>
  /// <remarks>
  /// 1. This class has the logic for searching styles for symbols, populating results in gallery and applying a symbol to a feature layer
  /// </remarks>
  internal class Gallery_Search_DockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "CustomSymbolPicker_Gallery_Search_Dockpane";

    //List of style items returned by search and displayed in the list box
    private IList<SymbolStyleItem> _styleItems = new List<SymbolStyleItem>();

    //The style item selected in the search results
    private SymbolStyleItem _selectedStyleItem = null;

    //Types of symbols (styles items) that can be searched for in a style
    private List<string> _choices = new List<string>() {
            "Point symbols", "Line symbols", "Polygon symbols"
        };

    //The type of symbol to search for in style
    private string _selectedChoice = "";

    //List of styles referenced in the current project
    private List<StyleProjectItem> _styleProjectItems = new List<StyleProjectItem>();

    //The style that will be searched
    private StyleProjectItem _selectedStyleProjectItem = null;

    //The search string
    private string _searchString = "";

    //The type of item to search (point, line or polygon symbol)
    private StyleItemType _itemTypeToSearch;

    private ICommand _searchResultCommand = null;


    protected Gallery_Search_DockpaneViewModel()
    {
      ArcGIS.Desktop.Core.Events.ProjectOpenedEvent.Subscribe((args) =>
      {
              //Get the list of styles in the current project
              FillReferencedStyleList();
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

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Search symbols in style";
    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }

    #region Properties for user inputs

    public IReadOnlyList<string> Choices
    {
      get
      {
        return _choices;
      }
    }
    public string SelectedChoice
    {
      get { return _selectedChoice; }
      set
      {
        SetProperty(ref _selectedChoice, value, () => SelectedChoice);
      }
    }

    public IReadOnlyList<StyleProjectItem> StyleProjectItems
    {
      get
      {
        if (_styleProjectItems.Count == 0)
          FillReferencedStyleList();
        return _styleProjectItems;
      }
    }

    public StyleProjectItem SelectedStyleProjectItem
    {
      get
      {
        return _selectedStyleProjectItem;
      }
      set
      {
        SetProperty(ref _selectedStyleProjectItem, value, () => SelectedStyleProjectItem);
      }
    }

    public string SearchString
    {
      get
      {
        return _searchString;
      }
      set
      {
        SetProperty(ref _searchString, value, () => SearchString);
      }
    }

    #endregion Properties for user inputs


    #region ListBox of SymbolStyleItems returned by search

    public IList<SymbolStyleItem> StyleItems
    {
      get
      {
        return _styleItems;
      }
    }

    public SymbolStyleItem SelectedStyleItem
    {
      get
      {
        return _selectedStyleItem;
      }
      set
      {
        if (_selectedStyleItem == value)
          return;
        _selectedStyleItem = value;
        ApplyTheSelectedSymbol(_selectedStyleItem);
        NotifyPropertyChanged(() => SelectedStyleItem);
      }
    }

    #endregion ListBox of StyleItems returned by search


    //Search button click
    public ICommand SearchResultCommand
    {
      get
      {
        if (_searchResultCommand == null)
        {
          _searchResultCommand = new RelayCommand(GetSearchResults, () =>
          {
            return _selectedStyleProjectItem != null &&
                   (_selectedChoice != null && _selectedChoice.Length > 0);
          });
        }
        return _searchResultCommand;
      }
    }


    #region Helper methods

    private async void GetSearchResults(object parameter)
    {
      //Clear for new search
      if (_styleItems.Count != 0)
        _styleItems.Clear();

      //Get results and populate symbol gallery
      if (_selectedChoice == "Line symbols") _itemTypeToSearch = StyleItemType.LineSymbol;
      else if (_selectedChoice == "Polygon symbols") _itemTypeToSearch = StyleItemType.PolygonSymbol;
      else _itemTypeToSearch = StyleItemType.PointSymbol;

      await QueuedTask.Run(() =>
      {
        //Search for symbols in the selected style
        _styleItems = SelectedStyleProjectItem.SearchSymbols(_itemTypeToSearch, _searchString);
      });
      NotifyPropertyChanged(() => StyleItems);
      NotifyPropertyChanged(() => StyleProjectItems);
      NotifyPropertyChanged(() => SearchResultCommand);
    }

    //Create a CIMSymbol from style item selected in the results gallery list box and
    //apply this newly created symbol to the feature layer currently selected in Contents pane
    private async void ApplyTheSelectedSymbol(SymbolStyleItem selectedStyleItemToApply)
    {
      if (selectedStyleItemToApply == null || string.IsNullOrEmpty(selectedStyleItemToApply.Key))
        return;

      await QueuedTask.Run(() =>
      {
              //Get the feature layer currently selected in the Contents pane
              var selectedLayers = MapView.Active.GetSelectedLayers();

              //Only one feature layer should be selected in the Contents pane
              if (selectedLayers.Count != 1)
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Select the feature layer to which you want to apply the selected symbol. Only one feature layer should be selected.");
                //Clear the current selection in gallery
                _selectedStyleItem = null;
          NotifyPropertyChanged(() => SelectedStyleItem);
          return;
        }

        FeatureLayer ftrLayer = selectedLayers[0] as FeatureLayer;

              //The selected layer should be a feature layer
              if (ftrLayer == null)
          return;

              //Get symbol from symbol style item. 
              CIMSymbol symbolFromStyleItem = selectedStyleItemToApply.Symbol;

              //Make sure there isn't a mismatch between the type of selected symbol in gallery and feature layer geometry type
              if ((symbolFromStyleItem is CIMPointSymbol && ftrLayer.ShapeType != esriGeometryType.esriGeometryPoint) ||
                  (symbolFromStyleItem is CIMLineSymbol && ftrLayer.ShapeType != esriGeometryType.esriGeometryPolyline) ||
                  (symbolFromStyleItem is CIMPolygonSymbol && ftrLayer.ShapeType != esriGeometryType.esriGeometryPolygon && ftrLayer.ShapeType != esriGeometryType.esriGeometryMultiPatch))
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("There is a mismatch between symbol type and feature layer geometry type.");
                //Clear the current selection in gallery
                _selectedStyleItem = null;
          NotifyPropertyChanged(() => SelectedStyleItem);
          return;
        }


              //Get simple renderer from feature layer
              CIMSimpleRenderer currentRenderer = ftrLayer.GetRenderer() as CIMSimpleRenderer;

        if (currentRenderer == null)
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Select a feature layer symbolized with a simple renderer.");
                //Clear the current selection in gallery
                _selectedStyleItem = null;
          NotifyPropertyChanged(() => SelectedStyleItem);
          return;
        }

              //Set real world setting for created symbol = feature layer's setting 
              //so that there isn't a mismatch between symbol and feature layer
              symbolFromStyleItem.SetRealWorldUnits(ftrLayer.UsesRealWorldSymbolSizes);

              //Set current renderer's symbol reference = symbol reference of the newly created symbol
              currentRenderer.Symbol = symbolFromStyleItem.MakeSymbolReference();

              //Update feature layer renderer with new symbol
              ftrLayer.SetRenderer(currentRenderer);
      });
    }

    internal void FillReferencedStyleList()
    {
      if (Project.Current != null)
      {
        _styleProjectItems.Clear();
        //rebuild list to get the currently referenced styles in project
        IEnumerable<StyleProjectItem> projectStyleContainer = Project.Current.GetItems<StyleProjectItem>();
        foreach (var style in projectStyleContainer)
        {
          _styleProjectItems.Add(style);
        }
        NotifyPropertyChanged(() => StyleProjectItems);
      }
    }

    #endregion
  }


  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  internal class Gallery_Search_Dockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      Gallery_Search_DockpaneViewModel.Show();
    }
  }

}
