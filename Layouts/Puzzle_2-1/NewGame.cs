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
