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
using ArcGIS.Desktop.Workflow.Client.Models;
using ArcGIS.Desktop.Workflow.Client.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace WorkflowManagerSampleAddIn
{
    internal class SendStepResponseButton : Button
    {
        protected override void OnClick()
        {
            // Get an instance of the NotificationManager
            var notifManager = WorkflowClientModule.NotificationManager;

            // Use the jobId of the first job found in the search or provide a jobId
            var jobId = Module1.Current.JobIdFromSearch;

            CurrentStep currentStep = null;
            QueuedTask.Run(() =>
            {
                try
                {
                    currentStep = StepHelper.GetCurrentStep(jobId);
                    if (currentStep != null)
                    {
                        // Send a step response to Workflow Manager Server with additional information required for the step to continue.
                        // In this case, provide an answer respond to a Question step.
                        var stepInfoResponseMessage = new QuestionStepInfoResponseMessage()
                        {
                            JobId = jobId,
                            StepId = currentStep.StepId,
                            QuestionResponse = 1,
                            Comment = "Selected question response option 1"
                        };
                        var stepResponse = new StepResponse()
                        {
                            Message = stepInfoResponseMessage
                        };

                        notifManager.SendStepResponse(stepResponse);
                        var title = "Sent Step Response to Server";
                        var msg = $"JobId: {jobId}\nStepId: {currentStep.StepId}";
                        MessageBox.Show(msg, title);
                    }
                }
                catch (Exception ex)
                {
                    var title = "Failed to Send Step Response to Server";
                    var msg = $"JobId: {jobId}\nStepId: {currentStep?.StepId}\nError: {ex.Message}";
                    MessageBox.Show(msg, title);
                }
            });

        }
    }
}
