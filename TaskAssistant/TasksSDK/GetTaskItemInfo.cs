/*

   Copyright 2018 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.TaskAssistant;

namespace TasksSDK
{
  /// <summary>
  /// Button which implements the GetTaskItemInfoAsync API method.
  /// </summary>
  /// <remarks>
  /// The GetTaskItemInfoAsync method retrieves the task information from an .esriTasks file or a task project item.  The method returns a 
  /// TaskItemInfo class which contains the name, description, unique identifier and task information from the task item. 
  /// </remarks>
  internal class GetTaskItemInfo : Button
  {
    protected override async void OnClick()
    {

      // obtain task information from an .esriTasks file

      // TODO - substitute your own .esriTasks file 
      string taskFile = @"c:\Tasks\Project Exploration Tasks.esriTasks";
      if (!System.IO.File.Exists(taskFile))
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Cannot find file " + taskFile + ". Check file location.");
        return;
      }

      try
      {
        // retrieve the task item information
        TaskItemInfo taskItemInfo = await TaskAssistantModule.GetTaskItemInfoAsync(taskFile);

        string message = "Name : " + taskItemInfo.Name;
        message += "\r\n" + "Description : " + taskItemInfo.Description;
        message += "\r\n" + "Guid : " + taskItemInfo.Guid.ToString("B");
        message += "\r\n" + "Task Count : " + taskItemInfo.GetTasks().Count();

        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, "Task Information");
      }
      catch (OpenTaskException e)
      {
        // exception thrown if task file doesn't exist or has incorrect format
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.Message, "Task Information");
      }
      catch (TaskFileVersionException e)
      {
        // exception thrown if task file does not support returning task information
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(e.Message, "Task Information");
      }


      // OR   obtain the task information from a task project item

      //// find the first task item in the project
      //var taskItem = Project.Current.GetItems<TaskProjectItem>().FirstOrDefault();
      //// if there isn't a project task item, return
      //if (taskItem == null)
      //  return;

      //string message = await QueuedTask.Run(async () =>
      //{
      //  bool isOpen = taskItem.IsOpen;
      //  Guid taskGuid = taskItem.TaskItemGuid;

      //  string msg = "";
      //  try
      //  {
      //    TaskItemInfo taskItemInfo = await taskItem.GetTaskItemInfoAsync();

      //    msg = "Name : " + taskItemInfo.Name;
      //    msg += "\r\n" + "Description : " + taskItemInfo.Description;
      //    msg += "\r\n" + "Guid : " + taskItemInfo.Guid.ToString("B");
      //    msg += "\r\n" + "Task Count : " + taskItemInfo.GetTasks().Count();

      //    // iterate the tasks in the task item
      //    IEnumerable<TaskInfo> taskInfos = taskItemInfo.GetTasks();
      //    foreach (TaskInfo taskInfo in taskInfos)
      //    {
      //      string name = taskInfo.Name;
      //      Guid guid = taskInfo.Guid;

      //      // do something 
      //    }
      //  }
      //  catch (OpenTaskException e)
      //  {
      //    // exception thrown if task file doesn't exist or has incorrect format
      //    msg = e.Message;
      //  }
      //  catch (TaskFileVersionException e)
      //  {
      //    // exception thrown if task file does not support returning task information
      //    msg = e.Message;
      //  }
      //  return msg;
      //});

      //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, "Task Information");


    }
  }
}
