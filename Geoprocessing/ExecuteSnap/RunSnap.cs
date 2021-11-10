using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ExecuteSnap
{
    internal class RunSnap : Button
    {
        // Snap Environment
        // Snap a line to polygon EDGE, END of another line and to points VERTEX
        //
        protected override async void OnClick()
        {
            GPExecuteToolFlags executeFlags = GPExecuteToolFlags.AddOutputsToMap | GPExecuteToolFlags.GPThread | GPExecuteToolFlags.AddToHistory;

            string toolName = "edit.Snap";

            var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);

            // make a copy of the input data - this data will be snapped to data specified in snap environment
            var inputFeatures = @"C:\\data\Snapping.gdb\otherline";

            // snapEnvironment parameter is of ValueTable type
            var snapEnvironments = @"C:\\data\Snapping.gdb\poly EDGE '15 Meters';C:\\data\Snapping.gdb\line END '15 Meters';C:\\data\Snapping.gdb\points VERTEX '18 Meters'";

            var parameters2 = Geoprocessing.MakeValueArray(inputFeatures, snapEnvironments);

            var gpResult = await Geoprocessing.ExecuteToolAsync(toolName, parameters2, environments, null, null, executeFlags);

            Geoprocessing.ShowMessageBox(gpResult.Messages, "GP Messages", gpResult.IsFailed ? GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
        }
    }
}
