/*

   Copyright 2026 Esri

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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Workflow.Client.Models;
using ArcGIS.Desktop.Workflow.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Dialogs;

namespace WorkflowManagerSampleAddIn
{
    internal class StepHelper
    {
        public static CurrentStep GetCurrentStep(string jobId)
        {
            // Get the first current stepId of the job
            var job = WorkflowClientModule.JobsManager.GetJob(jobId);
            var steps = job.CurrentSteps;
            var currentStep = steps[0];
            var currentStepId = currentStep.StepId;

            var title = "Current Step Id";
            var msg = $"Got current step {currentStep.StepName} with id {currentStep.StepId} for job: {jobId}";
            MessageBox.Show(msg, title);

            return currentStep;
        }
    }
}
