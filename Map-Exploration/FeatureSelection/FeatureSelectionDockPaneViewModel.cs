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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework.Events;
using ArcGIS.Core.Data;

namespace FeatureSelection
{
    /// <summary>
    /// ViewModel for the Dock Pane
    /// </summary>
    internal class FeatureSelectionDockPaneViewModel : DockPane
    {
        private const string _dockPaneID = "FeatureSelection_FeatureSelectionDockPane";
        private const string _selectToolID = "FeatureSelection_FeatureSelectionTool";
        private object _lock = new object();
        Dictionary<Map, SelectedLayerInfo> _selectedLayerInfos = new Dictionary<Map, SelectedLayerInfo>();
        Map _activeMap;

        protected FeatureSelectionDockPaneViewModel()
        {
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_layers, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_layerSelection, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_fieldAttributes, _lock);
            LayersAddedEvent.Subscribe(OnLayersAdded);
            LayersRemovedEvent.Subscribe(OnLayersRemoved);
            ActiveToolChangedEvent.Subscribe(OnActiveToolChanged);
            MapSelectionChangedEvent.Subscribe(OnSelectionChanged);
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            MapRemovedEvent.Subscribe(OnMapRemoved);
        }

        ~FeatureSelectionDockPaneViewModel()
        {
            LayersAddedEvent.Unsubscribe(OnLayersAdded);
            LayersRemovedEvent.Unsubscribe(OnLayersRemoved);
            ActiveToolChangedEvent.Unsubscribe(OnActiveToolChanged);
            MapSelectionChangedEvent.Unsubscribe(OnSelectionChanged);
            ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
        }

