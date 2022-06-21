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
using ArcGIS.Desktop.Internal.Mapping.CommonControls.Ribbon;

namespace GraphicElementSymbolPicker
{
  /// <summary>
  /// Represents the gallery of symbols
  /// </summary>
  internal class GallerySymbols : Gallery
  {
    private bool _isInitialized;

    public GallerySymbols()
    {
      AlwaysFireOnClick = true;
      Initialize();
    }
    internal void Initialize()
    {
      if (Module1.GallerySymbolItems.Count == 0) return;
      if (!_isInitialized) //first time
      {
        //This is where the binding happens.
        //Pro UI Gallery updates with "GallerySymbolItems" collection
        SetItemCollection(Module1.GallerySymbolItems);
        this.SelectedItem = this.ItemCollection[0];
        //Module1.SelectedSymbol = ((GeometrySymbolItem)this.SelectedItem).cimSymbol;
        _isInitialized = true;
        return;
      }
    }
    protected override void OnClick(object item)
    {
      var symbolItem = item as GeometrySymbolItem;
      if (symbolItem != null)
      {
        symbolItem.Execute(); //executes the tool
      }
    }
  }
}
