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
  internal class CreateReportBasic : Button
  {
    protected override async void OnClick()
    {
      try
      {
        var featLyr = MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();
        if (featLyr == null)
        {
          MessageBox.Show("No feature layer found in the active map.");
          return;
        }
        var reportCreated = await QueuedTask.Run(() =>
        {
          featLyr.ClearSelection();
          var reportDataSource = PrepareDataSource(featLyr);
          Module1.SimpleReport = ReportFactory.Instance.CreateReport("Simple Report", reportDataSource);
          return Module1.SimpleReport != null;
        });
        MessageBox.Show($@"Report creation: {(reportCreated ? "successful" : "failed (maybe 'New Report' exists)")}");
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error in Create Simple Report: {ex.Message}");
      }
    }

    private ReportDataSource PrepareDataSource(FeatureLayer featLyr)
    {
      var listOfFields = new List<CIMReportField>();
      var fields = featLyr.GetTable().GetDefinition().GetFields();
      // reverse the field order
      var fieldIdx = fields.Count -1;
      // make the first field invisible
      var isVisible = false;
      foreach (var field in fields)
      {
        listOfFields.Add(new CIMReportField { Name = field.Name,  IsVisible = isVisible, FieldOrder = fieldIdx-- });
        isVisible = true;
      }
      // don't use selection
      return new ReportDataSource(featLyr, "", false, listOfFields);
    }
  }
}
