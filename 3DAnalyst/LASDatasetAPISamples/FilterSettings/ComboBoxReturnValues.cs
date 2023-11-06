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
using ArcGIS.Core.Data.Analyst3D;
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LASDatasetAPISamples.FilterSettings
{
  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class ComboBoxReturnValues : ComboBox
  {

    private bool _isInitialized;
    private List<LasReturnType> _returnsInLASDataset = new List<LasReturnType>();
    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public ComboBoxReturnValues()
    {
      UpdateCombo();
    }

    /// <summary>
    /// Updates the combo box with all the items.
    /// </summary>

    private async void UpdateCombo()
    {
      var map = MapView.Active?.Map;
      LasDatasetLayer lasDatasetLayer = null;
      if (map != null)
        lasDatasetLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<LasDatasetLayer>().FirstOrDefault();

      await QueuedTask.Run(() =>
      {
        if (lasDatasetLayer != null)
        {          
          using (var lasDataset = lasDatasetLayer.GetLasDataset())
          {
            _returnsInLASDataset = lasDataset.GetUniqueReturns().ToList();
          }
        }
      });
      if (_isInitialized)
        SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox


      if (!_isInitialized)
      {
        Clear();
        
        foreach (var returnValue in _returnsInLASDataset)
        {
          var item = new CustomItemForCombo(returnValue.ToString(), false);
          Add(item);
          item.PropertyChanged += Item_PropertyChanged;
        }

        _isInitialized = true;
      }
      Enabled = true; //enables the ComboBox
      SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox

    }

    private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      var comboItem = sender as CustomItemForCombo;
      if (comboItem == null || e.PropertyName != "IsChecked")
        return;

      int index = -1;

      if (comboItem.IsChecked)
      {
        index = Module1.Current._selectedReturnTypes.IndexOf(GetReturnType(comboItem.Name));
        if (index == -1) //list does not contain the item. This prevents duplicate items in the list
        {
          Module1.Current._selectedReturnTypes.Add(GetReturnType(comboItem.Name));
        }
        SelectedIndex = ItemCollection.IndexOf(comboItem);
      }
      else //Not selected
      {
        index = Module1.Current._selectedReturnTypes.IndexOf(GetReturnType(comboItem.Name));
        if (index != -1) //list contains the item
        {
          Module1.Current._selectedClassCodes.RemoveAt(index);
        }
      }
    }

    /// <summary>
    /// The on comboBox selection change event. 
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override void OnSelectionChange(object item)
    {

     
    }

    private LasReturnType GetReturnType(string returnTypeString)
    {
      switch (returnTypeString)
      {
        case "Return1":
          return LasReturnType.Return1;
        case "Return2":
          return LasReturnType.Return2;
        case "Return3":
          return LasReturnType.Return3;
        case "Return4":
          return LasReturnType.Return4;
        case "Return5":
          return LasReturnType.Return5;
        case "Return6":
          return LasReturnType.Return6;
        case "Return7":
          return LasReturnType.Return7;
        case "Return8":
          return LasReturnType.Return8;
        case "Return9":
          return LasReturnType.Return9;
        case "Return10":
          return LasReturnType.Return10;
        case "Return11":
          return LasReturnType.Return11;
        case "Return12":
          return LasReturnType.Return12;
        case "Return13":
          return LasReturnType.Return13;
        case "Return14":
          return LasReturnType.Return14;
        case "Return15":
          return LasReturnType.Return15;
        case "ReturnFirstOfMany":
          return LasReturnType.ReturnFirstOfMany;
        case "ReturnLastOfMany":
          return LasReturnType.ReturnLastOfMany;
        case "ReturnLast":
          return LasReturnType.ReturnLast;
        case "ReturnSingle":
          return LasReturnType.ReturnSingle;
        default:
          return LasReturnType.ReturnSingle;
      }
    }

  }
}
