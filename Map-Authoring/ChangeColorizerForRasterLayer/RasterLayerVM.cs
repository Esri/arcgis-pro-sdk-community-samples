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

using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeColorizerForRasterLayer
{
  class RasterLayerVM
  {
    /// <summary>
    /// Creates an image service layer and add it to the first 2D map.
    /// </summary>
    /// <returns>Task that contains a layer.</returns>
    public static async Task AddRasterLayerToMapAsync()
    {
      try
      {
        // Gets the first 2D map from the project that is called Map.
        Map _map = await GetMapFromProject(Project.Current, "Map");

        if (_map == null)
        {
          ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("AddRasterLayerToMap: Failed to get map.");
          return;
        }

        // Creates a url pointing to the source. In this case it is a url to an image service 
        // which will result in an image service layer being created.
        //string dataSoureUrl = @"https://landsat2.arcgis.com/arcgis/services/Landsat/MS/ImageServer";
        string dataSourceUrl = @"http://sampleserver6.arcgisonline.com/arcgis/rest/services/CharlotteLAS/ImageServer";
        // Note: A url can also point to  
        // 1) An image on disk or in a file geodatabase, e.g., string dataSoureUrl = @"C:\temp\a.tif"; This results in a raster layer.
        // 2) A mosaic dataset in a file gdb, e.g., string dataSoureUrl = @"c:\temp\mygdb.gdb\MyMosaicDataset"; This results in a mosaic layer.
        // 3) A raster or mosaic dataset in an enterprise geodatabase.

        // Creates an ImageServiceLayer object to hold the new layer.
        ImageServiceLayer imageServiceLayer = null;

        // The layer has to be created on the Main CIM Thread (MCT).
        await QueuedTask.Run(() =>
        {
          // Creates a layer based on the url. In this case the layer we are creating is an image service layer.
          imageServiceLayer = (ImageServiceLayer)LayerFactory.Instance.CreateLayer(new Uri(dataSourceUrl), _map);
          if (imageServiceLayer == null)
          {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Failed to create layer for url:" + dataSourceUrl);
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
