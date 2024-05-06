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
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Events;
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

namespace RowEventTest
{
  internal class TestAnnoModify : Button
  {
    private int _ModifiedAnnoCount = 0;
    private int _ModifiedNonAnnoCount = 0;

    protected override async void OnClick()
    {
      try
      {
        // show the event log
        ShowEventsViewModel.Show();
        _ModifiedAnnoCount = 0;
        _ModifiedNonAnnoCount = 0;
        Module1.EventModifiedCount = 0;
        await Module1.StartListening();
        await ModifyAnnoFeatures();
        await Module1.StopListening();
        Module1.AddEntry($"RowEvent / modified counts are equal: {_ModifiedAnnoCount + _ModifiedNonAnnoCount == Module1.EventModifiedCount}");
        Module1.AddEntry($"Annotation Rows modified: {_ModifiedAnnoCount}");
        Module1.AddEntry($"Non-Annotation Rows modified: {_ModifiedNonAnnoCount}");
        Module1.AddEntry($"Total RowEvents count: {Module1.EventModifiedCount}");
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.Message}");
      }
    }

    private async Task ModifyAnnoFeatures()
    {
      var result = await QueuedTask.Run<string>(() =>
      {
        // find features in the current map extent
        var features = MapView.Active.GetFeatures(MapView.Active.Extent);
        if (features.Count == 0)
        {
          return "No features found in the current map extent";
        }
        EditOperation op = null;
        foreach (var annoLayer in features.ToDictionary().Keys.OfType<AnnotationLayer>())
        {
          // are there features?
          var featOids = features[annoLayer];
          if (featOids.Count == 0)
            continue;

          // create the edit operation
          op ??= new EditOperation
          {
            Name = "Update annotation symbol",
            SelectModifiedFeatures = false,
            SelectNewFeatures = false
          };

          // load an inspector with all the features
          var insp = new Inspector();
          insp.Load(annoLayer, featOids);

          // get the annotation properties
          var annoProperties = insp.GetAnnotationProperties();

          // change the text 
          annoProperties.TextString = DateTime.Now.ToLongTimeString();
          // you can use a textstring with embedded formatting information 
          //annoProperties.TextString = "My <CLR red = \"255\" green = \"0\" blue = \"0\" >Special</CLR> Text";

          // change font color to red
          annoProperties.Color = Module1.GetRandomColour();

          // change the horizontal alignment
          annoProperties.HorizontalAlignment = HorizontalAlignment.Center;

          // set the annotation properties back on the inspector
          insp.SetAnnotationProperties(annoProperties);

          // call modify
          op.Modify(insp);
          _ModifiedAnnoCount += featOids.Count;
        }

        foreach (var featureLayer in features.ToDictionary().Keys.OfType<FeatureLayer>().Where ((fl) => fl.ShapeType == esriGeometryType.esriGeometryPoint))
        {
          // are there features?
          var featOids = features[featureLayer];
          if (featOids.Count == 0)
            continue;
          // create the edit operation
          op ??= new EditOperation
          {
            Name = "Update text field",
            SelectModifiedFeatures = true,
            SelectNewFeatures = false
          };
          // load an inspector with all the features
          var insp = new Inspector();
          insp.Load(featureLayer, featOids);

          // Find the first text field
          var firstTextField = featureLayer.GetFeatureClass().GetDefinition().GetFields().FirstOrDefault(f => f.FieldType == FieldType.String);
          if (firstTextField != null)
          {
            // change the text 
            insp[firstTextField.Name] = DateTime.Now.ToLongTimeString();

            // call modify
            op.Modify(insp);
            _ModifiedNonAnnoCount += featOids.Count;
          }
        }
        // execute the operation
        if ((op != null) && !op.IsEmpty)
        {
          var result = op.Execute();
          var errors = result ? string.Empty : op.ErrorMessage;
          return $"Updated {_ModifiedAnnoCount} annos with result: {result} {errors}";
        }
        return "No annotation features were selected";
      });
      Module1.AddEntry(result);
    }

  }
}
