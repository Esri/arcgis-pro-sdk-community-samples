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
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ComboBoxSelectFeature
{
  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class SelectFeature : ComboBox
  {
    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public SelectFeature()
    {
    }

    protected override void OnDropDownOpened()
    {
      // collect all features in a ComboFeature collection '_comboFeatures'
      UpdateCombo();
    }

    /// <summary>
    /// Updates the combo box with all the items.
    /// </summary>
    private async void UpdateCombo()
    {
      Clear();
      // get the state layer
      var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(fl => fl.Name.Equals("States"));
      if (featureLayer == null) return;

      // Add feature layer names to the combobox
      await QueuedTask.Run(() =>
      {
        using var featCursor = featureLayer.Search();
        while (featCursor.MoveNext())
        {
          using var feature = featCursor.Current as Feature;
          Add (new FeatureComboBoxItem(feature["STATE_NAME"].ToString(), feature.GetShape().Clone()));
        };
      });
    }

    /// <summary>
    /// The on comboBox selection change event. 
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override void OnSelectionChange(ComboBoxItem item)
    {
      if (item is FeatureComboBoxItem featComboBoxItem)
      {
        MapView.Active?.ZoomToAsync(featComboBoxItem.Geometry, TimeSpan.FromSeconds(1.5));
      }
      return;
    }

  }
}
