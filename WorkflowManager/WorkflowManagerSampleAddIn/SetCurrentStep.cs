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
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Workflow.Client;
using System;

namespace WorkflowManagerSampleAddIn
{
    internal class SetCurrentStep : Button
    {
        protected override void OnClick()
        {
            // Get the Workflow Manager server url
            var jobsManager = WorkflowClientModule.JobsManager;

            // Use the jobId of the first job found in the search
            var jobId = Module1.Current.JobIdFromSearch;

            // Use a different step other than the current steps
            string otherStepId = "dea81717-cf6a-4965-bb47-3388a2df3632";

            QueuedTask.Run(() =>
            {
                try
                {
                    jobsManager.SetCurrentStep(jobId, otherStepId);
                    var title = "Updated the Current step to a different step.";
                    var msg = $"\nJobId: {jobId}\nNew Current StepId: {otherStepId}";
                    MessageBox.Show(msg, title);
                }
                catch (Exception ex)
                {
                    var title = "Failed to set the current step to the first step";
                    var msg = $"\nJobId: {jobId}"
                                        + $"\nError: {ex.Message}";
                    MessageBox.Show(msg, title);
                }
            });
        }
    }
}
