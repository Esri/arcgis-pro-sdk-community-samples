// Copyright 2017 Esri

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
using ArcGIS.Core.Data.Raster;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;

namespace RasterInspector
{
  internal class RasterInspectorTool : MapTool
  {
    private Raster _selectedRaster = null;
    private int _bandindex = -1;

    public RasterInspectorTool()
    {
      IsSketchTool = false;
    }

    private static bool bIsMouseMoveActive = false;

    protected override void OnToolMouseMove(MapViewMouseEventArgs e)
    {
      if (_selectedRaster == null)
        return;
      // bIsMouseMoveActive is preventing the creation of too many threads  via QueuedTask.Run
      // especially if imagery is via a remote service
      if (bIsMouseMoveActive) return;
      bIsMouseMoveActive = true;
      System.Diagnostics.Debug.WriteLine($@"{DateTime.Now} OnToolMouseMove");
      QueuedTask.Run(() =>
      {
        try
        {
          // create a pixelblock representing a 3x3 window to hold the raster values
          var pixelBlock = _selectedRaster.CreatePixelBlock(3, 3);

          // determine the cursor position in mapping coordinates
          var clientCoords = new System.Windows.Point(e.ClientPoint.X, e.ClientPoint.Y);
          if (clientCoords == null || ActiveMapView == null) return;
          var mapPointAtCursor = ActiveMapView.ClientToMap(clientCoords);
          if (mapPointAtCursor == null) return;

          // create a container to hold the pixel values
          Array pixelArray = new object[pixelBlock.GetWidth(), pixelBlock.GetHeight()];

          // reproject the raster envelope to match the map spatial reference
          var rasterEnvelope = GeometryEngine.Instance.Project(_selectedRaster.GetExtent(), mapPointAtCursor.SpatialReference);

          // if the cursor is within the extent of the raster
          if (GeometryEngine.Instance.Contains(rasterEnvelope, mapPointAtCursor))
          {
            // find the map location expressed in row,column of the raster
            var pixelLocationAtRaster = _selectedRaster.MapToPixel(mapPointAtCursor.X, mapPointAtCursor.Y);

            // fill the pixelblock with the pointer location
            _selectedRaster.Read(pixelLocationAtRaster.Item1, pixelLocationAtRaster.Item2, pixelBlock);

            if (_bandindex != -1)
            {
              // retrieve the actual pixel values from the pixelblock representing the red raster band
              pixelArray = pixelBlock.GetPixelData(_bandindex, false);
            }
          }
          else
          {
            // fill the container with 0s
            Array.Clear(pixelArray, 0, pixelArray.Length);
          }

          // pass the pass the raster values to the view model
          RasterValuesPaneViewModel.Current.RasterValues = ConvertArray(pixelArray);
        }
        finally
        {
          bIsMouseMoveActive = false;
        }
      });


    }

    /// <summary>
    /// Convert the array of raster values into an array of objects
    /// </summary>
    /// <param name="arr">Array of raster values.</param>
    /// <returns>2D array of raster values cast into object types.</returns>
    private object[,] ConvertArray(Array arr)
    {
      object[,] target = new object[arr.GetLength(0), arr.GetLength(1)];

      for (int rowIndex = 0; rowIndex < arr.GetLength(1); rowIndex++)
      {
        for (int columnIndex = 0; columnIndex < arr.GetLength(0); columnIndex++)
        {
          target[columnIndex, rowIndex] = (object)arr.GetValue(columnIndex, rowIndex);
        }
      }

      return target;
    }

    /// <summary>
    /// Happens if the tool is activated.
    /// </summary>
    /// <param name="active"></param>
    /// <returns></returns>
    protected override Task OnToolActivateAsync(bool active)
    {
      // get the first selected raster layer
      var selectedRasterLayer = ActiveMapView.GetSelectedLayers().OfType<BasicRasterLayer>().FirstOrDefault();

      QueuedTask.Run(() =>
      {
              // get the raster from layer
              _selectedRaster = selectedRasterLayer?.GetRaster();


        if (selectedRasterLayer?.GetColorizer() is CIMRasterRGBColorizer)
        {
                // if the rgb renderer is used get the index of the band used to render the reb color component
                var rgbColorizer = selectedRasterLayer?.GetColorizer() as CIMRasterRGBColorizer;
          _bandindex = rgbColorizer.RedBandIndex;
        }
        else if (selectedRasterLayer?.GetColorizer() is CIMRasterStretchColorizer)
        {
                // if the stretch renderer is used get the selected band index
                var stretchColorizer = selectedRasterLayer?.GetColorizer() as CIMRasterStretchColorizer;
          _bandindex = stretchColorizer.BandIndex;
        }
      });


      RasterValuesPaneViewModel.Show();

      return base.OnToolActivateAsync(active);
    }

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      return base.OnSketchCompleteAsync(geometry);
    }
  }
}
