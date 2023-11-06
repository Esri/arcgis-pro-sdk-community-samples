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

namespace DuplicateAndSelect
{
  internal class TableDuplicateAddSelect : Button
  {
    protected override void OnClick()
    {
      try
      {
        QueuedTask.Run(() =>
        {
          // get the currently selected features in the map
          var selectedFeatures = MapView.Active.Map.GetSelection();

          // get the first table and its corresponding selected feature OIDs
          var firstSelectionSet = selectedFeatures.ToDictionary().First(sel => sel.Key is StandaloneTable);

          // create an instance of the inspector class
          var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector();

          // load the first selected feature into the inspector using the first OID from the list of object IDs
          var mapMember = firstSelectionSet.Key;
          var oid = firstSelectionSet.Value[0];
          inspector.Load(mapMember, oid);

          //Create new feature from an existing inspector (copying the feature)
          var createOp = new EditOperation
          {
            Name = "Copy first selected row",
            SelectNewFeatures = false
          };
          var rowToken = createOp.Create(inspector.MapMember, inspector);  // inspector.ToDictionary(a => a.FieldName, a => a.CurrentValue)
          if (createOp.IsEmpty)
          {
            MessageBox.Show($@"Record oid: {oid} in table: [{mapMember.Name}] was not duplicated");
          }
          else
          {
            var result = createOp.Execute(); //Execute and ExecuteAsync will return true if the operation was successful and false if not
            if (!result)
              MessageBox.Show($@"Copying record oid: {oid} in table: [{mapMember.Name}] failed: [{createOp.ErrorMessage}]");
            else
            {
              var newOid = rowToken.ObjectID.Value;
              MessageBox.Show($@"New record oid: {newOid} in table: [{mapMember.Name}]");
              // now add this record to the existing selection
              (mapMember as StandaloneTable).Select(new QueryFilter() { ObjectIDs = new long[] { newOid } }, SelectionCombinationMethod.Add);
            }
          }
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }

    }
  }
}
