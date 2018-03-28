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

using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using System;
using System.Linq;
using System.Windows.Input;
using EditBox = ArcGIS.Desktop.Framework.Contracts.EditBox;

namespace ScientificDataStatisticalAnalysis
{
  public class TextEventArgs : EventArgs
  {
    public string text { get; set; }
  }
  internal class DefQueryEditBox : EditBox
  {
    public static string passingText;

    // Defines a delegate of the TextChangedEventHandler.
    public delegate void TextChangedEventHandler(object source, TextEventArgs e);

    // Defines an event of the TextChangedEventHandler.
    public static event TextChangedEventHandler TextChanged;

    /// <summary>
    /// Definition query editbox constructor. 
    /// Note: You can write your query or build it using the Definition Query builder in Layer Properties to verify its correctness.
    /// </summary>
    public DefQueryEditBox()
    {
      // By default, the edit box is empty.
      Text = "";
    }

    /// <summary>
    /// Called when the 'Enter' key is pressed inside this control or when the control loses keyboard focus.
    /// </summary>
    protected override void OnEnter()
    {
      // Passes the current Text in the EditBox to the global parameter passingText.
      passingText = Text;

      // Raises the text changed event. 
      TextChanged(this, new TextEventArgs() {text = Text});
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
      // Try and get the first selected layer.
      Layer firstSelectedLayer = null;
      try { firstSelectedLayer = MapView.Active.GetSelectedLayers().First(); } catch (Exception) { }
      // Check if there are any selected layers and if the first selected layer is a image service layer.
      if (!(firstSelectedLayer !=null && firstSelectedLayer is ImageServiceLayer))
        MessageBox.Show("Please select an image service layer.");
      else
      {
        // Passes the current Text in the EditBox to the global parameter passingText.
        passingText = Text;

        // Raises the text changed event. 
        TextChanged(this, new TextEventArgs() {text = Text});
      }
    }
    //protected override void OnTextChange(string text) => passingText = Text;
    //protected override void OnTextChange(string text)
    //{
    //  passingText = Text;

    //  // Raises the text changed event. 
    //  TextChanged(this, new TextEventArgs() { text = Text });

    //}


    }

}
