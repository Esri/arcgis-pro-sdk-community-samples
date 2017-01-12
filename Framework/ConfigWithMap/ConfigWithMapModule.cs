/*

   Copyright 2017 Esri

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ConfigWithMap
{
    /// <summary>
    /// “Managed Configurations” allow branding of ArcGIS Pro meaning you can customize the splash and startup screens, application icon, and modify the runtime ArcGIS Pro User Interface to best fit your user’s business needs.  This sample illustrates a configuration solution that includes those features.  
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a folder called 'C:\Data\Configurations\Projects' with sample data required for this solution.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Configurations\Projects" is available.
    /// 1. In Visual Studio 2015 click the Build menu. Then select Build Solution.
    /// 1. Click Start button to debug ArcGIS Pro.
    /// 1. ArcGIS Pro displays the custom splash screen.
    /// ![UI](Screenshots/ManagedConfigSplash.png)
    /// 1. Pro will then display the startup screen showing a map, click on the county of San Diego area on the map.
    /// ![UI](Screenshots/ManagedConfigStartup.png)
    /// 1. Pro will then open the San Diego project specifically tuned for this sample workflow.
    /// 1. Click on the 'Select Power Line Support' button to start a geoprocessing task that find all power line support structure further than 50 meters from the service road.
    /// 1. Click the 'Show Sites' button to show the geo processing task results.
    /// 1. Click the 'Edit' button to display the 'Edit' tab which contains existing ArcGIS Pro edit functionality.  
    /// ![UI](Screenshots/ManagedConfigRunning.png)
    /// </remarks>
    internal class ConfigWithMapModule : Module
    {
        private static ConfigWithMapModule _this;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static ConfigWithMapModule Current => _this ?? (_this = (ConfigWithMapModule)FrameworkApplication.FindModule("ConfigWithMapModule"));

        public ConfigWithMapModule()
        {
            _this = this;
        }
        
        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

        #region Private Properties

        private FeatureLayer _featureLayer;
        private ObservableCollection<FeatureRepresentation> _features = new ObservableCollection<FeatureRepresentation>();
        private NetworkDatasetLayer _networkDatasetLayer;
        private Map _outputMap;

        #endregion Private Properties

        #region Internal Properties and Methods

        internal static string UserName { get; set; }

        internal ObservableCollection<FeatureRepresentation> Features => _features;

        public string FeatureLayerName => _featureLayer == null ? string.Empty : _featureLayer.Name;

        internal async void SelectByLocation()
        {
            var mapView = MapView.Active;

            Layer towers = null;
            Layer roads = null;
            foreach (var layer in mapView.Map.Layers)
            {
                if (layer.Name == "Acme Transmission Structures")
                    towers = layer;
                if (layer.Name == "Acme Service Roads")
                    roads = layer;

                if (towers != null && roads != null)
                    break;
            }

            if (towers == null || roads == null)
            {
                MessageBox.Show("Could not find one or more input layers.");
                return;
            }

            // Show a progress dialog while GP operation is going
            var progressDialog = new ProgressDialog("Selecting structures > 50 meters from route.");
            progressDialog.Show();

            _featureLayer = towers as FeatureLayer;

            var list = new List<FeatureRepresentation>();

            //await QueuedTask.Run(() =>
            //{

            //var values = ArcGIS.Desktop.Core.Geoprocessing.Geoprocessing.MakeValueArray(towers, "WITHIN_A_DISTANCE", roads, 50, "NEW_SELECTION", "INVERT");
            string[] values = { "Acme Transmission Structures", "WITHIN_A_DISTANCE", "Acme Service Roads", "50", "NEW_SELECTION", "INVERT" };
            await Geoprocessing.ExecuteToolAsync("SelectLayerByLocation_management", values);

            await QueuedTask.Run(() =>
            {

                FeatureClass featureClass = _featureLayer.GetFeatureClass();

                IReadOnlyList<Field> fields = featureClass.GetDefinition().GetFields();
                if (fields == null)
                    return;

                // mapView.ZoomToSelected();

                var selectedFeatures = mapView.Map.GetSelection();

                var inspector = new Inspector(false);

                foreach (var kvEntry in selectedFeatures)
                {
                    foreach (var id in kvEntry.Value)
                    {
                        var featureRep = new FeatureRepresentation { Id = id };

                        inspector.Load(kvEntry.Key, id);
                        var attr = inspector["Linename"];
                        if (attr != null)
                            featureRep.Value = attr.ToString();

                        list.Add(featureRep);
                    }
                }
            });

            // Back on UI thread
            foreach (var featureRep in list)
            {
                _features.Add(featureRep);
            }

            progressDialog.Hide();
        }

        internal void ZoomToFeature(long oid)
        {
            if (oid == -1)
                return;

            var mapView = MapView.Active;

            QueuedTask.Run(() =>
            {
                mapView.ZoomTo(_featureLayer, oid);
                mapView.FlashFeature(_featureLayer, oid);
            });
        }

        internal async void BuildNetwork()
        {
            var mapView = MapView.Active;

            if (_networkDatasetLayer == null)
            {
                var layers = mapView.Map.GetLayersAsFlattenedList();
                foreach (var layer in layers)
                {
                    if (!(layer is NetworkDatasetLayer)) continue;
                    _networkDatasetLayer = layer as NetworkDatasetLayer;
                    break;
                }
            }

            if (_networkDatasetLayer == null)
                return;

            var values = Geoprocessing.MakeValueArray(_networkDatasetLayer);

            await Geoprocessing.ExecuteToolAsync("BuildNetwork_na", values);

        }

        internal async void CreatePackage()
        {

            if (_outputMap == null)
            {
                var maps = Project.Current.GetItems<MapProjectItem>();
                foreach (var mapProjectItem in maps)
                {
                    if (!mapProjectItem.Name.Contains("Map")) continue;
                    await QueuedTask.Run(() =>
                    {
                        _outputMap = mapProjectItem.GetMap();
                    });
                    break;
                }
            }

            if (_outputMap == null)
                return;

            string[] values = { "Map", @"c:\temp\MobileMapPackage.mmpk", "", "", "", "", "", "", "Description" };
            var gpResult = await Geoprocessing.ExecuteToolAsync("CreateMobileMapPackage_management", values);
                Geoprocessing.ShowMessageBox(gpResult.Messages, "GP Messages",
                gpResult.IsFailed ? GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
            //  ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Mobile map package created.","Geoprocessing Complete");

        }

        #endregion Internal Properties and Methods

    }

    internal class FeatureRepresentation
    {
        public long Id { get; set; }
        public string Value { get; set; }
    }
}