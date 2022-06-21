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

namespace Colorizer
{
    internal class BtnNewColor : Button
    {
        protected override async void OnClick()
        {
            try
            {
                var raster = MapView.Active.Map.GetLayersAsFlattenedList().OfType<RasterLayer>().FirstOrDefault();
                if (raster == null)
                    return;
                MessageBox.Show("In ArcGIS Pro 2.5 and older this method only works when using the 'value' field, unless you deploy the 'RecalculateColorizer' workaround.");
                await QueuedTask.Run(() =>
                {
                    string fieldName = "Value";
                    var style = Project.Current.GetItems<StyleProjectItem>().First(s => s.Name == "ArcGIS Colors");
                    var ramps = style.SearchColorRamps("Green Blues");
                    var colorizerDef = new UniqueValueColorizerDefinition(fieldName, ramps[0].ColorRamp);
                    var colorizer = raster.CreateColorizer(colorizerDef);
                    raster.SetColorizer(RecalculateColorizer(raster, colorizer, fieldName));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Error trying to set colorizer: {ex.ToString()}");
            }
        }

        private static CIMRasterColorizer RecalculateColorizer(RasterLayer rasterLayer, CIMRasterColorizer colorizer, string fieldName)
        {
            // fix up colorizer ... turns out the values are wrong ... the landuse value is always inserted
            // we use the Raster's attribute table to collect a dictionary with the correct replacement values
            Dictionary<string, string> landuseToFieldValue = new Dictionary<string, string>();
            if (colorizer is CIMRasterUniqueValueColorizer uvrColorizer)
            {
                var rasterTbl = rasterLayer.GetRaster().GetAttributeTable();
                var cursor = rasterTbl.Search();
                while (cursor.MoveNext())
                {
                    var row = cursor.Current;
                    var correctField = row[fieldName].ToString(); 
                    var key = row[uvrColorizer.Groups[0].Heading].ToString();
                    landuseToFieldValue.Add(key, correctField);
                }
                uvrColorizer.Groups[0].Heading = fieldName;
                for (var idxGrp = 0; idxGrp < uvrColorizer.Groups[0].Classes.Length; idxGrp++)
                {
                    var grpClass = uvrColorizer.Groups[0].Classes[idxGrp];
                    var oldValue = grpClass.Values[0];
                    var correctValue = landuseToFieldValue[oldValue];
                    grpClass.Values[0] = correctValue;
                    grpClass.Label = $@"{correctValue}";
                }
            }
            return colorizer;
        }

    }
}
