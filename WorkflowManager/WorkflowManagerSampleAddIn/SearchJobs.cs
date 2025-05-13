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
    internal class SearchJobs : Button
    {
        protected override void OnClick()
        {
            // Get an instance of the JobsManager
            var jobsManager = WorkflowClientModule.JobsManager;

            QueuedTask.Run(() =>
            {
                try
                {
                    // Search for open jobs and order results by jobName and priority
                    var searchResult = jobsManager.SearchJobs(
                        new ArcGIS.Desktop.Workflow.Client.Models.SearchQuery()
                        {
                            Fields = ["jobId", "jobName", "dueDate", "priority", "assignedTo"],
                            Q = "closed=0",
                            SortFields =
                            [
                                new ArcGIS.Desktop.Workflow.Client.Models.SortField()
                                {
                                    FieldName = "jobName",
                                    SortOrder = ArcGIS.Desktop.Workflow.Client.Models.SortOrder.Asc
                                },
                                new ArcGIS.Desktop.Workflow.Client.Models.SortField()
                                {
                                    FieldName = "priority",
                                    SortOrder = ArcGIS.Desktop.Workflow.Client.Models.SortOrder.Desc
                                }
                            ]
                        });
                    var title = "Job information and properties";
                    var nl = System.Environment.NewLine;
                    var msg = $"Search Results are modeled as:{nl} {searchResult}{nl} For example, searchResult.Num: {searchResult.Num}{nl}";
                    string jobId = null;
                    if (searchResult.Results.Count > 0)
                    {
                        // Find the jobId from the first record returned
                        jobId = searchResult.Results[0]["jobId"].ToString();
                        msg += $@"{nl}Also found the first jobId: {jobId}";
                        msg += $@"{nl}a.k.a.: {searchResult.Results[0]["jobName"].ToString()}";
                    }
                    // Save the jobId for use in other examples
                    Module1.Current.JobIdFromSearch = jobId;

                    MessageBox.Show(msg, title);
                }
                catch (Exception ex)
                {
                    var title = "Failed to get the job information";
                    var msg = $"\nError: {ex.Message}";
                    MessageBox.Show(msg, title);
                }
            });
        }
    }
}
