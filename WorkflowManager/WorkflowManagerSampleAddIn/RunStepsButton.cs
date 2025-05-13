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
    internal class RunStepsButton : Button
    {
        protected override void OnClick()
        {
            // Get an instance of the JobsManager
            var jobsManager = WorkflowClientModule.JobsManager;

            // Use the jobId of the first job found in the search or provide a jobId
            var jobId = Module1.Current.JobIdFromSearch;

            QueuedTask.Run(() =>
            {
                try
                {
                    // Run current steps on the job
                    jobsManager.RunSteps(jobId);
                    var title = "Running Current Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}";
                    MessageBox.Show(msg, title);
                } catch (Exception ex)
                {
                    var title = "Failed Running Current Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}\nError: {ex.Message}";
                    MessageBox.Show(msg, title);
                }

                try
                {
                    // Stop current steps on the job
                    jobsManager.StopSteps(jobId);
                    var title = "Stopped Current Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}";
                    MessageBox.Show(msg, title);
                }
                catch (Exception ex)
                {
                    var title = "Failed Stopping Current Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}\nError: {ex.Message}";
                    MessageBox.Show(msg, title);
                }

                try
                {
                    // Finish current steps on the job
                    jobsManager.FinishSteps(jobId);
                    var title = "Finished Current Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}";
                    MessageBox.Show(msg, title);
                }
                catch (Exception ex)
                {
                    var title = "Failed Finishing Current Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}\nError: {ex.Message}";
                    MessageBox.Show(msg, title);
                }
            });

        }
    }
}
