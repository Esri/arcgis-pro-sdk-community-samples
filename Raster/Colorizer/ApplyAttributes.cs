//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System.Windows;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;
using ComboBox = ArcGIS.Desktop.Framework.Contracts.ComboBox;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;

namespace Colorizer
{
    /// <summary>
    /// Represents the Apply Colorizers ComboBox.
    /// </summary>
    internal class ApplyAttributes : ComboBox
    {
        BasicRasterLayer basicRasterLayer = null;
        // holds the names of field names in the raster's attribute table
        private List<string> _fieldList = new List<string>();

        /// <summary>
        /// Combo Box constructor. Make sure the combox box is enabled if raster layer is selected
        /// and subscribe to the layer selection changed event.
        /// </summary>
        public ApplyAttributes()
        {
      SelectedLayersChanged(null); // new ArcGIS.Desktop.Mapping.Events.MapViewEventArgs(MapView.Active));
            ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe(SelectedLayersChanged);

            Add(new ComboBoxItem("Msg: Select a Raster in the TOC"));
        }

        /// <summary>
        /// Event handler for layer selection changes.
        /// </summary>
        private async void SelectedLayersChanged(ArcGIS.Desktop.Mapping.Events.MapViewEventArgs mapViewArgs)
        {
            // Clears the combo box items when layer selection changes. 
            Clear();
            if (mapViewArgs == null) return;
            // Checks the state of the active pane. 
            // Returns the active pane state if the active pane is not null, else returns null.
            State state = (FrameworkApplication.Panes.ActivePane != null) ? FrameworkApplication.Panes.ActivePane.State : null;

            if (state != null && mapViewArgs.MapView != null)
            {
                // Gets the selected layers from the current Map.
                IReadOnlyList<Layer> selectedLayers = mapViewArgs.MapView.GetSelectedLayers();

                // The combo box will update only if one layer is selected.
                if (selectedLayers.Count == 1)
                {
                    // Gets the selected layer.
                    Layer firstSelectedLayer = selectedLayers.First();

                    // The combo box will update only if a raster layer is selected.
                    if (firstSelectedLayer != null && (firstSelectedLayer is BasicRasterLayer || firstSelectedLayer is MosaicLayer))
                    {
                        // Gets the basic raster layer from the selected layer. 
                        if (firstSelectedLayer is BasicRasterLayer)
                            basicRasterLayer = (BasicRasterLayer)firstSelectedLayer;
                        else if (firstSelectedLayer is MosaicLayer)
                            basicRasterLayer = ((MosaicLayer)firstSelectedLayer).GetImageLayer() as BasicRasterLayer;

                        // Updates the combo box with the corresponding colorizers for the selected layer.
                        await UpdateCombo(basicRasterLayer);

                        // Sets the combo box to display the first combo box item.
                        SelectedIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Destructor. Unsubscribe from the layer selection changed event.
        /// </summary>
        ~ApplyAttributes()
        {
            ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Unsubscribe(SelectedLayersChanged);
            Clear();
        }

        /// <summary>
        /// Updates the combo box with the raster's attribute table field names.
        /// </summary>
        /// <param name="basicRasterLayer">the selected layer.</param>
        private async Task UpdateCombo(BasicRasterLayer basicRasterLayer)
        {
            try
            {
                _fieldList.Clear();
                await QueuedTask.Run(() =>
                {
                    var rasterTbl = basicRasterLayer.GetRaster().GetAttributeTable();
                    if (rasterTbl == null) return;
                    var fields = rasterTbl.GetDefinition().GetFields();
                    foreach (var field in fields)
                    {
                        _fieldList.Add(field.Name);
                    }
                });
                bool hasRgb = _fieldList.Contains("red", StringComparer.OrdinalIgnoreCase)
                                && _fieldList.Contains("green", StringComparer.OrdinalIgnoreCase)
                                && _fieldList.Contains("blue", StringComparer.OrdinalIgnoreCase);
                if (_fieldList.Count == 0)
                {
                    Add(new ComboBoxItem("Msg: Raster has no Attribute table"));
                    return;
                }
                foreach (var fieldName in _fieldList)
                {
                    Add(new ComboBoxItem(fieldName));
                }
                if (hasRgb)
                {
                    Add(new ComboBoxItem(@"Attribute driven RGB"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Exception caught on Update combo box: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// The on comboBox selection change event. 
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected override void OnSelectionChange(ComboBoxItem item)
        {
            if (item == null)
                MessageBox.Show("The combo box item is null.", "Combo box Error:", MessageBoxButton.OK, MessageBoxImage.Error);
            if (item.Text.StartsWith("Msg: ")) return;
            if (string.IsNullOrEmpty(item.Text))
                MessageBox.Show("The combo box item text is null or empty.", "Combo box Error:", MessageBoxButton.OK, MessageBoxImage.Error);
            if (MapView.Active == null)
                MessageBox.Show("There is no active MapView.", "Combo box Error:", MessageBoxButton.OK, MessageBoxImage.Error);

            try
            {
                // Gets the first selected layer in the active MapView.
                Layer firstSelectedLayer = MapView.Active.GetSelectedLayers().First();

                // Gets the BasicRasterLayer from the selected layer.
                if (firstSelectedLayer is BasicRasterLayer)
                    basicRasterLayer = (BasicRasterLayer)firstSelectedLayer;
                else if (firstSelectedLayer is MosaicLayer)
                    basicRasterLayer = ((MosaicLayer)firstSelectedLayer).GetImageLayer() as BasicRasterLayer;
                else
                    MessageBox.Show("The selected layer is not a basic raster layer", "Select Layer Error:", MessageBoxButton.OK, MessageBoxImage.Error);
                if (basicRasterLayer != null)
                {
                    switch (item.Text) {
                        case @"Attribute driven RGB":
                            SetRasterColorByRGBAttributeFields(basicRasterLayer as RasterLayer, _fieldList);
                            break;
                        default:
                            var style = Project.Current.GetItems<StyleProjectItem>().First(s => s.Name == "ArcGIS Colors");
                            SetRasterColorByAttributeField(basicRasterLayer as RasterLayer, item.Text, style);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception caught in OnSelectionChange:" + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static async void SetRasterColorByRGBAttributeFields(RasterLayer raster, List<string>fields)
        {
            await QueuedTask.Run(() =>
            {
                // set fieldName to the first column
                var fieldName = "n/a";
                bool setFieldName = false;
                foreach (var attributeColumn in fields)
                {
                    if (attributeColumn.Equals("value", StringComparison.OrdinalIgnoreCase))
                        setFieldName = true;
                    else 
                    {
                        if (setFieldName)
                        {
                            fieldName = attributeColumn;
                            break;
                        }
                    }
                }
                var colorizerDef = new UniqueValueColorizerDefinition(fieldName);
                var colorizer = raster.CreateColorizer(colorizerDef);
                
                raster.SetColorizer(colorizer);
            });
        }

        private static async void SetRasterColorByAttributeField(RasterLayer raster, string fieldName, StyleProjectItem styleProjItem)
        {
            await QueuedTask.Run(() =>
            {
                var ramps = styleProjItem.SearchColorRamps("Green Blues");
                CIMColorRamp colorRamp = ramps[0].ColorRamp;
                var colorizerDef = new UniqueValueColorizerDefinition(fieldName, colorRamp);
                var colorizer = raster.CreateColorizer(colorizerDef);
                // fix up colorizer ... due to a problem with getting the values for different attribute table fields:
                // we use the Raster's attribute table to collect a dictionary with the correct replacement values
                Dictionary<string, string> landuseToFieldValue = new Dictionary<string, string>();
                if (colorizer is CIMRasterUniqueValueColorizer uvrColorizer)
                {
                    var rasterTbl = raster.GetRaster().GetAttributeTable();
                    var cursor = rasterTbl.Search();
                    while (cursor.MoveNext())
                    {
                        var row = cursor.Current;
                        var correctField = row[fieldName].ToString();
                        var key = row[uvrColorizer.Groups[0].Heading].ToString();
                        landuseToFieldValue.Add(key.ToLower(), correctField);
                    }
                    uvrColorizer.Groups[0].Heading = fieldName;
                    for (var idxGrp = 0; idxGrp < uvrColorizer.Groups[0].Classes.Length; idxGrp++)
                    {
                        var grpClass = uvrColorizer.Groups[0].Classes[idxGrp];
                        var oldValue = grpClass.Values[0].ToLower();
                        var correctField = landuseToFieldValue[oldValue];
                        grpClass.Values[0] = correctField;
                        grpClass.Label = $@"{correctField}";
                    }
                }
                raster.SetColorizer(colorizer);
            });
        }
    }
}
