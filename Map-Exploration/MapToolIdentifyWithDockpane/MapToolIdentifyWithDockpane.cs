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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace MapToolIdentifyWithDockpane
{
    internal class MapToolIdentifyWithDockpane : MapTool
    {
        public MapToolIdentifyWithDockpane()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            var mv = MapView.Active;
            Module1.MapToolIdentifyDockpaneVM.ClearListOfFeatures();
            await QueuedTask.Run(() =>
                {
                // Get the features that intersect the sketch geometry. 
                // getFeatures returns a dictionary of featurelayer and a list of Object ids for each
                Dictionary<BasicFeatureLayer, List<long>> featuresObjectIds = mv.GetFeatures(geometry);

                // go through all feature layers and do a spatial query to find features 
                foreach (var featOids in featuresObjectIds)
                {
                    var featLyr = featOids.Key;
                    var qf = new QueryFilter() { ObjectIDs = featOids.Value };
                    var rowCursor = featLyr.Search(qf);
                    while (rowCursor.MoveNext())
                    {
                        using (var feat = rowCursor.Current as Feature)
                        {
                            var listOID = new List<long> {feat.GetObjectID() }; 
                            var displayExp = String.Join(Environment.NewLine, featLyr.QueryDisplayExpressions(listOID.ToArray()));
                            Module1.MapToolIdentifyDockpaneVM.AddToListOfFeatures($@"Layer: {featLyr.Name} obj id: {feat.GetObjectID()} Display Expression: {displayExp}");
                            //Access all field values
                            var count = feat.GetFields().Count();
                            for (int i = 0; i < count; i++)
                            {
                                var val = feat[i];
                                //TODO use the value(s)
                            }
                            }
                        }                    
                    }
                });
            return true;
        }
    }
}
