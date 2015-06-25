//   Copyright 2015 Esri
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

namespace AddRasterLayer
{
    /// <summary>
    /// This is the class for a button that adds a new raster layer to the map.
    /// </summary>
    /// <remarks>
    /// Switch to the ADD-IN tab and click the Add Raster Layer button to add a new raster layer to the map.
    /// </remarks>
    class AddRasterLayerButton: Button
    {
        /// <summary>
        /// Click handler for the button.
        /// </summary>
        protected override void OnClick()
        {
            AddRasterLayerToMap();
        }

        /// <summary>
        /// Create a raster layer and add it to a map.
        /// </summary>
        /// <returns>Task that contains a layer.</returns>
        private async Task AddRasterLayerToMap()
        {
            try
            {
                // Get the first 2D map from the current project.
                Map myMap = null;
                myMap = await GetMapFromProject(Project.Current, "CIMPATH=map/map.xml");
                if (myMap == null)
                {
                    System.Windows.MessageBox.Show("Failed to get map.");
                    return;
                }

               
                // Create a url pointing to the source. In this case it is a url to an image service 
                // which will result in an image service layer being created.
                string dataSoureUrl = @"http://imagery.arcgisonline.com/arcgis/services/LandsatGLS/GLS2010_Enhanced/ImageServer";
                // Note: A url can also point to  
                // 1.) An image on disk or an in a file geodatabase. e.g. string dataSoureUrl = @"C:\temp\a.tif"; This results in a raster layer.
                // 2.) A mosaic dataset in a file gdb e.g. string dataSoureUrl = @"c:\temp\mygdb.gdb\MyMosaicDataset"; This results in a mosaic layer.
                // 3.) A raster or mosaic dataset in an enterprise geodatabase.

                // Create an ImageServiceLayer object to hold the new layer.
                ImageServiceLayer rasterLayer = null;

                // The layer has to be created on the Main CIM Thread (MCT).
                await QueuedTask.Run(() =>
                {
                    // Create a layer based on the url. In this case the layer we are creating is an image service layer.
                    rasterLayer = (ImageServiceLayer)LayerFactory.CreateLayer(new Uri(dataSoureUrl), myMap);

                    // Check if it is created.
                    if (rasterLayer == null)
                    {
                        System.Windows.MessageBox.Show("Failed to create layer for url:" + dataSoureUrl);
                        return;
                    }
                    
                    // Validate the colorizer to see if the layer is colorized correctly.
                    if (!(rasterLayer.GetColorizer() is CIMRasterRGBColorizer))
                        System.Windows.MessageBox.Show("Colorizer does not match for layer created from url: " + dataSoureUrl);
                });
            }
            catch (Exception exc)
            {
                // Catch any exception found and display a message box.
                System.Windows.MessageBox.Show("Exception caught: " + exc.Message);
                return;
            }
        }

        /// <summary>
        /// Gets the map from a project that matches a map path.
        /// </summary>
        /// <param name="project">The project in which the map resides.</param>
        /// <param name="mapPath">The map path to identify the map.</param>
        /// <returns>A Task representing the map.</returns>
        private Task<Map> GetMapFromProject(Project project, string mapPath)
        {
            // Return null if either of the two parameters are invalid.
            if (project == null || string.IsNullOrEmpty(mapPath))
                return null;

            // Get all the maps from the project and
            var mapProjectItems = project.GetItems<MapProjectItem>().ToList();
            // iterate through them to see if any of them match the map path provided.
            foreach (MapProjectItem nextMapProjectItem in mapProjectItems)
            {
                if (nextMapProjectItem.Path == mapPath)
                    return QueuedTask.Run<Map>(() => { return nextMapProjectItem.GetMap(); }, Progressor.None);
            }
            // If none match, return null.
            return null;
        }
    }
}
