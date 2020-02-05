/*

   Copyright 2020 Esri

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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace CalculateStatistics
{
    internal class CalculateLength : Button
    {
        protected override void OnClick()
        {
            try
            {
                var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Contains("TestLines")).FirstOrDefault();
                var len = QueuedTask.Run(() =>
                {
                    var enterpriseDbType = Utilities.GetDatabaseType(featureLayer);
                    var fc = featureLayer.GetFeatureClass();
                    return GetLength(fc, enterpriseDbType);
                });
                MessageBox.Show($@"Len: {len.Result}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        double GetLength(FeatureClass fc, EnterpriseDatabaseType enterpriseDbType)
        {
            try
            {
                using (FeatureClassDefinition fcd = fc.GetDefinition())
                {                    
                    // the name of the length field changes depending on what enterprise geodatabase is used
                    var areaFieldName = "Shape_Length";
                    switch (enterpriseDbType)
                    {
                        case EnterpriseDatabaseType.SQLServer:
                            areaFieldName = "STLength";
                            break;
                    }
                    Field lengthField = fcd.GetFields().FirstOrDefault(x => x.Name.Contains(areaFieldName));
                    if (lengthField == null) return 0;
                    System.Diagnostics.Debug.WriteLine(lengthField.Name); 

                    StatisticsDescription SumDesc = new StatisticsDescription(lengthField, new List<StatisticsFunction>() { StatisticsFunction.Sum });
                    TableStatisticsDescription tsd = new TableStatisticsDescription(new List<StatisticsDescription>() { SumDesc });
                    double sum = 0;
                    try
                    {
                        sum = fc.CalculateStatistics(tsd).FirstOrDefault().StatisticsResults.FirstOrDefault().Sum; // exception is thrown on this line
                    }
                    catch
                    {
                        sum = Utilities.GetSumWorkAround(fc, lengthField.Name);
                    }
                    return sum;
                }
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.ToString(), "Error");
                return 0;
            }
        }

    }
}
