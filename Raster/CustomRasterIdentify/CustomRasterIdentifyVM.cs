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

using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data.Raster;

namespace CustomRasterIdentify
{
    /// <summary>
    /// Viewmodel class to house functions that enable custom raster identify to be UI agnostic.
    /// </summary>
    static class CustomRasterIdentifyVM
    {
        /// <summary>
        /// Function to identify rendered and source dataset pixel values for one or more 
        /// raster, mosaic and image service layers.
        /// </summary>
        /// <param name="mapPoint">Map point to identify pixel values.</param>
        public static async void CustomRasterIdentify(MapPoint mapPoint)
        {
            // Create a new list of popup pages.
            var popupContents = new List<PopupContent>();
            // Create an empty string that will represent what goes into a popup page.
            string identifiedVal = "";
            // Create the popup pages to show.
            await QueuedTask.Run(() =>
            {
                // Check if there is an active map view.
                if (MapView.Active != null)
                {
                    // Get the active map view.
                    var mapView = MapView.Active;
                    // Get the list of selected layers.
                    IReadOnlyList<Layer> selectedLayerList = MapView.Active.GetSelectedLayers();
                    if (selectedLayerList.Count == 0)
                    {
                        // If no layers are selected fill the popup page with the appropriate message.
                        // Note: you can use html tags to format the text.
                        identifiedVal += "<p>No Layers selected. Please select one or more Raster, Mosaic or Image Service layers.</p>";
                        // Add the popup page to the list of pages.
                        popupContents.Add(new PopupContent(identifiedVal, "Custom Raster Identify"));
                    }
                    else
                    {
                        // Iterate over the list of selected layers.
                        foreach (Layer currentSelectedLayer in selectedLayerList)
                        {
                            #region Get a basic raster layer from the selected layer.
                            BasicRasterLayer currentRasterLayer = null;
                            if (currentSelectedLayer is MosaicLayer)
                            {
                                // If the current selected layer is a mosaic layer,
                                MosaicLayer mosaicLayer = currentSelectedLayer as MosaicLayer;
                                // Get the image sub-layer from the mosaic layer. This is a basic raster layer.
                                currentRasterLayer = mosaicLayer.GetImageLayer() as BasicRasterLayer;
                            }
                            else if (currentSelectedLayer is BasicRasterLayer)
                            {
                                // If the current selected layer is a raster layer or image service layer, 
                                // both are already basic raster layers.
                                currentRasterLayer = currentSelectedLayer as BasicRasterLayer;
                            }
                            else
                            {
                                // If the current selected layer is neither a mosaic nor a raster or image service layer,
                                // fill the popup page with the appropriate message.
                                identifiedVal += "<p>Selected layer is not a raster layer. Please select one or more Raster, Mosaic or Image Service layers.</p>";
                                // Add the popup page to the list of pages.
                                popupContents.Add(new PopupContent(identifiedVal, "Custom Raster Identify for: " + currentSelectedLayer.Name));
                                continue;
                            }
                            #endregion

                            #region Get the pixel value for the rendered raster.
                            // Add the label for the rendered pixel value.
                            identifiedVal += "<b>Rendered Pixel value: </b>";
                            // Get the raster from the current selected raster layer.
                            Raster raster = currentRasterLayer.GetRaster();
                            // If the map spatial reference is different from the spatial reference of the raster,
                            // set the map spatial reference on the raster. This will ensure the map points are 
                            // correctly reprojected to image points.
                            if (mapView.Map.SpatialReference.Name != raster.GetSpatialReference().Name)
                                raster.SetSpatialReference(mapView.Map.SpatialReference);
                            
                            // Convert the map point to be identified into an image point.
                            Tuple<int, int> imagePoint = raster.MapToPixel(mapPoint.X, mapPoint.Y);
                            if ((int)imagePoint.Item1 < 0 || (int)imagePoint.Item1 > raster.GetWidth()
                            || (int)imagePoint.Item2 < 0 || (int)imagePoint.Item2 > raster.GetHeight())
                            {
                                // If the point is outside the image, fill the pixel value with the appropriate message.
                                identifiedVal += "Point is not within image. \n";
                            }
                            else
                            {
                                // If the point is within the image. Iterate over the bands in the raster.
                                for (int band = 0; band < raster.GetBandCount(); band++)
                                {
                                    // Get the pixel value based on the band, column and row and add the 
                                    // formatted pixel value to the popup page.
                                    if (band == 0)
                                        identifiedVal += raster.GetPixelValue(band, (int)imagePoint.Item1, (int)imagePoint.Item2).ToString();
                                    else
                                        identifiedVal += ", " + raster.GetPixelValue(band, (int)imagePoint.Item1, (int)imagePoint.Item2).ToString();
                                }
                                identifiedVal += ".";
                            }
                            // Add the rendered pixel value to the popup page contents.
                            string htmlContent = "<p>" + identifiedVal + "</p>";
                            #endregion

                            #region Get the pixel value for the source raster dataset.
                            // Add the label for the source dataset pixel value.
                            identifiedVal = "<b>Dataset Pixel value: </b>";
                            // Get the basic raster dataset from the raster.
                            BasicRasterDataset basicRasterDataset = raster.GetRasterDataset();
                            if (!(basicRasterDataset is RasterDataset))
                            {
                                // If the dataset is not a raster dataset, fill the pixel value with the appropriate message.
                                identifiedVal += "Selected layer is not a Raster Layer. Please select one or more Raster, Mosaic or Image Service layers.";
                                htmlContent += "<p>" + identifiedVal + "</p>";
                                popupContents.Add(new PopupContent(htmlContent, "Custom Raster Identify for " + currentSelectedLayer.Name));
                            }
                            // Get the raster dataset.
                            RasterDataset rasterDataset = basicRasterDataset as RasterDataset;
                            // Create a full raster from the raster dataset.
                            raster = rasterDataset.CreateFullRaster();
                            // If the map spatial reference is different from the spatial reference of the raster,
                            // Set the map spatial reference on the raster. This will ensure the map points are 
                            // correctly reprojected to image points.
                            if (mapView.Map.SpatialReference.Name != raster.GetSpatialReference().Name)
                                raster.SetSpatialReference(mapView.Map.SpatialReference);

                            // Convert the map point to be identified to an image point.
                            imagePoint = raster.MapToPixel(mapPoint.X, mapPoint.Y);
                            if ((int)imagePoint.Item1 < 0 || (int)imagePoint.Item1 > raster.GetWidth()
                            || (int)imagePoint.Item2 < 0 || (int)imagePoint.Item2 > raster.GetHeight())
                            {
                                // If the point is outside the image, fill the pixel value with the appropriate message.
                                identifiedVal += "Point is not within image. \n";
                            }
                            else
                            {
                                // If the point is within the image. Iterate over the bands in the raster.
                                for (int band = 0; band < raster.GetBandCount(); band++)
                                {
                                    // Get the pixel value based on the band, column and row and add the 
                                    // formatted pixel value to the popup page.
                                    if (band == 0)
                                    {
                                        identifiedVal += raster.GetPixelValue(band, (int)imagePoint.Item1, (int)imagePoint.Item2).ToString();
                                    }
                                    else
                                        identifiedVal += ", " + raster.GetPixelValue(band, (int)imagePoint.Item1, (int)imagePoint.Item2).ToString();
                                }
                                identifiedVal += ".";
                            }
                            // Add the source dataset pixel value to the popup page contents.
                            htmlContent += "<p>" + identifiedVal + "</p>";
                            #endregion

                            // Add the popup page to the list of pages.
                            popupContents.Add(new PopupContent(htmlContent, "Custom Raster Identify for " + currentSelectedLayer.Name));
                            // Reset
                            identifiedVal = "";
                        }
                    }
                }
            });
            // Show custom popup with the list of popup pages created above.
            MapView.Active.ShowCustomPopup(popupContents);
        }
    }
}
