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
using ArcGIS.Desktop.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CreateReportBasic
{
  internal class ShowReportBasic : Button
  {
    protected override async void OnClick()
    {
      try
      {
        if (Module1.SimpleReport == null)
        {
          MessageBox.Show("No report found.  Please create a report first.");
          return;
        }
        //Create the new pane
        IReportPane iNewReportPane = await ProApp.Panes.CreateReportPaneAsync(Module1.SimpleReport);
        MessageBox.Show($@"Report pane open: {(iNewReportPane != null ? "successful" : "failed")}");
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error in Create Simple Report: {ex.Message}");
      }
    }

  }
}
