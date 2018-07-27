//Copyright 2018 Esri

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
using System.Windows;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
//using ArcGIS.Desktop.Editing.EditTools;
using ServiceContracts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ConstructionTool
{
    internal class SimpleSketchTool : MapTool
    {
        public SimpleSketchTool() : base()
        {
            // select the type of construction tool you wish to implement.  
            // Make sure that the tool is correctly registered with the correct component category type in the daml
            SketchType = SketchGeometryType.Point;
            // indicate that we need a sketch feedback 
            IsSketchTool = true;
            // and the sketch geometry should be in map coordinates
            SketchOutputMode = ArcGIS.Desktop.Mapping.SketchOutputMode.Map;
        }

        /// <summary>
        /// Fires once the user completes the onscreen sketch. In the context of a construction tool the sketch geometry 
        /// is used for the shape of the feature.
        /// </summary>
        /// <param name="geometry">The sketch geometry the user digitized on the screen.</param>
        /// <returns></returns>
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            if (geometry == null)
                return Task.FromResult(false);

            return QueuedTask.Run(() =>
                {
                    // create an edit operation
                    var editOperation = new EditOperation();
                    editOperation.Name = string.Format("Create point in '{0}'", CurrentTemplate.Layer.Name);
                    editOperation.ProgressMessage = "Working...";
                    editOperation.CancelMessage = "Operation canceled";
                    editOperation.ErrorMessage = "Error creating point";

                    // queue the edit operation using the sketch geometry as the shape of the feature and any attribute 
                    // configurations from the editing template
                    editOperation.Create(CurrentTemplate, geometry);                    

                    //execute the operation
                    return editOperation.ExecuteAsync();
                });
        }
    }
}
