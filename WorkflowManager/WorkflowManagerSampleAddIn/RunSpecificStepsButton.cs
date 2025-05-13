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
    internal class RunSpecificStepsButton : Button
    {
        protected override void OnClick()
        {
            // Get the Workflow Manager server url
            var jobsManager = WorkflowClientModule.JobsManager;

            // Use the jobId of the first job found in the search or provide a jobId
            var jobId = Module1.Current.JobIdFromSearch;
            var stepIds = new List<string>();

            QueuedTask.Run(() =>
            {
                try
                {
                    // Get current steps on the job
                    var job = jobsManager.GetJob(jobId);
                    var currentSteps = job.CurrentSteps;
                    if (currentSteps == null || currentSteps.Count < 1)
                    {
                        var title = "Failed Retrieving Current Step(s) on a Job";
                        var msg = $"\nJobId: {jobId}\n";
                        MessageBox.Show(msg, title);
                        return;
                    }

                    stepIds.Add(currentSteps[0].StepId); // Set one or more current steps
                }
                catch (Exception ex)
                {
                    var title = "Failed Retrieving Current Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}\n"
                        + $"\nError: {ex.Message}";
                    MessageBox.Show(msg, title);
                    return;
                }

                var stepIdStr = string.Join(",", stepIds);
                try
                {
                    // Run specific current steps on the job
                    jobsManager.RunSteps(jobId, stepIds);
                    var title = "Running Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}\nStepId(s): {stepIdStr}";
                    MessageBox.Show(msg, title);
                }
                catch (Exception ex)
                {
                    var title = "Failed Running Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}\nStepId(s): {stepIdStr}"
                        + $"\nError: {ex.Message}";
                    MessageBox.Show(msg, title);
                }

                try
                {
                    // Stop specific current steps on the job
                    jobsManager.StopSteps(jobId, stepIds);
                    var title = "Stopped Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}\nStepId(s): {stepIdStr}";
                    MessageBox.Show(msg, title);
                }
                catch (Exception ex)
                {
                    var title = "Failed Stopping Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}\nStepId(s): {stepIdStr}"
                        + $"\nError: {ex.Message}";
                    MessageBox.Show(msg, title);
                }

                try
                {
                    // Finish specific current steps on the job
                    jobsManager.FinishSteps(jobId, stepIds);
                    var title = "Finished Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}\nStepId(s): {stepIdStr}";
                    MessageBox.Show(msg, title);
                }
                catch (Exception ex)
                {
                    var title = "Failed Finishing Step(s) on a Job";
                    var msg = $"\nJobId: {jobId}\nStepId(s): {stepIdStr}"
                        + $"\nError: {ex.Message}";
                    MessageBox.Show(msg, title);
                }
            });
        }
    }
}
