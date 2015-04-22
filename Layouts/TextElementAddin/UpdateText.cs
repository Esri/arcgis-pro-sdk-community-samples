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

//added references
using ArcGIS.Desktop.Core;                       //access to Project
using ArcGIS.Desktop.Layouts;                    //access to LayoutProjectItem
using ArcGIS.Desktop.Layouts.Models;             //access to Layout Class
using ArcGIS.Desktop.Framework.Threading.Tasks;  //access to QueuedTask

namespace TextElementAddin
{
    /// <summary>
    /// This sample addin references a text element on a layout and changes it text and placement properties.
    /// </summary>
    /// <remarks>
    /// In Visual Studio:
    /// 1. Select Build --> Build Solution.
    /// 2. Click the Start button to fire up ArcGIS Pro.
    /// In ArcGIS Pro
    /// 3. Create a new project.
    /// 4. Insert a new layout called "Layout".
    /// 5. Insert a new text element called "Text".
    /// 6. Click ADD-IN Tab --> UpdateText
    /// </remarks>
    internal class UpdateText : Button
    {
        async protected override void OnClick()
        {
            //Reference a layout in the project called "Layout"
            var layoutItems = Project.Current.GetItems<LayoutProjectItem>().ToList();  //a list a layout items in the Project
            var layoutItem = layoutItems.Find(item => item.Name.Equals("Layout"));  // a specific layout item called "Layout"

            //Update all the text properties at once within a single task
            await QueuedTask.Run(() =>
            {
                var lyt = layoutItem.GetLayout();  //Each layoutItem has metadata and an associated Layout object

                //Reference a text element called "Text"
                var txtElm = lyt.Elements.Single(item => (item is TextElement && item.Name.Equals("Text"))) as TextElement;

                //Modify the text elements placement properties
                var txtPlacement = txtElm.GetPlacement();
                txtPlacement.Anchor = ArcGIS.Core.CIM.Anchor.CenterPoint;
                txtPlacement.X = 4.25;
                txtPlacement.Y = 5.5;
                txtElm.SetPlacement(txtPlacement);

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
