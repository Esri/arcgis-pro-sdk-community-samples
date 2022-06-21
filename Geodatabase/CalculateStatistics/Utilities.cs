/*

   Copyright 2020 Esri

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

namespace CalculateStatistics
{
  public static class Utilities
  {
    /// <summary>
    /// returns the enterprise gdb type for a given feature layer
    /// </summary>
    /// <param name="lyr"></param>
    /// <returns>EnterpriseDatabaseType enum of database or .Unknown</returns>
    public static EnterpriseDatabaseType GetDatabaseType(FeatureLayer lyr)
    {
      EnterpriseDatabaseType enterpriseDatabaseType = EnterpriseDatabaseType.Unknown;
      using (Table table = lyr.GetTable())
      {
        try
        {
          var geodatabase = table.GetDatastore() as Geodatabase;
          enterpriseDatabaseType = (geodatabase.GetConnector() as DatabaseConnectionProperties).DBMS;
        }
        catch (InvalidOperationException)
        {
        }
      }
      return enterpriseDatabaseType;
    }

    /// <summary>
    /// workaround to get sum from enterprise gdb lenght/area fields
    /// see https://community.esri.com/message/889796-problem-using-shapestlength-field-in-the-calculatestatistics-method
    /// </summary>
    /// <param name="fc">feature class to get sum from</param>
    /// <param name="fieldName">fieldname to sum up</param>
    /// <returns>sum</returns>
    public static double GetSumWorkAround(FeatureClass fc, string fieldName)
    {
      try
      {
        using (FeatureClassDefinition fcd = fc.GetDefinition())
        {
          double totalLen = 0.0;
          var cur = fc.Search();
          while (cur.MoveNext())
          {
            var feat = cur.Current;
            totalLen += Convert.ToDouble(feat[fieldName]);
          }
          return totalLen;
        }
      }
      catch (Exception)
      {
        throw;
      }
    }

  }
}
