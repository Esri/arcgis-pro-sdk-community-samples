/*

   Copyright 2023 Esri

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
using ArcGIS.Core.Data.DDL;
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

namespace CopyLayer
{
  internal class CopyLayer : Button
  {
    protected override async void OnClick()
    {
      var originalLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault();
      if (originalLayer ==  null)
      {
        MessageBox.Show("Unable to find a FeatureLayer in the Table of Content");
        return;
      }
      var newLayerName = $@"New_{originalLayer.Name}";
      var isOk = await QueuedTask.Run<bool>(() =>
      {
        var LayerDef = originalLayer.GetFeatureClass().GetDefinition();
        using Geodatabase geodatabase = new(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath)));
          // Creating the attribute fields
          ArcGIS.Core.Data.DDL.FieldDescription objectIDFieldDescription = ArcGIS.Core.Data.DDL.FieldDescription.CreateObjectIDField();
          ArcGIS.Core.Data.DDL.FieldDescription field1 = new("field1", FieldType.Double);
          ArcGIS.Core.Data.DDL.FieldDescription field2 = new("field2", FieldType.Double);
        List<ArcGIS.Core.Data.DDL.FieldDescription> fieldDescriptions = new()
        {
          objectIDFieldDescription, field1, field2
        };
        FeatureClassDefinition originalFeatureClassDefinition = originalLayer.GetFeatureClass().GetDefinition();
        FeatureClassDescription originalFeatureClassDescription = new(originalFeatureClassDefinition);
        FeatureClassDescription LayerDescription = new(newLayerName, fieldDescriptions, originalFeatureClassDescription.ShapeDescription);
        SchemaBuilder schemaBuilder = new(geodatabase);
        schemaBuilder.Create(LayerDescription);
        bool success = schemaBuilder.Build();
        return success;
      });
      if (!isOk)
      {
        MessageBox.Show($@"Failed to create {newLayerName}");
        return;
      }
      // add the new FeatureClass to the map
      var newLyr = await QueuedTask.Run(() =>
      {
        using Geodatabase geodatabase = new(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath)));
        var newFc = geodatabase.OpenDataset<FeatureClass>(newLayerName);
        return LayerFactory.Instance.CreateLayer<FeatureLayer>(new FeatureLayerCreationParams(newFc) { Name = $@"New: {newLayerName}" }, MapView.Active.Map);
      });

      // copy some data
      await QueuedTask.Run(() =>
      {
        // create an edit operation
        EditOperation copyOperation = new EditOperation()
        {
          Name = "Copy Data",
          ProgressMessage = "Working...",
          CancelMessage = "Operation canceled.",
          ErrorMessage = "Error copying polygons",
          SelectModifiedFeatures = false,
          SelectNewFeatures = false
        };
        using var rowCursor = originalLayer.Search();
        while (rowCursor.MoveNext())
        {
          using (var row = rowCursor.Current as Feature)
          {
            var geom = row.GetShape().Clone();
            if (geom == null)
              continue;
            var newAttributes = new Dictionary<string, object>
            {
              { "field1", 1.0 },
              { "field2", 2.0 }
            };
            copyOperation.Create(newLyr, geom, newAttributes);
          }
        }
        // execute the operation onoy if changes where made
        if (!copyOperation.IsEmpty
            && !copyOperation.Execute())
        {
          MessageBox.Show($@"Copy operation failed {copyOperation.ErrorMessage}");
          return;
        }
      });
      // check for edits
      if (Project.Current.HasEdits)
      {
        var saveEdits = MessageBox.Show("Save edits?",
                      "Save Edits?", System.Windows.MessageBoxButton.YesNoCancel);
        if (saveEdits == System.Windows.MessageBoxResult.Cancel)
          return;
        else if (saveEdits == System.Windows.MessageBoxResult.No)
          _ = Project.Current.DiscardEditsAsync();
        else
        {
          _ = Project.Current.SaveEditsAsync();
        }
      }
    }
  }
}
