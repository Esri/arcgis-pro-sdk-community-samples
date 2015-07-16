//Copyright 2015 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;

namespace LayersPane.Extensions
{
    static class ExtendsLayer
    {

        /// <summary>
        /// Returns the feature class associated with layer.
        /// </summary>
        /// <param name="layer">The input layer.</param>
        /// <returns>The table or the feature class associated with the layer.</returns>
        public static async Task<Table> getFeatureClass(this Layer layer)
        {
            // get the feature class associated with the layer
            return await ArcGIS.Desktop.Internal.Editing.EditingModuleInternal.GetTableAsync(layer);
        }
    }
}
