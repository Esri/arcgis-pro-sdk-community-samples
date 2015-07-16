//   Copyright 2015 Esri
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;

namespace FilterFeaturesBasedOnAttributesWithinAnExtent
{
    internal class Dockpane1ViewModel : DockPane
    {
        private const string _dockPaneID = "ProAppModule3_Dockpane1";

        protected Dockpane1ViewModel()
        {
            TOCSelectionChangedEvent.Subscribe(UpdateFields);

        }

        /// <summary>
        /// Make sure there is a feature layer selected
        /// Update the Fields Combobox with the Fields corresponding to the Layer Selected
        /// </summary>
        /// <param name="args"></param>
        private void UpdateFields(MapViewEventArgs args)
        {
            if (args.MapView.GetSelectedLayers().Count == 0) return;
            var selectedLayer = args.MapView.GetSelectedLayers()[0];
            if (selectedLayer is FeatureLayer)
            {
                featureLayer = selectedLayer as FeatureLayer;
                QueuedTask.Run(() =>
                {
                    using (var table = featureLayer.GetTable())
                    {
                        Fields = new ObservableCollection<string>(table.GetDefinition().GetFields().Select(field => field.Name));
                    }
                });
            }
        }


        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            
            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Highlight Features";

        private FeatureLayer featureLayer;
        private ObservableCollection<string> fields;
        private ObservableCollection<FeatureData> featureData;
        private string selectedField;
        private string fieldValue;

        public ObservableCollection<string> Fields
        {
            get { return fields; }
            set
            {
                fields = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("Fields"));
            }
        }

        public ObservableCollection<FeatureData> FeatureData
        {
            get { return featureData; }
            set
            {
                featureData = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("FeatureData"));
            }
        }

        public string SelectedField
        {
            get { return selectedField; }
            set
            {
                SetProperty(ref selectedField, value, () => SelectedField);
            }
        }

        public string FieldValue
        {
            get { return fieldValue; }
            set
            {
                SetProperty(ref fieldValue, value, () => FieldValue);
            }
        }

        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        /// <summary>
        /// Get the selected Feature Layer and the corresponding FeatureClass
        /// If there are any features selected for that layer, Zoom to the extent and use the extent for a Spatial Query
        /// If there are no feature selected, perform a normal query on all the features for that FeatureClass
        /// List the selected Object Ids in the datagrid by assigning them to FeatureData property (bound to the datagrid)
        /// </summary>
        public async void Work()
        {
            var selectedLayer = MapView.Active.GetSelectedLayers()[0];
            if (selectedLayer is FeatureLayer)
            {
                var featureLayer = selectedLayer as FeatureLayer;
                QueuedTask.Run(async () => {
                    using (var table = featureLayer.GetTable())
                    {
                        var whereClause = String.Format("{0} = '{1}'", SelectedField, FieldValue);
                        using (var mapSelection = featureLayer.GetSelection())
                        {
                            QueryFilter queryFilter;
                            if (mapSelection.GetCount() > 0)
                            {
                                Envelope envelope = null;
                                using (var cursor = mapSelection.Search())
                                {
                                    while (cursor.MoveNext())
                                    {
                                        using (var feature = cursor.Current as Feature)
                                        {
                                            if (envelope == null)
                                                envelope = feature.GetShape().Extent;
                                            else
                                                envelope = envelope.Union(feature.GetShape().Extent);
                                        }
                                    }
                                }
                                queryFilter = new SpatialQueryFilter
                                {
                                    FilterGeometry = new EnvelopeBuilder(envelope).ToGeometry(),
                                    SpatialRelationship = SpatialRelationship.Contains,
                                    WhereClause = whereClause
                                };
                            }
                            else
                            {
                                queryFilter = new QueryFilter {WhereClause = whereClause};
                            }
                            try
                            {
                                using (var selection = table.Select(queryFilter))
                                {
                                    var readOnlyList = selection.GetObjectIDs();
                                    FeatureData =
                                        new ObservableCollection<FeatureData>(
                                            readOnlyList.Select(
                                                objectId => new FeatureData {ObjectId = objectId.ToString()}));
                                    MapView.Active.Map.SetSelection(new Dictionary<MapMember, List<long>>
                                    {
                                        {featureLayer, new List<long>(readOnlyList)}
                                    });
                                }

                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                });
                
            }
        }
    }

    internal class FeatureData
    {
        public string ObjectId { get; set; }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Dockpane1_ShowButton : Button
    {
        protected override void OnClick()
        {
            Dockpane1ViewModel.Show();
        }
    }
}
