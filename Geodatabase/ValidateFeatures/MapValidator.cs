/*

   Copyright 2019 Esri

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
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidateFeatures
{
  class MapValidator
  {
    /// <summary>
    /// This model class validates the selected features in a map.
    /// </summary>
    /// <param name="map">The map to validate</param>
    /// <returns></returns>
    public static Task<string> ValidateMap(Map map)
    {
     return QueuedTask.Run<string>(() =>
     {
       StringBuilder validationStringBuilder = new StringBuilder();

       // Get the selection from the map

       Dictionary<MapMember, List<long>> mapMembersWithSelection = map.GetSelection().ToDictionary();

       // Step through each MapMember (FeatureLayer or StandaloneTable) that contains selected features

       foreach (KeyValuePair<MapMember, List<long>> dictionaryItem in mapMembersWithSelection)
       {
         if (dictionaryItem.Key is FeatureLayer)
         {
           FeatureLayer featureLayer = dictionaryItem.Key as FeatureLayer;
           using (Table table = featureLayer.GetTable())
           {
             validationStringBuilder.Append(ValidateTable(table, featureLayer.GetSelection()));
           }
         }
         else if (dictionaryItem.Key is StandaloneTable)
         {
           StandaloneTable standaloneTable = dictionaryItem.Key as StandaloneTable;
           using (Table table = standaloneTable.GetTable())
           {
             validationStringBuilder.Append(ValidateTable(table, standaloneTable.GetSelection()));
           }
         }
       }
       return validationStringBuilder.ToString();
     });

    }

    /// <summary>
    /// ValidateTable calls Validate on the selected features of a table.
    /// </summary>
    /// <param name="table">The table to validate.</param>
    /// <param name="selection">The selected features in the table to validate.</param>
    /// <returns>A string indicating the results of Validate; either a list of errors or a message saying that the table contains no invalid data.</returns>
    private static string ValidateTable(Table table, Selection selection)
    {
      IReadOnlyDictionary<long, string> validateResults = table.Validate(selection);

      if (validateResults.Count == 0)
      {
        return table.GetName() + " contains no invalid data.\n";
      }
      else
      {
        StringBuilder stringBuilder = new StringBuilder(table.GetName() + "\n");

        foreach (KeyValuePair<long, string> validateResult in validateResults)
        {
          stringBuilder.Append(validateResult.Key);
          stringBuilder.Append(": " + validateResult.Value + "\n");
        }

        stringBuilder.Append("\n");
        return stringBuilder.ToString();
      }
    }
  }
}
