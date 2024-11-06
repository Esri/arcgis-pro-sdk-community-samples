//
// Copyright 2024 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;

namespace KnowledgeGraphConstructionTools
{
  internal class CreateEntity : MapTool
  {
    public CreateEntity()
    {
      IsSketchTool = true;
      UseSnapping = true;
      // Select the type of construction tool you wish to implement.  
      // Make sure that the tool is correctly registered with the correct component category type in the daml 
      SketchType = SketchGeometryType.Point;
      // SketchType = SketchGeometryType.Line;
      // SketchType = SketchGeometryType.Polygon;
      //Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
      UsesCurrentTemplate = true;
      //Gets or sets whether the tool supports firing sketch events when the map sketch changes. 
      //Default value is false.
      FireSketchEvents = true;
    }

    /// <summary>
    /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch.</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (CurrentTemplate == null || geometry == null)
        return Task.FromResult(false);

      // Create an edit operation
      var createOperation = new EditOperation();
      createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
      createOperation.SelectNewFeatures = true;

      // Queue feature creation
      createOperation.Create(CurrentTemplate, geometry);

      // Execute the operation
      return createOperation.ExecuteAsync();
    }
  }
}
