/*

   Copyright 2017 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Mapping;

namespace Renderer.Helpers
{
    internal static class SDKHelpers
    {
        internal static CIMColorRamp GetColorRamp()
        {
            StyleProjectItem style =
              Project.Current.GetItems<StyleProjectItem>().FirstOrDefault(s => s.Name == "ArcGIS Colors");
            if (style == null) return null;
            var colorRampList = style.SearchColorRamps("Heat Map 4 - Semitransparent");
            if (colorRampList == null || colorRampList.Count == 0) return null;
            return colorRampList[0].ColorRamp;
        }

        internal static string GetNumericField(FeatureLayer featureLayer)
        {
            //Get all the visible numeric fields for the layer.
            var numericField = featureLayer.GetFieldDescriptions().FirstOrDefault(f => IsNumericFieldType(f.Type) && f.IsVisible);
            return numericField.Name;
        }

        internal static string GetDisplayField(FeatureLayer featureLayer)
        {
            // get the CIM definition from the layer
            var cimFeatureDefinition = featureLayer.GetDefinition() as ArcGIS.Core.CIM.CIMBasicFeatureLayer;
            // get the view of the source table underlying the layer
            var cimDisplayTable = cimFeatureDefinition.FeatureTable;
            // this field is used as the 'label' to represent the row
            return cimDisplayTable.DisplayField;
        }

        internal static bool IsNumericFieldType(FieldType type)
        {
            switch (type)
            {
                case FieldType.Double:
                case FieldType.Integer:
                case FieldType.Single:
                case FieldType.SmallInteger:
                    return true;
                default:
                    return false;
            }
        }

        internal static Tuple<string, string> GetFieldMinMax(FeatureLayer featureLayer, string fieldName)
        {

            //Get the file gdb from the feature layer
            var tableDef = featureLayer.GetTable().GetDefinition();
            var fields = tableDef.GetFields();
            //var fieldName = fields[2].Name;
            //var field = tableDef.GetFields().First(f => f.Name == "FID_1");

            QueryFilter queryFilter = new QueryFilter()
            {
                WhereClause = "1 = 1",
            };
            using (var rowCursor = featureLayer.Search(queryFilter))
            {
                long iMin = -1;
                long iMax = -1;
                while (rowCursor.MoveNext())
                {
                    var iVal = Convert.ToInt64(rowCursor.Current[fieldName]);
                    if ((iMin > iVal) || (iMin == -1))
                        iMin = iVal;
                    if ((iMax < iVal) || (iMax == -1))
                        iMax = iVal;
                }
                return new Tuple<string, string>(iMin.ToString(), iMax.ToString());

            }
        }

    }
}
