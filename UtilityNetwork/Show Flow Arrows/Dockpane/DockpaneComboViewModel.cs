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
using Microsoft.Win32;
using Show_Flow_Arrows.FlowButton;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Show_Flow_Arrows
{
    internal class DockpaneComboViewModel : DockPane
    {

        #region Members

        private const string _dockPaneID = "ShowFlowArrows_DockpaneCombo";

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
                            AddMessage("Unable to load selected tier.");
                            NotifyPropertyChanged("Messages");
                            return;
                        }

                        using var subnetworkManager = _utilityNetwork.GetSubnetworkManager();
                        var selectedSubnetwork = subnetworkManager.GetSubnetwork(_selectedSubnetwork);
                        if (selectedTier == null)
                        {
                            AddMessage("Unable to load selected subnetwork.");
                            NotifyPropertyChanged("Messages");
                            return;
                        }

                        var activeProject = ArcGIS.Desktop.Core.Project.Current;
                        var projectGeodatabasePath = activeProject.DefaultGeodatabasePath;
                        var projectGeodatabase = SubnetworkParser.GetOutputGeodatabase(projectGeodatabasePath);
                        var outputLineClass = SubnetworkParser.GetOutputLineClass(projectGeodatabase);
                        if (outputLineClass == null)
                            return;

                        var activeMapView = MapView.Active;
                        var activeMap = activeMapView.Map;
                        var outputFeatureLayer = activeMap.GetLayersAsFlattenedList()
                            .OfType<FeatureLayer>()
                            .FirstOrDefault(featureLayer => featureLayer.GetFeatureClass().GetPath() == outputLineClass.GetPath());
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

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Show Flow Arrows";
        public string Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value);
        }

        /// <summary>
        /// Messages/warnings during processing.
        /// </summary>
        private string _message = string.Empty;
        public string Messages
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        private void AddMessage(string message)
        {
            _message += "\n" + message;
            NotifyPropertyChanged("Messages");
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

        private bool _includeDirtySubnetworks = true;
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


        // Set this to True to make the analyze tiers command visible
        private bool _showVisualizeTier = true;
        public bool ShowVisualizeTier
        {
            get { return _showVisualizeTier; }
            set
            {
                SetProperty(ref _showVisualizeTier, value, () => ShowVisualizeTier);
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
                        .OrderBy(tier => tier.Name);

                    _tierList.Clear();
                    foreach (var tier in discoveredTiers)
                        _tierList.Add(tier.Name);
                    NotifyPropertyChanged("Tiers");

                    Status = "Select a tier";
                });
            }
            catch (Exception ex)
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

        public ICommand CmdVisualizeSubnetwork
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
                            if (!TraceHelper.SupportsFlowDirectionResults(_utilityNetwork))
                            {
                                Messages = string.Empty;
                                AddMessage("Service does not support 'SupportsFlowDirectionResults' capability");
                                NotifyPropertyChanged("Messages");
                                Status = "Reading subnetwork";
                                return;
                            }

                            _running = true;
                            NotifyPropertyChanged("Running");
                            Messages = string.Empty;
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
                                AddMessage("Unable to load selected tier.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            using var subnetworkManager = _utilityNetwork.GetSubnetworkManager();
                            var selectedSubnetwork = subnetworkManager.GetSubnetwork(_selectedSubnetwork);
                            if (selectedTier == null)
                            {
                                AddMessage("Unable to load selected subnetwork.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            var subnetworkControllers = selectedSubnetwork.GetControllers();
                            if (subnetworkControllers.Count > 1)
                            {
                                AddMessage("Flow arrows may not be accurate when there are multiple subnetwork controllers.");
                                NotifyPropertyChanged("Messages");
                            }

                            var activeProject = ArcGIS.Desktop.Core.Project.Current;
                            var homeFolderPath = activeProject.HomeFolderPath;
                            var exportFile = string.Format("{0}\\{1}.json", homeFolderPath, selectedSubnetwork.Name);

                            FeatureClass outputClass = null;

                            try
                            {
                                // If the subnetwork isn't clean, or the tool is set to explicitly handle networks as if they're dirty
                                if (_includeDirtySubnetworks || selectedSubnetwork.GetState() != SubnetworkStates.Clean)
                                {

                                    #region Analyze a dirty subnetwork

                                    AddMessage("Exporting subnetwork: " + selectedSubnetwork.Name);
                                    NotifyPropertyChanged("Messages");

                                    if (!TraceHelper.ExportDirtySubnetwork(_utilityNetwork, selectedSubnetwork, exportFile))
                                    {
                                        AddMessage("Unable to read connectivity information from subnetwork.");
                                        NotifyPropertyChanged("Messages");
                                        return;
                                    }

                                    Status = "Analyzing flow";
                                    AddMessage("Analyzing subnetwork: " + selectedSubnetwork.Name);
                                    NotifyPropertyChanged("Messages");

                                    var subnetworkParser = new SubnetworkParser(utilityNetworkDefinition, selectedTier);

                                    // The Trace response doesn't include a subnetwork controllers element
                                    // This code will create subnetwork controllers for analysis using the features from the subnetwork definition
                                    var subnetworkControllerElements = subnetworkControllers.Select(controller => controller.Element);
                                    var startingElements = subnetworkParser.GetStartingElementKeys(subnetworkControllerElements).ToArray();

                                    // The Trace response doesn't include a spatial reference
                                    // Use the spatial reference from the structure junction class
                                    var networkSource = utilityNetworkDefinition.GetNetworkSources().First(source => source.UsageType == SourceUsageType.StructureJunction);
                                    var structureJunctionClass = (FeatureClass)_utilityNetwork.GetTable(networkSource);
                                    var structureJunctionClassDefinition = structureJunctionClass.GetDefinition();
                                    var spatialReference = structureJunctionClassDefinition.GetSpatialReference();

                                    var featureelements = subnetworkParser.ParseTraceWithFlowDirection(selectedSubnetwork.Name, exportFile);
                                    outputClass = subnetworkParser.OutputGeometry(spatialReference, selectedSubnetwork.Name, deleteAllRows: _clearResults, replaceRows: true);

                                    #endregion

                                }
                                else
                                {

                                    #region Analyze a clean subnetwork

                                    AddMessage("Exporting subnetwork: " + selectedSubnetwork.Name);
                                    NotifyPropertyChanged("Messages");

                                    if (!TraceHelper.ExportSubnetwork(_utilityNetwork, selectedSubnetwork, exportFile))
                                    {
                                        AddMessage("Unable to read connectivity information from subnetwork.");
                                        NotifyPropertyChanged("Messages");
                                        return;
                                    }

                                    Status = "Analyzing flow";
                                    AddMessage("Analyzing subnetwork: " + selectedSubnetwork.Name);
                                    NotifyPropertyChanged("Messages");

                                    // The Trace response doesn't include a spatial reference
                                    // Use the spatial reference from the structure junction class
                                    var networkSource = utilityNetworkDefinition.GetNetworkSources().First(source => source.UsageType == SourceUsageType.StructureJunction);
                                    var structureJunctionClass = (FeatureClass)_utilityNetwork.GetTable(networkSource);
                                    var structureJunctionClassDefinition = structureJunctionClass.GetDefinition();
                                    var spatialReference = structureJunctionClassDefinition.GetSpatialReference();

                                    var subnetworkParser = new SubnetworkParser(utilityNetworkDefinition, selectedTier);

                                    var featureelements = subnetworkParser.ParseTraceWithFlowDirection(selectedSubnetwork.Name, exportFile);
                                    outputClass = subnetworkParser.OutputGeometry(spatialReference, selectedSubnetwork.Name, deleteAllRows: _clearResults, replaceRows: true);

                                    #endregion

                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                                AddMessage(ex.Message);
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
                                AddMessage("Unable to analyze flow for subnetwork.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            #region Update the map

                            AddMessage("Analysis complete.");
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
                                AddMessage("Please add the layer to your map to review the results.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            if (ApplyFilter)
                            {
                                var definitionQueries = outputFeatureLayer.DefinitionQueries;
                                if (ClearResults)
                                    outputFeatureLayer.RemoveAllDefinitionQueries();
                                
                                definitionQueries = outputFeatureLayer.DefinitionQueries;
                                if (definitionQueries.Count > 100)
                                {
                                    AddMessage("Layer contains too many definition queries. Removing active definition query");
                                    NotifyPropertyChanged("Messages");
                                }
                                else
                                {
                                    var desiredDefinitionQuery = definitionQueries.FirstOrDefault(definitionQuery => definitionQuery.Name.Equals(selectedSubnetwork.Name, StringComparison.InvariantCultureIgnoreCase));
                                    if (desiredDefinitionQuery == null)
                                    {
                                        var newDefinitionQuery = new DefinitionQuery { Name = selectedSubnetwork.Name, WhereClause = string.Format("ExportName='{0}'", selectedSubnetwork.Name) };
                                        outputFeatureLayer.InsertDefinitionQuery(newDefinitionQuery, true);
                                        activeMapView.ZoomTo(outputFeatureLayer);
                                    }
                                    else if (!desiredDefinitionQuery.WhereClause.Equals(outputFeatureLayer.DefinitionQuery))
                                    {
                                        // Only update and zoom to if we're changing the definition query
                                        outputFeatureLayer.SetActiveDefinitionQuery(selectedSubnetwork.Name);
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
                            AddMessage(ex.Message);
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

        public ICommand CmdVisualizeTier
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
                            if (!TraceHelper.SupportsFlowDirectionResults(_utilityNetwork))
                            {
                                Messages = string.Empty;
                                AddMessage("Service does not support 'SupportsFlowDirectionResults' capability");
                                Status = "Reading subnetworks";
                                return;
                            }

                            _running = true;
                            NotifyPropertyChanged("Running");
                            Messages = string.Empty;
                            NotifyPropertyChanged("Messages");
                            Status = "Reading subnetworks";

                            using var utilityNetworkDefinition = _utilityNetwork.GetDefinition();
                            Tier selectedTier = null;
                            foreach (var domainNetwork in utilityNetworkDefinition.GetDomainNetworks())
                                foreach (var tier in domainNetwork.Tiers)
                                    if (tier.Name.Equals(_selectedTier, StringComparison.InvariantCultureIgnoreCase))
                                        selectedTier = tier;

                            if (selectedTier == null)
                            {
                                AddMessage("Unable to load selected tier.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            var activeProject = ArcGIS.Desktop.Core.Project.Current;
                            var homeFolderPath = activeProject.HomeFolderPath;
                            IReadOnlyList<Subnetwork> subnetworks = null;

                            FeatureClass outputClass = null;

                            try
                            {
                                Status = "Loading subnetworks";

                                SubnetworkStates states = _includeDirtySubnetworks
                                    ? SubnetworkStates.Dirty | SubnetworkStates.Clean
                                    : SubnetworkStates.Clean;
                                var subnetworkManager = _utilityNetwork.GetSubnetworkManager();
                                subnetworks = subnetworkManager.GetSubnetworks(selectedTier, states).ToList();

                                AddMessage("Processing " + subnetworks.Count + " subnetworks.");
                                NotifyPropertyChanged("Messages");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                                AddMessage(ex.Message);
                                AddMessage("Failed loading subnetworks.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            if (subnetworks == null || subnetworks.Count == 0)
                            {
                                AddMessage("No subnetworks to analyze.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            // The Trace response doesn't include a spatial reference
                            // Use the spatial reference from the structure junction class
                            using var networkSource = utilityNetworkDefinition.GetNetworkSources().First(source => source.UsageType == SourceUsageType.StructureJunction);
                            using var structureJunctionClass = (FeatureClass)_utilityNetwork.GetTable(networkSource);
                            using var structureJunctionClassDefinition = structureJunctionClass.GetDefinition();
                            var spatialReference = structureJunctionClassDefinition.GetSpatialReference();

                            Geodatabase outputGeodatabase;
                            FeatureClass outputLineClass, outputPointClass;
                            var subnetworkParser = new SubnetworkParser(utilityNetworkDefinition, selectedTier);
                            subnetworkParser.GetOutputClasses(string.Empty, true, spatialReference, out outputGeodatabase, out outputLineClass, out outputPointClass);

                            int index = 0;
                            double totalExportTime = 0;
                            double totalAnalysisTime = 0;
                            double totalOutputTime = 0;
                            var timingMessages = new List<string>();

                            int subnetworkCount = subnetworks.Count;
                            var addedSubnetworks = new List<string>();
                            foreach (var selectedSubnetwork in subnetworks.OrderBy(subnetwork => subnetwork.Name))
                            {
                                if (!_running)
                                {
                                    AddMessage("Cancelled exports");
                                    NotifyPropertyChanged("Messages");

                                    break;
                                }

                                index += 1;
                                Status = string.Format("Analyzing subnetwork ({0} / {1}", index, subnetworkCount);

                                double featureCount = 0, exportTime = 0, analysisTime = 0, outputTime = 0;
                                var subnetworkName = selectedSubnetwork.Name;
                                var exportFile = string.Format("{0}\\{1}.json", homeFolderPath, subnetworkName);
                                if (subnetworkName.Contains("/"))
                                    exportFile.Replace("/", "_");

                                try
                                {

                                    if (_includeDirtySubnetworks || selectedSubnetwork.GetState() != SubnetworkStates.Clean)
                                    {
                                        // For consistency's sake, always use this method when in this mode

                                        #region Analyze a dirty subnetwork

                                        if (subnetworkCount < 1000)
                                        {
                                            AddMessage("Exporting subnetwork: " + selectedSubnetwork.Name);
                                            NotifyPropertyChanged("Messages");
                                        }

                                        var start = DateTime.Now;
                                        if (!TraceHelper.ExportDirtySubnetwork(_utilityNetwork, selectedSubnetwork, exportFile))
                                        {
                                            if (subnetworkCount > 1000)
                                                AddMessage("Exporting subnetwork: " + selectedSubnetwork.Name);

                                            _message += " -> Unable to read connectivity information from subnetwork";
                                            NotifyPropertyChanged("Messages");
                                            return;
                                        }
                                        exportTime = (DateTime.Now - start).TotalSeconds;
                                        totalExportTime += exportTime;

                                        start = DateTime.Now;
                                        featureCount = subnetworkParser.ParseTraceWithFlowDirection(selectedSubnetwork.Name, exportFile);
                                        analysisTime = (DateTime.Now - start).TotalSeconds;
                                        totalAnalysisTime += analysisTime;

                                        if (featureCount == 0)
                                        {
                                            if (subnetworkCount > 1000)
                                                AddMessage("Exporting subnetwork: " + selectedSubnetwork.Name);

                                            _message += " -> Subnetwork contains no flow directions";
                                            NotifyPropertyChanged("Messages");
                                            timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\tNo flow directions", subnetworkName, featureCount, exportTime, analysisTime, ""));
                                            continue;
                                        }

                                        start = DateTime.Now;
                                        subnetworkParser.OutputGeometry(outputGeodatabase, selectedSubnetwork.Name, outputLineClass, outputPointClass);
                                        outputTime = (DateTime.Now - start).TotalSeconds;
                                        totalOutputTime += outputTime;

                                        timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\tSuccess", subnetworkName, featureCount, exportTime, analysisTime, outputTime));
                                        NotifyPropertyChanged("Messages");

                                        #endregion

                                    }
                                    else
                                    {

                                        #region Analyze a clean subnetwork

                                        if (subnetworkCount < 1000)
                                        {
                                            AddMessage("Exporting subnetwork: " + selectedSubnetwork.Name);
                                            NotifyPropertyChanged("Messages");
                                        }

                                        var start = DateTime.Now;
                                        if (!TraceHelper.ExportSubnetwork(_utilityNetwork, selectedSubnetwork, exportFile))
                                        {
                                            if (subnetworkCount > 1000)
                                                AddMessage("Exporting subnetwork: " + selectedSubnetwork.Name);

                                            _message += " -> Unable to read connectivity information from subnetwork";
                                            NotifyPropertyChanged("Messages");
                                            return;
                                        }
                                        exportTime = (DateTime.Now - start).TotalSeconds;
                                        totalExportTime += exportTime;

                                        start = DateTime.Now;
                                        featureCount = subnetworkParser.ParseTraceWithFlowDirection(selectedSubnetwork.Name, exportFile);
                                        analysisTime = (DateTime.Now - start).TotalSeconds;
                                        totalAnalysisTime += analysisTime;

                                        if (featureCount == 0)
                                        {
                                            if (subnetworkCount > 1000)
                                                AddMessage("Exporting subnetwork: " + selectedSubnetwork.Name);

                                            _message += " -> Subnetwork contains no flow directions";
                                            NotifyPropertyChanged("Messages");
                                            timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\tNo flow directions", subnetworkName, featureCount, exportTime, analysisTime,""));
                                            continue;
                                        }

                                        start = DateTime.Now;
                                        subnetworkParser.OutputGeometry(outputGeodatabase, selectedSubnetwork.Name, outputLineClass, outputPointClass);
                                        outputTime = (DateTime.Now - start).TotalSeconds;
                                        totalOutputTime += outputTime;

                                        timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\tSuccess", subnetworkName, featureCount, exportTime, analysisTime, outputTime));
                                        NotifyPropertyChanged("Messages");

                                        #endregion

                                    }

                                    addedSubnetworks.Add(subnetworkName);
                                }
                                catch (GeodatabaseException ex)
                                {
                                    AddMessage("Analyzing subnetwork failed: " + subnetworkName + " - " + ex.Message);
                                    if (ex.Message.Contains("One or more dirty areas were discovered."))
                                        timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", subnetworkName, featureCount, exportTime, analysisTime, outputTime, "One or more dirty areas were discovered."));
                                    else
                                        timingMessages.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", subnetworkName, featureCount, exportTime, analysisTime, outputTime, ex.Message));
                                    NotifyPropertyChanged("Messages");

                                    Debug.WriteLine("Analyzing subnetwork failed: " + subnetworkName);
                                    Debug.WriteLine(ex.ToString());
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.ToString());
                                    AddMessage(ex.Message);
                                    NotifyPropertyChanged("Messages");
                                    return;
                                }
                                finally
                                {
                                    if (Path.Exists(exportFile))
                                        File.Delete(exportFile);
                                }
                            }

                            AddMessage("Analysis complete.");
                            NotifyPropertyChanged("Messages");

                            AddMessage("");
                            AddMessage("Total Subnetworks Analyzed: " + index);
                            AddMessage("Total Export Time: " + totalExportTime);
                            AddMessage("Average Export Time: " + (totalExportTime / index));
                            AddMessage("Total Analysis Time: " + totalAnalysisTime);
                            AddMessage("Total Output Time: " + totalOutputTime);
                            AddMessage("Average Analysis Time: " + (totalAnalysisTime / index));
                            AddMessage("Subnetwork\tFeatures\tExport Time\tAnalysis Time\tOutput Time\tMessage");
                            foreach (var message in timingMessages)
                                AddMessage(message);
                            NotifyPropertyChanged("Messages");

                            if (outputClass == null)
                            {
                                AddMessage("Unable to analyze flow for subnetwork.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            #region Update the map

                            var activeMapView = MapView.Active;
                            var activeMap = activeMapView.Map;
                            var outputFeatureLayer = activeMap.GetLayersAsFlattenedList()
                                .OfType<FeatureLayer>()
                                .Where(featureLayer => featureLayer.GetFeatureClass() != null)
                                .FirstOrDefault(featureLayer => featureLayer.GetFeatureClass().GetPath() == outputClass.GetPath());
                            if (outputFeatureLayer == null)
                            {
                                // A sample layer is provided in the project directory
                                AddMessage("Please add the layer to your map to review the results.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            if (ApplyFilter)
                            {
                                // Only put the first 100 values in the list, otherwise that dropdown suffers from performance issues
                                if (addedSubnetworks.Count > 100)
                                {
                                    AddMessage("Layer contains too many definition queries. Removing active definition query");
                                    NotifyPropertyChanged("Messages");
                                }
                                else
                                {
                                    var definitionQueries = outputFeatureLayer.DefinitionQueries;
                                    if(ClearResults)
                                        outputFeatureLayer.RemoveAllDefinitionQueries();

                                    definitionQueries = outputFeatureLayer.DefinitionQueries;
                                    foreach (var subnetworkName in addedSubnetworks)
                                    {
                                        var desiredDefinitionQuery = definitionQueries.FirstOrDefault(definitionQuery => definitionQuery.Name.Equals(subnetworkName, StringComparison.InvariantCultureIgnoreCase));
                                        if (desiredDefinitionQuery != null)
                                            continue;

                                        var newDefinitionQuery = new DefinitionQuery { Name = subnetworkName, WhereClause = string.Format("ExportName='{0}'", subnetworkName) };
                                        outputFeatureLayer.InsertDefinitionQuery(newDefinitionQuery, true);
                                    }
                                }

                                outputFeatureLayer.SetActiveDefinitionQuery(null);
                            }

                            activeMapView.RedrawAsync(false);

                            #endregion

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            AddMessage(ex.Message);
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


        public ICommand CmdVisualizeTrace
        {
            get
            {
                return new RelayCommand((cmdParams) =>
                {
                    QueuedTask.Run(() =>
                    {
                        try
                        {
                            FeatureClass outputClass = null;

                            _running = true;
                            NotifyPropertyChanged("Running");
                            Messages = string.Empty;
                            Status = "Reading subnetwork";

                            using var utilityNetworkDefinition = _utilityNetwork.GetDefinition();
                            Tier selectedTier = null;
                            if(!string.IsNullOrEmpty(_selectedTier))
                            {
                                foreach (var domainNetwork in utilityNetworkDefinition.GetDomainNetworks())
                                    foreach (var tier in domainNetwork.Tiers)
                                        if (tier.Name.Equals(_selectedTier, StringComparison.InvariantCultureIgnoreCase))
                                            selectedTier = tier;

                                if (selectedTier == null)
                                {
                                    AddMessage("Unable to load selected tier.");
                                    NotifyPropertyChanged("Messages");
                                    return;
                                }
                            }

                            int featureCount = -1;
                            var ofd = new OpenFileDialog { Title = "Select JSON Export", Filter = "*.json|*.json", Multiselect = true };
                            var fileDialogResult = ofd.ShowDialog();
                            if (fileDialogResult.HasValue && !fileDialogResult.Value)
                                return;

                            // The file could be a trace response or a subnetwork export, so we can't guarantee a spatial reference
                            // Use the spatial reference from the structure junction class
                            var networkSource = utilityNetworkDefinition.GetNetworkSources().First(source => source.UsageType == SourceUsageType.StructureJunction);
                            var structureJunctionClass = (FeatureClass)_utilityNetwork.GetTable(networkSource);
                            var structureJunctionClassDefinition = structureJunctionClass.GetDefinition();
                            var spatialReference = structureJunctionClassDefinition.GetSpatialReference();

                            Status = "Loading flow";

                            var resultNames = new List<string>();
                            foreach (var jsonFile in ofd.FileNames)
                            {
                                if (string.IsNullOrEmpty(jsonFile))
                                    continue;

                                var fileName = Path.GetFileNameWithoutExtension(jsonFile);
                                resultNames.Add(fileName);

                                AddMessage("Loading file: " + fileName);
                                NotifyPropertyChanged("Messages");

                                var subnetworkParser = new SubnetworkParser(utilityNetworkDefinition, selectedTier);
                                featureCount = subnetworkParser.ParseTraceWithFlowDirection(fileName, jsonFile);
                                outputClass = subnetworkParser.OutputGeometry(spatialReference, fileName, deleteAllRows: _clearResults);

                                if (outputClass == null)
                                {
                                    AddMessage("Unable to analyze flow for subnetwork.");
                                    NotifyPropertyChanged("Messages");
                                    continue;
                                }
                            }

                            #region Update the map

                            AddMessage("Analysis complete.");
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
                                AddMessage("Please add the layer to your map to review the results.");
                                NotifyPropertyChanged("Messages");
                                return;
                            }

                            if (ApplyFilter)
                            {
                                var definitionQueries = outputFeatureLayer.DefinitionQueries;
                                if (ClearResults)
                                    outputFeatureLayer.RemoveAllDefinitionQueries();

                                definitionQueries = outputFeatureLayer.DefinitionQueries;
                                if (definitionQueries.Count > 100)
                                {
                                    AddMessage("Layer contains too many definition queries. Removing active definition query");
                                    NotifyPropertyChanged("Messages");
                                }
                                else
                                {
                                    DefinitionQuery desiredDefinitionQuery = null;
                                    foreach (var fileName in resultNames)
                                    {
                                        desiredDefinitionQuery = definitionQueries.FirstOrDefault(definitionQuery => definitionQuery.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
                                        if (desiredDefinitionQuery == null)
                                        {
                                            var newDefinitionQuery = new DefinitionQuery { Name = fileName, WhereClause = string.Format("ExportName='{0}'", fileName) };
                                            outputFeatureLayer.InsertDefinitionQuery(newDefinitionQuery, resultNames.Count == 1);
                                        }
                                    }

                                    if (resultNames.Count == 1)
                                    {
                                        activeMapView.ZoomTo(outputFeatureLayer);
                                        activeMapView.RedrawAsync(false);
                                    }
                                }

                            }

                            #endregion

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            AddMessage(ex.Message);
                            NotifyPropertyChanged("Messages");
                        }
                        finally
                        {
                            _running = false;
                            NotifyPropertyChanged("Running");
                            Status = "Ready";
                        }
                    });
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
