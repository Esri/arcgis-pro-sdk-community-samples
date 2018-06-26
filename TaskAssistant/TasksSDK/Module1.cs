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
  ///   1. Open a task file (.esriTasks)
  ///   2. Close a specified task item
  ///   3. Export a specified task item to an .esriTasks file
  /// </summary>
  /// <remarks>
  ///1. Open this solution in Visual Studio  
  ///2. Save the Project Exploration Tasks.esriTasks file in this solution to a location on your disk.
  ///3. Open the OpenTask.cs file and change the parameter of the TaskAssistantModule.OpenTaskAsync method to be the 
  ///location where you saved the esriTasks file. 
  ///4. Open the ExportTask.cs file and modify the path parameter in the TaskAssistantModule.ExportTaskAsync method 
  ///if you wish the task item to be exported to a different location.
  ///5. Click the build menu and select Build Solution.
  ///6. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
  ///7. Open any project - it can be an existing project containing data or a new empty project.
  ///8. Click on the Add-in tab and see that 3 buttons are added to a Tasks group.
  ///9. The Open Task button will open the .esriTasks file, add it to the project and load it into the Tasks pane. 
  ///The Close Task button will close the task item loaded by the Open Task button.  It will also remove it 
  ///from the project.   The Export Task button will export a project task item to an .esriTasks file. 
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
