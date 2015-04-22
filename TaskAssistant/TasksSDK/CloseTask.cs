//   Copyright 2015 Esri
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
using ArcGIS.Desktop.TaskAssistant;

namespace TasksSDK
{
  /// <summary>
  /// Button which implements the CloseTask API  method.  
  /// </summary>
  /// <remarks>
  /// Close the task item which is represented by the unique identifier passed to the CloseTask API call. 
  /// In this example we will close the task item which was opened using the OpenTask button.  The CloseTask
  /// API call unloads the task item from the Tasks pane and removes it from the project.
  /// <para>
  /// Note the condition associated with this button in the config.daml (esri_tasks_IsTaskFileLoadedCondition).   
  /// It ensures that the button is only enabled when a task item is loaded in the Tasks pane. 
  /// </para>
  /// </remarks>
  internal class CloseTask : Button
  {
    protected override async void OnClick()
    {
      // unload 
      if (!string.IsNullOrEmpty(Module1.Current.taskGuid))
        await TaskAssistantModule.CloseTask(Module1.Current.taskGuid);
    }
  }
}
