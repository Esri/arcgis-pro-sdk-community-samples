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
using ArcGIS.Core.Data.UtilityNetwork.Trace;
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

namespace TINApiSamples.TinRenderers
{
  internal class TinLayerRendererGallery : Gallery
  {
    private bool _isInitialized;
    public TinLayerRendererGallery()
    {
      this.AlwaysFireOnClick= true;
    }

    protected override void OnDropDownOpened()
    {
      var map = MapView.Active?.Map;
      if (map == null)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("A map with a TIN  layer is required to use the controls in this gallery");
        _isInitialized = true;
        return;
      }

      var tinLayer = map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      if (tinLayer == null)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("A map with a TIN layer is required to use the controls in this gallery");
        _isInitialized = true;
        return;
      }
      Initialize();
    }

    private void Initialize()
    {
      if (_isInitialized)
        return;

      foreach (var kvp in _tinLayerGroupMappingButtonIDs)
      {
        try
        {
          //check we get a plugin

          var plugin = ArcGIS.Desktop.Framework.FrameworkApplication.GetPlugInWrapper(kvp.Key);
          if (plugin != null)
          {

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

    protected override void OnClick(Object item)
    {
      var rendererItem = item as RendererButtonGalleryItem;
      rendererItem.Execute();
    }
    private IDictionary<string, string> _tinLayerGroupMappingButtonIDs = new Dictionary<string, string> {
      { "TINApiSamples_Points_Simple", "Symbolize your layer using Points" },
      { "TINApiSamples_Points_Elevation", "Symbolize your layer using Points" },
      {"TINApiSamples_Lines_Contour", "Symbolize your layer using lines" },
      {"TINApiSamples_Lines_SimpleEdge", "Symbolize your layer using lines" },
      { "TINApiSamples_Lines_EdgeType", "Symbolize your layer using lines"},
      { "TINApiSamples_Surface_Simple", "Symbolize your layer using Surface" },
      { "TINApiSamples_Surface_Elevation", "Symbolize your layer using Surface" },
      {"TINApiSamples_Surface_Slope", "Symbolize your layer using Surface" },
      {"TINApiSamples_Surface_Aspect", "Symbolize your layer using Surface" }
    };
  }
}
