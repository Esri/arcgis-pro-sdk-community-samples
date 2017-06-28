//Copyright 2015-2016 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;

namespace SketchToolDemo
{
    /// <summary>
    /// A sample sketch tool that uses the sketch line geometry to cut 
    /// underlying polygons.
    /// </summary>
    class CutTool : MapTool
    {
        public CutTool() : base()
        {
            // select the type of construction tool you wish to implement.  
            // Make sure that the tool is correctly registered with the correct component category type in the daml
            SketchType = SketchGeometryType.Line;
            // a sketch feedback is need
            IsSketchTool = true;
            // the geometry is needed in map coordinates
            SketchOutputMode = ArcGIS.Desktop.Mapping.SketchOutputMode.Map;
        }

        /// <summary>
        /// Called when the sketch finishes. This is where we will create the sketch 
        /// operation and then execute it.
        /// </summary>
        /// <param name="geometry">The geometry created by the sketch.</param>
        /// <returns>A Task returning a Boolean indicating if the sketch complete event 
        /// was successfully handled.</returns>
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return QueuedTask.Run(() => ExecuteCut(geometry));
        }

        /// <summary>
        /// Method to perform the cut operation on the geometry and change attributes
        /// </summary>
        /// <param name="geometry">Line geometry used to perform the cut against in the polygon features
        /// in the active map view.</param>
        /// <returns>If the cut operation was successful.</returns>
        protected Task<bool> ExecuteCut(Geometry geometry)
        {
            if (geometry == null)
                return Task.FromResult(false);

            // create a collection of feature layers that can be edited
            var editableLayers = ActiveMapView.Map.GetLayersAsFlattenedList()
                .OfType<FeatureLayer>()
                .Where(lyr => lyr.CanEditData() == true).Where(lyr =>
                lyr.ShapeType == esriGeometryType.esriGeometryPolygon);

            // ensure that there are target layers
            if (editableLayers.Count() == 0)
                return Task.FromResult(false);

            // create an edit operation
            EditOperation cutOperation = new EditOperation()
            {
                Name = "Cut Elements",
                ProgressMessage = "Working...",
                CancelMessage = "Operation canceled.",
                ErrorMessage = "Error cutting polygons",
                SelectModifiedFeatures = false,
                SelectNewFeatures = false
            };

            // initialize a list of ObjectIDs that need to be cut
            var cutOIDs = new List<long>();

            // for each of the layers 
            foreach (FeatureLayer editableFeatureLayer in editableLayers)
            {
                // find the features crossed by the sketch geometry
                var rowCursor = editableFeatureLayer.Search(geometry, SpatialRelationship.Crosses);

                // get the feature class associated with the layer
                Table fc = editableFeatureLayer.GetTable();

                // find the field index for the 'Description' attribute
                int descriptionIndex = -1;
                descriptionIndex = fc.GetDefinition().FindField("Description");

                // add the feature IDs into our prepared list
                while (rowCursor.MoveNext())
                {
                    var feature = rowCursor.Current as Feature;
                    var geomTest = feature.GetShape();
                    if (geomTest != null)
                    {
                        // make sure we have the same projection for geomProjected and geomTest
                        var geomProjected = GeometryEngine.Instance.Project(geometry, geomTest.SpatialReference);
                        // we are looking for polygons are completely intersected by the cut line
                        if (GeometryEngine.Instance.Relate(geomProjected, geomTest, "TT*F*****"))
                        {
                            // add the current feature to the overall list of features to cut
                            cutOIDs.Add(rowCursor.Current.GetObjectID());

                            // adjust the attribute before the cut
                            if (descriptionIndex != -1)
                                cutOperation.Modify(rowCursor.Current, descriptionIndex, "Pro Sample");
                        }
                    }
                }
                // add the elements to cut into the edit operation
                cutOperation.Cut(editableFeatureLayer, cutOIDs, geometry);
            }

            //execute the operation
            var operationResult = cutOperation.Execute();

            return Task.FromResult(operationResult);
        }

        /// <summary>
        /// Method to override the sketch symbol after collecting the second vertex
        /// </summary>
        /// <returns>If the sketch symbology was successfully changed.</returns>
        protected override async Task<bool> OnSketchModifiedAsync()
        {
            // retrieve the current sketch geometry
            Polyline cutGeometry = await base.GetCurrentSketchAsync() as Polyline;

            await QueuedTask.Run(() =>
            {
                // if there are more than 2 vertices in the geometry
                if (cutGeometry.PointCount > 2)
                {
                    // adjust the sketch symbol
                    var symbolReference = base.SketchSymbol;
                    if (symbolReference == null)
                    {
                        var cimLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.RedRGB, 3,
                            SimpleLineStyle.DashDotDot);
                        base.SketchSymbol = cimLineSymbol.MakeSymbolReference();
                    }
                    else
                    {
                        symbolReference.Symbol.SetColor(ColorFactory.Instance.RedRGB);
                        base.SketchSymbol = symbolReference;
                    }
                }
            });

            return true;
        }
    }

    /// <summary>
    /// Extension method to search and retrieve rows
    /// </summary>
    public static class LayerExtensions
    {
        /// <summary>
        /// Performs a spatial query against a feature layer.
        /// </summary>
        /// <remarks>It is assumed that the feature layer and the search geometry are using the same spatial reference.</remarks>
        /// <param name="searchLayer">The feature layer to be searched.</param>
        /// <param name="searchGeometry">The geometry used to perform the spatial query.</param>
        /// <param name="spatialRelationship">The spatial relationship used by the spatial filter.</param>
        /// <returns>Cursor containing the features that satisfy the spatial search criteria.</returns>
        public static RowCursor Search(this BasicFeatureLayer searchLayer, Geometry searchGeometry, SpatialRelationship spatialRelationship)
        {
            RowCursor rowCursor = null;

            // define a spatial query filter
            var spatialQueryFilter = new SpatialQueryFilter
            {
                // passing the search geometry to the spatial filter
                FilterGeometry = searchGeometry,
                // define the spatial relationship between search geometry and feature class
                SpatialRelationship = spatialRelationship
            };

            // apply the spatial filter to the feature layer in question
            rowCursor = searchLayer.Search(spatialQueryFilter);

            return rowCursor;
        }
    }
}