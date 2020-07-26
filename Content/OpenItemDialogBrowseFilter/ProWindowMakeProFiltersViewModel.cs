/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OpenItemDialogBrowseFilter
{
  public class ProWindowMakeProFiltersViewModel : PropertyChangedBase
  {
    private List<ItemFilter> FilterTypeIDsList;
    public ProWindowMakeProFiltersViewModel()
    {
      //Collect all typeIDs and Browse Filters 
      FilterTypeIDsList = ItemFilter.GetDamlItems();
      ShowItems("BrowseFilters");
    }

    #region Filters
    private string _selectedItemDetailLabel;
    public string SelectedItemDetailLabel
    {
      get { return _selectedItemDetailLabel; }
      set
      {
        SetProperty(ref _selectedItemDetailLabel, value, () => SelectedItemDetailLabel);
      }
    }
    private string _selectedItemDetails;
    public string SelectedItemDetails
    {
      get { return _selectedItemDetails; }
      set
      {
        SetProperty(ref _selectedItemDetails, value, () => SelectedItemDetails);
      }
    }

    private DataView _displayItemsDataView;
    public DataView DisplayItemsDataView
    {
      get { return _displayItemsDataView; }
      set
      {
        SetProperty(ref _displayItemsDataView, value, () => DisplayItemsDataView);
      }
    }
    private DataTable _displayItemsDataTable;


    private DataRowView _selectedDisplayItem;
    public DataRowView SelectedDisplayItem
    {
      get { return _selectedDisplayItem; }
      set
      {
        SetProperty(ref _selectedDisplayItem, value, () => SelectedDisplayItem);
        if (_selectedDisplayItem == null)
        {
          return;
        }
        switch (_radioButtonChoice)
        {
          case "BrowseFilters":
            GetSelectedFilterDAML();
            break;
          case "TypeIDs":
            GetSelectedTypeIDDAML();
            break;
          case "FilterFlags":
            GetTypeIDsForSelectedFilterFlag();
            break;
        }

      }
    }
    private Visibility _showOpenFilterButton = Visibility.Collapsed;
    public Visibility ShowOpenFilterButton
    {
      get { return _showOpenFilterButton; }
      set
      {
        SetProperty(ref _showOpenFilterButton, value, () => ShowOpenFilterButton);
      }
    }
    #endregion
    private string _searchText;
    public string SearchText
    {
      get { return _searchText; }
      set
      {
        SetProperty(ref _searchText, value, () => SearchText);
        PerformSearch();
      }
    }
    private void PerformSearch()
    {
      var noSearch = (string.IsNullOrEmpty(SearchText));
      DisplayItemsDataView.RowFilter = noSearch ? string.Empty : $@"name LIKE '%{SearchText}%'";
      if (DisplayItemsDataView.Count > 0)
        SelectedDisplayItem = DisplayItemsDataView[0];
      else
      {
        //Clear DAML text window
        SelectedItemDetails = string.Empty;
        SelectedItemDetailLabel = "Nothing selected";
      }
    }
    private string _radioButtonChoice = "BrowseFilters";
    private void GetSelectedFilterDAML()
    {
      var browseFilter = new BrowseProjectFilter(SelectedDisplayItem.Row["id"].ToString());
      SelectedItemDetailLabel = $"DAML definition for {SelectedDisplayItem.Row["id"]}:";
      SelectedItemDetails = ArcGIS.Desktop.Internal.Core.BrowseFilter.BrowseFilterUtils.GetDamlFromBrowseFilter(browseFilter);
    }
    private void GetSelectedTypeIDDAML()
    {
      if (SelectedDisplayItem is null) return;
      var typeIDName = SelectedDisplayItem.Row["name"].ToString();
      ItemFilter typeID = FilterTypeIDsList.Where(d => d.ElementName == "esri_item" && d.ID == typeIDName).FirstOrDefault();
      if (typeID == null) return;
      SelectedItemDetails = typeID.Content;
      SelectedItemDetailLabel = $"DAML definition for {SelectedDisplayItem.Row["name"]} typeID:";      
    }
    private void GetTypeIDsForSelectedFilterFlag()
    {
      if (SelectedDisplayItem is null) return;
      var filterFlag = (BrowseProjectFilter.FilterFlag)Enum.Parse(typeof(BrowseProjectFilter.FilterFlag), SelectedDisplayItem.Row["name"].ToString());
      var flagsForSelectedTypeID = ArcGIS.Desktop.Internal.Core.BrowseFilter.BrowseFilterUtils.GetTypeIdsForFlag(filterFlag);
      var sb = new StringBuilder();
      foreach (var flag in flagsForSelectedTypeID)
      {
        sb.AppendLine(flag);
      }
      SelectedItemDetails = sb.ToString();
      SelectedItemDetailLabel = $"TypeIDs that participate in the {filterFlag} FilterFlag:";
    }

    #region Commands

    public ICommand DisplayContentCommand => new RelayCommand(ShowItems, () => true);
    private void ShowItems(object radioButtonChoice)
    {
      _radioButtonChoice = radioButtonChoice.ToString();
      ShowOpenFilterButton = Visibility.Collapsed;
      switch (radioButtonChoice.ToString())
      {
        case "BrowseFilters":
          {
            ShowOpenFilterButton = Visibility.Visible;
            _displayItemsDataTable = new DataTable();
            _displayItemsDataTable.Columns.Add(new DataColumn("id", typeof(string)) { Caption = "ID" });
            _displayItemsDataTable.Columns.Add(new DataColumn("name", typeof(string)) { Caption = "Name" });         
            var filtersInPro = FilterTypeIDsList.Where(d => d.ElementName == "esri_browseFilters").ToList();
            filtersInPro.Sort();
            //Reading DAML to get the filters defined by Pro.
            foreach (var filter in filtersInPro)
            {
              var row = _displayItemsDataTable.NewRow();
              row["id"] = filter.ID;
              row["name"] = filter.ElementName;
              _displayItemsDataTable.Rows.Add(row);
            }
            break;
          }

        case "FilterFlags":
          {
            _displayItemsDataTable = new DataTable();
            _displayItemsDataTable.Columns.Add(new DataColumn("name", typeof(string)) { Caption = "Name" });

            //Get all the enum values for BrowseProjectFilter.FilterFlag 
            foreach (var flag in Enum.GetValues(typeof(BrowseProjectFilter.FilterFlag)))
            {
              var row = _displayItemsDataTable.NewRow();
              row["name"] = flag.ToString();
              _displayItemsDataTable.Rows.Add(row);
            }
            break;
          }
        case "TypeIDs":
          {
            _displayItemsDataTable = new DataTable();
            _displayItemsDataTable.Columns.Add(new DataColumn("name", typeof(string)) { Caption = "Name" });

            var typeIDsInPro = FilterTypeIDsList.Where(d => d.ElementName == "esri_item").ToList();
            typeIDsInPro.Sort();

            //Reading DAML to get all the typeIDs. This includes custom item typeIDs also.
            //foreach (var typeID in ArcGIS.Desktop.Internal.Core.BrowseFilter.BrowseFilterUtils.GetTypeIds())
            foreach (var typeID in typeIDsInPro)
            {
              var row = _displayItemsDataTable.NewRow();
              row["name"] = typeID.ID;
              _displayItemsDataTable.Rows.Add(row);
            }           
            break;
          }
        default:
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("BrowseFilters");
          break;
      }
      DisplayItemsDataView = new DataView(_displayItemsDataTable);
      PerformSearch();
    }
    public ICommand OpenFilterCommand => new RelayCommand(() =>
    {
      var bpf = new BrowseProjectFilter($"{SelectedDisplayItem.Row["id"].ToString()}");
      DialogBrowseFilters.DisplayOpenItemDialog(bpf, $"Open {SelectedDisplayItem.Row["name"].ToString()}");
    }, () => true);

    #endregion
  }
}
