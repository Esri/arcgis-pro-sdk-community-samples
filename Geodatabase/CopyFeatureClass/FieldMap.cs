/*

   Copyright 2025 Esri

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
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyFeatureClass
{
  /// <summary>
  /// Provide field mapping support for a feature class
  /// </summary>
  internal class FieldMap
  {
    /// <summary>
    /// Create the FieldMap for a FeatureLayer
    /// </summary>
    /// <param name="featureLayer">feature layer to map</param>
    internal FieldMap(FeatureLayer featureLayer)
    {
      var featureClass = featureLayer.GetFeatureClass();
      var fcDef = featureClass.GetDefinition();
      var excludeNames = new List<string>
      {
        fcDef.GetShapeField(),
        fcDef.GetAreaField(),
        fcDef.GetLengthField(),
        fcDef.GetObjectIDField(),
        fcDef.GetGlobalIDField(),
        fcDef.GetEditorField(),
        fcDef.GetEditedAtField(),
        fcDef.GetCreatedAtField(),
        fcDef.GetCreatorField()
      };
      var fields = fcDef.GetFields().Select((f) => f.Name);
      foreach (var field in fields)
      {
        // exclude shape, area and length
        if (excludeNames.Contains(field)) continue;
        FieldNames.Add(field);
      }
    }

    internal List<string> FieldNames = [];

    internal Dictionary<string, object> GetAttributeDict (Feature feature)
    {
      var dict = new Dictionary<string, object>();
      foreach (var field in FieldNames)
      {
        dict.Add (field, feature[field]);
      }
      return dict;
    }
  }
}
