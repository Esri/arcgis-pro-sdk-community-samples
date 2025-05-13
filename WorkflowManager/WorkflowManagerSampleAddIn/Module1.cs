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
using ArcGIS.Desktop.Workflow.Client.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WorkflowManagerSampleAddIn
{
    /// <summary>
    /// This ArcGIS Pro Add-In sample utilizes the Workflow Manager Pro SDK to perform various operations against ArcGIS Workflow Manager items.
    /// </summary>
    /// <remarks>
    /// 1. Prerequisite: This sample requires [ArcGIS Workflow Manager](https://pro.arcgis.com/en/pro-app/latest/help/workflow/an-introduction-to-arcgis-workflow-manager.htm) and a connection to a workflow item.  For details see [Manage workflow item connections](https://pro.arcgis.com/en/pro-app/latest/help/workflow/manage-workflow-item-connections.htm).
    /// 1. Prerequisite: Using Workflow Manager make sure you have at least one job created in your workflow item.  For details see [Create jobs](https://pro.arcgis.com/en/pro-app/latest/help/workflow/create-jobs.htm).
    /// 1. In Visual Studio 2022 debug this add-in.
    /// 1. After ArcGIS Pro opens, open Pro project that contains a Workflow Manager connection or add a Workflow Manager connection to the  project.
    /// 1. Open the 'Workflow' dockpane if not already open using the 'View' tab and click on 'Workflow Manager'.
    /// 1. You should see your jobs on the 'Workflow' dockpane.
    /// 1. Feel free to engage with the Add-In which makes use of the first job in the search list. Some functionality is specific to certain Workflow Manager steps and will need to be configured specifically. Please refer to each example for more information.
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("WorkflowManagerSampleAddIn_Module");

        /// <summary>
        /// The jobId of the running Open Pro Project Items step. Use the jobId to get job properties or do additional processing of the job.
        /// 
        /// For details see [Add and configure Open Pro Project Items](https://doc.arcgis.com/en/workflow-manager/latest/help/open-pro-project-items.htm)
        /// </summary>
        internal string JobIdFromOpenProProjectStep { get; private set; }

        /// <summary>
        /// The jobId of the first job found in a search. This job will be used in various examples.
        /// </summary>
        internal string JobIdFromSearch { get; set; }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        /// <summary>
        /// Override this method to allow execution of DAML commands specified in this module.
        /// This is needed to run commands using the Open Pro Project Items step.
        /// </summary>
        /// <param name="id">The DAML control identifier.</param>
        /// <returns>A user defined function that will execute asynchronously when invoked.</returns>
        protected override Func<Task> ExecuteCommand(string id)
        {
            return () => QueuedTask.Run(() =>
            {
                try
                {
                    // Run the command specified by the id
                    IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper(id);
                    ICommand command = wrapper as ICommand;
                    if ((command != null) && command.CanExecute(null))
                        command.Execute(null);
                }
                catch (Exception e)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"ERROR: {e}", "Error running command");
                }
            });
        }

        /// <summary>
        /// Override this method to allow execution of DAML commands specified in this module.
        /// This is needed to run commands using the Open Pro Project Items step with arguments.
        /// </summary>
        /// <param name="id">The DAML control identifier.</param>
        /// <returns>A user defined function, with arguments, that will execute asynchronously when invoked.</returns>
        protected override Func<Object[], Task> ExecuteCommandArgs(string id)
        {
            return (object[] args) => RunCommand(id, args);
        }

        private Task RunCommand(string id, object[] args)
        {
            return QueuedTask.Run(() =>
            {
                try
                {
                    // Get the jobId property from the OpenProProjectItemsStep arguments and store it.
                    OpenProProjectItemsStepCommandArgs stepArgs = (OpenProProjectItemsStepCommandArgs)args[0];
								    JobIdFromOpenProProjectStep = stepArgs.JobId;
                    // ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"Got job id from ProMappingStep args: {JobId_OpenProProjectStep}", "Project Info");

                    // Run the command specified by the id
                    IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper(id);
                    var command = wrapper as ICommand;
                    if (command != null && command.CanExecute(null))
                        command.Execute(null);
                }
                catch (System.Exception e)
                {
                    // ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"ERROR: {e}", "Error running command");
                }
            });
        }

        #endregion Overrides

    }
}
