/*

   Copyright 2019 Esri

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
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutMapSeries.LayoutSettings
{
  public class MapSeriesDefinition
  {
    public string FeatureClassName { get; set; }

    public IList<MapSeriesItem> MapSeriesItems = new List<MapSeriesItem>();

    public void LoadFromFeatureClass(string layoutName, FeatureClass featureClass, string fieldList)
    {
      MapSeriesItems.Clear();
      var oidName = featureClass.GetDefinition().GetObjectIDField();
      QueryFilter getQf = new QueryFilter
      {
        SubFields = $@"{oidName},{fieldList}"
      };
      var fields = fieldList.Split(new char []{ ',' });
      if (fields.Length < 2)
      {
        throw new Exception($@"List of fields {fieldList} needs to contain at least ID and Name");
      }
      // For Selecting all matching entries.
      using (var rowCursor = featureClass.Search(getQf))
      {
        var oidIdx = rowCursor.FindField(oidName);
        var idIdx = rowCursor.FindField(fields[0]);
        var nameIdx = rowCursor.FindField(fields[1]);
        while (rowCursor.MoveNext())
        {
          using (var row = rowCursor.Current)
          {
            var oid = Convert.ToInt64(row[oidIdx]);
            var id = Convert.ToInt32(row[idIdx]);
            var name = row[nameIdx].ToString();
            if (string.IsNullOrEmpty(layoutName)) MessageBox.Show("test");
            MapSeriesItems.Add(new MapSeriesItem { Oid = oid, Id = id, Name = name, LayoutName = layoutName });
          }
        }
      }
    }
  }

  public class MapSeriesItem
  {
    public long Oid { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string LayoutName { get; set; }
  }
}
