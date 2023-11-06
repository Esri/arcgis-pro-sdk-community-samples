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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace LASDatasetAPISamples.LASRenderers
{
  /// <summary>
  /// Represents the gallery used to display the LAS layer renderer buttons
  /// </summary>
  internal class LasLayerRendererGallery : Gallery
  {
    private bool _isInitialized;

    public LasLayerRendererGallery()
    {
      this.AlwaysFireOnClick = true;
    }

    protected override void OnDropDownOpened()
    {
      var map = MapView.Active?.Map;
      if (map == null)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("A map with a LAS layer is required to use the controls in this gallery");
        _isInitialized = true;
        return;
      }

      var lasLayer = map.GetLayersAsFlattenedList().OfType<LasDatasetLayer>().FirstOrDefault();
      if (lasLayer == null)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("A map with a LAS layer is required to use the controls in this gallery");
        _isInitialized = true;
        return;
      }
      Initialize();
    }

    private void Initialize()
    {
      if (_isInitialized)
        return;

      foreach (var kvp in _lasLayerGroupMappingButtonIDs)
      {
        try
        {
          //check we get a plugin
          //This is where the magic happens. We get the plugin wrapper for the button DAML ID
          var plugin = ArcGIS.Desktop.Framework.FrameworkApplication.GetPlugInWrapper(kvp.Key);
          if (plugin != null)
          {
            //Populate the gallery with the buttons
            Add(new RendererButtonGalleryItem(kvp.Key, kvp.Value, plugin));
          }
        }
        catch (Exception e)
        {
          string x = e.Message;
        }
      }
      _isInitialized = true;

    }
    /// <summary>
    /// Triggered when a gallery button is clicked
    /// </summary>
    /// <param name="item"></param>
    protected override void OnClick(object item)
    {
      //Gallery item that is clicked is cast into the custom RendererButtonGalleryItem class
      var rendererButton= item as RendererButtonGalleryItem;
      //Invoke the Execute method on the RendererButtonGalleryItem
      rendererButton.Execute();
    }

    //Dictionary mapping the LAS layer group to the button DAML ID
    private IDictionary<string, string> _lasLayerGroupMappingButtonIDs = new Dictionary<string, string> {
      { "LASDatasetAPISamples_LASRenderers_Points_Elevation", "Symbolize your layer using Points" },
      { "LASDatasetAPISamples_LASRenderers_Points_Class", "Symbolize your layer using Points" },
      { "LASDatasetAPISamples_LASRenderers_points_Returns", "Symbolize your layer using Points"  },
      { "LASDatasetAPISamples_LASRenderers_surface_Elevation", "Symbolize your layer using Surface" },
      { "LASDatasetAPISamples_LASRenderers_surface_Aspect", "Symbolize your layer using Surface" },
      { "LASDatasetAPISamples_LASRenderers_surface_slope", "Symbolize your layer using Surface" },
      {"LASDatasetAPISamples_LASRenderers_lines_contour", "Symbolize your layer using lines" },
      {"LASDatasetAPISamples_LASRenderers_lines_edges", "Symbolize your layer using lines" }
    };
  }
}
