/*

   Copyright 2022 Esri

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

namespace TableConstructionTool
{
  class BasicTableConstTool : MapTool
  {
    public BasicTableConstTool()
    {
      IsSketchTool = false;
      // set the SketchType to None
      SketchType = SketchGeometryType.None;
      //Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
      UsesCurrentTemplate = true;
    }

    /// <summary>
    /// Called when the "Create" button is clicked. This is where we will create the edit operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch - will be null because SketchType = SketchGeometryType.None</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (CurrentTemplate == null)
        return Task.FromResult(false);

      // geometry will be null

      // Create an edit operation
      var createOperation = new EditOperation();
      createOperation.Name = string.Format("Create {0}", CurrentTemplate.StandaloneTable.Name);
      createOperation.SelectNewFeatures = false;

      // determine the number of rows to add
      var numRows = this.CurrentTemplateRows;
      for (int idx = 0; idx < numRows; idx++)
        // Queue feature creation
        createOperation.Create(CurrentTemplate, null);

      // Execute the operation
      var returnVal =  createOperation.ExecuteAsync();
      ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"{numRows} records added to {CurrentTemplate.StandaloneTable.Name} table.");
      return returnVal;
    }
  }
}
