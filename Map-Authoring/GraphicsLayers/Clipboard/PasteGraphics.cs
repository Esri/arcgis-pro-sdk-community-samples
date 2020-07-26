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
using GraphicsLayers.Helpers;

namespace GraphicsLayers.Clipboard
{
  internal class PasteGraphics : Button
  {
    protected override void OnClick()
    {
      if (Module1.Current.SelectedGraphicsLayerTOC == null)
      {
        MessageBox.Show("Select a graphics layer in the TOC", "No graphics layer selected",
          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
        return;
      }      
      if (Module1.Current.ClipboardGraphicsLyrSelelectedElements == null)
      {
        MessageBox.Show("No graphic elements to paste", "Unable to paste",
          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
        return;
      }
      QueuedTask.Run( () => {
        foreach (var kvp in Module1.Current.ClipboardGraphicsLyrSelelectedElements)
        {
          var selElements = kvp.Value;
          if (selElements.Count == 0) continue;
          var gl = kvp.Key;
          Module1.Current.SelectedGraphicsLayerTOC.CustomCopyElements(selElements);         
        }
      });      
    }
  }
}
