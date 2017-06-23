// Copyright 2017 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       http://www.apache.org/licenses/LICENSE-2.0 
//
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
using System.Windows;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;

namespace ConstructionToolWithOptions
{
  internal class BufferedLineTool : MapTool
  {
    public BufferedLineTool()
    {
      IsSketchTool = true;
      UseSnapping = true;
      // Select the type of construction tool you wish to implement.  
      // Make sure that the tool is correctly registered with the correct component category type in the daml 
      SketchType = SketchGeometryType.Line;
    }

    #region Tool Options
    private ReadOnlyToolOptions ToolOptions => CurrentTemplate?.GetToolOptions(ID);

    private double BufferDistance
    {
      get
      {
        if (ToolOptions == null)
          return BufferedLineToolOptionsViewModel.DefaultBuffer;

        return ToolOptions.GetProperty(BufferedLineToolOptionsViewModel.BufferOptionName, BufferedLineToolOptionsViewModel.DefaultBuffer);
      }
    }
    #endregion

    /// <summary>
    /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch.</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (CurrentTemplate == null || geometry == null)
        return Task.FromResult(false);

      // create the buffered geometry
      Geometry bufferedGeometry = GeometryEngine.Instance.Buffer(geometry, BufferDistance);

      // Create an edit operation
      var createOperation = new EditOperation()
      {
        Name = string.Format("Create {0}", CurrentTemplate.Layer.Name),
        SelectNewFeatures = true
      };

      // Queue feature creation
      createOperation.Create(CurrentTemplate, bufferedGeometry);

      // Execute the operation
      return createOperation.ExecuteAsync();
    }
  }
}
