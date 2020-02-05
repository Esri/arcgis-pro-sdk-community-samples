//   Copyright 2019 Esri
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
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Dialogs;

namespace LockToSelectedRasters
{
    internal class LockToSelectedRasterButton : Button
    {
        CIMMosaicRule mosaicRule;
        bool excludeOverviews;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LockToSelectedRasterButton()
        {
            this.IsChecked = false;
            mosaicRule = null;
            excludeOverviews = true;
        }

        /// <summary>
        /// Destructor. Unsubscribe from the map selection changed event.
        /// </summary>
        ~LockToSelectedRasterButton()
        {
            // Reset the stored mosaic rule.
            mosaicRule = null;
            ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEvent.Unsubscribe(MapSelectionChanged);
        }

        /// <summary>
        /// Event handler for map selection changes. Once a selection is changed, 
        /// lock to the selected rasters in the mosaic layer selected in the Contents pane.
        /// </summary>
        private void MapSelectionChanged(ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEventArgs mapSelectionChangedArgs)
        {
            // Check if there is an active map.
            if (MapView.Active != null)
            {
                // For each map member with a selection on it.
                foreach (var element in mapSelectionChangedArgs.Selection)
                {
                    // Check if the current MapMember is a Feature sub-layer of a mosaic layer.
                    if (element.Key is FeatureMosaicSubLayer && ((FeatureLayer)element.Key).Name == "Footprint")
                    {
                        ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                        {
                            // Try and get the first selected layer.
                            Layer firstSelectedLayer = null;
                            try { firstSelectedLayer = MapView.Active.GetSelectedLayers().First(); } catch (Exception) { }
                            // Check if there are any selected layers and if the first selected layer is a mosaic layer.
                            if (firstSelectedLayer != null && firstSelectedLayer is MosaicLayer)
                            {
                                // Check if the MapMember with the selection is part of the mosaic layer that is selected.
                                FeatureMosaicSubLayer footprintSubLayer = element.Key as FeatureMosaicSubLayer;
                                if (((MosaicLayer)footprintSubLayer.Parent).Name == firstSelectedLayer.Name)
                                    SetMosaicRule((MosaicLayer)firstSelectedLayer, element.Key as FeatureLayer, element.Value);
                            }
                            else
                                MessageBox.Show("Please select a Mosaic Layer and make a selection for the Lock To Selected Rasters tool to work.");
                        });
                    }
                }
            }
        }

        void SetMosaicRule(MosaicLayer mosaicLayer, FeatureLayer footprintLayer, List<long> selectedItems)
        {
            ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                // Check if overviews are supposed to be excluded from the selection.
                string objectIDs = null;
                if (excludeOverviews)
                {
                    // Get the selected rows from the feature layer.
                    RowCursor selectedRows = footprintLayer.GetSelection().Search();
                    if (selectedRows.MoveNext())
                    {
                        using (var selectedRow = selectedRows.Current)
                        {
                            // Get the value for the Category field.
                            int tag = Convert.ToInt32(selectedRow.GetOriginalValue(selectedRows.FindField("Category")));
                            // For each row, if Category is not 2 (2 = overview), then add the object id to the list of items to lock to.
                            if (tag != 2)
                                objectIDs = selectedRow.GetOriginalValue(selectedRows.FindField("OBJECTID")).ToString();
                            while (selectedRows.MoveNext())
                            {
                                tag = Convert.ToInt32(selectedRow.GetOriginalValue(selectedRows.FindField("Category")));
                                if (tag != 2)
                                    objectIDs += "," + selectedRow.GetOriginalValue(selectedRows.FindField("OBJECTID")).ToString();
                            }
                        }
                    }
                }
                // Get the mosaic rule of the image sub-layer of the mosaic layer.
                ImageServiceLayer imageLayer = mosaicLayer.GetImageLayer() as ImageServiceLayer;
                CIMMosaicRule newMosaicRule = imageLayer.GetMosaicRule();
                // If there is no saved mosaic rule, then save the original mosaic rule of the mosaic layer.
                if (mosaicRule == null)
                {
                    mosaicRule = newMosaicRule;
                    // Then create a new mosaic rule.
                    newMosaicRule = new CIMMosaicRule();
                }
                // Set the Mosaic Method to 'Lock Raster'
                newMosaicRule.MosaicMethod = RasterMosaicMethod.LockRaster;
                // Set the object id's to lock to.
                if (excludeOverviews)
                    newMosaicRule.LockRasterID = objectIDs;
                else
                    newMosaicRule.LockRasterID = string.Join(",", selectedItems);
                // Update the mosaic layer with the changed mosaic rule.
                imageLayer.SetMosaicRule(newMosaicRule);
            });
        }

        /// <summary>
        /// OnClick method. Switch the tool on and off.
        /// </summary>
        protected override void OnClick()
        {
            // Check if the button is checked (tool is on).
            if (this.IsChecked)
            {
                // If button is checked, check off the button.
                this.IsChecked = false;
                // Set the mosaic rule back to what it was before the button was checked.
                ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    // Get the first selected layer.
                    Layer firstSelectedLayer = null;
                    try { firstSelectedLayer = MapView.Active.GetSelectedLayers().First(); } catch (Exception) { }
                    // Check if there are any selected layers and if the first selected layer is a mosaic layer.
                    if (firstSelectedLayer != null && firstSelectedLayer is MosaicLayer)
                    {
                        MosaicLayer selectedMosaicLayer = firstSelectedLayer as MosaicLayer;
                        ImageServiceLayer mosaicImageSublayer = selectedMosaicLayer.GetImageLayer() as ImageServiceLayer;
                        // Update the image service with the original mosaic rule.
                        mosaicImageSublayer.SetMosaicRule(mosaicRule);
                        // Reset the mosaic rule parameter
                        mosaicRule = null;
                    }
                });
                // Unsubscribe from the MapSelectionChanged event.
                ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEvent.Unsubscribe(MapSelectionChanged);
            }
            else
            {
                // If the button is checked off, check on the button.
                this.IsChecked = true;
                // Subscribe to the MapSelectionChanged event.
                ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEvent.Subscribe(MapSelectionChanged);
            }
        }
    }
}