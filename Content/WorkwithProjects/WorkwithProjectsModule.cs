//   Copyright 2018 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace WorkwithProjects
{
    /// <summary>
    /// This sample illustrates working with ArcGIS Projects.  The sample provides the following functionality
    /// 1. Open an existing project.
    /// 1. Opens an existing project, imports a map document (via a folder connection), and saves the project to a different location
    /// 1. Creates a new project using the supplied name from a project template.
    /// </summary>
    /// <remarks>
    /// 1. Open this solution in Visual Studio 2013.  
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
    /// 1. Open any project - it can be an existing project containing data or a new empty project.
    /// 1. Click on the Add-in tab and see that a 'Work with Projects' buttons are added to a Tasks group.
    /// 1. The 'Work with Projects' button opens the 'Work with Projects' pane. 
    /// 1. To open a new project enter a valid project path and click "open Project"
    /// 1. To import an existing mxd document into an existing project, add a folder that contains mxd files under 'Add Folder [Path]:' then select an item from the list of MXDs
    /// 1. To create a new project enter a new Project Name, and an existing folder path, then select a template to create a new project
    /// ![UI](Screenshots/Screen.png)
    /// </remarks>
    internal class WorkwithProjectsModule : Module
    {
        private static WorkwithProjectsModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static WorkwithProjectsModule Current
        {
            get
            {
                return _this ?? (_this = (WorkwithProjectsModule)FrameworkApplication.FindModule("WorkwithProjects_Module"));
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

        /// <summary>
        /// Generic implementation of ExecuteCommand to allow calls to
        /// <see cref="FrameworkApplication.ExecuteCommand"/> to execute commands in
        /// your Module.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected override Func<Task> ExecuteCommand(string id)
        {

            //TODO: replace generic implementation with custom logic
            //etc as needed for your Module
            var command = FrameworkApplication.GetPlugInWrapper(id) as ICommand;
            if (command == null)
                return () => Task.FromResult(0);
            if (!command.CanExecute(null))
                return () => Task.FromResult(0);

            return () =>
            {
                command.Execute(null); // if it is a tool, execute will set current tool
                return Task.FromResult(0);
            };
        }
        #endregion Overrides

    }
}
