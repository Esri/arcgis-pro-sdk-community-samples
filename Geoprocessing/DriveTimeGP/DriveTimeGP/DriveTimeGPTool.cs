using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using System.IO;

namespace DriveTimeGP
{
    internal class DriveTimeGPTool : MapTool
    {

        public DriveTimeGPTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point; 
            SketchOutputMode = SketchOutputMode.Map;
        }

        /// <summary>
        /// Constructs the point and drive time distances to be passed as parameter to ExecuteToolAsync
        /// Runs the DriveTime Analysis 
        /// http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Network/ESRI_DriveTime_US/GPServer/CreateDriveTimePolygons
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns>Geoprocessing result object as a Task</returns>
        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {

            var result = await QueuedTask.Run(() =>
            {                                                                      
                string tool_path = string.Format(@"{0}\ArcGIS.ags\Network/ESRI_DriveTime_US\CreateDriveTimePolygons", Directory.GetCurrentDirectory());
                return Geoprocessing.ExecuteToolAsync(tool_path, Geoprocessing.MakeValueArray(geometry, "1 2 3"));       
                        
            });
         
            Geoprocessing.ShowMessageBox(result.Messages, "GP Messages", result.IsFailed ? GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);         
            return true;
        }

    
        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            base.OnToolMouseDown(e);
        }
    }
}
