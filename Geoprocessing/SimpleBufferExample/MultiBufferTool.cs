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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Editing;
using ArcGIS.Desktop.Internal.Mapping.Locate;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleBufferExample
{
  /// <summary>
  /// This tool can be used to digitize a polygon on a map and once complete
  /// the tool performs a series of buffer operations with expanding buffers
  /// During the buffer creation process the Progress Dialog is displayed
  /// </remarks>
  internal class MultiBufferTool : MapTool
  {
    /// <summary>
    /// Constructor of BufferGeometry tool
    /// </summary>
    public MultiBufferTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Polygon;
      SketchOutputMode = SketchOutputMode.Map;
    }

    /// <summary>
    /// Constructs the value array to be passed as parameter to ExecuteToolAsync
    /// Runs the Buffer tool of Analysis toolbox
    /// </summary>
    /// <param name="geometry"></param>
    /// <returns>Geoprocessing result object as a Task</returns>
    protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      uint numBuffers = 7;

      // create and initialize the progress dialog
      // Note: Progress dialogs are not displayed when debugging in Visual Studio
      var progDlg = new ProgressDialog($@"Creating {numBuffers} buffers", "Canceled", false);
      var progsrc = new CancelableProgressorSource(progDlg);
      for (uint iBuffer = 1; iBuffer <= numBuffers; iBuffer++)
      {
        var valueArray = await QueuedTask.Run<IReadOnlyList<string>>(() =>
        {
          var geometries = new List<object>() { geometry };
          // Creates a 100-meter buffer around the geometry object
          // null indicates a default output name is used
          var valueArray = Geoprocessing.MakeValueArray(geometries, null, $@"{iBuffer*100} Meters");
          return valueArray;
        });
        progsrc.ExtendedStatus = $@"Creating buffer #: {iBuffer} of {numBuffers}";
        progsrc.Value = 100 * (iBuffer-1);
        progsrc.Max = 100 * numBuffers + 1;
        var gpResult = await Geoprocessing.ExecuteToolAsync("analysis.Buffer", valueArray, null, progsrc.Progressor);
        if(gpResult.IsFailed)
        {
          // display error messages if the tool fails, otherwise shows the default messages
          if (gpResult.Messages.Count() != 0)
          {
            Geoprocessing.ShowMessageBox(gpResult.Messages, progsrc.Message,
                            gpResult.IsFailed ?
                            GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
          }
          else
          {
            MessageBox.Show($@"{progsrc.Message} failed with errorcode, check parameters.");
          }
          break;
        }
        // check if the operator canceled
        if (progsrc.CancellationTokenSource.IsCancellationRequested) break;
      }
      if (progsrc.CancellationTokenSource.IsCancellationRequested)
      {
        MessageBox.Show("The operation was canceled.");
      }
      return true;
    }
  }
}
