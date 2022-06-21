/*

   Copyright 2022 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GraphicElementSymbolPicker
{
    /// <summary>
    /// The gallery that holds all the tools
    /// </summary>
    internal class GalleryGraphicElementTools : Gallery
    {
        private bool _isInitialized;
        protected override void OnDropDownOpened()
        {
            System.Diagnostics.Debug.WriteLine("Gallery OnDropOpened");
            AlwaysFireOnClick = true;
            Initialize();
        }
        private void Initialize()
        {
            if (!_isInitialized) //first time opened
            {
                //Binding
                this.SetItemCollection(Module1.GalleryElementToolItems);
                //Defaults
                this.SelectedItem = this.ItemCollection[0];
                this.Caption = (this.SelectedItem as ElementCreationToolGalleryItem).Name;
                this.LargeImage = (this.SelectedItem as ElementCreationToolGalleryItem).Icon32;
                _isInitialized = true;
            }
        }

        protected override void OnClick(object item)
        {
            var galleryToolItem = item as ElementCreationToolGalleryItem;

            //Clear the symbols if the active tool and the clicked tool do not match
            //Populate the symbol gallery with the symbols that match the tool type clicked.
            if (Module1.ActiveToolGeometry != galleryToolItem.ToolShapeType)
            {
                Module1.GallerySymbolItems.Clear();
                switch (galleryToolItem.ToolShapeType)
                {
                    case Module1.ToolType.Point:
                        Module1.ActiveToolGeometry = Module1.ToolType.Point;
                        foreach (var ptSymbol in Module1.PointGallerySymbolItems)
                        {
                            Module1.GallerySymbolItems.Add(ptSymbol);
                        }
                        break;
                    case Module1.ToolType.Line:
                        Module1.ActiveToolGeometry = Module1.ToolType.Line;
                        foreach (var lineSymbol in Module1.LineGallerySymbolItems)
                        {
                            Module1.GallerySymbolItems.Add(lineSymbol);
                        }
                        break;
                    case Module1.ToolType.Polygon:
                        Module1.ActiveToolGeometry = Module1.ToolType.Polygon;
                        foreach (var polySymbol in Module1.PolygonGallerySymbolItems)
                        {
                            Module1.GallerySymbolItems.Add(polySymbol);
                        }
                        break;
                    case Module1.ToolType.Text:
                        Module1.ActiveToolGeometry = Module1.ToolType.Text;
                        foreach (var textSymbol in Module1.TextGallerySymbolItems)
                        {
                            Module1.GallerySymbolItems.Add(textSymbol);
                        }
                        break;

                }
                //Selected symbol
                Module1.SelectedSymbol = ((GeometrySymbolItem)Module1.GallerySymbolItems.FirstOrDefault()).cimSymbol;
                Module1.SelectedSymbolName = ((GeometrySymbolItem)Module1.GallerySymbolItems.FirstOrDefault()).Name;
            }

            if (galleryToolItem != null)
            {
                galleryToolItem.Execute(); //executes the tool        
                this.LargeImage = galleryToolItem.Icon32;
                this.Caption = galleryToolItem.Name;
                return;
            }
        }
    }
}
