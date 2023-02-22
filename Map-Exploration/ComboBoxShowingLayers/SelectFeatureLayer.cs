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

namespace ComboBoxShowingLayers
{
  /// <summary>
  /// Represents the ComboBox
  /// </summary>
  internal class SelectFeatureLayer : ComboBox
  {
    private bool _isInitialized;

    /// <summary>
    /// Combo Box constructor
    /// </summary>
    public SelectFeatureLayer()
    {
      UpdateCombo();
    }

    /// <summary>
    /// Updates the combo box with all the items.
    /// </summary>
    private async void UpdateCombo()
    {
      if (_isInitialized)
        SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
      if (!_isInitialized)
      {
        Clear();
        //subscribe to events to populate snap layer list when the map changes, layers added/removed
        ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
        if (MapView.Active != null)
        {
          OnActiveMapViewChanged(new ActiveMapViewChangedEventArgs(MapView.Active, null));
        }
        LayersAddedEvent.Subscribe(OnLayersAdd);
        LayersRemovedEvent.Subscribe(OnLayersRem);
        _isInitialized = true;
      }
      //set the default item in the comboBox
      SelectedItem = ItemCollection.FirstOrDefault();
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
    {
      Clear();
      if (args.IncomingView != null)
      {
        var layerlist = args.IncomingView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
        //Add feature layer names to the combobox
        QueuedTask.Run(() =>
        {
          foreach (var layer in layerlist)
          {
            Add(MakeComboBoxItem(layer.GetDefinition() as CIMFeatureLayer));
          }
        });
      }
    }

    private async void OnLayersAdd(LayerEventsArgs args)
    {
      //populate combobox list when layers are added or removed
      //run on UI Thread to sync layersadded event (which runs on background)
      var existingLayerNames = this.ItemCollection.Select(i => i.ToString());
      foreach (var addedLayer in args.Layers)
      {
        var featureLayer = addedLayer as FeatureLayer;
        if (featureLayer == null) continue;
        if (!existingLayerNames.Contains(addedLayer.Name))
        {
          var comboItem = await QueuedTask.Run(() =>
          {
            return MakeComboBoxItem(addedLayer.GetDefinition() as CIMFeatureLayer);
          });
          _ = System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() => { this.Add(comboItem); }));
        }
      }
    }

    private void OnLayersRem(LayerEventsArgs args)
    {
      //populate combobox list when layers are added or removed
      //run on UI Thread to sync layersadded event (which runs on background)
      var existingLayerNames = this.ItemCollection.Select(i => i.ToString());
      foreach (var removedLayer in args.Layers)
      {
        if (existingLayerNames.Contains(removedLayer.Name))
          System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() => { this.Remove(removedLayer.Name); }));
      }
    }

    /// <summary>
    /// The on comboBox selection change event. 
    /// </summary>
    /// <param name="item">The newly selected combo box item</param>
    protected override void OnSelectionChange(ComboBoxItem item)
    {
      if (item == null)
        return;

      if (string.IsNullOrEmpty(item.Text))
        return;

      MessageBox.Show($@"This feature layer was selected: {item.Text}");
    }

    ComboBoxItem MakeComboBoxItem(CIMFeatureLayer cimFeatureLayer)
    {
      var toolTip = $@"Select this feature layer: {cimFeatureLayer.Name}";
      var cimRenderer = cimFeatureLayer.Renderer as CIMSimpleRenderer;
      if (cimRenderer == null)
      {
        return new ComboBoxItem(cimFeatureLayer.Name, null, toolTip);
      }
      var si = new SymbolStyleItem()
      {
        Symbol = cimRenderer.Symbol.Symbol,
        PatchHeight = 16,
        PatchWidth = 16
      };
      var bm = si.PreviewImage as BitmapSource;
      bm.Freeze();
      return new ComboBoxItem(cimFeatureLayer.Name, bm, toolTip);
    }
  }
}
