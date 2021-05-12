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
using System.Windows.Media;
using System.Windows;
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

namespace GraphicsLayers
{
    internal class GraphicElementsGallery : Gallery
    {
        private bool _isInitialized;
        public GraphicElementsGallery()
        {
            Initialize();
            this.AlwaysFireOnClick = true;
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;
            //Get our components from the category
            // Get all the button/tool components registered in our category
            foreach (var component in Categories.GetComponentElements("GraphicsLayerExamples_Category"))
            {
                try
                {
                    var content = component.GetContent();
                    //This flavor (off component) returns empty string
                    //if the attribute is not there
                    var group = component.ReadAttribute("group") ?? "";
                    var name = component.ReadAttribute("name") ?? "";
                    //check we get a plugin
                    var plugin = FrameworkApplication.GetPlugInWrapper(component.ID);
                    if (plugin != null)
                    {
                        Add(new GraphicsElementToolItem(component.ID, group, plugin, name));
                    }
                }
                catch (Exception e)
                {
                    string x = e.Message;
                }
            }

        }

        protected override void OnClick(object item)
        {
            //TODO - insert your code to manipulate the clicked gallery item here
            var graphicsItem = item as GraphicsElementToolItem;
            graphicsItem.Execute();
        }
    }
}
