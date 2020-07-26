/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsLayers
{
  class CommandFilterOrder : CustomizationFilter
  {
    public CommandFilterOrder()
    {
      Register();
    }
    /// <summary>
    /// Implement your custom filtering logic here. OnCommandExecute is called every time
    /// a command is clicked on the UI.
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    protected override bool OnCommandToExecute(string ID)
    {
      if (ID == "esri_layouts_sendBackward")
      {
        //Send to back was clicked. So selected elements can all go front
        Module1.SetState("can_bring_forward_state", true);
        //calculate if elements can backwards after button click (ZOrder change)
        Module1.SetState("can_send_backward_state", ZOrderCheck(ID));
      }
      if (ID == "esri_layouts_bringForward")
      {
        //Bring fwd was clicked. So selected elements can all go backwards.
        Module1.SetState("can_send_backward_state", true);
        //calculate if elements can forward after button click (ZOrder change)
        Module1.SetState("can_bring_forward_state", ZOrderCheck(ID));
      }
      if (ID == "GraphicsLayerExamples_Clipboard_CopyGraphics") 
          Module1.SetState("can_paste_graphics_state", true);
      return true;
    }

    /// <summary>
    /// Register for command filtering. Customization filters must be registered before they are
    /// called.
    /// </summary>
    public void Register()
    {
      FrameworkApplication.RegisterCustomizationFilter(this);

    }
    /// <summary>
    /// Unregister for command filtering
    /// </summary>
    public void UnRegister()
    {
      FrameworkApplication.UnregisterCustomizationFilter(this);
    }

    /// <summary>
    /// Implements IDisposable
    /// </summary>
    public void Dispose()
    {
      UnRegister();
    }

    internal static bool ZOrderCheck(string buttonId)
    {
      /////////////////////////////////////////////////////////////////////
      //command filter triggered (Pro's back or forward button was clicked.)
      // State for our button needs to be calculated based on this
      // Calculate the ZOrder of the elements after Pro's button is clicked.      

      int zorderCheck;
      var h = Module1.Current.GLWithElements.FirstOrDefault().Key;
      
      int elementCount = Module1.Current.GLWithElements.FirstOrDefault().Key.GetElements().Count;
      var selectedElements = Module1.Current.GLWithElements.FirstOrDefault().Value;
      //User has clicked the Pro Send To back button. 
      //if current Zorder is 1, it changes to 0 after button click.
      //Any element with a current zorder of 1 should disable "our" send to back button.
      // zorderCheck = 1 Check all the selected elements in this case for zorder value of 1.
      //User has clicked Pro's Bring Forward button. 
      //If current Zorder of button is (Selected Elements count - 1)
      //Our Bring Forward button should disable
      //zorderCheck = elementCount - 1
      zorderCheck = buttonId == "esri_layouts_sendBackward" ? 1 : elementCount - 2;
      //Check if any of the current elements have this zorder.
      //Exit if they do
      foreach (var el in selectedElements)
      {
        var zorder = el.GetZOrder();
        if (zorder == zorderCheck)
          return false;
      }
      return true;
    }
  }
}
