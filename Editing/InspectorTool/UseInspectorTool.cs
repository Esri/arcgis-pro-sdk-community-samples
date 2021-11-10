// Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace InspectorTool
{
    class UseInspectorTool : MapTool
    {
        private AttributeControlViewModel _attributeVM = null;
        private DelayedInvoker _invoker = new DelayedInvoker(10);

        public UseInspectorTool() : base()
        {
            // indicate that you need feedback graphics
            IsSketchTool = true;
            // set the type of the feedback graphics
            SketchType = SketchGeometryType.Rectangle;
            // the coordinates of the sketch geometry should be returned in map coordinates
            SketchOutputMode = SketchOutputMode.Map;

            // specify the ID for the embeddable control as declared in the config.daml
            // and specified in the AttributeControl.xaml for the UI and AttributeControlViewModel for the 
            // view model.
            ControlID = "InspectorTool_AttributeControl";
        }

        /// <summary>
        /// Prepares the content for the embeddable control when the tool is activated.
        /// </summary>
        /// <param name="hasMapViewChanged"></param>
        /// <returns></returns>
        protected override Task OnToolActivateAsync(bool hasMapViewChanged)
        {
            // if reference to the view model exists, create one
            if (_attributeVM == null)
            {
                _attributeVM = this.EmbeddableControl as AttributeControlViewModel;
            }

            // initiate the following code to execute on the MCT
            QueuedTask.Run(() =>
            {
                // get the current selection for the active map
                var selectionInMap = ActiveMapView.Map.GetSelection();

                // get the first point layer from the map
                var pointLayer = ActiveMapView.Map.GetLayersAsFlattenedList().
                OfType<FeatureLayer>().
                Where(lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint).
                FirstOrDefault();

                if (pointLayer != null)
                {
                    // if the point layer contains selected features
                    if (selectionInMap.ContainsKey(pointLayer))
                    {
                        // get a list of the selected point features 
                        var selectionDictionary = new Dictionary<MapMember, List<long>>();
                        selectionDictionary.Add(pointLayer as MapMember, selectionInMap[pointLayer]);
                        // and store it in the view model, this property populates the tree view
                        _attributeVM.SelectedMapFeatures = selectionDictionary;

                        // load the first selected point feature
                        _attributeVM.AttributeInspector.Load(pointLayer, selectionInMap[pointLayer][0]);
                    }

                    // subscribe to the PropertyChanged event for the string attributes
                    foreach (ArcGIS.Desktop.Editing.Attributes.Attribute featureAttribute in _attributeVM.AttributeInspector)
                    {
                        if (featureAttribute.FieldType == FieldType.String)
                        {
                           featureAttribute.PropertyChanged += FeatureAttributeChanged;
                        }
                    }
                }
            });

            return Task.FromResult(true);
        }

        /// <summary>
        /// Delegate for the PropertyChanged event.
        /// </summary>
        /// <param name="sender">Attribute who initiated the change.</param>
        /// <param name="e">Provides data for the PropertyChanged event.</param>
        private async void FeatureAttributeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // cast the sender to the Attribute class
            var attribute = sender as ArcGIS.Desktop.Editing.Attributes.Attribute;

            // check if the attribute is valid
            if (attribute.IsValid)
            {
                // if the view model has an inspector instance and the attribute value contains a string
                // the apply the changed value which results in an executed edit operation 
                if (_attributeVM.AttributeInspector != null && !String.IsNullOrEmpty(attribute.CurrentValue.ToString()))
                    await _attributeVM.AttributeInspector.ApplyAsync();
            }
        }

        /// <summary>
        /// Clears resources when the tool is deactivated.
        /// </summary>
        /// <param name="hasMapViewChanged"></param>
        /// <returns></returns>
        protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
        {
            // if we have a valid view model
            if (_attributeVM != null)
            {
                // if there is a valid instance for the attribute inspector
                // unsubscribe from the events for the string attributes
                if (_attributeVM.AttributeInspector != null)
                {
                    foreach (ArcGIS.Desktop.Editing.Attributes.Attribute featureAttribute in _attributeVM.AttributeInspector)
                    {
                        if (featureAttribute.FieldType == FieldType.String)
                        {
                            featureAttribute.PropertyChanged -= FeatureAttributeChanged;
                        }
                    }
                }

                // free the embeddable control resources
                _attributeVM.InspectorView = null;
                _attributeVM.InspectorViewModel.Dispose();

                _attributeVM = null;
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// Occurs once the user completed the sketch and select the information for the
        /// embeddable control
        /// </summary>
        /// <param name="geometry">Sketch geometry in map coordinates.</param>
        /// <returns></returns>
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            // select the first point feature layer in the active map
            var pointLayer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().
                Where(lyr => lyr.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint).FirstOrDefault();

            if (pointLayer == null)
                return Task.FromResult(true);

            // execute the select on the MCT
            QueuedTask.Run(() =>
            {
                // define the spatial query filter
                var spatialQuery = new SpatialQueryFilter() { FilterGeometry = geometry, SpatialRelationship = SpatialRelationship.Contains };

                // gather the selection
                var pointSelection = pointLayer.Select(spatialQuery);

                List<long> oids = pointSelection.GetObjectIDs().ToList();
                if (oids.Count == 0)
                  return;

                // set up a dictionary to store the layer and the object IDs of the selected features
                var selectionDictionary = new Dictionary<MapMember, List<long>>();
                selectionDictionary.Add(pointLayer as MapMember, pointSelection.GetObjectIDs().ToList());

                // assign the dictionary to the view model
                _attributeVM.SelectedMapFeatures = selectionDictionary;
                // load the first feature into the attribute inspector
                _attributeVM.AttributeInspector.Load(pointLayer, pointSelection.GetObjectIDs().First());
            });

            return Task.FromResult(true);
        }
    }
}
