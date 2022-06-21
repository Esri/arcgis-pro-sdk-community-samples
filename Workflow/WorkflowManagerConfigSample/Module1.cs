//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace WorkflowManagerConfigSample
{
	/// <summary>
	/// This sample allows users to only interact with the Workflow Pane in ArcGIS Pro, and hides the Workflow View, the Job view and the Workflow ribbon on map from the user. This configuration allows organizations to deploy a slimmed down version of Workflow Manager for ArcGIS Pro that focusses on work completion. Users see only their jobs or jobs in their groups, and is for users who don’t have the requirement to edit job properties, search for jobs and see a workflow diagram. This creates a simpler UI 
	/// </summary>
	/// <remarks>
	/// In order to use this sample you must have a Workflow manager database set up and accessible  
	/// Please refer to setting up a Workflow manager database before using this sample 
	/// Configurations are built using the configuration project template provided in the Pro SDK templates in Visual Studio.  Once selected, the configuration template creates the necessary project components which developers can use. Please refer to ArcGIS Pro SDK Configurations for more information 
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.  
	/// 1. Click Start button to open ArcGIS Pro. You can also install the configuration  'WorkflowManagerConfigSample.proconfigx' and then deploy the sample using the following command line: C:[file location]\ArcGISPro.exe /config:[name of config] 
	/// 1. The ArcGIS Pro Project must have a valid Workflow Manager database connection prior to using 
	/// 1. To establish a valid Workflow Manager database connection just use the ‘Add Workflow Connection’ option under the connections menu on the project tab. Save the project and reopen it again to view all open jobs assigned to the current user or their groups. Users can then execute and finish jobs using only the workflow pane. 
	/// </remarks>
	internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("WorkflowManagerConfigSample_Module"));
            }
        }

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

        #endregion Overrides

    }
}