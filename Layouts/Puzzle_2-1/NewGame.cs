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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

//added references
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Media;

namespace Puzzle_2_1
{
  internal class NewGame : Button
  {
    public static string AddinAssemblyLocation()
    {
      var asm = System.Reflection.Assembly.GetExecutingAssembly();
      return System.IO.Path.GetDirectoryName(
                        Uri.UnescapeDataString(
                                new Uri(asm.CodeBase).LocalPath));
    }
    async protected override void OnClick()
    {
      Globals.mf_Name = "";
      Globals.i_correct = 0;
      Globals.i_guesses = 0;
      Globals.selEvents = false;
      Globals.elmType = "MF";

      LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals("Game Board"));
      if (layoutItem == null)
      {
        return;
      }
      await QueuedTask.Run(() => Project.Current.RemoveItem(layoutItem));
      await ProApp.Current.Dispatcher.BeginInvoke((Action)(() =>
      {
        FrameworkApplication.SetCurrentToolAsync("esri_mapping_selectByRectangleTool");
      }));
 
      IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("Puzzle_2_1_CreateGameBoard");
      var command = wrapper as ICommand; // tool and command(Button) supports this
      if ((command != null) && command.CanExecute(null))
        command.Execute(null);
    }
  }
}
