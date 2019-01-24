//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MappingSampleAddIns.AddLayer
{
  /// <summary>
  /// Container of extension methods
  /// </summary>
  static class ExtensionMethods
  {
    /// <summary>
    /// An extension method appears as if it were FeatuerLayer's method. Returns the total number of features within the layer
    /// </summary>
    /// <param name="flyr">A Feature Layer</param>
    /// <returns>An interger represents total number of features</returns>
    public static int GetCount(this FeatureLayer flyr)
    {
      int count = 0;
      return QueuedTask.Run<int>(() =>
      {
        RowCursor rows = flyr.Search();
        while (rows.MoveNext())
          count++;

        return count;
      }).Result;
    }
  }
}
