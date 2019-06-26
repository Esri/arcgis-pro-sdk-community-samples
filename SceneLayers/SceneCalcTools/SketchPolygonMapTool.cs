/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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
using ArcGIS.Desktop.Mapping;

namespace SceneCalcTools
{
    internal class SketchPolygonMapTool : MapTool
    {
        public SketchPolygonMapTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Lasso;
        }

        #region Overrides

        protected override Task OnToolActivateAsync(bool hasMapViewChanged)
        {
			Module1.SceneCalcVM.SketchToolBackground = new SolidColorBrush(Color.FromRgb(185, 209, 234));
            return base.OnToolActivateAsync(hasMapViewChanged);
        }

        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
        {
			Module1.SceneCalcVM.SketchToolBackground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            return base.OnToolDeactivateAsync(hasMapViewChanged);
        }

        protected async override Task<bool> OnSketchCompleteAsync(ArcGIS.Core.Geometry.Geometry geometry)
        {
            await QueuedTask.Run(async () =>
            {
                var polygonTemplate = MapView.Active.Map.FindLayers("Clip_Polygon_Asphalt").FirstOrDefault().GetTemplate("Clip_Polygon_Asphalt");
                // Create an edit operation
                var createOperation = new EditOperation();
                createOperation.Name = string.Format("Create {0}", "Volume Polygon");
                createOperation.SelectNewFeatures = true;
                createOperation.Create(polygonTemplate, geometry);
                // Execute the operation
                await createOperation.ExecuteAsync();
            });
            return true;
        }

        #endregion

    }
}
