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

using System.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;

namespace AnnoTools
{
  /// <summary>
  /// An annotation construction tool that uses a point sketch type.  
  /// </summary>
  /// <remarks>
  /// Annotation construction tools work as per other construction tools.  Set the categoryRefID in the daml file to be "esri_editing_construction_annotation".
  /// <para></para>
  /// Annotation feature classes store polygon geometry.  This polygon is the bounding box of the text of an annotation feature. The bounding box 
  /// is calculated from the text string, font, font size, angle orientation and other text formatting attributes of the feature. It is automatically 
  /// updated by the application each time the annotation attributes are modified. You should never need to access or modify an annotation features 
  /// polygon shape.  
  /// <para></para>
  /// The text attributes of an annotation feature are represented by a CIMTextGraphic. The CIMTextGraphic consists of the text string, text attributes 
  /// (such as verticalAlignment, horizontalAlignment, fontFamily, fontSize etc), callouts, leader lines and the CIMTextGraphic geometry. This geometry 
  /// can be a point, straight line, bezier curve or multipoint geometry and represents the baseline geometry that the text string sits upon. 
  /// <para></para>
  /// When creating an annotation feature the geometry passed to the Create method is the cimTextGraphic geometry. 
  /// </remarks>
  internal class AnnoSimpleConstructionTool : MapTool
  {
    public AnnoSimpleConstructionTool()
    {
      IsSketchTool = true;
      UseSnapping = true;
      // set the sketch type to point
      SketchType = SketchGeometryType.Point;
    }

    /// <summary>
    /// Called when the sketch finishes. This is where we will create the edit operation and then execute it.
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

      // pass the point geometry to the Create method
      createOperation.Create(CurrentTemplate, geometry);

      // Execute the operation
      return createOperation.ExecuteAsync();
    }
  }
}
