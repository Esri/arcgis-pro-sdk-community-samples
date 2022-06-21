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
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace MultipatchBuilder
{
    internal class InspectMultiPatch : Button
    {
        protected override void OnClick()
        {
            // find footprint layer
            var buildingLyr = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault(l => l.Name == "BuildingStructure") as FeatureLayer;
            if (buildingLyr == null)
            {
                MessageBox.Show("Can't find layer: BuildingStructure");
                return;
            }

            // create new structural features
            _ = QueuedTask.Run(() =>
            {
                // get all selected lines and use them as the building footprint
                var bldgSelection = buildingLyr.GetSelection();
                Multipatch mp = null;
                #region Get MultiPatch
                foreach (var footprintOid in bldgSelection.GetObjectIDs())
                {
                    // get the multipatch shape using the Inspector
                    var insp = new Inspector();
                    insp.Load(buildingLyr, footprintOid);
                    mp = insp.Shape.Clone() as Multipatch;
                    break;
                }
                #endregion
                if (mp == null)
                {
                    MessageBox.Show("No multipatch selected");
                    return;
                }

                // create a builder
                var mpb = new ArcGIS.Core.Geometry.MultipatchBuilderEx(mp);

                // apply the texture materials to the patches
                var patches = mpb.Patches;
                foreach (var patch in patches)
                {
                    System.Diagnostics.Debug.WriteLine(patch.Material);
                    MyMultipatchBuilder.ShowPatchLabels(patch);
                    foreach (var coord in patch.TextureCoords2D)
                    {
                        System.Diagnostics.Debug.WriteLine($@"new Coordinate2D({coord.X},{coord.Y}),");
                    }
                    break;
                }
            });
        }
    }
}