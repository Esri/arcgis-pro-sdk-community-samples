/*

   Copyright 2024 Esri

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

namespace DAML.LayersPropertySheet
{
  internal class LayersPropertySheetViewModel : Page
  {
    /// <summary>
    /// Invoked when the OK or apply button on the property sheet has been clicked.
    /// </summary>
    /// <returns>A task that represents the work queued to execute in the ThreadPool.</returns>
    /// <remarks>This function is only called if the page has set its IsModified flag to true.</remarks>
    protected override Task CommitAsync()
    {
      return Task.FromResult(0);
    }

    /// <summary>
    /// Called when the page loads because it has become visible.
    /// </summary>
    /// <returns>A task that represents the work queued to execute in the ThreadPool.</returns>
    protected override Task InitializeAsync()
    {
      return Task.FromResult(true);
    }

    /// <summary>
    /// Called when the page is destroyed.
    /// </summary>
    protected override void Uninitialize()
    {
    }

    /// <summary>
    /// Text shown inside the page hosted by the property sheet
    /// </summary>
    public string DataUIContent
    {
      get  
      { 
        return "Insert UI content here"; 
      }
      set => SetProperty(ref base.Data[0], value);
    }
  }

  /// <summary>
  /// Button implementation to show the property sheet.
  /// </summary>
  internal class LayersPropertySheet_ShowButton : Button
  {
    protected override void OnClick()
    {
      // collect data to be passed to the page(s) of the property sheet
      Object[] data = new object[] { "Page UI content" };

      if (!PropertySheet.IsVisible)
        PropertySheet.ShowDialog("DAML_LayersPropertySheet_LayersPropertySheet", "DAML_LayersPropertySheet_LayersPropertySheet", data);

    }
  }
}
