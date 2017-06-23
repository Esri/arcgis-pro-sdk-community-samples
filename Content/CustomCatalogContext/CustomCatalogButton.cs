/*

   Copyright 2017 Esri

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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;

namespace CustomCatalogContext
{
  /// <summary>
  /// Access the Catalog context via the FrameworkApplication.ActiveWindow
  /// </summary>
  internal class CustomCatalogButton : Button
  {
    protected override void OnClick() {
      //window will be null if the ActiveWindow is not the Catalog dock pane or a Catalog view.
      IProjectWindow window = FrameworkApplication.ActiveWindow as ArcGIS.Desktop.Core.IProjectWindow;
      if (window?.SelectedItems.Count() > 0) {
        var sb = new StringBuilder();
        sb.AppendLine("Selection:");
        foreach (var item in window.SelectedItems) {
          sb.AppendLine($"  {item.Name}, {item.TypeID}, {item.GetType().ToString()}");
        }
        MessageBox.Show(sb.ToString(),"Catalog Selection");
      }
    }
  }
}
