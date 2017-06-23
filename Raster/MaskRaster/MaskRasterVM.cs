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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Core.Data;
using System.IO;
using ArcGIS.Desktop.Core;

namespace MaskRaster
{
    /// <summary>
    /// Viewmodel class that allows functions to mask raster pixels to be UI agnostic.
    /// </summary>
    static class MaskRasterVM
    {
        /// <summary>
        /// Mask raster pixels based on the rectangle given and save the output in the 
        /// current project folder.
        /// </summary>
        /// <param name="geometry">Rectangle to use to mask raster pixels.</param>
        public static async void MaskRaster(Geometry geometry)
        {
            try
            {
                // Check if there is an active map view.
                if (MapView.Active != null)
                {
                    // Get the active map view.
                    var mapView = MapView.Active;
                    // Get the list of selected layers.
                    IReadOnlyList<Layer> selectedLayerList = mapView.GetSelectedLayers();
                    if (selectedLayerList.Count == 0)
                    {
                        // If no layers are selected show a message box with the appropriate message.
                        MessageBox.Show("No Layers selected. Please select one Raster layer.");
                    }
                    else
                    {
                        // Get the most recently selected layer.                
                        Layer firstSelectedLayer = mapView.GetSelectedLayers().First();
                        if (firstSelectedLayer is RasterLayer)
                        {
                            // Working with rasters requires the MCT.
                            await QueuedTask.Run(() =>
                            {
                                #region Get the raster dataset from the currently selected layer
                                // Get the raster layer from the selected layer.
                                RasterLayer currentRasterLayer = firstSelectedLayer as RasterLayer;
                                // Get the raster from the current selected raster layer.
                                Raster inputRaster = currentRasterLayer.GetRaster();
                                // Get the basic raster dataset from the raster.
                                BasicRasterDataset basicRasterDataset = inputRaster.GetRasterDataset();
                                if (!(basicRasterDataset is RasterDataset))
                                {
                                    // If the dataset is not a raster dataset, show a message box with the appropriate message.
                                    MessageBox.Show("No Raster Layers selected. Please select one Raster layer.");
                                    return;
                                }
                                // Get the input raster dataset from the basic raster dataset.
                                RasterDataset rasterDataset = basicRasterDataset as RasterDataset;
                                #endregion

                                #region Save a copy of the raster dataset in the project folder and open it
                                // Create a full raster from the input raster dataset.
                                inputRaster = rasterDataset.CreateFullRaster();
                                // Setup the paths and name of the output file and folder inside the project folder.
                                string ouputFolderName = "MaskedOuput";
                                string outputFolder = Path.Combine(Project.Current.HomeFolderPath, ouputFolderName); ;
                                string outputName = "MaskedRaster.tif";
                                // Delete the output directory if it exists and create it.
                                // Note: You will need write access to the project directory for this sample to work.
                                if (Directory.Exists(outputFolder))
                                    Directory.Delete(outputFolder, true);
                                Directory.CreateDirectory(outputFolder);

                                // Create a new file system connection path to open raster datasets using the output folder path.
                                FileSystemConnectionPath outputConnectionPath = new FileSystemConnectionPath(
                                new System.Uri(outputFolder), FileSystemDatastoreType.Raster);
                                // Create a new file system data store for the connection path created above.
                                FileSystemDatastore outputFileSytemDataStore = new FileSystemDatastore(outputConnectionPath);
                                // Create a new raster storage definition. 
                                RasterStorageDef rasterStorageDef = new RasterStorageDef();
                                // Set the pyramid level to 0 meaning no pyramids will be calculated. This is required 
                                // because we are going to change the pixels after we save the raster dataset and if the 
                                // pyramids are calculated prior to that, the pyramids will be incorrect and will have to
                                // be recalculated.
                                rasterStorageDef.SetPyramidLevel(0);
                                // Save a copy of the raster using the file system data store and the raster storage definition.
                                inputRaster.SaveAs(outputName, outputFileSytemDataStore, "TIFF", rasterStorageDef);

                                // Open the raster dataset you just saved.
                                rasterDataset = OpenRasterDataset(outputFolder, outputName);
                                // Create a full raster from it so we can modify pixels.
                                Raster outputRaster = rasterDataset.CreateFullRaster();
                                #endregion

                                #region Get the Min/Max Row/Column to mask
                                // If the map spatial reference is different from the spatial reference of the input raster,
                                // set the map spatial reference on the input raster. This will ensure the map points are 
                                // correctly reprojected to image points.
                                if (mapView.Map.SpatialReference.Name != inputRaster.GetSpatialReference().Name)
                                    inputRaster.SetSpatialReference(mapView.Map.SpatialReference);

                                // Use the MapToPixel method of the input raster to get the row and column values for the 
                                // points of the rectangle.
                                Tuple<int, int> tlcTuple = inputRaster.MapToPixel(geometry.Extent.XMin, geometry.Extent.YMin);
                                Tuple<int, int> lrcTuple = inputRaster.MapToPixel(geometry.Extent.XMax, geometry.Extent.YMax);

                                int minCol = (int)tlcTuple.Item1;
                                int minRow = (int)tlcTuple.Item2;
                                int maxCol = (int)lrcTuple.Item1;
                                int maxRow = (int)lrcTuple.Item2;

                                // Ensure the min's are less than the max's.
                                if (maxCol < minCol)
                                {
                                    int temp = maxCol;
                                    maxCol = minCol;
                                    minCol = temp;
                                }

                                if (maxRow < minRow)
                                {
                                    int temp = maxRow;
                                    maxRow = minRow;
                                    minRow = temp;
                                }
                                // Ensure the mins and maxs are within the raster.
                                minCol = (minCol < 0) ? 0 : minCol;
                                minRow = (minRow < 0) ? 0 : minRow;
                                maxCol = (maxCol > outputRaster.GetWidth()) ? outputRaster.GetWidth() : maxCol;
                                maxRow = (maxRow > outputRaster.GetHeight()) ? outputRaster.GetHeight() : maxRow;
                                #endregion

                                #region Mask pixels based on the rectangle drawn by the user
                                // Calculate the width and height of the pixel block to create.
                                int pbWidth = maxCol - minCol;
                                int pbHeight = maxRow - minRow;
                                // Check to see if the output raster can be edited.
                                if (!outputRaster.CanEdit())
                                {
                                    // If not, show a message box with the appropriate message.
                                    MessageBox.Show("Cannot edit raster :(");
                                    return;
                                }
                                // Create a new pixel block from the output raster of the height and width calculated above.
                                PixelBlock currentPixelBlock = outputRaster.CreatePixelBlock(pbWidth, pbHeight);
                                // Iterate over the bands of the output raster.
                                for (int plane = 0; plane < currentPixelBlock.GetPlaneCount(); plane++)
                                {
                                    // For each band, clear the pixel block.
                                    currentPixelBlock.Clear(plane);
                                    //Array noDataMask = currentPixelBlock.GetNoDataMask(plane, true);
                                    //for (int i = 0; i < noDataMask.GetLength(0); i++)
                                    //    noDataMask.SetValue(Convert.ToByte(0), i);
                                    //currentPixelBlock.SetNoDataMask(plane, noDataMask);
                                }
                                // Write the cleared pixel block to the output raster dataset.
                                outputRaster.Write(minCol, minRow, currentPixelBlock);
                                // Refresh the properties of the output raster dataset.
                                outputRaster.Refresh();
                                #endregion

                                // Create a new layer from the masked raster dataset and add it to the map.
                                LayerFactory.Instance.CreateLayer(new Uri(Path.Combine(outputFolder, outputName)),
                                mapView.Map);
                                // Disable the layer representing the original raster dataset.
                                firstSelectedLayer.SetVisibility(false);
                            });
                        }
                        else
                        {
                            // If the selected layer is not a raster layer show a message box with the appropriate message.
                            MessageBox.Show("No Raster layers selected. Please select one Raster layer.");
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Exception caught in MaskRaster: " + exc.Message);
            }
        }

        /// <summary>
        /// Open a Raster Dataset given a folder and a dataset name.
        /// </summary>
        /// <param name="folder">Full path to the folder containing the raster dataset.</param>
        /// <param name="name">Name of the raster dataset to open.</param>
        /// <returns></returns>
        public static RasterDataset OpenRasterDataset(string folder, string name)
        {
            // Create a new raster dataset which is set to null
            RasterDataset rasterDatasetToOpen = null;
            try
            {
                // Create a new file system connection path to open raster datasets using the folder path.
                FileSystemConnectionPath connectionPath = new FileSystemConnectionPath(new System.Uri(folder), FileSystemDatastoreType.Raster);
                // Create a new file system data store for the connection path created above.
                FileSystemDatastore dataStore = new FileSystemDatastore(connectionPath);
                // Open the raster dataset.
                rasterDatasetToOpen = dataStore.OpenDataset<RasterDataset>(name);
                // Check if it is not null. If it is show a message box with the appropriate message.
                if (rasterDatasetToOpen == null)
                    MessageBox.Show("Failed to open raster dataset: " + name);
            }
            catch (Exception exc)
            {
                // If an exception occurs, show a message box with the appropriate message.
                MessageBox.Show("Exception caught in OpenRasterDataset for raster: " + name + exc.Message);
            }
            return rasterDatasetToOpen;
        }
    }
}
