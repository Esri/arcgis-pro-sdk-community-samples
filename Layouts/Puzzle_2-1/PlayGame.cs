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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Media;

namespace Puzzle_2_1
{
  internal class PlayGame : Button
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
      //Prevent tool from executing if layout view doesn't exist
      LayoutView layoutView = LayoutView.Active;
      if (layoutView == null)
      {
        System.Windows.MessageBox.Show("Can't find layout view, try clicking New Game Board");
        return;
      }

      //Prevent tool from executing if puzzle pieces are not scrambled
      Layout layout = layoutView.Layout;
      MapFrame mfTest = layout.FindElement("MF1") as MapFrame;
      if (mfTest == null)
      {
        System.Windows.MessageBox.Show("Game board not scrambled");
        return;
      }
      if (mfTest.IsVisible == false)
      {
        System.Windows.MessageBox.Show("Game finished, click New Game");
        return;
      }

      Globals.selEvents = true;
      await FrameworkApplication.SetCurrentToolAsync("esri_mapping_selectByRectangleTool");

      //Update Instructions
      TextElement instr = layout.FindElement("Instructions") as TextElement;
      string text = "<bol>Instructions: </bol> \n\n  Select a Map Frame puzzle piece" as String;
      var instrProp = instr.TextProperties;
      instrProp.Text = text;
      await QueuedTask.Run(() => instr.SetTextProperties(instrProp));
    }
   
  }

}
