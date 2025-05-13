/*

   Copyright 2025 Esri

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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstructionToolWithOptions
{
    class UIActivator : MapTool
    {
        public UIActivator()
        {
            IsSketchTool = true;
            UseSnapping = true;
            SketchType = SketchGeometryType.Polygon;
            UsesCurrentTemplate = true;
        }

        #region Tool Options
        private ReadOnlyToolOptions ToolOptions => CurrentTemplate?.GetToolOptions(ID);


    #endregion
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
