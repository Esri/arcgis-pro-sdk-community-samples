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
using ArcGIS.Desktop.Workflow.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowManagerSampleAddIn
{
    internal class ActiveMapJobIdButton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                var title = "Active Map View - Job Id";
                try
                {
                    // Get the jobId of the job associated with the active map view.
                    // A map will have an association with a job if it is opened by Workflow Manager using the Open Pro Project Items step.
                    // For details see [Add and configure Open Pro Project Items](https://doc.arcgis.com/en/workflow-manager/latest/help/open-pro-project-items.htm)
                    var jobManager = WorkflowClientModule.JobsManager;
                    var jobId = jobManager.GetJobId();
                    if (jobId != null)
                    {
                        var msg = $"JobId: {jobId}";
                        MessageBox.Show(msg, title);
                    }
                    else
                    {
                        var msg = "No jobId associated with active map view";
                        MessageBox.Show(msg, title);
                    }
                }
                catch (Exception ex)
                {
                    var msg = $"Error retrieving jobId from active map view: {ex.Message}";
                    MessageBox.Show(msg, title);
                }
            });
        }
    }
}
