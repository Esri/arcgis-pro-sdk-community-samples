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
using ArcGIS.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VersionUpdatePerformance
{
  internal class FieldMap
  {
    /// <summary>
    /// Create the FieldMap to map from fields to to fields
    /// </summary>
    /// <param name="fromFc"></param>
    /// <param name="toFc"></param>
    public FieldMap(FeatureClass fromFc, FeatureClass toFc)
    {
      var fromFcDef = fromFc.GetDefinition();
      var toFcDef = toFc.GetDefinition();
      var excludeNames = new List<string>
      {
        fromFcDef.GetShapeField(),
        fromFcDef.GetAreaField(),
        fromFcDef.GetLengthField(),
        fromFcDef.GetObjectIDField(),
        fromFcDef.GetGlobalIDField(),
        fromFcDef.GetEditorField(),
        fromFcDef.GetEditedAtField(),
        fromFcDef.GetCreatedAtField(),
        fromFcDef.GetCreatorField()
      };
      ToShapeName = toFcDef.GetShapeField();
      var fromFields = fromFcDef.GetFields().Select((f) => f.Name);
      var toFields = toFcDef.GetFields().Select((f) => f.Name);
      foreach (var fromField in fromFields)
      {
        // exclude shape, area and length
        if (excludeNames.Contains(fromField)) continue;
        if (toFields.Contains(fromField))
        {
          FromToMap.Add((fromField, fromField));
        }
      }
    }

    public List<(string FromFieldName, string ToFieldName)> FromToMap = [];

    public string ToShapeName { get; set; }
  }

}
