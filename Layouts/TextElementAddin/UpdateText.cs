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

//added references
using ArcGIS.Desktop.Core;                       //access to Project
using ArcGIS.Desktop.Layouts;                    //access to LayoutProjectItem and Layout class
using ArcGIS.Desktop.Framework.Threading.Tasks;  //access to QueuedTask

namespace TextElementAddin
{
  /// <summary>
  /// This sample add-in references a text element on a layout view and changes it's text and (optionally) placement properties.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. Create a new project.
  /// 1. Insert a new layout called "Layout".
  /// 1. Insert a new text element called "Text".
  /// ![UI](Screenshots/Screen0.png)
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Click on the Add-In Tab.
  /// 1. Click the UpdateText button.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Note that the text of the layout text element changed
  /// </remarks>
  internal class UpdateText : Button
  {
    async protected override void OnClick()
    {
      //Reference a layout in the project called "Layout"
      LayoutProjectItem layoutItem = Project.Current.GetItems<LayoutProjectItem>().FirstOrDefault(item => item.Name.Equals("Layout"));

      //Update all the text properties at once within a single task
      await QueuedTask.Run(() =>
      {
        var lyt = layoutItem.GetLayout();  //Each layoutItem has metadata and an associated Layout object

              //Reference a text element called "Text"
              var txtElm = lyt.Elements.Single(item => (item is TextElement && item.Name.Equals("Text"))) as TextElement;

              //Modify the text elements placement properties
              var xPlacement = txtElm.GetX();
        var yPlacement = txtElm.GetY();
        txtElm.SetAnchor(ArcGIS.Core.CIM.Anchor.CenterPoint);
        xPlacement = 4.25;
        yPlacement = 5.5;
        txtElm.SetX(xPlacement);
        txtElm.SetY(yPlacement);

              //Modify the text properties
              var txtProp = txtElm.TextProperties;
        txtProp.Text = "Some New Text";
        txtProp.Font = "Tahoma";
        txtProp.FontSize = 24;
        txtElm.SetTextProperties(txtProp);
      });
    }
  }
}
