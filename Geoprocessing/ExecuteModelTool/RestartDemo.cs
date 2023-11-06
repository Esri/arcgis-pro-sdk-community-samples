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
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;

namespace ExecuteModelTool
{
  internal class RestartDemo : Button
  {
    protected override async void OnClick()
    {
      try
      {
        var map = MapView.Active.Map;
        if (map == null) return;
        var bufferSuffix = Module1.BufferSuffix;

        // remove all buffer layers from map
        var removeLayers = MapView.Active.Map.GetLayersAsFlattenedList().
                            OfType<FeatureLayer>().
                            Where(fl => fl.Name.EndsWith(bufferSuffix));
        await QueuedTask.Run(() =>
        {
          map.RemoveLayers(removeLayers);        
          // remove all demo featureclasses from the default geodatabase
          using (Geodatabase defaultGDB =
                 new(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
          {
            DeleteFeatureClass(defaultGDB, bufferSuffix);
          }
        });
      }
      catch (GeodatabaseException ex)
      {
        if (ex.Message == "The table was not found.") return;
        MessageBox.Show(ex.Message);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    public static bool DeleteFeatureClass(Geodatabase geodatabase, string featureClassNameSuffix)
    {
      // Create a SchemaBuilder object
      SchemaBuilder schemaBuilder = new(geodatabase);

      // Create a FeatureClassDescription object
      IReadOnlyList<Definition> fcList = geodatabase.GetDefinitions<FeatureClassDefinition>();
      foreach (FeatureClassDefinition fcDefinition in fcList)
      {
        if (!fcDefinition.GetName().EndsWith(featureClassNameSuffix)) continue;
        FeatureClassDescription fcDescription = new(fcDefinition);

        // Add the deletion fo the feature class to our list of DDL tasks
        schemaBuilder.Delete(fcDescription);
      }
      // Execute the DDL
      bool success = schemaBuilder.Build();
      return success;
    }
  }
}
