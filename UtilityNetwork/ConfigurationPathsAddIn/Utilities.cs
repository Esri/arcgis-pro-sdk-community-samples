using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConfigurationPathsAddIn
{
    //   Copyright 2019 Esri
    //   Licensed under the Apache License, Version 2.0 (the "License");
    //   you may not use this file except in compliance with the License.
    //   You may obtain a copy of the License at

    //       https://www.apache.org/licenses/LICENSE-2.0

    //   Unless required by applicable law or agreed to in writing, software
    //   distributed under the License is distributed on an "AS IS" BASIS,
    //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    //   See the License for the specific language governing permissions and
    //   limitations under the License.

    /// <summary>
    /// This class contains generic functions that are used by the 
    /// ConfigurationPathTool class.
    /// </summary>
    internal class Utilities
    {        
        internal static bool IsDesiredShapeType(FeatureClass featureClass, GeometryType desiredShapeType)
        {
            using (FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition())
            {
                return featureClassDefinition.GetShapeType() == desiredShapeType;
            }
        }

        // Retrieve the utility network from a feature class
        internal static UtilityNetwork GetUtilityNetwork(FeatureClass featureClass)
        {
            IReadOnlyList<Dataset> controllerDatasets = featureClass.GetControllerDatasets();

            bool foundUtilityNetwork = false;

            UtilityNetwork utilityNetwork = null;

            // Make sure to dispose all non-utility network controller datasets.

            foreach (Dataset dataset in controllerDatasets)
            {
                if (!foundUtilityNetwork && dataset.Type == DatasetType.UtilityNetwork && dataset is UtilityNetwork)
                {
                    foundUtilityNetwork = true;
                    utilityNetwork = dataset as UtilityNetwork;
                }
                else
                {
                    dataset.Dispose();
                }
            }

            return utilityNetwork;
        }
    }
}
