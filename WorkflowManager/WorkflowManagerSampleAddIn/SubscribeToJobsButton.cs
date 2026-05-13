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
using ArcGIS.Core.Events;
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
using ArcGIS.Desktop.Workflow.Client.Events;
using ArcGIS.Desktop.Workflow.Client.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WorkflowManagerSampleAddIn
{
    internal class SubscribeToJobsButton : Button
    {
        protected override void OnClick()
        {
            // Get an instance of the NotificationManager
            var notifManager = WorkflowClientModule.NotificationManager;
            var jobsManager = WorkflowClientModule.JobsManager;

            // Use the jobId of the first job found in the search or provide a jobId
            var jobIds = new List<string>() { Module1.Current.JobIdFromSearch };
            string jobStr = string.Join(",", jobIds);

            QueuedTask.Run(() =>
            {
                try
                {
                    FrameworkApplication.AddNotification(new Notification()
                    {
                        Title = "Subscribing to Jobs",
                        Message = $"JobIds: {jobStr} ..."
                    });

                    // Subscribe to the Job Message event to get job and step notifications
                    JobMessageEvent.Subscribe(OnJobMessageReceived);
                    // Subscribe to one or more jobs to initiate notifications for those jobs
                    notifManager.SubscribeToJobs(jobIds);
                }
                catch (Exception ex)
                {
                    FrameworkApplication.AddNotification(new Notification()
                    {
                        Title = "Failed Subscribing to Jobs",
                        Message = $"JobIds: {jobStr}\nError: {ex.Message}"
                    });
                }
            });
        }

        private void OnJobMessageReceived(JobMessageEventArgs<JobMessage> msg)
        {
            var jobId = msg.Message.JobId;
            var msgType = msg.MessageType;
            var message = msg.Message;

            // Add logic for filtering or handling of job notifications depending on the type of message received.
            FrameworkApplication.AddNotification(new Notification()
            {
                Title = "Notification Message Received",
                Message = $"Job ID: {jobId}\nMessage Type: {msgType}\nMessage: {message}",
            });
        }
    }
}
