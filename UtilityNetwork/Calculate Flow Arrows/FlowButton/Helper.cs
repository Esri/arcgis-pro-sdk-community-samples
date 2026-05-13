/*

   Copyright 2026 Esri

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
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Mapping;

namespace Calculate_Flow_Arrows.FlowButton
{
    internal class Helper
    {
        internal static UtilityNetwork GetFirstUtilityNetworkFromMap(Map activeMap)
        {
            foreach (var layer in activeMap.GetLayersAsFlattenedList())
            {
                // This doesn't take into account non-spatial objects
                var featureLayer = layer as FeatureLayer;
                if (featureLayer == null)
                    continue;

                var featureClass = featureLayer.GetFeatureClass();
                if (featureClass == null)
                    continue;

                if (!featureClass.IsControllerDatasetSupported())
                    continue;

                UtilityNetwork thisNetwork = null;
                var controllerDatasets = featureClass.GetControllerDatasets();
                foreach (Dataset controllerDataset in controllerDatasets)
                {
                    thisNetwork = controllerDataset as UtilityNetwork;
                    if (thisNetwork != null)
                        return thisNetwork;
                }
            }

            return null;
        }
    }
}
