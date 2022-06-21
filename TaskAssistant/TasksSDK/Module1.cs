//   Copyright 2017 Esri
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

namespace TasksSDK
{
  /// <summary>
  /// This sample illustrates the methods available in the Tasks API.  The methods provide the following functionality
  /// 1. Open a task file (.esriTasks)
  /// 1. Close a specified task item
  /// 1. Export a specified task item to an .esriTasks file
  /// </summary>
  /// <remarks>
  /// 1. Open this solution in Visual Studio
  /// 1. Save the Project Exploration Tasks.esriTasks file in this solution to a location on your disk.
  /// 1. Open the OpenTask.cs file and change the taskFile variable to be the location where you saved the esriTasks file.
  /// 1. Open the ExportTask.cs file and modify the exportFolder variable if you wish the task item to be exported to a different location.  
  /// 1. Open the GetTaskItemInfo.cs file and modify the taskFile variable to be the location where you saved the esriTasks file.
  /// 1. Click the build menu and select Build Solution.  
  /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.  
  /// 1. Open any project - it can be an existing project containing data or a new empty project.
  /// 1. Click on the Add-in tab and see that 4 buttons are added to a Tasks group.
  /// 1. The Open Task button will open the.esriTasks file, add it to the project and load it into the Tasks pane.   
  /// The Export Task button will export a project task item to an .esriTasks file.  
  /// The Close Task button will close the task item loaded by the Open Task button.It will also remove it from the project.    
  /// The Task Info button will retrieve the task information from either a project task item or an.esriTasks file.
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("TasksSDK_Module"));
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

    /// <summary>
    /// The unique identifier of the current task item.
    /// </summary>
    public Guid taskGuid;

  }
}
