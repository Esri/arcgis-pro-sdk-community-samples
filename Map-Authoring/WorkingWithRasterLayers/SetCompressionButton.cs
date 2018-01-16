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

namespace WorkingWithRasterLayers
{
  internal class SetCompressionButton : Button
  {
    /// <summary>
    /// Constructor. Make sure button is enabled if raster layer is selected and subscribe 
    /// to the layer selection changed event.
    /// </summary>
    public SetCompressionButton()
    {
      if (MapView.Active != null)
      {
        SelectedLayersChanged(new ArcGIS.Desktop.Mapping.Events.MapViewEventArgs(MapView.Active));
      }
      ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Subscribe(SelectedLayersChanged);
    }

    /// <summary>
    /// Destructor. Unsubscribe from the layer selection changed event.
    /// </summary>
    ~SetCompressionButton()
    {
      ArcGIS.Desktop.Mapping.Events.TOCSelectionChangedEvent.Unsubscribe(SelectedLayersChanged);
    }

    /// <summary>
    /// Event handler for layer selection changes.
    /// </summary>
    private void SelectedLayersChanged(ArcGIS.Desktop.Mapping.Events.MapViewEventArgs mapViewArgs)
    {
      State state = (FrameworkApplication.Panes.ActivePane != null) ? FrameworkApplication.Panes.ActivePane.State : null;
      if (state != null)
      {
        IReadOnlyList<Layer> selectedLayers = mapViewArgs.MapView.GetSelectedLayers();
        if (selectedLayers.Count == 1)
          state.Activate("esri_custom_mutipleLayersNotSelectedState");
        else
        {
          state.Deactivate("esri_custom_mutipleLayersNotSelectedState");
          return;
        }

        Layer firstSelectedLayer = selectedLayers.First();
        if (firstSelectedLayer != null)
        {
          if (firstSelectedLayer is ImageServiceLayer && !(firstSelectedLayer is ImageMosaicSubLayer))
            state.Activate("esri_custom_imageServiceLayerSelectedState");
          else
            state.Deactivate("esri_custom_imageServiceLayerSelectedState");
        }
        else
          state.Deactivate("esri_custom_imageServiceLayerSelectedState");
      }
    }

    protected override async void OnClick()
    {
      await RasterLayersVM.SetCompressionAsync("JPEG", 85);
    }
  }
}
