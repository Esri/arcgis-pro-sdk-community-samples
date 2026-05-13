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
using ArcGIS.Core.Data.Exceptions;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Calculate_Flow_Arrows.FlowButton;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Calculate_Flow_Arrows
{
    internal class DockpaneComboViewModel : DockPane
    {

        #region Members

        private const string _dockPaneID = "CalculateFlowArrows_DockpaneCombo";

        // Create a dictionary by all state names with the capitals as the value
        // This is used to populate the ComboBox in the DockPane
        private readonly List<string> _tierList = new List<string>();
        private readonly List<string> _subnetworkList = new List<string>();
        private UtilityNetwork _utilityNetwork = null;

        // By default, limit the number of subnetworks shown in the dropdown for performance reasons
        private int _maxSubnetworks = 500000;

        #endregion


        #region Properties

        public ObservableCollection<string> Tiers
        {
            get { return [.. _tierList]; }
        }

        public ObservableCollection<string> Subnetworks
        {
            get { return [.. _subnetworkList]; }
        }

        private string _selectedTier;
        public string Tier
        {
            get { return _selectedTier; }
            set
            {
                SetProperty(ref _selectedTier, value, () => Tier);

                Subnetwork = string.Empty;
                NotifyPropertyChanged("Subnetwork");

                if (string.IsNullOrEmpty(_selectedTier)) return;
                if (!_tierList.Contains(_selectedTier)) return;

                QueuedTask.Run(() =>
                {
                    using var utilityNetworkDefinition = _utilityNetwork.GetDefinition();
                    Tier selectedTier = null;
                    foreach (var domainNetwork in utilityNetworkDefinition.GetDomainNetworks())
                        foreach (var tier in domainNetwork.Tiers)
                            if (tier.Name.Equals(_selectedTier, StringComparison.InvariantCultureIgnoreCase))
                                selectedTier = tier;

                    Status = "Loading subnetworks";

                    var subnetworkManager = _utilityNetwork.GetSubnetworkManager();
                    SubnetworkStates states = _includeDirtySubnetworks
                        ? SubnetworkStates.Dirty | SubnetworkStates.Clean
                        : SubnetworkStates.Clean;
                    IEnumerable<Subnetwork> subnetworks = subnetworkManager.GetSubnetworks(selectedTier, states)
                        .OrderBy(tier => tier.Name);

                    _subnetworkList.Clear();
                    var subnetworkCount = subnetworks.Count();
                    if (subnetworkCount == 0)
                        Status = _includeDirtySubnetworks
                        ? "No subnetworks for the selected tier."
                        : "No clean subnetworks for the selected tier.";
                    else if (subnetworkCount > _maxSubnetworks)
                    {
                        foreach (var subnetwork in subnetworks.Take(_maxSubnetworks))
                            _subnetworkList.Add(subnetwork.Name);

                        Status = "Select a subnetwork (first " + _maxSubnetworks + " subnetworks displayed).";
                    }
                    else
                    {
                        foreach (var subnetwork in subnetworks)
                            _subnetworkList.Add(subnetwork.Name);
                        Status = "Select a subnetwork.";
                    }
                    NotifyPropertyChanged("Subnetworks");
                });
            }
        }

        private string _selectedSubnetwork;
        public string Subnetwork
        {
            get { return _selectedSubnetwork; }
            set
            {
                SetProperty(ref _selectedSubnetwork, value, () => Subnetwork);
                Status = "Ready";

                if (!_applyFilter) return;
                if (_utilityNetwork == null) return;
                if (string.IsNullOrEmpty(_selectedTier)) return;
                if (!_tierList.Contains(_selectedTier)) return;
                if (string.IsNullOrEmpty(_selectedSubnetwork)) return;

                QueuedTask.Run(() =>
                {
                    try
                    {
                        using var utilityNetworkDefinition = _utilityNetwork.GetDefinition();
                        Tier selectedTier = null;
                        foreach (var domainNetwork in utilityNetworkDefinition.GetDomainNetworks())
                            foreach (var tier in domainNetwork.Tiers)
                                if (tier.Name.Equals(_selectedTier, StringComparison.InvariantCultureIgnoreCase))
                                    selectedTier = tier;

                        if (selectedTier == null)
                        {
                            _messages.Add("Unable to load selected tier.");
                            NotifyPropertyChanged("Messages");
                            return;
                        }

                        using var subnetworkManager = _utilityNetwork.GetSubnetworkManager();
                        var selectedSubnetwork = subnetworkManager.GetSubnetwork(_selectedSubnetwork);
                        if (selectedTier == null)
                        {
                            _messages.Add("Unable to load selected subnetwork.");
                            NotifyPropertyChanged("Messages");
                            return;
                        }

                        var activeProject = ArcGIS.Desktop.Core.Project.Current;
                        var projectGeodatabase = activeProject.DefaultGeodatabasePath;
                        var outputClass = SubnetworkParser.GetOutputClass(projectGeodatabase);
                        if (outputClass == null)
                            return;

                        var activeMapView = MapView.Active;
                        var activeMap = activeMapView.Map;
                        var outputFeatureLayer = activeMap.GetLayersAsFlattenedList()
                            .OfType<FeatureLayer>()
                            .FirstOrDefault(featureLayer => featureLayer.GetFeatureClass().GetPath() == outputClass.GetPath());
                        if (outputFeatureLayer == null)
                            return;

                        var definitionQueries = outputFeatureLayer.DefinitionQueries;
                        if (!definitionQueries.Any(definitionQuery => definitionQuery.Name.Equals(selectedSubnetwork.Name, StringComparison.InvariantCultureIgnoreCase)))
                            return;

                        outputFeatureLayer.SetActiveDefinitionQuery(selectedSubnetwork.Name);
                        activeMapView.ZoomTo(outputFeatureLayer);

                        activeMapView.RedrawAsync(false);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                });
            }
        }

        private bool _applyFilter = true;
        public bool ApplyFilter
        {
            get { return _applyFilter; }
            set
            {
                SetProperty(ref _applyFilter, value, () => ApplyFilter);
            }
        }

        private bool _clearResults = false;
        public bool ClearResults
        {
            get { return _clearResults; }
            set
            {
                SetProperty(ref _clearResults, value, () => ClearResults);
            }
        }

        // Set this to True to make the analyze tiers command visible
        private bool _showAnalyzeTier = false;
        public bool ShowAnalyzeTier
        {
            get { return _showAnalyzeTier; }
            set
            {
                SetProperty(ref _showAnalyzeTier, value, () => ShowAnalyzeTier);
            }
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Calculate Flow Arrows";
        public string Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value);
        }

        /// <summary>
        /// Messages/warnings during processing.
        /// </summary>
        private readonly List<string> _messages = new List<string>();
        public string Messages
        {
            get => string.Join(Environment.NewLine, _messages);
            set { return; }
        }

        /// <summary>
        /// Text shown near the bottom of the DockPane.
        /// </summary>
        private string _status = "Ready";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private bool _running = false;
        public bool Running
        {
            get => _running;
        }

        private bool _includeDirtySubnetworks = false;
        public bool IncludeDirtySubnetworks
        {
            get => _includeDirtySubnetworks;
            set
            {
                SetProperty(ref _includeDirtySubnetworks, value, () => IncludeDirtySubnetworks);

                if (string.IsNullOrEmpty(_selectedTier)) return;
                if (!_tierList.Contains(_selectedTier)) return;

                QueuedTask.Run(() =>
                {
                    using var utilityNetworkDefinition = _utilityNetwork.GetDefinition();
                    Tier selectedTier = null;
                    foreach (var domainNetwork in utilityNetworkDefinition.GetDomainNetworks())
                        foreach (var tier in domainNetwork.Tiers)
                            if (tier.Name.Equals(_selectedTier, StringComparison.InvariantCultureIgnoreCase))
                                selectedTier = tier;

                    Status = "Loading subnetworks";

                    var subnetworkManager = _utilityNetwork.GetSubnetworkManager();
                    SubnetworkStates states = _includeDirtySubnetworks
                        ? SubnetworkStates.Dirty | SubnetworkStates.Clean
                        : SubnetworkStates.Clean;

                    IEnumerable<Subnetwork> allSubnetworks = subnetworkManager.GetSubnetworks(selectedTier, states)
                        .OrderBy(tier => tier.Name);

                    _subnetworkList.Clear();
                    var subnetworkCount = allSubnetworks.Count();
                    if (subnetworkCount == 0)
                        Status = _includeDirtySubnetworks
                        ? "No subnetworks for the selected tier."
                        : "No clean subnetworks for the selected tier.";
                    else if (subnetworkCount > _maxSubnetworks)
                    {
                        foreach (var subnetwork in allSubnetworks.Take(_maxSubnetworks))
                            _subnetworkList.Add(subnetwork.Name);

                        Status = "Select a subnetwork (first 100 subnetworks displayed).";
                    }
                    else
                    {
                        foreach (var subnetwork in allSubnetworks)
                            _subnetworkList.Add(subnetwork.Name);
                        Status = "Select a subnetwork.";
                    }
                    NotifyPropertyChanged("Subnetworks");

                    // Check to see if we need to clear the subnetwork box
                    if (!_subnetworkList.Contains(_selectedSubnetwork))
                    {
                        Subnetwork = string.Empty;
                        NotifyPropertyChanged("Subnetwork");
                    }
                });
            }
        }

        #endregion Properties


        #region Methods

        protected DockpaneComboViewModel() { }

        protected override void OnActivate(bool isActive)
        {
            base.OnActivate(isActive);

            try
            {
                var activeMapView = MapView.Active;
                if (activeMapView == null)
                {
                    _utilityNetwork = null;
                    if (_tierList.Count > 0)
                    {
                        _tierList.Clear();
                        NotifyPropertyChanged("Tiers");
                    }
                    if (_subnetworkList.Count > 0)
                    {
                        _subnetworkList.Clear();
                        NotifyPropertyChanged("Subnetworks");
                    }
                    if (!string.IsNullOrEmpty(Tier)) Tier = null;
                    if (!string.IsNullOrEmpty(Subnetwork)) Subnetwork = null;
                    Status = "No active map";
                    return;
                }

                var activeMap = activeMapView.Map;
                if (activeMap == null)
                {
                    _utilityNetwork = null;
                    if (_tierList.Count > 0)
                    {
                        _tierList.Clear();
                        NotifyPropertyChanged("Tiers");
                    }
                    if (_subnetworkList.Count > 0)
                    {
                        _subnetworkList.Clear();
                        NotifyPropertyChanged("Subnetworks");
                    }
                    if (!string.IsNullOrEmpty(Tier)) Tier = null;
                    if (!string.IsNullOrEmpty(Subnetwork)) Subnetwork = null;
                    Status = "No active map";
                    return;
                }

                var queuedTask = QueuedTask.Run(() =>
                {
                    var utilityNetwork = Helper.GetFirstUtilityNetworkFromMap(activeMap);
                    if (utilityNetwork == null)
                    {
                        _utilityNetwork = null;
                        if (_tierList.Count > 0)
                        {
                            _tierList.Clear();
                            NotifyPropertyChanged("Tiers");
                        }
                        if (_subnetworkList.Count > 0)
                        {
                            _subnetworkList.Clear();
                            NotifyPropertyChanged("Subnetworks");
                        }
                        if (!string.IsNullOrEmpty(Tier)) Tier = null;
                        if (!string.IsNullOrEmpty(Subnetwork)) Subnetwork = null;
                        Status = "No utility network in current map";
                        return;
                    }

                    _utilityNetwork = utilityNetwork;

                    // We present all tiers, regardless of their update subnetwork policy.
                    using var utilityNetworkDefinition = _utilityNetwork.GetDefinition();
                    IEnumerable<Tier> discoveredTiers = utilityNetworkDefinition.GetDomainNetworks()
                        .SelectMany(domainNetwork => domainNetwork.Tiers.Select(tier => tier))
                        .OrderBy(tier=>tier.Name);
                    
                    _tierList.Clear();
                    foreach(var tier in discoveredTiers)
                        _tierList.Add(tier.Name);
                    NotifyPropertyChanged("Tiers");

                    Status = "Select a tier";
                });
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
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

        #endregion

        #region Commands

        public ICommand CmdAnalyzeSubnetwork
        {
            get
            {
                return new RelayCommand((cmdParams) =>
                {
                    if (string.IsNullOrEmpty(_selectedTier)) return;
                    if (!_tierList.Contains(_selectedTier)) return;
                    if (string.IsNullOrEmpty(_selectedSubnetwork)) return;

                    QueuedTask.Run(() =>
                    {
                        try
                        {
                            _running = true;
                            NotifyPropertyChanged("Running");
                            _messages.Clear();
                            NotifyPropertyChanged("Messages");
                            Status = "Reading subnetwork";

                            using var utilityNetworkDefinition = _utilityNetwork.GetDefinition();
                            Tier selectedTier = null;
                            foreach (var domainNetwork in utilityNetworkDefinition.GetDomainNetworks())
                                foreach (var tier in domainNetwork.Tiers)
                                    if (tier.Name.Equals(_selectedTier, StringComparison.InvariantCultureIgnoreCase))
                                        selectedTier = tier;

                            if (selectedTier == null)
                            {
                                _messages.Add("Unable to load selected tier.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            using var subnetworkManager = _utilityNetwork.GetSubnetworkManager();
                            var selectedSubnetwork = subnetworkManager.GetSubnetwork(_selectedSubnetwork);
                            if (selectedTier == null)
                            {
                                _messages.Add("Unable to load selected subnetwork.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            var subnetworkControllers = selectedSubnetwork.GetControllers().Where(controller => controller.Element.ObjectID != -1);
                            if (subnetworkControllers.Count() > 1)
                            {
                                _messages.Add("Flow arrows may not be accurate when there are multiple subnetwork controllers.");
                                NotifyPropertyChanged("Messages");
                            }

                            var statusInfo = TraceHelper.GetNetworkStatusAttribute(selectedTier);
                            if (string.IsNullOrEmpty(statusInfo.fieldName))
                            {
                                _messages.Add("Unable to find a Status attribute to represent open/closed.");
                                NotifyPropertyChanged("Messages");
                            }

                            var subnetworkName = selectedSubnetwork.Name;
                            var activeProject = ArcGIS.Desktop.Core.Project.Current;
                            var homeFolderPath = activeProject.HomeFolderPath;
                            var exportFile = string.Format("{0}\\{1}.json", homeFolderPath, subnetworkName);
                            if (subnetworkName.Contains("/"))
                                exportFile.Replace("/", "_");

                            int featureCount = 0;
                            FeatureClass outputClass = null;

                            try
                            {

                                if (_includeDirtySubnetworks)
                                {
                                    // For consistency's sake, always use this method when in this mode

                                    #region Analyze a dirty subnetwork

                                    _messages.Add("Exporting subnetwork: " + subnetworkName);
                                    NotifyPropertyChanged("Messages");

                                    if (!TraceHelper.ExportDirtySubnetwork(_utilityNetwork, selectedSubnetwork, exportFile, [statusInfo.fieldName]))
                                    {
                                        _messages.Add("Unable to read connectivity information from subnetwork.");
                                        NotifyPropertyChanged("Messages");
                                        return;
                                    }

                                    Status = "Analyzing flow";
                                    _messages.Add("Analyzing subnetwork: " + subnetworkName);
                                    NotifyPropertyChanged("Messages");

                                    var subnetworkParser = new SubnetworkParser(utilityNetworkDefinition, selectedTier);

                                    // The Trace response doesn't include a subnetwork controllers element
                                    // This code will create subnetwork controllers for analysis using the features from the subnetwork definition
                                    var subnetworkControllerElements = subnetworkControllers.Select(controller => controller.Element);
                                    var startingElements = subnetworkParser.GetStartingElementKeys(subnetworkControllerElements).ToArray();

                                    // The Trace response doesn't include a spatial reference
                                    // Use the spatial reference from the structure junction class
                                    using var networkSource = utilityNetworkDefinition.GetNetworkSources().First(source => source.UsageType == SourceUsageType.StructureJunction);
                                    using var structureJunctionClass = (FeatureClass)_utilityNetwork.GetTable(networkSource);
                                    using var structureJunctionClassDefinition = structureJunctionClass.GetDefinition();
                                    var spatialReference = structureJunctionClassDefinition.GetSpatialReference();

                                    featureCount = subnetworkParser.ParseSubnetwork(subnetworkName, exportFile, statusInfo.fieldName, statusInfo.statusValue, _clearResults, startingElements);
                                    if (featureCount == 0)
                                    {
                                        _messages.Add("Subnetwork contains no connectivity/features: " + subnetworkName);
                                        NotifyPropertyChanged("Messages");
                                        return;
                                    }

                                    outputClass = subnetworkParser.OutputGeometry(spatialReference, subnetworkName, deleteAllRows: _clearResults);

                                    #endregion

                                }
                                else
                                {

                                    #region Analyze a clean subnetwork

                                    _messages.Add("Exporting subnetwork: " + subnetworkName);
                                    NotifyPropertyChanged("Messages");

                                    if (!TraceHelper.ExportSubnetwork(_utilityNetwork, selectedSubnetwork, exportFile, [statusInfo.fieldName]))
                                    {
                                        _messages.Add("Unable to read connectivity information from subnetwork.");
                                        NotifyPropertyChanged("Messages");
                                        return;
                                    }

                                    Status = "Analyzing flow";
                                    _messages.Add("Analyzing subnetwork: " + subnetworkName);
                                    NotifyPropertyChanged("Messages");

                                    // The Trace response doesn't include a spatial reference
                                    // Use the spatial reference from the structure junction class
                                    using var networkSource = utilityNetworkDefinition.GetNetworkSources().First(source => source.UsageType == SourceUsageType.StructureJunction);
                                    using var structureJunctionClass = (FeatureClass)_utilityNetwork.GetTable(networkSource);
                                    using var structureJunctionClassDefinition = structureJunctionClass.GetDefinition();
                                    var spatialReference = structureJunctionClassDefinition.GetSpatialReference();

                                    var subnetworkParser = new SubnetworkParser(utilityNetworkDefinition, selectedTier);
                                    featureCount = subnetworkParser.ParseSubnetwork(subnetworkName, exportFile, statusInfo.fieldName, statusInfo.statusValue, _clearResults);
                                    if (featureCount == 0)
                                    {
                                        _messages.Add("Subnetwork contains no connectivity/features: " + subnetworkName);
                                        NotifyPropertyChanged("Messages");
                                        return;
                                    }

                                    outputClass = subnetworkParser.OutputGeometry(spatialReference, subnetworkName, deleteAllRows: _clearResults);

                                    #endregion

                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                                _messages.Add(ex.Message);
                                NotifyPropertyChanged("Messages");
                                return;
                            }
                            finally
                            {
                                if (Path.Exists(exportFile))
                                    File.Delete(exportFile);
                            }

                            if (outputClass == null)
                            {
                                _messages.Add("Unable to analyze flow for subnetwork.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            #region Update the map

                            _messages.Add("Analysis complete.");
                            NotifyPropertyChanged("Messages");

                            var activeMapView = MapView.Active;
                            var activeMap = activeMapView.Map;
                            var outputFeatureLayer = activeMap.GetLayersAsFlattenedList()
                                .OfType<FeatureLayer>()
                                .Where(featureLayer => featureLayer.GetFeatureClass() != null)
                                .FirstOrDefault(featureLayer => featureLayer.GetFeatureClass().GetPath() == outputClass.GetPath());
                            if (outputFeatureLayer == null)
                            {
                                // A sample layer is provided in the project directory
                                _messages.Add("Please add the layer to your map to review the results.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            if (ApplyFilter)
                            {
                                var definitionQueries = outputFeatureLayer.DefinitionQueries;
                                var desiredDefinitionQuery = definitionQueries.FirstOrDefault(definitionQuery => definitionQuery.Name.Equals(selectedSubnetwork.Name, StringComparison.InvariantCultureIgnoreCase));
                                if (desiredDefinitionQuery != null)
                                {
                                    // Only update and zoom to if we're changing the definition query
                                    if(!desiredDefinitionQuery.WhereClause.Equals(outputFeatureLayer.DefinitionQuery))
                                    {
                                        outputFeatureLayer.SetActiveDefinitionQuery(selectedSubnetwork.Name);
                                        activeMapView.ZoomTo(outputFeatureLayer);
                                    }
                                }
                                else
                                {
                                    if (definitionQueries.Count > 100)
                                    {
                                        _messages.Add("Layer contains too many definition queries. Removing active definition query");
                                        NotifyPropertyChanged("Messages");
                                    }
                                    else
                                    {
                                        var newDefinitionQuery = new DefinitionQuery { Name = selectedSubnetwork.Name, WhereClause = string.Format("ExportName='{0}'", selectedSubnetwork.Name) };
                                        outputFeatureLayer.InsertDefinitionQuery(newDefinitionQuery, true);
                                        activeMapView.ZoomTo(outputFeatureLayer);
                                    }
                                }
                            }

                            activeMapView.RedrawAsync(false);

                            #endregion

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            _messages.Add(ex.Message);
                            NotifyPropertyChanged("Messages");
                        }
                        finally
                        {
                            _running = false;
                            NotifyPropertyChanged("Running");
                            Status = "Ready";
                        }
                    });

                    return;
                }, () => true);
            }
        }

        // By default this command is hidden on the UI.
        // Update the ShowAnalyzeTier parameter to make this visible
        public ICommand CmdAnalyzeTier
        {
            get
            {
                return new RelayCommand((cmdParams) =>
                {
                    if (string.IsNullOrEmpty(_selectedTier)) return;
                    if (!_tierList.Contains(_selectedTier)) return;

                    QueuedTask.Run(() =>
                    {
                        try
                        {
                            _running = true;
                            NotifyPropertyChanged("Running");
                            _messages.Clear();
                            NotifyPropertyChanged("Messages");
                            Status = "Exporting Tier";

                            using var utilityNetworkDefinition = _utilityNetwork.GetDefinition();
                            Tier selectedTier = null;
                            foreach (var domainNetwork in utilityNetworkDefinition.GetDomainNetworks())
                                foreach (var tier in domainNetwork.Tiers)
                                    if (tier.Name.Equals(_selectedTier, StringComparison.InvariantCultureIgnoreCase))
                                        selectedTier = tier;

                            if (selectedTier == null)
                            {
                                _messages.Add("Unable to load selected tier.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            var statusInfo = TraceHelper.GetNetworkStatusAttribute(selectedTier);
                            if (string.IsNullOrEmpty(statusInfo.fieldName))
                            {
                                _messages.Add("Unable to find a Status attribute to represent open/closed.");
                                NotifyPropertyChanged("Messages");
                            }

                            List<string> networkAttributes = null;
                            if (!string.IsNullOrEmpty(statusInfo.fieldName))
                                networkAttributes = [statusInfo.fieldName];

                            var activeProject = ArcGIS.Desktop.Core.Project.Current;
                            var homeFolderPath = activeProject.HomeFolderPath;
                            IReadOnlyList<Subnetwork> subnetworks = null;

                            try
                            {
                                Status = "Loading subnetworks";

                                SubnetworkStates states = _includeDirtySubnetworks
                                    ? SubnetworkStates.Dirty | SubnetworkStates.Clean
                                    : SubnetworkStates.Clean;
                                var subnetworkManager = _utilityNetwork.GetSubnetworkManager();
                                subnetworks = subnetworkManager.GetSubnetworks(selectedTier, states).ToList();

                                _messages.Add("Processing "+ subnetworks.Count + " subnetworks.");
                                NotifyPropertyChanged("Messages");
                            }
                            catch(Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                                _messages.Add(ex.Message);
                                _messages.Add("Failed loading subnetworks.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            if (subnetworks == null || subnetworks.Count == 0)
                            {
                                _messages.Add("No subnetworks to analyze.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            using var networkSource = utilityNetworkDefinition.GetNetworkSources().First(source => source.UsageType == SourceUsageType.StructureJunction);
                            using var structureJunctionClass = (FeatureClass)_utilityNetwork.GetTable(networkSource);
                            using var structureJunctionClassDefinition = structureJunctionClass.GetDefinition();
                            var spatialReference = structureJunctionClassDefinition.GetSpatialReference();

                            FeatureClass outputClass = null;
                            int index = 0;
                            double totalExportTime = 0;
                            double totalAnalysisTime = 0;
                            var timingMessages = new List<string>();

                            int subnetworkCount = subnetworks.Count;
                            foreach (var selectedSubnetwork in subnetworks.OrderBy(subnetwork => subnetwork.Name))
                            {
                                if (!_running)
                                {
                                    _messages.Add("Cancelled exports");
                                    NotifyPropertyChanged("Messages");

                                    break;
                                }

                                index += 1;

                                double featureCount = 0, exportTime = 0, analysisTime = 0;
                                var subnetworkName = selectedSubnetwork.Name;
                                var exportFile = string.Format("{0}\\{1}.json", homeFolderPath, subnetworkName);
                                if (subnetworkName.Contains("/"))
                                    exportFile.Replace("/", "_");

                                try
                                {
                                    Status = string.Format("Analyzing Subnetwork {0} ({1} of {2})", subnetworkName, index, subnetworkCount);


                                    if (_includeDirtySubnetworks)
                                    {

                                        #region Analyze a dirty subnetwork

                                        var start = DateTime.Now;
                                        if (!TraceHelper.ExportDirtySubnetwork(_utilityNetwork, selectedSubnetwork, exportFile, [statusInfo.fieldName]))
                                        {
                                            _messages.Add("Unable to export dirty subnetwork using trace.");
                                            NotifyPropertyChanged("Messages");
                                            timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\tUnable to export", subnetworkName, featureCount, exportTime, analysisTime));
                                            return;
                                        }
                                        exportTime = (DateTime.Now - start).TotalSeconds;
                                        totalExportTime += exportTime;

                                        var subnetworkParser = new SubnetworkParser(utilityNetworkDefinition, selectedTier);

                                        var subnetworkControllers = selectedSubnetwork.GetControllers().Where(controller => controller.Element.ObjectID != -1);
                                        var subnetworkControllerElements = subnetworkControllers.Select(controller => controller.Element);
                                        var startingElements = subnetworkParser.GetStartingElementKeys(subnetworkControllerElements).ToArray();

                                        start = DateTime.Now;
                                        featureCount = subnetworkParser.ParseSubnetwork(selectedSubnetwork.Name, exportFile, statusInfo.fieldName, statusInfo.statusValue, _clearResults, startingElements);
                                        if(featureCount == 0)
                                        {
                                            _messages.Add("Subnetwork contains no connectivity/features: "  + subnetworkName);
                                            NotifyPropertyChanged("Messages");
                                            timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\tNo connectivity/features", subnetworkName, featureCount, exportTime, analysisTime));
                                            continue;
                                        }

                                        outputClass = subnetworkParser.OutputGeometry(spatialReference, selectedSubnetwork.Name, deleteAllRows: false);
                                        analysisTime = (DateTime.Now - start).TotalSeconds;
                                        totalAnalysisTime += analysisTime;

                                        timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\tSuccess", subnetworkName, featureCount, exportTime, analysisTime));
                                        NotifyPropertyChanged("Messages");

                                        #endregion

                                    }
                                    else
                                    {

                                        #region Analyze a clean subnetwork

                                        var start = DateTime.Now;
                                        if (!TraceHelper.ExportSubnetwork(_utilityNetwork, selectedSubnetwork, exportFile, [statusInfo.fieldName]))
                                        {
                                            _messages.Add("Unable to export subnetwork: " + subnetworkName);
                                            NotifyPropertyChanged("Messages");
                                            timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\tUnable to export", subnetworkName, featureCount, exportTime, analysisTime));
                                            return;
                                        }
                                        exportTime = (DateTime.Now - start).TotalSeconds;
                                        totalExportTime += exportTime;

                                        var subnetworkParser = new SubnetworkParser(utilityNetworkDefinition, selectedTier);
                                        start = DateTime.Now;
                                        featureCount = subnetworkParser.ParseSubnetwork(selectedSubnetwork.Name, exportFile, statusInfo.fieldName, statusInfo.statusValue, _clearResults);
                                        if (featureCount == 0)
                                        {
                                            _messages.Add("Subnetwork contains no connectivity/features: " + subnetworkName);
                                            NotifyPropertyChanged("Messages");
                                            timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\tNo connectivity/features", subnetworkName, featureCount, exportTime, analysisTime));
                                            continue;
                                        }

                                        outputClass = subnetworkParser.OutputGeometry(spatialReference, selectedSubnetwork.Name, deleteAllRows: false);
                                        analysisTime = (DateTime.Now - start).TotalSeconds;
                                        totalAnalysisTime += analysisTime;

                                        timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\tSuccess", subnetworkName, featureCount, exportTime, analysisTime));
                                        NotifyPropertyChanged("Messages");

                                        #endregion

                                    }
                                }
                                catch (GeodatabaseException ex)
                                {
                                    _messages.Add("Analyzing subnetwork failed: " + subnetworkName + " - " + ex.Message);
                                    if(ex.Message.Contains("One or more dirty areas were discovered."))
                                        timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", subnetworkName, featureCount, exportTime, analysisTime, "One or more dirty areas were discovered."));
                                    else
                                        timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", subnetworkName, featureCount, exportTime, analysisTime, ex.Message));
                                    NotifyPropertyChanged("Messages");

                                    Debug.WriteLine("Analyzing subnetwork failed: " + subnetworkName);
                                    Debug.WriteLine(ex.ToString());
                                }
                                catch (Exception ex)
                                {
                                    _messages.Add("Analyzing subnetwork failed: " + subnetworkName);
                                    timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\tUnknown Error", subnetworkName, featureCount, exportTime, analysisTime));
                                    NotifyPropertyChanged("Messages");

                                    Debug.WriteLine("Analyzing subnetwork failed: " + subnetworkName);
                                    Debug.WriteLine(ex.ToString());
                                }
                                finally
                                {
                                    if (Path.Exists(exportFile))
                                        File.Delete(exportFile);
                                }
                            }

                            _messages.Add("");
                            _messages.Add("Total Subnetworks Analyzed: " + index);
                            _messages.Add("Total Export Time: " + totalExportTime);
                            _messages.Add("Average Export Time: " + (totalExportTime/index));
                            _messages.Add("Total Analysis Time: " + totalAnalysisTime);
                            _messages.Add("Average Analysis Time: " + (totalAnalysisTime/index));
                            _messages.Add("Subnetwork\tFeatures\tExport Time\tAnalysis Time\tMessage");
                            foreach (var message in timingMessages)
                                _messages.Add(message);
                            NotifyPropertyChanged("Messages");

                            if (outputClass == null)
                            {
                                _messages.Add("Failed to analyze flow for subnetwork.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            var activeMapView = MapView.Active;
                            var activeMap = activeMapView.Map;
                            var outputFeatureLayer = activeMap.GetLayersAsFlattenedList()
                                .OfType<FeatureLayer>()
                                .Where(featureLayer => featureLayer.GetFeatureClass() != null)
                                .FirstOrDefault(featureLayer => featureLayer.GetFeatureClass().GetPath() == outputClass.GetPath());
                            if (outputFeatureLayer == null)
                            {
                                _messages.Add("Please add the layer to your map to review the results.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            if (ApplyFilter)
                            {
                                var definitionQueries = outputFeatureLayer.DefinitionQueries;
                                outputFeatureLayer.SetActiveDefinitionQuery(null);
                            }

                            activeMapView.RedrawAsync(false);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                        finally
                        {
                            _running = false;
                            NotifyPropertyChanged("Running");
                            Status = "Ready";
                        }
                    });

                    return;
                }, () => true);
            }
        }

        public ICommand CmdStop
        {
            get
            {
                
                return new RelayCommand((cmdParams) =>
                {
                    _running = false;
                    NotifyPropertyChanged("Running");
                }, () => true, true, false);
            }
        }

                    #endregion

                }


    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class DockpaneCombo_ShowButton : Button
    {
        protected override void OnClick()
        {
            DockpaneComboViewModel.Show();
        }
    }
}