        /// <summary>
        /// Called when the dock pane is first initialized.
        /// </summary>
        protected override Task InitializeAsync()
        {
            if (MapView.Active == null)
                return Task.FromResult(0);

            _activeMap = MapView.Active.Map;
            return UpdateForActiveMap();
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

        #region Bindable Properties

        private ObservableCollection<BasicFeatureLayer> _layers = new ObservableCollection<BasicFeatureLayer>();
        public ObservableCollection<BasicFeatureLayer> Layers
        {
            get { return _layers; }
        }

        private BasicFeatureLayer _selectedLayer;
        public BasicFeatureLayer SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                SetProperty(ref _selectedLayer, value, () => SelectedLayer);
                if (_selectedLayer == null)
                {
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                    return;
                }
                _selectedLayerInfos[_activeMap].SelectedLayer = _selectedLayer;
                SelectedLayerChanged();
            }
        }

        private string _whereClause = "";
        public string WhereClause
        {
            get { return _whereClause; }
            set
            {
                SetProperty(ref _whereClause, value, () => WhereClause);
                IsValid = false;
                HasError = false;
            }
        }

        private ObservableCollection<long?> _layerSelection = new ObservableCollection<long?>();
        public ObservableCollection<long?> LayerSelection
        {
            get { return _layerSelection; }
        }

        private long? _selectedOID;
        public long? SelectedOID
        {
            get { return _selectedOID; }
            set
            {
                SetProperty(ref _selectedOID, value, () => SelectedOID);
                _selectedLayerInfos[_activeMap].SelectedOID = _selectedOID;
                if (_selectedOID.HasValue && MapView.Active != null)
                    MapView.Active.FlashFeature(SelectedLayer, _selectedOID.Value);
                ShowAttributes();
            }
        }

        private ObservableCollection<FieldAttributeInfo> _fieldAttributes = new ObservableCollection<FieldAttributeInfo>();
        public ObservableCollection<FieldAttributeInfo> FieldAttributes
        {
            get { return _fieldAttributes; }
        }

        private FieldAttributeInfo _selectedRow;
        public FieldAttributeInfo SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                SetProperty(ref _selectedRow, value, () => SelectedRow);
            }
        }

        private bool _hasError = false;
        public bool HasError
        {
            get { return _hasError; }
            set
            {
                SetProperty(ref _hasError, value, () => HasError);
            }
        }

        private bool _isValid = false;
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                SetProperty(ref _isValid, value, () => IsValid);
            }
        }

        public System.Windows.Controls.ContextMenu RowContextMenu
        {
            get { return FrameworkApplication.CreateContextMenu("FeatureSelection_RowContextMenu"); }
        }

        #region Commands

        private RelayCommand _selectToolCmd;
        public ICommand SelectToolCmd
        {
            get
            {
                if (_selectToolCmd == null)
                {
                    _selectToolCmd = new RelayCommand(() => FrameworkApplication.SetCurrentToolAsync(_selectToolID), () => { return MapView.Active != null && SelectedLayer != null; });
                }
                return _selectToolCmd;
            }
        }

        private bool _selectToolActive = false;
        public bool SelectToolActive
        {
            get { return _selectToolActive; }
            set
            {
                SetProperty(ref _selectToolActive, true, () => SelectToolActive);
            }
        }

        private RelayCommand _clearSelectionCmd;
        public ICommand ClearSelectionCmd
        {
            get
            {
                if (_clearSelectionCmd == null)
                {
                    _clearSelectionCmd = new RelayCommand(() => QueuedTask.Run(() => SelectedLayer.Select(null, SelectionCombinationMethod.Subtract)), () => { return MapView.Active != null && SelectedLayer != null; });
                }
                return _clearSelectionCmd;
            }
        }

        private RelayCommand _newSelectionCmd;
        public ICommand NewSelectionCmd
        {
            get
            {
                if (_newSelectionCmd == null)
                {
                    _newSelectionCmd = new RelayCommand(() => ModifyLayerSelection(SelectionCombinationMethod.New), () => { return MapView.Active != null && SelectedLayer != null && WhereClause != ""; });
                }
                return _newSelectionCmd;
            }
        }

        private RelayCommand _addSelectionCmd;
        public ICommand AddSelectionCmd
        {
            get
            {
                if (_addSelectionCmd == null)
                {
                    _addSelectionCmd = new RelayCommand(() => ModifyLayerSelection(SelectionCombinationMethod.Add), () => { return MapView.Active != null && SelectedLayer != null && WhereClause != ""; });
                }
                return _addSelectionCmd;
            }
        }

        private RelayCommand _removeSelectionCmd;
        public ICommand RemoveSelectionCmd
        {
            get
            {
                if (_removeSelectionCmd == null)
                {
                    _removeSelectionCmd = new RelayCommand(() => ModifyLayerSelection(SelectionCombinationMethod.Subtract), () => { return MapView.Active != null && SelectedLayer != null && WhereClause != ""; });
                }
                return _removeSelectionCmd;
            }
        }

        private RelayCommand _selectSelectionCmd;
        public ICommand SelectSelectionCmd
        {
            get
            {
                if (_selectSelectionCmd == null)
                {
                    _selectSelectionCmd = new RelayCommand(() => ModifyLayerSelection(SelectionCombinationMethod.And), () => { return MapView.Active != null && SelectedLayer != null && WhereClause != ""; });
                }
                return _selectSelectionCmd;
            }
        }

        private RelayCommand _verifyExpressionCmd;
        public ICommand VerifyExpressionCmd
        {
            get
            {
                if (_verifyExpressionCmd == null)
                {
                    _verifyExpressionCmd = new RelayCommand(() => QueuedTask.Run(() => ValidateExpresion(true)), () => { return MapView.Active != null && SelectedLayer != null && WhereClause != ""; });
                }
                return _verifyExpressionCmd;
            }
        }

        private RelayCommand _clearExpressionCmd;
        public ICommand ClearExpressionCmd
        {
            get
            {
                if (_clearExpressionCmd == null)
                {
                    _clearExpressionCmd = new RelayCommand(() => WhereClause = "", () => { return WhereClause != ""; });
                }
                return _clearExpressionCmd;
            }
        }

        internal bool ValidateExpresion(bool showValidationSuccessMsg)
        {
            try
            {
                var qf = new QueryFilter() { WhereClause = WhereClause };
                SelectedLayer.Search(qf);
            }
            catch (Exception)
            {
                HasError = true;
                IsValid = false;
                return false;
            }

            if (showValidationSuccessMsg)
                IsValid = true;
            HasError = false;
            return true;
        }

        #endregion

        #endregion

        #region Private Methods

        private Task UpdateForActiveMap(bool activeMapChanged = true, Dictionary<MapMember, List<long>> mapSelection = null)
        {
            return QueuedTask.Run(() =>
            {
                SelectedLayerInfo selectedLayerInfo = null;
                if (!_selectedLayerInfos.ContainsKey(_activeMap))
                {
                    selectedLayerInfo = new SelectedLayerInfo();
                    _selectedLayerInfos.Add(_activeMap, selectedLayerInfo);
                }
                else
                    selectedLayerInfo = _selectedLayerInfos[_activeMap];

                if (activeMapChanged)
                {
                    RefreshLayerCollection();

                    SetProperty(ref _selectedLayer, (selectedLayerInfo.SelectedLayer != null) ? selectedLayerInfo.SelectedLayer : Layers.FirstOrDefault(), () => SelectedLayer);
                    if (_selectedLayer == null)
                    {
                        FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                        return;
                    }
                    selectedLayerInfo.SelectedLayer = SelectedLayer;
                }

                if (SelectedLayer == null)
                    RefreshSelectionOIDs(new List<long>());
                else
                {
                    List<long> oids = new List<long>();
                    if (mapSelection != null)
                    {
                        if (mapSelection.ContainsKey(SelectedLayer))
                            oids.AddRange(mapSelection[SelectedLayer]);
                    }
                    else
                    {
                        oids.AddRange(SelectedLayer.GetSelection().GetObjectIDs());
                    }
                    RefreshSelectionOIDs(oids);
                }

                SetProperty(ref _selectedOID, (selectedLayerInfo.SelectedOID != null && LayerSelection.Contains(selectedLayerInfo.SelectedOID)) ? selectedLayerInfo.SelectedOID : LayerSelection.FirstOrDefault(), () => SelectedOID);
                selectedLayerInfo.SelectedOID = SelectedOID;
                ShowAttributes();
            });
        }

        private void RefreshLayerCollection()
        {
            Layers.Clear();
            if (_activeMap == null)
                return;

            var layers = _activeMap.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>();
            lock (_lock)
            {
                foreach (var layer in layers)
                {
                    Layers.Add(layer);
                }
            }
        }

        private Task SelectedLayerChanged()
        {
            return QueuedTask.Run(() =>
            {
                if (SelectedLayer == null)
                    RefreshSelectionOIDs(new List<long>());
                else
                {
                    var selection = SelectedLayer.GetSelection();
                    RefreshSelectionOIDs(selection.GetObjectIDs());
                }
                SelectedOID = LayerSelection.FirstOrDefault();
            });
        }

        private void RefreshSelectionOIDs(IEnumerable<long> oids)
        {
            FieldAttributes.Clear();
            SetProperty(ref _selectedOID, null, () => SelectedOID);
            LayerSelection.Clear();
            lock (_lock)
            {
                foreach (var oid in oids)
                {
                    LayerSelection.Add(oid);
                }
            }
        }

        private Task ShowAttributes()
        {
            return QueuedTask.Run(() =>
            {
                try
                {
                    _fieldAttributes.Clear();
                    if (SelectedLayer == null || SelectedOID == null)
                        return;

                    var oidField = SelectedLayer.GetTable().GetDefinition().GetObjectIDField();
                    var qf = new ArcGIS.Core.Data.QueryFilter() { WhereClause = string.Format("{0} = {1}", oidField, SelectedOID) };
                    var cursor = SelectedLayer.Search(qf);
                    Row row = null;

                    if (!cursor.MoveNext()) return;

                    using (row = cursor.Current)
                    {
                        var fields = row.GetFields();
                        lock (_lock)
                        {
                            foreach (ArcGIS.Core.Data.Field field in fields)
                            {
                                if (field.FieldType == FieldType.Geometry)
                                    continue;
                                var val = row[field.Name];
                                FieldAttributeInfo fa = new FieldAttributeInfo(field, (val is DBNull || val == null) ? null : val.ToString());
                                _fieldAttributes.Add(fa);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            });
        }

        private Task ModifyLayerSelection(SelectionCombinationMethod method)
        {
            return QueuedTask.Run(() =>
            {
                if (MapView.Active == null || SelectedLayer == null || WhereClause == null)
                    return;

                if (!ValidateExpresion(false))
                    return;

                SelectedLayer.Select(new QueryFilter() { WhereClause = WhereClause }, method);
            });
        }

        #endregion

        #region Event Handlers

        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
        {
            if (args.IncomingView == null)
            {
                SetProperty(ref _selectedLayer, null, () => SelectedLayer);
                _layers.Clear();
                SetProperty(ref _selectedOID, null, () => SelectedOID);
                _layerSelection.Clear();
                _fieldAttributes.Clear();
                return;
            }

            _activeMap = args.IncomingView.Map;
            UpdateForActiveMap();
        }

        private void OnSelectionChanged(MapSelectionChangedEventArgs args)
        {
            if (args.Map != _activeMap)
                return;

            UpdateForActiveMap(false, args.Selection.ToDictionary());
        }

        private void OnLayersRemoved(LayerEventsArgs args)
        {
            foreach (var layer in args.Layers)
            {
                if (layer.Map == _activeMap)
                {
                    if (Layers.Contains(layer))
                        Layers.Remove((BasicFeatureLayer)layer);
                }
            }

            if (SelectedLayer == null)
            {
                SelectedLayer = Layers.FirstOrDefault();
                SelectedLayerChanged();
            }
        }

        private void OnLayersAdded(LayerEventsArgs args)
        {
            foreach (var layer in args.Layers)
            {
                if (layer.Map == _activeMap && layer is BasicFeatureLayer)
                {
                    Layers.Add((BasicFeatureLayer)layer);
                    if (SelectedLayer == null)
                        SelectedLayer = (BasicFeatureLayer)layer;
                }
            }
        }

        private void OnMapRemoved(MapRemovedEventArgs args)
        {
            var map = _selectedLayerInfos.Where(kvp => kvp.Key.URI == args.MapPath).FirstOrDefault().Key;
            if (map != null)
                _selectedLayerInfos.Remove(map);
        }

        private void OnActiveToolChanged(ToolEventArgs args)
        {
            SetProperty(ref _selectToolActive, (args.CurrentID == _selectToolID), () => SelectToolActive);
        }

        #endregion
    }

    /// <summary>
    /// Represents a single row in the data grid storing the field name, alias, and attribute value for a given row.
    /// </summary>
    internal class FieldAttributeInfo
    {
        private string _fieldName;
        private string _fieldAlias;
        private string _fieldValue;
        private FieldType _fieldType;

        internal FieldAttributeInfo(Field field, string fieldValue)
        {
            _fieldName = field.Name;
            _fieldAlias = field.AliasName;
            _fieldValue = fieldValue;
            _fieldType = field.FieldType;
        }

        public string FieldName
        {
            get { return _fieldName; }
        }

        public string FieldAlias
        {
            get { return _fieldAlias; }
        }

        public string FieldValue
        {
            get { return _fieldValue; }
        }

        public FieldType FieldType
        {
            get { return _fieldType; }
        }
    }

    /// <summary>
    /// Used to persist the state of the selected layer and object ID for a given map.
    /// </summary>
    internal class SelectedLayerInfo
    {
        public SelectedLayerInfo() { }
        public SelectedLayerInfo(BasicFeatureLayer selectedLayer, long? selectedOID)
        {
            SelectedLayer = selectedLayer;
            SelectedOID = selectedOID;
        }

        public BasicFeatureLayer SelectedLayer { get; set; }

        public long? SelectedOID { get; set; }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class FeatureSelectionDockPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            FeatureSelectionDockPaneViewModel.Show();
        }
    }
}
