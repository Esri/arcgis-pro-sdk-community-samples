//   Copyright 2017 Esri
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
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.TaskAssistant;

namespace TasksSDK
{
  /// <summary>
  /// Button which implements the ExportTaskAsync API  method.  
  /// </summary>
  /// <remarks>
  /// Export the task item which is represented by the unique identifier passed to the ExportTaskAsync API call.  Export
  /// the task to an .esriTasks file. 
  /// <para>
  /// In this example we will export the first task item found in the collection of project task items.
  /// Note the condition associated with this button in the config.daml (esri_tasks_HasProjectTasksCondition).   
  /// It ensures that the button is only enabled when task items exist in the project.
  /// </para>
  /// 
  /// </remarks>
  internal class ExportTask : Button
  {
    protected override async void OnClick()
    {
      // get the first taskItem from the project pane
      var taskItem = Project.Current.GetItems<TaskProjectItem>().FirstOrDefault();
      if (taskItem == null)
        return;

      // use the TaskGuid property on the ProjectItem to obtain the unique identifier for the task item
      // pass this guid to the ExportTaskAsync method
      try
      {
        string fileName = await TaskAssistantModule.ExportTaskAsync(taskItem.TaskItemGuid, "c:\\temp");
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Task saved to " + fileName);
      }
      catch (ExportTaskException e)
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error saving task " + e.Message);
      }
    }
  }
}
