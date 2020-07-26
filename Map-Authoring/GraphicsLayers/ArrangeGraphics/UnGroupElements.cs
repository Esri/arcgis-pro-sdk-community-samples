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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace GraphicsLayers.ArrangeGraphics
{
    internal class UnGroupElements : Button
    {
        protected override void OnClick()
        {
            //return if no target graphics layer
            QueuedTask.Run( () => {
              //get the selected graphic elements
              //elements selected in multiple Graphic layers, no op.
              if (Module1.Current.GLWithElements.Count > 1) return;
              //get graphics layer
              var gl = Module1.Current.GLWithElements.FirstOrDefault().Key;
              //Get selected elements in graphics layer
              var elements = Module1.Current.GLWithElements.FirstOrDefault().Value;
              List<ArcGIS.Desktop.Layouts.GroupElement> groupElements = new List<ArcGIS.Desktop.Layouts.GroupElement>();
                foreach (var element in elements)
                {
                    if (element is GroupElement)
                        groupElements.Add(element as GroupElement);
                }
                //return if no graphic elements are grouped selected
                if (groupElements.Count() == 0) return;
                //Ungroup the selected elements
                gl.UnGroupElements(groupElements);          
           });            
        }
    }
}
