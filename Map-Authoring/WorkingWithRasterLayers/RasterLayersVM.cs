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
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Core;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Collections.Specialized;
using ArcGIS.Desktop.Framework.Dialogs;
using System.Threading;

namespace WorkingWithRasterLayers
{
    static class RasterLayersVM
    {
        /// <summary>
        /// Create an image service layer and add it to the first 2D map.
        /// </summary>
        /// <returns>Task that contains a layer.</returns>
        public static async Task AddRasterLayerToMapAsync()
        {
            try
            {
                // Get the first 2D map from the project that is called Map.
                Map _map = await GetMapFromProject(Project.Current, "Map");
                if (_map == null)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("AddRasterLayerToMap: Failed to get map.");
                    return;
                }

                // Create a url pointing to the source. In this case it is a url to an image service 
                // which will result in an image service layer being created.
                string dataSoureUrl = @"https://landsat2.arcgis.com/arcgis/services/Landsat/MS/ImageServer";
                // Note: A url can also point to  
                // 1.) An image on disk or an in a file geodatabase. e.g. string dataSoureUrl = @"C:\temp\a.tif"; This results in a raster layer.
                // 2.) A mosaic dataset in a file gdb e.g. string dataSoureUrl = @"c:\temp\mygdb.gdb\MyMosaicDataset"; This results in a mosaic layer.
                // 3.) A raster or mosaic dataset in an enterprise geodatabase.

                // Create an ImageServiceLayer object to hold the new layer.
                ImageServiceLayer imageServiceLayer = null;

                // The layer has to be created on the Main CIM Thread (MCT).
                await QueuedTask.Run(() =>
                {
                    // Create a layer based on the url. In this case the layer we are creating is an image service layer.
                    imageServiceLayer = (ImageServiceLayer)LayerFactory.Instance.CreateLayer(new Uri(dataSoureUrl), _map);
                    if (imageServiceLayer == null)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Failed to create layer for url:" + dataSoureUrl);
                        return;
                    }
                });
            }
            catch (Exception exc)
            {
                // Catch any exception found and display a message box.
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught while trying to add layer: " + exc.Message);
                return;
            }
        }

        /// <summary>
        /// Set the resampling type on the first selected image service layer in the first open 2D map.
        /// </summary>
        /// <param name="resamplingType">The resampling type to set on the layer.</param>
        /// <returns></returns>
        public static async Task SetResamplingTypeAsync(RasterResamplingType resamplingType)
        {
            try
            {
                // Get the first 2D map from the project that is called Map.
                Map _map = await GetMapFromProject(Project.Current, "Map");
                if (_map == null)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("SetResamplingType: Failed to get map.");
                    return;
                }

                // Get the most recently selected layer.                
                Layer firstSelectedLayer = MapView.Active.GetSelectedLayers().First();
                // Check if the first selected layer is an image service layer.
                if (firstSelectedLayer is ImageServiceLayer)
                {
                    // Set the colorizer on the most recently selected layer.
                    // The colorizer has to be get/set on the Main CIM Thread (MCT).
                    await QueuedTask.Run(() =>
                    {
                        // Get the colorizer from the selected layer.
                        CIMRasterColorizer newColorizer = ((BasicRasterLayer)firstSelectedLayer).GetColorizer();
                        // Set the resampling type on the colorizer.
                        newColorizer.ResamplingType = resamplingType;
                        // Update the image service with the new colorizer
                        ((BasicRasterLayer)firstSelectedLayer).SetColorizer(newColorizer);
                    });
                }
            }
            catch (Exception exc)
            {
                // Catch any exception found and display a message box.
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught while trying to set resampling type: " + exc.Message);
                return;
            }
        }

        /// <summary>
        /// Set the processing template on the first selected image service layer in the first open 2D map.
        /// </summary>
        /// <param name="templateName">The name of the processing template to set on the layer.</param>
        /// <returns></returns>
        public static async Task SetProcessingTemplateAsync(string templateName)
        {
            try
            {
                // Get the first 2D map from the project that is called Map.
                Map _map = await GetMapFromProject(Project.Current, "Map");
                if (_map == null)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("SetResamplingType: Failed to get map.");
                    return;
                }

                // Get the most recently selected layer.                
                Layer firstSelectedLayer = MapView.Active.GetSelectedLayers().First();
                // Check if the first selected layer is an image service layer.
                if (firstSelectedLayer is ImageServiceLayer)
                {
                    // Set the colorizer on the most recently selected layer.
                    // The colorizer has to be set on the Main CIM Thread (MCT).
                    ImageServiceLayer isLayer = (ImageServiceLayer)firstSelectedLayer;
                    await QueuedTask.Run(() =>
                    {
                      // Create a new Rendering rule
                      CIMRenderingRule setRenderingrule = new CIMRenderingRule()
                      {
                        // Set the name of the rendering rule.
                        Name = templateName
                      };
                      // Update the image service with the new mosaic rule.
                      isLayer.SetRenderingRule(setRenderingrule);
                    });
                }
            }
            catch (Exception exc)
            {
                // Catch any exception found and display a message box.
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught while trying to set processing template: " + exc.Message);
                return;
            }
        }

        /// <summary>
        /// Set the stretch type on the first selected image service layer in the first open 2D map.
        /// </summary>
        /// <param name="stretschType">The stretch type to set on the layer.</param>
        /// <param name="statsType">The stretch statistics type to set on the layer. This lets you pick between Dataset, Area of View (to enable DRA) or Custom statistics.</param>
        /// <returns></returns>
        public static async Task SetStretchTypeAsync(RasterStretchType stretschType, RasterStretchStatsType statsType = RasterStretchStatsType.Dataset)
        {
            try
            {
                // Get the first 2D map from the project that is called Map.
                Map _map = await GetMapFromProject(Project.Current, "Map");
                if (_map == null)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("SetStretchType: Failed to get map.");
                    return;
                }

                // Get the most recently selected layer.                
                Layer firstSelectedLayer = MapView.Active.GetSelectedLayers().First();
                // Check if the first selected layer is an image service layer.
                if (firstSelectedLayer is ImageServiceLayer)
                {
                    // Set the colorizer on the most recently selected layer.
                    // The colorizer has to be set on the Main CIM Thread (MCT).
                    await QueuedTask.Run(() =>
                    {
                        // Get the colorizer from the selected layer.
                        CIMRasterColorizer newColorizer = ((BasicRasterLayer)firstSelectedLayer).GetColorizer();
                        // Set the stretch type and stretch statistics type on the colorizer.
                        // Theese parameters only apply to the Stretch and RGB colorizers.
                        if (newColorizer is CIMRasterRGBColorizer)
                        {
                            ((CIMRasterRGBColorizer)newColorizer).StretchType = stretschType;
                            ((CIMRasterRGBColorizer)newColorizer).StretchStatsType = statsType;
                        }
                        else if (newColorizer is CIMRasterStretchColorizer)
                        {
                            ((CIMRasterStretchColorizer)newColorizer).StretchType = stretschType;
                            ((CIMRasterStretchColorizer)newColorizer).StatsType = statsType;
                        }
                        else
                            MessageBox.Show("Selected layer must be visualized using the RGB or Stretch colorizer");
                        // Update the image service with the new colorizer
                        ((BasicRasterLayer)firstSelectedLayer).SetColorizer(newColorizer);
                    });
                }
            }
            catch (Exception exc)
            {
                // Catch any exception found and display a message box.
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught while trying to set stretch type: " + exc.Message);
                return;
            }
        }

        /// <summary>
        /// Set the compression type and compression quality (if applicable) on the first selected image service layer in the first open 2D map.
        /// </summary>
        /// <param name="type">The compression type to set on the layer.</param>
        /// <param name="quality">The compression quality to set on the layer.</param>
        /// <returns></returns>
        public static async Task SetCompressionAsync(string type, int quality = 80)
        {
            try
            {
                // Get the first 2D map from the project that is called Map.
                Map _map = await GetMapFromProject(Project.Current, "Map");
                if (_map == null)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("SetCompression: Failed to get map.");
                    return;
                }

                // Get the most recently selected layer.                
                Layer firstSelectedLayer = MapView.Active.GetSelectedLayers().First();
                // Check if the first selected layer is an image service layer.
                if (firstSelectedLayer is ImageServiceLayer)
                {
                    // Set the compression type and quality on the most recently selected layer.
                    // The compression has to be set on the Main CIM Thread (MCT).
                    await QueuedTask.Run(() =>
                    {
                        ((ImageServiceLayer)firstSelectedLayer).SetCompression(type, quality);
                    });
                }
            }
            catch (Exception exc)
            {
                // Catch any exception found and display a message box.
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught while trying to set compression: " + exc.Message);
                return;
            }
        }

        /// <summary>
        /// Gets the map from a project that matches a map name.
        /// </summary>
        /// <param name="project">The project in which the map resides.</param>
        /// <param name="mapName">The map name to identify the map.</param>
        /// <returns>A Task representing the map.</returns>
        private static Task<Map> GetMapFromProject(Project project, string mapName)
        {
            // Return null if either of the two parameters are invalid.
            if (project == null || string.IsNullOrEmpty(mapName))
                return null;

            // Find the first project item with name matches with mapName
            MapProjectItem mapProjItem =
                project.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals(mapName, StringComparison.CurrentCultureIgnoreCase));

            if (mapProjItem != null)
                return QueuedTask.Run<Map>(() => { return mapProjItem.GetMap(); }, Progressor.None);
            else
                return null;
        }
    }
}
