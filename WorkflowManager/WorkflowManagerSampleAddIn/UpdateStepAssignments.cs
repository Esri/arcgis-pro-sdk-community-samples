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
	internal class UpdateStepAssignments : Button
	{
		protected override void OnClick()
		{
			// Get the Workflow Manager server url
			var jobsManager = WorkflowClientModule.JobsManager;

            // Use the jobId of the first job found in the search or provide a jobId
            var jobId = Module1.Current.JobIdFromSearch;
            string stepId = null;

			QueuedTask.Run(() =>
			{
				try
				{
					// Get the current steps on the job
					var job = jobsManager.GetJob(jobId);
					var currentSteps = job.CurrentSteps;
					if (currentSteps == null || currentSteps.Count < 1)
					{
						var title = "Failed Retrieving Current Step(s) on a Job";
						var msg = $"\nJobId: {jobId}\n";
						MessageBox.Show(msg, title);
						return;
					}

					stepId = currentSteps[0].StepId;
				}
				catch (Exception ex)
				{
					var title = "Failed Retrieving Current Step(s) on a Job";
					var msg = $"\nJobId: {jobId}\n"
										+ $"\nError: {ex.Message}";
					MessageBox.Show(msg, title);
					return;
				}

				var assignedToUser = "admin";
				try
				{
					// Update the assignment of the current step on a job to a different user or group, or unassign the step.
					// This assumes there is only a single current step.
					jobsManager.AssignCurrentStep(jobId, ArcGIS.Desktop.Workflow.Client.Models.AssignedType.User, assignedToUser);
					var title = "Assigned the current step in the job to another user";
					var msg = $"\nJobId: {jobId}\nUser: {assignedToUser}";
					MessageBox.Show(msg, title);
				}
				catch (Exception ex)
				{
					var title = "Failed to assign the current step to another user";
					var msg = $"\nJobId: {jobId}"
										+ $"\nError: {ex.Message}";
					MessageBox.Show(msg, title);
				}

				assignedToUser = "testuser";
				try
				{
                    // Update the assignment of a specific current step on a job to a different user or group, or unassign the step.
                    // Use this to update assignment for a specific step in a parallel workflow where there could be multiple current steps.
                    jobsManager.AssignStep(jobId, stepId, ArcGIS.Desktop.Workflow.Client.Models.AssignedType.User, assignedToUser);
					var title = "Assigned a specific current step in the job to another user";
					var msg = $"\nJobId: {jobId}\nStepId: {stepId}\nUser: {assignedToUser}";
					MessageBox.Show(msg, title);
				}
				catch (Exception ex)
				{
					var title = "Failed to assign a step to another user";
					var msg = $"\nJobId: {jobId}\nStepId: {stepId}\nUser: {assignedToUser}"
										+ $"\nError: {ex.Message}";
					MessageBox.Show(msg, title);
				}
			});
		}
	}
}
