using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.NetworkDiagrams;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static ExportDiagramToFeatureClasses.ExportDiagramToFeatureClassesModule;
using DataDDL = ArcGIS.Core.Data.DDL;
using Newtonsoft.Json.Linq;

namespace ExportDiagramToFeatureClasses
{
  internal class ExportPaneViewModel : DockPane
  {
    #region Pane Fields
    private const string _dockPaneID = "ExportDiagramToFeatureClasses_ExportPane";

    private const string _FileGeo = "File";
    private const string _MobileGeo = "Mobile";

    private string _name = "";
    private string _folder = "";
    private string _typeGeodatabase = _FileGeo;
    private List<string> _information;
    private List<string> _warning;
    private string _canceledStatus;
    private bool _createMap = false;
    private bool _exportAggregation = true;

    private string _optionLabel = ">";
    private string _geodatabaseLabel = "v";

    private System.Windows.Visibility _isGeodataseVisible = System.Windows.Visibility.Visible;
    private System.Windows.Visibility _isOptionVisible = System.Windows.Visibility.Collapsed;
    #endregion

    #region Pane Command
    public ICommand Execute => _execute;
    public ICommand SetFolder => _setFolder;
    public ICommand CloseInformation => _closeInformation;
    public ICommand CloseWarning => _closeWarning;
    public ICommand OpenOption => _openOption;
    public ICommand OpenGeodatabase => _openGeodatabase;
    #endregion

    #region Private Fields
    private ICommand _setFolder;
    private ICommand _execute;
    private ICommand _closeInformation;
    private ICommand _closeWarning;
    private ICommand _openOption;
    private ICommand _openGeodatabase;

    private NetworkDiagram _diagram;
    private DiagramLayer _diagramLayer;
    private Geodatabase _newGeodatabase;
    private SpatialReference _reference;
    private DataDDL.FeatureDatasetDescription _featureDatasetDesc;
    private IReadOnlyList<NetworkSource> _networkSources;
    private GeodatabaseType _originGeodatabaseType;
    private Geodatabase _originGeodatabase;
    private List<string> _createdFeatureClass;

    private const string _Aggregations = "Aggregations";
    private const string _DiagramGUIDField = "DiagramGuid";
    private const string _AssociatedObjectSrcName = "AssociatedSourceName";
    private const string _AssociatedObjectGuid = "AssociatedObjectGUID";
    private const string _AggregationType = "AggregationType";
    private const string _AggregationDEID = "AggregationDEID";

    private const string _connectivityLabel = "Connectivity";
    private const string _structuralLabel = "Structural Attachment";
    private const string _associationLayerName = "Associations";

    private Map _originMap;
    private Map _newMap;
    private ILayerContainerEdit _activeGroupLayer;
    private SubtypeGroupLayer _activeSubTypeGroupLayer;

    private List<CustomField> _activeCustomFields;
    private List<CustomField> _associationFieldNames;

    private FeatureClass _activeFeatureClass;
    private FeatureClass _activeDestination;
    private AssetGroup _activeAssetGroup;

    private string _featureDatasetName;
    private string _diagramGuidFieldName;
    private string _diagramGuid;
    private string _activeFeatureClassName;
    private string _activeFeatureClassAliasName;
    private string _originAggregationTableName;

    private Dictionary<int, string> _networkSourceList;
    #endregion

    #region Initialization and Events
    /// <summary>
    /// Constructor
    /// </summary>
    protected ExportPaneViewModel() { }

    /// <summary>
    /// Show the Export Diagram To Feature Classes pane window
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    /// <summary>
    /// Initialize commands and private fields in the Export Diagram To Feature Classes pane window
    /// </summary>
    /// <returns>Task</returns>
    protected override Task InitializeAsync()
    {
      _setFolder = new RelayCommand(() => SetFolderInternal());
      _execute = new RelayCommand(() => ExecuteInternal());
      _closeInformation = new RelayCommand(() => CloseInformationInternal());
      _closeWarning = new RelayCommand(() => CloseWarningInternal());
      _createdFeatureClass = new();

      _openOption = new RelayCommand(() => OpenOptionInternal());
      _openGeodatabase = new RelayCommand(() => OpenGeodatabaseInternal());

#if (DEBUG)
      _name = "Test";
      _createMap = true;
#endif

      _folder = Project.Current.HomeFolderPath;
      ProjectOpenedEvent.Subscribe(OnProjectOpened);
      ProjectClosingEvent.Subscribe(OnProjectClosingEvent);
      _warning = new();
      _information = new();
      return base.InitializeAsync();
    }

    /// <summary>
    /// Clean and hide the Export Diagram To Feature Classes pane window at the project closure
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private Task OnProjectClosingEvent(ProjectClosingEventArgs arg)
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane != null)
        pane.Hide();

      Clean();
      _newGeodatabase = null;

      return null;
    }

    /// <summary>
    /// Delete the class objects
    /// </summary>
    /// <returns>Task</returns>
    protected override Task UninitializeAsync()
    {
      ProjectOpenedEvent.Unsubscribe(OnProjectOpened);
      ProjectClosingEvent.Subscribe(OnProjectClosingEvent);
      Clean();
      _newGeodatabase = null;

      return base.UninitializeAsync();
    }

    /// <summary>
    /// Retrieve the home project folder path at the project opening
    /// </summary>
    /// <param name="obj"></param>
    private void OnProjectOpened(ProjectEventArgs obj)
    {
      Folder = obj.Project.HomeFolderPath;
      _warning = new();
      _information = new();
      NotifyPropertyChanged(() => Information);
      NotifyPropertyChanged(() => Warning);
    }

    /// <summary>
    /// Clean the private fields
    /// </summary>
    internal void Clean()
    {
      _diagram = null;
      _diagramLayer = null;
      _diagramGuid = null;
      _reference = null;
      _featureDatasetDesc = null;
      _networkSources = null;
      _createdFeatureClass = null;
      _activeGroupLayer = null;
      _activeSubTypeGroupLayer = null;
      _activeCustomFields = null;
      _associationFieldNames = null;
      _activeFeatureClass = null;
      _activeDestination = null;
      _activeAssetGroup = null;
      _originMap = null;
      _newGeodatabase = null;
      _newMap = null;
      _originAggregationTableName = null;
      _networkSourceList = null;

      _canceledStatus = "";
      _warning = new();
      _information = new();
      NotifyPropertyChanged(() => Information);
      NotifyPropertyChanged(() => Warning);
    }
    #endregion 

    #region Pane properties
    /// <summary>
    /// Open a dialog box to specify the Output Geodatabase's Folder
    /// </summary>
    private void SetFolderInternal()
    {
      OpenItemDialog openDialog = new()
      {
        Title = "Output Folder",
        MultiSelect = false,
        Filter = ItemFilters.Folders
      };

      if (openDialog.ShowDialog() == true)
      {
        IEnumerable<Item> selectedItem = openDialog.Items;
        foreach (Item i in selectedItem)
        {
          Folder = i.Path;
        }
      }
    }

    /// <summary>
    /// Folder path
    /// </summary>
    public string Folder
    {
      get { return _folder; }
      set
      {
        SetProperty(ref _folder, value, () => Folder);
      }
    }

    /// <summary>
    /// Flag to manage the Output Geodatabase section state
    /// </summary>
    public System.Windows.Visibility IsGeodataseVisible
    {
      get { return _isGeodataseVisible; }

    }

    /// <summary>
    /// Flag to manage the Options section state
    /// </summary>
    public System.Windows.Visibility IsOptionVisible
    {
      get { return _isOptionVisible; }
    }

    /// <summary>
    /// Expand or collapse the Options section
    /// </summary>
    private void OpenOptionInternal()
    {
      if (_isOptionVisible == System.Windows.Visibility.Collapsed)
      {
        _isOptionVisible = System.Windows.Visibility.Visible;
        _optionLabel = "v";
      }
      else
      {
        _isOptionVisible = System.Windows.Visibility.Collapsed;
        _optionLabel = ">";
      }

      NotifyPropertyChanged(() => OptionLabel);
      NotifyPropertyChanged(() => IsOptionVisible);
    }

    /// <summary>
    /// Expand or collapse the Output Geodatabse section
    /// </summary>
    private void OpenGeodatabaseInternal()
    {
      if (_isGeodataseVisible == System.Windows.Visibility.Collapsed)
      {
        _isGeodataseVisible = System.Windows.Visibility.Visible;
        _geodatabaseLabel = "v";
      }
      else
      {
        _isGeodataseVisible = System.Windows.Visibility.Collapsed;
        _geodatabaseLabel = ">";
      }

      NotifyPropertyChanged(() => GeodatabaseLabel);
      NotifyPropertyChanged(() => IsGeodataseVisible);
    }

    /// <summary>
    /// Display a > or a V depending on the Options section state
    /// </summary>
    public string OptionLabel
    {
      get { return _optionLabel; }
    }

    /// <summary>
    /// Display a > or a V depending on the Output Geodatabase section state
    /// </summary>
    public string GeodatabaseLabel
    { get { return _geodatabaseLabel; } }

    /// <summary>
    /// Flag used to manage the Export aggregations option
    /// </summary>
    public bool ExportAggregation
    {
      get { return _exportAggregation; }
      set { SetProperty(ref _exportAggregation, value, () => ExportAggregation); }
    }

    /// <summary>
    /// Output Geodatabase's Type drop down list
    /// </summary>
    public static List<string> TypeList
    {
      get { return new() { _FileGeo, _MobileGeo }; }
    }

    /// <summary>
    /// Get or set the output geodatabase type
    /// </summary>
    public string TypeGeodatabase
    {
      get { return _typeGeodatabase; }
      set
      {
        SetProperty(ref _typeGeodatabase, value, () => TypeGeodatabase);
      }
    }

    /// <summary>
    /// Output Geodatabase's Name
    /// </summary>
    public string Name
    {
      get { return _name; }
      set
      {
        SetProperty(ref _name, value, () => Name);
      }
    }

    /// <summary>
    /// Flag used to manage the Add to a new map option
    /// </summary>
    public bool CreateMap
    {
      get => _createMap;
      set
      {
        SetProperty(ref _createMap, value, () => CreateMap);
      }
    }

    /// <summary>
    /// Warning message raised up once the process completes
    /// </summary>
    public string Warning
    {
      get
      {
        if (_warning == null)
        { _warning = new(); }

        NotifyPropertyChanged(() => IsWarningVisible);
        return FormatErrors(_warning);
      }
    }

    /// <summary>
    /// Diplayed informative message
    /// </summary>
    public string Information
    {
      get
      {
        if (_information == null)
        { _information = new(); }

        NotifyPropertyChanged(() => IsInformationVisible);
        return FormatErrors(_information);
      }
    }

    /// <summary>
    /// Manage informative message visibility
    /// </summary>
    public System.Windows.Visibility IsInformationVisible
    {
      get
      {
        if (_information.Any())
        {
          return System.Windows.Visibility.Visible;
        }
        else
        {
          return System.Windows.Visibility.Collapsed;
        }
      }
    }

    /// <summary>
    /// Diplayed warning message
    /// </summary>
    public System.Windows.Visibility IsWarningVisible
    {
      get
      {
        if (_warning.Any())
        {
          return System.Windows.Visibility.Visible;
        }
        else
        {
          return System.Windows.Visibility.Collapsed;
        }
      }
    }

    /// <summary>
    /// Hide the informative message
    /// </summary>
    private void CloseInformationInternal()
    {
      _information = new();
      NotifyPropertyChanged(() => Information);
    }

    /// <summary>
    /// Hide the warning message
    /// </summary>
    private void CloseWarningInternal()
    {
      _warning = new();
      NotifyPropertyChanged(() => Warning);
    }
    #endregion

    #region Execute
    /// <summary>
    /// Internal execution
    /// </summary>
    private async void ExecuteInternal()
    {
      Clean();

      _warning = new();
      _information = new();

      if (string.IsNullOrEmpty(_folder))
      {
        _warning.Add("The folder name is empty");
      }
      if (string.IsNullOrEmpty(Name))
      {
        _warning.Add("The database name is empty");
      }

      if (MapView.Active == null || MapView.Active.Map == null)
      {
        _warning.Add("There is no active map");
      }

      NotifyPropertyChanged(() => Warning);
      NotifyPropertyChanged(() => Information);

      if (_warning.Count > 0)
      {
        return;
      }

      _originMap = MapView.Active.Map;
      _canceledStatus = "";

      var pd = new ProgressDialog("Export Diagram To Geodatabase", "Canceled", 6, false);

      DateTime startTime = DateTime.Now;
      _information.Add(string.Format("Start at {0}", startTime.ToLongTimeString()));
      NotifyPropertyChanged(() => Information);

      try
      {
        await RunCancelableExportDiagramToGeodatabase(new CancelableProgressorSource(pd));

        if (!string.IsNullOrEmpty(_canceledStatus))
          ShowMessage(_canceledStatus, "Errors");
      }
      catch (Exception ex)
      {
        ShowMessage(ExceptionFormat(ex), "Exception");
      }

      NotifyPropertyChanged(() => Warning);
      DateTime endTime = DateTime.Now;
      _information.Add(string.Format("End at {0}", endTime.ToLongTimeString()));
      _information.Add(string.Format("Elapsed time {0}", (endTime - startTime).ToString(@"hh\:mm\:ss")));

      NotifyPropertyChanged(() => Information);
    }

    /// <summary>
    /// Show a cancelable window during execution
    /// </summary>
    /// <param name="Cps">Cancelable Progressor Source</param>
    /// <returns>Task</returns>
    private async Task RunCancelableExportDiagramToGeodatabase(CancelableProgressorSource Cps)
    {
      Cps.Progressor.Max = GetNumberOfOperation();
      Cps.Progressor.Value = 0;

      await QueuedTask.Run(() =>
      {
        if (CancelOrIncreaseProgressor(Cps, "Create new geodatabase"))
        {
          return;
        }

        _canceledStatus = CreateNewDatabase();
      }, Cps.Progressor);

      await QueuedTask.Run(() =>
      {
        if (CancelOrIncreaseProgressor(Cps, "Create new feature dataset"))
        {
          return;
        }

        _canceledStatus = CreateFeatureDataset();
      }, Cps.Progressor);

      await QueuedTask.Run(() =>
      {
        if (_createMap)
        {
          if (CancelOrIncreaseProgressor(Cps, "Create new map"))
          {
            return;
          }

          _newMap = CreateNewMap(_featureDatasetName, false).Result;
          if (_newMap == null)
          {
            _warning.Add("Error when creating the new map, turn off Add to a new map");
            CreateMap = false;
            Cps.Progressor.Max = (uint)(_originMap.GetLayersAsFlattenedList().Count * 2 + 2);
          }
          else
          {
            while (_newMap.Layers.Count > 0)
            {
              _newMap.RemoveLayer(_newMap.Layers[0]);
            }
          }
        }
      }, Cps.Progressor);

      await QueuedTask.Run(() =>
      {
        if (CancelOrIncreaseProgressor(Cps, "Export layer definitions"))
        {
          return;
        }

        _createdFeatureClass = new();

        foreach (Layer layer in _diagramLayer.Layers)
        {
          _activeCustomFields = new();
          _activeGroupLayer = null;
          ExportLayer(LayerToExport: layer, Cps: Cps, ParentLayer: _newMap);
          if (!string.IsNullOrEmpty(_canceledStatus))
          {
            return;
          }
        }

        if (_createMap)
        {
          ReorderGroupLayer(_diagramLayer, _newMap);
        }
        _createdFeatureClass = null;
        _activeCustomFields = null;
        _associationFieldNames = null;
        _activeGroupLayer = null;
      }, Cps.Progressor);

      await QueuedTask.Run(() =>
      {
        if (_exportAggregation)
        {
          if (CancelOrIncreaseProgressor(Cps, "Export aggregations"))
          {
            return;
          }

          Cps.Progressor.Max += 2;
          Table aggregationTable = CreateAndFillAggregationTable(Cps);

          if (!string.IsNullOrEmpty(_canceledStatus))
          {
            return;
          }

          if (_createMap)
          {
            AddTableLayerToMap(_newMap, _Aggregations, aggregationTable, string.Format("{0}='{1}'", _DiagramGUIDField, _diagramGuid), _DiagramGUIDField);
          }
        }
      }, Cps.Progressor);

      if (_createMap && _newMap != null && string.IsNullOrEmpty(_canceledStatus))
      {
        IMapPane mapPane = await ShowMap(_newMap);

        if (mapPane != null)
        {
          await QueuedTask.Run(() =>
          {
            try
            {
              mapPane.MapView.ZoomTo(_originMap.GetDefaultExtent());
            }
            catch
            { }
          });
        }
      }
    }
    #endregion

    #region Create and fill geodatabase
    /// <summary>
    /// Create the new output geodatabase
    /// </summary>
    /// <returns>Errors</returns>
    /// <remarks>During this operation, we retrieve the network diagram, set the datasource and some private fields</remarks>
    private string CreateNewDatabase()
    {
      _diagram = _diagramLayer.GetNetworkDiagram();
      if (_diagram == null) return "Network diagram not found";

      NetworkDiagramInfo info = _diagram.GetDiagramInfo();
      Envelope networkExtent = info.NetworkExtent;
      _featureDatasetName = info.IsStored ? _diagram.Name.Replace(" ", "").Replace('-', '_') : "Temporary";
      _reference = networkExtent.SpatialReference;
      if (_reference == null)
      {
        _reference = info.DiagramExtent.SpatialReference;
      }
      if (_reference == null)
      {
        _reference = _diagramLayer.GetSpatialReference();
      }

      if (_reference == null)
      {
        return "Fail to get the spatial reference";
      }

      UtilityNetwork utilityNetwork = _diagram.DiagramManager.GetNetwork<UtilityNetwork>();
      try
      {
        _networkSources = utilityNetwork.GetDefinition().GetNetworkSources();
      }
      catch
      {
        _networkSources = new List<NetworkSource>();

        _networkSourceList = new();
        JObject diagramContent = JObject.Parse(_diagram.GetContent(addAggregations: false, addAttributes: false, addDiagramInfo: false, addGeometries: false));

        JToken source = diagramContent["sourceMapping"];

        foreach (JProperty item in source)
        {
          string name = item.Name;
          int id = Convert.ToInt32(item.Value);

          if (name.Contains('.'))
          {
            name = name[(name.LastIndexOf('.') + 1)..];
          }

          if (name.Contains("TN_", StringComparison.OrdinalIgnoreCase) || name.Contains("UN_", StringComparison.OrdinalIgnoreCase))
          {
            name = name[(name.LastIndexOf('_') + 1)..];
          }
          _networkSourceList.Add(id, name);
        }
      }

      _originGeodatabase = utilityNetwork.GetDatastore() as Geodatabase;
      _originGeodatabaseType = _originGeodatabase.GetGeodatabaseType();

      string newGeodatabaseName;

      if (TypeGeodatabase == _FileGeo)
      {
        newGeodatabaseName = Path.Combine(Folder, string.Format("{0}.gdb", Name));

        if (_newGeodatabase == null || !_newGeodatabase.GetPath().ToString().Contains(newGeodatabaseName.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase))
        {
          try
          {
            _newGeodatabase = DataDDL.SchemaBuilder.CreateGeodatabase(new FileGeodatabaseConnectionPath(new Uri(newGeodatabaseName)));
          }
          catch
          {
            _newGeodatabase = new(new FileGeodatabaseConnectionPath(new Uri(newGeodatabaseName)));
          }
        }
      }
      else
      {
        newGeodatabaseName = Path.Combine(Folder, string.Format("{0}.geodatabase", Name));

        if (_newGeodatabase == null || !_newGeodatabase.GetPath().ToString().Contains(newGeodatabaseName.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase))
        {
          try
          {
            _newGeodatabase = DataDDL.SchemaBuilder.CreateGeodatabase(new MobileGeodatabaseConnectionPath(new Uri(newGeodatabaseName)));
          }
          catch
          {
            _newGeodatabase = new(new MobileGeodatabaseConnectionPath(new Uri(newGeodatabaseName)));
          }
        }
      }

      return "";
    }

    /// <summary>
    /// Create the output feature dataset for the network diagram to export
    /// </summary>
    /// <returns>Errors</returns>
    private string CreateFeatureDataset()
    {
      DataDDL.SchemaBuilder Builder = new(_newGeodatabase);

      int index = 0;
      string newName = _featureDatasetName;
      while (true)
      {
        _featureDatasetDesc = new(newName, _reference);

        Builder.Create(_featureDatasetDesc);

        if (!Builder.Build())
        {
          if (FormatErrors(Builder.ErrorMessages).Contains("The item with path already exists"))
          {
            index++;
            newName = string.Format("{0}_{1}", _featureDatasetName, index);
          }
          else
          {
            break;
          }
        }
        else
        {
          _featureDatasetName = newName;
          break;
        }
      }

      return FormatErrors(Builder.ErrorMessages);
    }

    /// <summary>
    /// Create a CustomField list and a FieldDescription list for the new feature class
    /// </summary>
    /// <param name="ClassDefinition">Network source feature class definition</param>
    /// <param name="NewDescriptionList">Field description list</param>
    private void FillCustomFields(FeatureClassDefinition ClassDefinition, out List<DataDDL.FieldDescription> NewDescriptionList)
    {
      NewDescriptionList = new();
      CustomField customField = null;
      foreach (Field field in ClassDefinition.GetFields())
      {
        if (field.FieldType == FieldType.GlobalID || field.FieldType == FieldType.OID)
        { continue; }

        if (field.FieldType == FieldType.Geometry)
        {
          _activeCustomFields.Add(new(field));
          continue;
        }
        if (field.Name.Contains("SHAPE", StringComparison.OrdinalIgnoreCase))
        { continue; }

        try
        {
          Debug.WriteLine(field.Name);

          customField = new(field);
          _activeCustomFields.Add(customField);

          DataDDL.FieldDescription NewDescription = new(customField.NewName, customField.NewType);
          NewDescriptionList.Add(NewDescription);

          NewDescription.Length = customField.Length;
          NewDescription.Precision = customField.Precision;
          NewDescription.AliasName = customField.AliasName;
          if (customField.HasDefaultValue)
            NewDescription.DefaultValue = customField.DefaultValue;
          NewDescription.Scale = customField.Scale;

          if (customField.IsAssetGroup)
          {
            NetworkSource source;
            if (_originGeodatabaseType == GeodatabaseType.Service)
            {
              source = _networkSources.FirstOrDefault(a => a.Name.Replace(" ", "").Contains(_activeFeatureClassAliasName, StringComparison.OrdinalIgnoreCase));
              if (source == null && _activeFeatureClassAliasName.Contains(" ("))
              {
                string newName = _activeFeatureClassAliasName[.._activeFeatureClassAliasName.IndexOf(" (")].Trim();
                source = _networkSources.FirstOrDefault(a => a.Name.Replace(" ", "").Contains(newName, StringComparison.OrdinalIgnoreCase));
              }

              if (source == null)
              {
                source = _networkSources.FirstOrDefault(a => a.Name.Replace(" ", "").Contains(_activeFeatureClassName, StringComparison.OrdinalIgnoreCase));
              }
            }
            else
            {
              source = _networkSources.FirstOrDefault(a => a.Name.Contains(_activeFeatureClassName, StringComparison.OrdinalIgnoreCase));
            }

            if (source == null && _activeFeatureClassName.Contains('_'))
            {
              if (_originGeodatabaseType == GeodatabaseType.Service)
              {
                string newName = _activeFeatureClassName[.._activeFeatureClassName.IndexOf('_')].Trim();
                source = _networkSources.FirstOrDefault(a => a.Name.Replace(" ", "").Contains(newName, StringComparison.OrdinalIgnoreCase));

                if (source == null && _activeFeatureClassAliasName.Contains(" ("))
                {
                  newName = _activeFeatureClassAliasName[.._activeFeatureClassAliasName.IndexOf(" (")].Trim();
                  source = _networkSources.FirstOrDefault(a => a.Name.Replace(" ", "").Contains(newName, StringComparison.OrdinalIgnoreCase));
                }
              }
              else
              {
                string newName = _activeFeatureClassName[.._activeFeatureClassName.IndexOf('_')];
                source = _networkSources.FirstOrDefault(a => a.Name.Contains(newName, StringComparison.OrdinalIgnoreCase));
              }
            }

            if (source != null)
            {
              customField.AssetGroups = source.GetAssetGroups();
            }
            else
            {

            }
          }

          if (NewDescriptionList.FirstOrDefault(a => a.Name.Contains(_DiagramGUIDField, StringComparison.OrdinalIgnoreCase)) == null)
          {
            customField = new(_DiagramGUIDField, FieldType.GUID);
            _activeCustomFields.Add(customField);
            NewDescriptionList.Add(new DataDDL.FieldDescription(_DiagramGUIDField, FieldType.GUID));
          }
        }
        catch (Exception ex)
        {
          string msg = ExceptionFormat(ex);
          _warning.Add(string.Format("Error {0} when FillFieldNames {1}", ex.Message, customField.NewName));
        }
      }
    }

    /// <summary>
    /// Create an output feature class
    /// </summary>
    /// <param name="FieldDescriptions">Field description list</param>
    /// <param name="TypeGeometry">Geometry type</param>
    /// <returns>Errors</returns>
    private string CreateFeatureClass(List<DataDDL.FieldDescription> FieldDescriptions, GeometryType TypeGeometry)
    {
      int index = 0;
      string newName = _activeFeatureClassName;
      try
      {
        while (true)
        {
          OpenDataset<FeatureClass>(_newGeodatabase, newName);
          index++;
          newName = string.Format("{0}_{1}", _activeFeatureClassName, index);
        };
      }
      catch { }

      DataDDL.ShapeDescription shapeDesc = new(TypeGeometry, _reference);
      if (_networkSources.Any())
      {
        shapeDesc.HasM = true;
        shapeDesc.HasZ = true;
      }
      else
      {
        shapeDesc.HasM = false;
        shapeDesc.HasZ = false;
      }


      DataDDL.FieldDescription fieldDescription = FieldDescriptions.FirstOrDefault(a => string.Compare(a.Name, "TierRank", true) == 0);

      DataDDL.FeatureClassDescription newDescription = new(newName, FieldDescriptions, shapeDesc);
      newDescription.AliasName = _activeFeatureClassAliasName;

      DataDDL.SchemaBuilder builder = new(_newGeodatabase);
      builder.Create(_featureDatasetDesc, newDescription);

      if (!builder.Build())
      {
        _activeDestination = null;
        return FormatErrors(builder.ErrorMessages);
      }

      try
      {
        _activeDestination = OpenDataset<FeatureClass>(_newGeodatabase, newName);
      }
      catch (Exception ex)
      {
        _activeDestination = null;
        return ExceptionFormat(ex);
      }

      _createdFeatureClass.Add(_activeFeatureClassName);

      return "";
    }

    /// <summary>
    /// Fill the new output feature class from the network source feature class
    /// </summary>
    /// <param name="Origin">Network source feature class</param>
    private void FillFeatureClass(FeatureClass Origin)
    {
      RowCursor originCursor;
      QueryFilter query;
      bool useFilter = false;

      if (_originGeodatabaseType == GeodatabaseType.Service)
      {
        originCursor = Origin.Search();
      }
      else if (string.Compare(_activeFeatureClassName, "ReductionEdges", true) == 0)
      {
        query = new();
        query.WhereClause = string.Format("{0}='{1}' AND ASSOCIATEDOBJECTID = -1", _diagramGuidFieldName, _diagramGuid);
        try
        {
          originCursor = Origin.Search(query);
        }
        catch
        {
          originCursor = Origin.Search();
          useFilter = true;
        }

      }
      else
      {
        query = new();
        query.WhereClause = string.Format("{0}='{1}'", _diagramGuidFieldName, _diagramGuid);
        try
        {
          originCursor = Origin.Search(query);
        }
        catch
        {
          originCursor = Origin.Search();
          useFilter = true;
        }
      }

      if (originCursor == null)
      {
        _canceledStatus = string.Format("Error opening row cursor for _activeFeatureClass {0}", Origin.GetName());
        return;
      }

      query = new();
      query.WhereClause = "1=-1";
      RowCursor _activeDestinationCursor = _activeDestination.Search(query);
      IReadOnlyList<Field> fields = _activeDestinationCursor.GetFields();

      Field shapeField = null;

      foreach (Field field in _activeDestinationCursor.GetFields())
      {
        string fieldName = field.Name;
        if (fieldName.Contains('.'))
        {
          fieldName = fieldName[(fieldName.IndexOf('.') + 1)..];
        }

        CustomField custom = GetCustomField(field.Name, true);
        if (custom != null)
        {
          custom.NewModelName = field.Name;
        }
        if (field.FieldType == FieldType.Geometry && shapeField == null)
        {
          shapeField = field;
        }
      }

      GeometryType featureType = _activeDestination.GetDefinition().GetShapeType();
#if (DEBUG)
      int count = 0;
#endif

      while (originCursor.MoveNext())
      {
        if (useFilter)
        {
          string currentGuid = originCursor.Current[_DiagramGUIDField].ToString();
          if (string.Compare(currentGuid, _diagramGuid, true) != 0)
          {
            continue;
          }
        }
        AssetGroup assetGroup = null;
        RowBuffer rowBuffer = _activeDestination.CreateRowBuffer();
        foreach (CustomField customField in _activeCustomFields)
        {
          if (string.Compare(customField.NewName, _DiagramGUIDField, true) == 0)
          {
            rowBuffer[_DiagramGUIDField] = _diagramGuid;
            continue;
          }
          object obj = originCursor.Current[customField.OriginName];

          if (obj != null)
          {
            if (customField.IsSourceName)
            {
              string sourcName = GetSourceName((int)obj);
              rowBuffer[customField.NewModelName] = sourcName;
            }
            else if (customField.IsAssetGroup)
            {
              assetGroup = customField.GetAssetGroup((int)obj);
              rowBuffer[customField.NewModelName] = assetGroup == null ? obj.ToString() : assetGroup.Name;
            }
            else if (customField.IsAssetType)
            {
              if (assetGroup == null)
              {
                rowBuffer[customField.NewModelName] = obj.ToString();
              }
              else
              {
                int assetCode = Convert.ToInt32(obj);
                AssetType assetType = assetGroup.GetAssetTypes().FirstOrDefault(a => a.Code == assetCode);
                if (assetType == null)
                {
                  rowBuffer[customField.NewModelName] = obj.ToString();
                }
                else
                {
                  rowBuffer[customField.NewModelName] = assetType.Name;
                }
              }
            }
            else if (customField.IsAssociationType)
            {
              int objValue = Convert.ToInt32(obj);
              if (customField.NewType == FieldType.Integer)
              {
                rowBuffer[customField.NewModelName] = objValue;
              }
              else if (objValue == 1)
              {
                rowBuffer[customField.NewModelName] = _connectivityLabel;
              }
              else if (objValue == 3)
              {
                rowBuffer[customField.NewModelName] = _structuralLabel;
              }
              else
              {
                rowBuffer[customField.NewModelName] = obj.ToString();
              }
            }
            else if (customField.HasDomain)
            {
              try
              {
                rowBuffer[customField.NewModelName] = customField.GetDomainValue(obj);
              }
              catch (Exception ex)
              {
                rowBuffer[customField.NewModelName] = obj.ToString();
                string msg = ExceptionFormat(ex);

                _warning.Add(string.Format("Error {0} when converting field in Fill_activeFeatureClass {1}, Row {2}", ex.Message, _activeDestination.GetName(), originCursor.Current["OBJECTID"]));
              }
            }
            else
            {
              try
              {
                switch (customField.NewType)
                {
                  case FieldType.Integer:
                    rowBuffer[customField.NewModelName] = Convert.ToInt32(obj);
                    break;

                  case FieldType.SmallInteger:
                    rowBuffer[customField.NewModelName] = Convert.ToInt16(obj);
                    break;

                  case FieldType.Date:
                    rowBuffer[customField.NewModelName] = Convert.ToDateTime(obj);
                    break;

                  case FieldType.Single:
                    rowBuffer[customField.NewModelName] = Convert.ToSingle(obj);
                    break;

                  case FieldType.Double:
                    rowBuffer[customField.NewModelName] = Convert.ToDouble(obj);
                    break;

                  case FieldType.GUID:
                    rowBuffer[customField.NewModelName] = obj;
                    break;

                  case FieldType.Geometry:
                    Geometry shape;
                    if (featureType == GeometryType.Point)
                    {
                      shape = (MapPoint)obj;
                    }
                    else if (featureType == GeometryType.Polygon)
                    {
                      shape = (Polygon)obj;
                    }
                    else
                    {
                      shape = (Polyline)obj;
                    }
                    rowBuffer[customField.NewModelName] = shape;
                    break;

                  case FieldType.String:
                  default:
                    rowBuffer[customField.NewModelName] = obj.ToString();
                    break;
                }
              }
              catch
              { }
            }
          }
        }

        try
        {
          _activeDestination.CreateRow(rowBuffer);
#if (DEBUG)
          count++;
#endif
        }
        catch (Exception ex)
        {
          string msg = ExceptionFormat(ex);

          _warning.Add(string.Format("Error {0} when Fill FeatureClass {1}", ex.Message, _activeDestination.GetName()));
        }
      }

#if (DEBUG)
      _information.Add(string.Format("Insert {0} lines in {1}", count, _activeFeatureClassAliasName));
      NotifyPropertyChanged(() => Information);
#endif

    }

    /// <summary>
    /// Create an output table
    /// </summary>
    /// <param name="SourceName">Source table name</param>
    /// <param name="NewFieldsDescription">Field description list</param>
    /// <returns>Errors</returns>
    private string CreateTable(string SourceName, List<DataDDL.FieldDescription> NewFieldsDescription)
    {
      DataDDL.SchemaBuilder Builder = new(_newGeodatabase);
      int index = 0;
      string newName = SourceName;

      while (true)
      {
        DataDDL.TableDescription FCDescription = new(newName, NewFieldsDescription);

        Builder.Create(FCDescription);

        if (!Builder.Build())
        {
          if (FormatErrors(Builder.ErrorMessages).Contains("The table already exists"))
          {
            index++;
            newName = string.Format("{0}_{1}", SourceName, index);
          }
        }
        else
        {
          break;
        }
      }
      _canceledStatus = FormatErrors(Builder.ErrorMessages);
      return newName;
    }

    /// <summary>
    /// Create and fill that aggregation table
    /// </summary>
    /// <param name="Cps">Cancelable progressor source</param>
    /// <returns>Table</returns>
    private Table CreateAndFillAggregationTable(CancelableProgressorSource Cps)
    {
      if (CancelOrIncreaseProgressor(Cps, "Create the Aggregation table"))
        return null;

      Table originAggregationTable = null;
      try
      {
        originAggregationTable = OpenDataset<Table>(_originGeodatabase, _originAggregationTableName);
      }
      catch { }

      List<DataDDL.FieldDescription> fieldsDescription = new();
      fieldsDescription.Add(new(_DiagramGUIDField, FieldType.String));
      fieldsDescription.Add(new(_AssociatedObjectSrcName, FieldType.String));
      fieldsDescription.Add(new(_AssociatedObjectGuid, originAggregationTable == null ? FieldType.String : FieldType.GUID));
      fieldsDescription.Add(new(_AggregationType, FieldType.String));
      fieldsDescription.Add(new(_AggregationDEID, FieldType.Integer));

      string newName = CreateTable(_Aggregations, fieldsDescription);

      if (CancelOrIncreaseProgressor(Cps, "Fill the Aggregation table"))
        return null;

      Table TableAggregations = OpenDataset<Table>(_newGeodatabase, newName);


      FillAggregations(TableAggregations, originAggregationTable);

      return TableAggregations;
    }

    /// <summary>
    /// Fill the Aggregation table
    /// </summary>
    /// <param name="newAggregationTable">New Aggregation table</param>
    /// <param name="originAggregationTable">Source Aggregation table</param>
    private void FillAggregations(Table newAggregationTable, Table originAggregationTable)
    {
      if (originAggregationTable == null)
      {
        FillAggregationByDiagramAggregation(newAggregationTable);
      }
      else
      {
        QueryFilter query = new();
        bool useFilter = false;
        _diagramGuidFieldName = "";

        foreach (Field field in originAggregationTable.GetDefinition().GetFields())
        {
          if (field.Name.Contains(_DiagramGUIDField, StringComparison.OrdinalIgnoreCase))
          {
            _diagramGuidFieldName = field.Name;
            break;
          }
        }

        if (string.IsNullOrEmpty(_diagramGuidFieldName))
        {
          _diagramGuidFieldName = _DiagramGUIDField;
        }

        RowCursor originCursor;
        query.SubFields = "*";
        query.WhereClause = string.Format("{0} = '{1}'", _diagramGuidFieldName, _diagramGuid);

        try
        {
          originCursor = originAggregationTable.Search(query);
        }
        catch
        {
          originCursor = originAggregationTable.Search();
          useFilter = true;
        }

        RowBuffer rowBuffer;
        while (originCursor.MoveNext())
        {
          var item = originCursor.Current;
          if (item != null)
          {
            if (useFilter)
            {
              string itemGuid = item[_DiagramGUIDField].ToString();
              Debug.WriteLine(itemGuid, _diagramGuid);
              if (!_diagramGuid.Contains(itemGuid, StringComparison.OrdinalIgnoreCase))
              {
                continue;
              }
            }
            rowBuffer = newAggregationTable.CreateRowBuffer();
            rowBuffer[_DiagramGUIDField] = item[_DiagramGUIDField].ToString(); //_diagramGuid;
            rowBuffer[_AssociatedObjectSrcName] = GetSourceName((int)item["AssociatedOBJECTSrcID"]);
            rowBuffer[_AssociatedObjectGuid] = item["AssociatedOBJECTGUID"];

            NetworkDiagramAggregationType typeAggreg = (NetworkDiagramAggregationType)Enum.Parse(typeof(NetworkDiagramAggregationType), item["AggregationType"].ToString());
            rowBuffer[_AggregationType] = typeAggreg.ToString();
            rowBuffer[_AggregationDEID] = item["AggregationDEID"];

            try
            {
              newAggregationTable.CreateRow(rowBuffer);
            }
            catch (Exception ex)
            {
              _canceledStatus = ExceptionFormat(ex);

              if (_canceledStatus.Contains("A requested row object could not be located", StringComparison.OrdinalIgnoreCase))
              {
                _canceledStatus = "";
              }
              else
              {
                return;
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Fill the Aggregation table using the Diagram Aggregation Function
    /// </summary>
    /// <param name="AggregationTable">Aggregation table</param>
    /// <remarks>Only use when the Aggregation Table can not retrieved. This can happen when working with published trace network.</remarks>
    private void FillAggregationByDiagramAggregation(Table AggregationTable)
    {
      RowBuffer rowBuffer;
      IReadOnlyList<DiagramAggregation> aggregations = _diagram.GetAggregations();
      foreach (DiagramAggregation aggregation in aggregations)
      {
        rowBuffer = AggregationTable.CreateRowBuffer();
        rowBuffer[_DiagramGUIDField] = _diagramGuid;
        rowBuffer[_AssociatedObjectSrcName] = GetSourceName(aggregation.AssociatedSourceID);
        rowBuffer[_AssociatedObjectGuid] = aggregation.AssociatedGlobalID.ToString();
        rowBuffer[_AggregationType] = aggregation.AggregationType.ToString();
        rowBuffer[_AggregationDEID] = aggregation.AggregatedBy;

        try
        {
          AggregationTable.CreateRow(rowBuffer);
        }
        catch (Exception ex)
        {
          _canceledStatus = ExceptionFormat(ex);

          if (_canceledStatus.Contains("A requested row object could not be located", StringComparison.OrdinalIgnoreCase))
          {
            _canceledStatus = "";
          }
          else
          {
            return;
          }
        }
      }

    }

    /// <summary>
    /// Create a feature class, fill it and create a layer when the Add to a new map option is turned on
    /// </summary>
    /// <param name="LayerToExport">Layer</param>
    /// <param name="Cps">Cancelable progressor source</param>
    /// <param name="ParentLayer">Parent layer</param>
    private void ExportLayer(Layer LayerToExport, CancelableProgressorSource Cps, ILayerContainerEdit ParentLayer = null)
    {
      if (LayerToExport == null)
        return;

      if (LayerToExport is FeatureLayer featureLayer)
      {
        if (CancelOrIncreaseProgressor(Cps, string.Format("Export layer {0}", LayerToExport.Name)))
          return;

        if (featureLayer == null)
        { return; }

        _activeFeatureClass = featureLayer.GetFeatureClass();
        if (_activeFeatureClass == null)
        { return; }

        if (_originGeodatabase == null)
        {
          _originGeodatabase = _activeFeatureClass.GetDatastore() as Geodatabase;
        }

        Layer parent = (Layer)LayerToExport.Parent;
        _activeFeatureClassAliasName = LayerToExport.Parent == null || parent is DiagramLayer ? LayerToExport.Name : parent.Name;

        if (_activeFeatureClassAliasName.Contains('.'))
        {
          _activeFeatureClassAliasName = _activeFeatureClassAliasName[(_activeFeatureClassAliasName.LastIndexOf('.') + 1)..];
        }

        GeometryType typeGeometry;
        if (string.Compare(_activeFeatureClassAliasName, "Reduction Edges", StringComparison.OrdinalIgnoreCase) == 0)
        {
          _activeFeatureClassName = "ReductionEdges";
          typeGeometry = GeometryType.Polyline;
        }
        else
        {
          _activeFeatureClassName = GetFeatureClassName(featureLayer.ShapeType, out typeGeometry);

          if (string.Compare(_activeFeatureClassName, "Associations", true) == 0)
          {
            _activeFeatureClassAliasName = _associationLayerName;
          }
        }

        if (_createdFeatureClass.Contains(_activeFeatureClassName))
        {
          if (_originGeodatabaseType == GeodatabaseType.Service && _activeFeatureClassName == _associationLayerName && parent is DiagramLayer)
          {
            if (_activeCustomFields == null || _activeCustomFields.Count == 0)
            {
              _activeCustomFields = _associationFieldNames;
            }
            FillFeatureClass(_activeFeatureClass);
            if (!string.IsNullOrEmpty(_canceledStatus))
            { return; }
          }
        }
        else if (_activeCustomFields == null || _activeCustomFields.Count == 0)
        {
          _activeCustomFields = new();

          FillCustomFields(_activeFeatureClass.GetDefinition(), out List<DataDDL.FieldDescription> fieldDescriptions);

          _canceledStatus = CreateFeatureClass(fieldDescriptions, typeGeometry);
          if (!string.IsNullOrEmpty(_canceledStatus))
          { return; }

          SetDiagramFieldName(featureLayer);

          FillFeatureClass(_activeFeatureClass);
          if (!string.IsNullOrEmpty(_canceledStatus))
          { return; }
        }

        if (_activeFeatureClassName == _associationLayerName)
        {
          if (_associationFieldNames == null)
          {
            _associationFieldNames = _activeCustomFields;
          }
          if (_activeCustomFields == null || _activeCustomFields.Count == 0)
          {
            _activeCustomFields = _associationFieldNames;
          }
        }

        if (_createMap)
        {
          //_subLabelToShow = new();
          string whereClause = "";
          string filterName = "";
          if (string.Compare(LayerToExport.Name, "Connectivity Associations", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(LayerToExport.Name, "Structural Attachments", StringComparison.OrdinalIgnoreCase) == 0)
          {
            CustomField associationField = GetCustomField("AssociationType", true);
            if (string.Compare(LayerToExport.Name, "Connectivity Associations", StringComparison.OrdinalIgnoreCase) == 0)
            {
              filterName = _connectivityLabel;
              if (associationField != null)
              {
                whereClause = string.Format("{0}= '{1}'", associationField.NewModelName, _connectivityLabel);
              }
              else
              {
                whereClause = "USERTYPE = '" + _connectivityLabel + "'";
              }
            }
            else
            {
              filterName = _structuralLabel;
              if (associationField != null)
              {
                whereClause = string.Format("{0}= '{1}'", associationField.NewModelName, _structuralLabel);
              }
              else
              {
                whereClause = "USERTYPE = '" + _structuralLabel + "'";
              }
            }
          }

          CreateLayer(LayerToExport: featureLayer, ParentLayer: ParentLayer, AssociatedFeatureClass: _activeDestination, WhereClause: whereClause, FilterName: filterName);
        }
      }
      else if (LayerToExport is SubtypeGroupLayer subTypeLayer)
      {
        if (_createMap)
        {
          //_subLabelToShow = new();
          _activeGroupLayer = CreateGroupLayerInMap(_newMap, subTypeLayer.Name);
          //_labelToShow.Add(subTypeLayer.Name, _subLabelToShow);
        }

        _activeCustomFields = new();
        _activeSubTypeGroupLayer = subTypeLayer;
        foreach (Layer layer in subTypeLayer.Layers)
        {
          ExportLayerSubType(LayerToExport: layer, Cps: Cps);
          if (!string.IsNullOrEmpty(_canceledStatus))
            return;
        }

        if (_createMap)
        {
          ReorderGroupLayer(Reference: subTypeLayer, GroupToReorder: _activeGroupLayer);
        }

        _activeSubTypeGroupLayer = null;
        _activeGroupLayer = null;
        _activeCustomFields = null;
      }
      else if (LayerToExport is GroupLayer groupLayer)
      {
        if (_createMap)
        {
          //_subLabelToShow = new();
          //_labelToShow.Add(groupLayer.Name, _subLabelToShow);
          _activeGroupLayer = CreateGroupLayerInMap(_newMap, groupLayer.Name);
        }

        _activeCustomFields = new();
        foreach (Layer layer in groupLayer.Layers)
        {
          ExportLayer(LayerToExport: layer, Cps: Cps, ParentLayer: groupLayer);
          if (!string.IsNullOrEmpty(_canceledStatus))
            return;
        }

        if (_createMap)
        {
          ReorderGroupLayer(Reference: groupLayer as ILayerContainerEdit, GroupToReorder: _activeGroupLayer);
        }

        _activeGroupLayer = null;
        _activeCustomFields = null;
      }
    }

    /// <summary>
    /// Create a feature Class, fill it and create the subtype group layers when the Add to a new map option is turned on
    /// </summary>
    /// <param name="LayerToExport">Layer</param>
    /// <param name="Cps">Cancelable progressor source</param>
    private void ExportLayerSubType(Layer LayerToExport, CancelableProgressorSource Cps)
    {
      if (LayerToExport == null)
      { return; }

      if (LayerToExport is FeatureLayer featureLayer)
      {
        if (CancelOrIncreaseProgressor(Cps, string.Format("Export layer {0}", LayerToExport.Name)))
        { return; }

        if (featureLayer == null)
        { return; }

        _activeFeatureClass = featureLayer.GetFeatureClass();

        if (_activeFeatureClass == null)
        { return; }

        if (_originGeodatabase == null)
        {
          _originGeodatabase = _activeFeatureClass.GetDatastore() as Geodatabase;
        }

        Layer parent = (Layer)LayerToExport.Parent;
        _activeFeatureClassAliasName = LayerToExport.Parent == null || parent is DiagramLayer ? LayerToExport.Name : parent.Name;

        _activeFeatureClassName = GetFeatureClassName(featureLayer.ShapeType, out GeometryType typeGeometry);

        if (!_createdFeatureClass.Contains(_activeFeatureClassName) || _activeCustomFields == null || _activeCustomFields.Count == 0)
        {
          _activeCustomFields = new();

          FillCustomFields(_activeFeatureClass.GetDefinition(), out List<DataDDL.FieldDescription> fieldDescriptions);

          if (CancelOrIncreaseProgressor(Cps, string.Format("Create and fill feature class {0}", _activeFeatureClassName)))
          { return; }

          _canceledStatus = CreateFeatureClass(fieldDescriptions, typeGeometry);
          if (!string.IsNullOrEmpty(_canceledStatus))
          { return; }

          SetDiagramFieldName(featureLayer);

          FillFeatureClass(_activeFeatureClass);
          if (!string.IsNullOrEmpty(_canceledStatus))
          { return; }
        }

        if (_createMap)
        {
          string layerName = LayerToExport.Name;
          string filterName = string.Format("{0}-{1}", _activeSubTypeGroupLayer.Name, layerName);

          CustomField customField = GetAssetGroupField();
          if (LayerToExport is FeatureLayer fl)
          {
            _activeAssetGroup = customField.AssetGroups.FirstOrDefault(a => a.Code == fl.SubtypeValue);
          }
          else
          {
            _activeAssetGroup = customField.GetAssetGroup(layerName);
          }

          string whereClause = string.Format("{0} = '{1}'", customField.NewName, _activeAssetGroup?.Name);

          CreateLayer(LayerToExport: featureLayer, ParentLayer: _activeGroupLayer, AssociatedFeatureClass: _activeDestination, WhereClause: whereClause, FilterName: filterName);
        }
      }
    }

    /// <summary>
    /// Get the new feature class name
    /// </summary>
    /// <param name="LayerGeometryType">Diagram layer geometry type</param>
    /// <param name="fcGeometryType">Network feature class geometry type</param>
    /// <returns>Network feature class name</returns>
    /// <remarks>A diagram feature and its related network feature can have different geometry type. For example, network container points represented as diagram container polygons.
    /// In this case, we add an extension to prevent any name conflict.</remarks>
    private string GetFeatureClassName(esriGeometryType LayerGeometryType, out GeometryType fcGeometryType)
    {
      _activeFeatureClassName = "";
      if (_originGeodatabaseType == GeodatabaseType.Service)
      {
        QueryFilter filter = new();
        filter.WhereClause = "1 = -1";
        RowCursor rowCursor = _activeFeatureClass.Search(filter);

        if (rowCursor != null)
        {
          foreach (Field field in rowCursor.GetFields())
          {
            string fieldName = field.Name;

            if (!fieldName.Contains("UN_", StringComparison.OrdinalIgnoreCase) && !fieldName.Contains("TN_", StringComparison.OrdinalIgnoreCase))
            {
              if (fieldName.Contains('.'))
              {
                _activeFeatureClassName = fieldName[..fieldName.LastIndexOf('.')];
                break;
              }
            }
            else if (fieldName.Contains("_Associations", StringComparison.OrdinalIgnoreCase))
            {
              _activeFeatureClassName = _associationLayerName;
              break;
            }
            else if (fieldName.Contains("SystemJunction", StringComparison.OrdinalIgnoreCase))
            {
              _activeFeatureClassName = "SystemJunctions";
              break;
            }
            else if (fieldName.Contains("_Junctions", StringComparison.OrdinalIgnoreCase) ||
                     fieldName.Contains("_Edges", StringComparison.OrdinalIgnoreCase) ||
                     fieldName.Contains("_Containers", StringComparison.OrdinalIgnoreCase) ||
                     fieldName.Contains("_TmpJunctions", StringComparison.OrdinalIgnoreCase) ||
                     fieldName.Contains("_TmpEdges", StringComparison.OrdinalIgnoreCase) ||
                     fieldName.Contains("_TmpContainers", StringComparison.OrdinalIgnoreCase))
            {
              continue;
            }
            else
            {
              _activeFeatureClassName = fieldName[..fieldName.LastIndexOf('.')];
              if (_activeFeatureClassName.Contains('_'))
              {
                _activeFeatureClassName = _activeFeatureClassName[(_activeFeatureClassName.LastIndexOf('_') + 1)..];
              }
              break;

            }
          }
        }
      }
      else
      {
        _activeFeatureClassName = _activeFeatureClass.GetName();
      }

      switch (LayerGeometryType)
      {
        case esriGeometryType.esriGeometryPolyline:
        case esriGeometryType.esriGeometryLine:
          fcGeometryType = GeometryType.Polyline;
          break;
        case esriGeometryType.esriGeometryPolygon:
          fcGeometryType = GeometryType.Polygon;
          break;
        default:
          fcGeometryType = GeometryType.Point;
          break;
      }

      if (string.IsNullOrEmpty(_activeFeatureClassName))
      {
        _activeFeatureClassName = _activeFeatureClassAliasName.Replace(" ", "");
      }
      else if (_activeFeatureClassName.Contains('.'))
      {
        _activeFeatureClassName = _activeFeatureClassName[(_activeFeatureClassName.LastIndexOf('.') + 1)..];
      }

      if (!_networkSources.Any())
      {
        return _activeFeatureClassName;
      }

      NetworkSource networkSource;

      if (_originGeodatabaseType == GeodatabaseType.Service)
      {
        networkSource = _networkSources.FirstOrDefault(a => a.Name.Replace(" ", "").Contains(_activeFeatureClassName, StringComparison.OrdinalIgnoreCase));
      }
      else
      {
        networkSource = _networkSources.FirstOrDefault(a => a.Name.Contains(_activeFeatureClassName, StringComparison.OrdinalIgnoreCase));
      }

      if (networkSource == null)
      {
        if (_activeFeatureClassName.Contains("_Edges", StringComparison.OrdinalIgnoreCase) || _activeFeatureClassName.Contains("_TmpEdges", StringComparison.OrdinalIgnoreCase))
        {
          _activeFeatureClassAliasName = "Reduction Edges";
          _activeFeatureClassName = "ReductionEdges";
        }
        else if (_activeFeatureClassName != "SystemJunctions" && _activeFeatureClassName != _associationLayerName)
        { return null; }
      }
      else
      {
        switch (networkSource.UsageType)
        {
          case SourceUsageType.Device:
          case SourceUsageType.Junction:
          case SourceUsageType.JunctionObject:
          case SourceUsageType.StructureJunctionObject:
          case SourceUsageType.StructureJunction:
          case SourceUsageType.Assembly:
            if (fcGeometryType == GeometryType.Polygon)
            {
              _activeFeatureClassName += "_C";
            }
            else if (fcGeometryType == GeometryType.Polyline)
            {
              _activeFeatureClassName += "_E";
            }
            break;

          case SourceUsageType.Line:
          case SourceUsageType.StructureLine:
          case SourceUsageType.EdgeObject:
            if (fcGeometryType == GeometryType.Polygon)
            {
              _activeFeatureClassName += "_C";
            }
            else if (fcGeometryType == GeometryType.Point)
            {
              _activeFeatureClassName += "_J";
            }
            break;

          case SourceUsageType.StructureBoundary:
            if (fcGeometryType == GeometryType.Point)
            {
              _activeFeatureClassName += "_J";
            }
            else if (fcGeometryType == GeometryType.Polyline)
            {
              _activeFeatureClassName += "_E";
            }
            break;

          case SourceUsageType.SystemJunction:
            _activeFeatureClassAliasName = "System Junctions";
            _activeFeatureClassName = "SystemJunctions";
            break;

          case SourceUsageType.Association:
            _activeFeatureClassAliasName = _associationLayerName;
            _activeFeatureClassName = _associationLayerName;
            break;

        }
      }

      return _activeFeatureClassName;
    }

    /// <summary>
    /// Set the qualified diagram field name, used to filter  
    /// </summary>
    /// <param name="LayerToExport">Layer to export</param>
    private void SetDiagramFieldName(FeatureLayer LayerToExport)
    {
      string queryDef;
      if (_originGeodatabaseType == GeodatabaseType.Service)
      {
        queryDef = LayerToExport.GetDataConnection().ToJson();

        if (!string.IsNullOrEmpty(queryDef))
        {
          int start = queryDef.IndexOf("definitionExpression") + 25;
          int end = queryDef.IndexOf('=', start) - 1;
          _diagramGuidFieldName = queryDef.AsSpan(start, end - start).ToString().Trim();

          if (string.IsNullOrEmpty(_diagramGuid))
          {
            start = queryDef.IndexOf("DIAGRAMGUID");
            start = queryDef.IndexOf("'{", start) + 1;
            end = queryDef.IndexOf('}', start) + 1;
            _diagramGuid = queryDef.AsSpan(start, end - start).ToString();
          }
        }
      }
      else
      {
        queryDef = LayerToExport.DefinitionQuery;

        if (!string.IsNullOrEmpty(queryDef))
        {
          int start = queryDef.IndexOf('=') - 1;
          _diagramGuidFieldName = queryDef[..start].ToString();

          if (string.IsNullOrEmpty(_diagramGuid))
          {
            start = queryDef.IndexOf('{');
            int end = queryDef.IndexOf('}') + 1;
            _diagramGuid = queryDef.AsSpan(start, end - start).ToString();
          }
        }
      }

      if (string.IsNullOrEmpty(_originAggregationTableName))
      {
        int index = _diagramGuidFieldName.IndexOf("UN_");

        if (index < 0)
        {
          index = _diagramGuidFieldName.IndexOf("TN_");
        }

        if (_diagram.GetDiagramInfo().IsStored)
        {
          _originAggregationTableName = _diagramGuidFieldName[..(_diagramGuidFieldName.IndexOf('_', index + 4) + 1)] + "Aggregations";
        }
        else
        {
          _originAggregationTableName = _diagramGuidFieldName[..(_diagramGuidFieldName.IndexOf('_', index + 4) + 1)] + "TmpAggregations";
        }
      }
    }

    /// <summary>
    /// Change SourceId to Source Name
    /// </summary>
    /// <param name="AssociatedSourceID">Associated Source ID</param>
    /// <returns>Source Name</returns>
    private string GetSourceName(int AssociatedSourceID)
    {
      switch (AssociatedSourceID)
      {
        case 0:
          return "Reduction Edge";
        case 1:
          return "Associations";
        case 2:
          return "System Junctions";
        default:
          if (_networkSources.Any())
          {
            NetworkSource source = _networkSources.FirstOrDefault(a => a.ID == AssociatedSourceID);
            if (source != null)
            {
              if (source.Name.Contains('.'))
                return source.Name[(source.Name.LastIndexOf('.') + 1)..];

              return source.Name;
            }

            return AssociatedSourceID.ToString();
          }
          else
          {
            if (_networkSourceList.TryGetValue(AssociatedSourceID, out string name))
            {
              return name;
            }
            return AssociatedSourceID.ToString();
          }
      }
    }
    #endregion

    #region Create map and symbology
    /// <summary>
    /// Create a feature layer
    /// </summary>
    /// <param name="LayerToExport">Layer to export</param>
    /// <param name="ParentLayer">Parent layer</param>
    /// <param name="AssociatedFeatureClass">Associated feature class</param>
    /// <param name="WhereClause">Layer definition query</param>
    /// <param name="FilterName">Filter name</param>
    /// <returns>Exported feature layer</returns>
    private FeatureLayer CreateLayer(FeatureLayer LayerToExport, ILayerContainerEdit ParentLayer, FeatureClass AssociatedFeatureClass, string WhereClause = "", string FilterName = "")
    {
      RendererDefinition rendererDefinition = GetNewRenderer(LayerToExport.GetRenderer(), LayerToExport.Name, out CIMRenderer newRenderer);

      FeatureLayer newLayer = AddFeatureLayerToGroup(ParentLayer: ParentLayer, Name: LayerToExport.Name, AssociatedFeatureClass: AssociatedFeatureClass, WhereClause: WhereClause, FilterName: FilterName, Renderer: rendererDefinition);

      if (newLayer == null)
      {
        _canceledStatus = string.Format("An error occured when creating the layer {0} in parent layer {1}", LayerToExport.Name, ((GroupLayer)ParentLayer).Name);
        return null;
      }

      SetLabelClassesAndPopupInfos(LayerToExport, newLayer);

      newLayer.SetRenderer(newRenderer);
      newLayer.SetExpanded(false);
      newLayer.SetLabelVisibility(LayerToExport.IsLabelVisible);
      newLayer.SetTransparency(LayerToExport.Transparency);
      newLayer.SetFeatureBlendingMode(LayerToExport.FeatureBlendingMode);
      newLayer.SetBlendingMode(LayerToExport.BlendingMode);

      return newLayer;
    }

    /// <summary>
    /// Reorder the layers under a layer group so they display in the same order as the original diagram map
    /// </summary>
    /// <param name="Reference">Original diagram layer</param>
    /// <param name="GroupToReorder">Group to reorder</param>
    private static void ReorderGroupLayer(ILayerContainerEdit Reference, ILayerContainerEdit GroupToReorder)
    {
      bool associationHasMoved = false;
      for (int i = 0; i < Reference.Layers.Count; i++)
      {
        Layer layer = Reference.Layers[i];

        Layer reorderLayer = GroupToReorder.Layers.FirstOrDefault(x => x.Name == layer.Name);
        if (reorderLayer != null)
        {
          GroupToReorder.MoveLayer(reorderLayer, i);
        }
        else if ((string.Compare(layer.Name, _connectivityLabel, true) == 0 || string.Compare(layer.Name, _structuralLabel, true) == 0) && !associationHasMoved)
        {
          reorderLayer = GroupToReorder.Layers.FirstOrDefault(x => x.Name == _associationLayerName);

          if (reorderLayer != null)
          {
            associationHasMoved = true;
            GroupToReorder.MoveLayer(reorderLayer, i);
          }
        }
      }
    }

    /// <summary>
    /// Reorder the layers under a layer group so they display in the same order as the original diagram map
    /// </summary>
    /// <param name="Reference">Original diagram layer</param>
    /// <param name="GroupToReorder">Group to Reorder</param>
    private static void ReorderGroupLayer(CompositeLayer Reference, ILayerContainerEdit GroupToReorder)
    {
      bool associationHasMoved = false;
      for (int i = 0; i < Reference.Layers.Count; i++)
      {
        Layer layer = Reference.Layers[i];
        Layer reorderLayer = GroupToReorder.Layers.FirstOrDefault(x => x.Name == layer.Name);

        if (reorderLayer != null)
        {
          GroupToReorder.MoveLayer(reorderLayer, i);
        }
        else if ((string.Compare(layer.Name, _connectivityLabel, true) == 0 || string.Compare(layer.Name, _structuralLabel, true) == 0) && !associationHasMoved)
        {
          reorderLayer = GroupToReorder.Layers.FirstOrDefault(x => x.Name == _associationLayerName);

          if (reorderLayer != null)
          {
            associationHasMoved = true;
            GroupToReorder.MoveLayer(reorderLayer, i);
          }
        }
      }
    }

    /// <summary>
    /// Get the new layer renderer
    /// </summary>
    /// <param name="OriginRenderer">Diagram layer renderer</param>
    /// <param name="LayerName">Layer name</param>
    /// <param name="NewCimRenderer">CIMRenderer used to create the layer</param>
    /// <returns>Renderer definition</returns>
    private RendererDefinition GetNewRenderer(CIMRenderer OriginRenderer, string LayerName, out CIMRenderer NewCimRenderer)
    {
      NewCimRenderer = null;

      if (OriginRenderer is CIMSimpleRenderer simpleRenderer)
      {
        NewCimRenderer = simpleRenderer.Clone();
        ((CIMSimpleRenderer)NewCimRenderer).VisualVariables = ChangeFieldNameInVisualVariables(simpleRenderer.VisualVariables);

        return new SimpleRendererDefinition();
      }
      else if (OriginRenderer is CIMUniqueValueRenderer uniqueRenderer)
      {
        CIMUniqueValueRenderer newCIMUniqueValueRenderer = new()
        {
          AuthoringInfo = uniqueRenderer.AuthoringInfo,
          ColorRamp = uniqueRenderer.ColorRamp,
          DefaultDescription = uniqueRenderer.DefaultDescription,
          DefaultLabel = uniqueRenderer.DefaultLabel,
          DefaultSymbol = ChangeSymbolProperties(uniqueRenderer.DefaultSymbol),
          DefaultSymbolPatch = uniqueRenderer.DefaultSymbolPatch,
          PolygonSymbolColorTarget = uniqueRenderer.PolygonSymbolColorTarget,
          StyleGallery = uniqueRenderer.StyleGallery,
          UseDefaultSymbol = uniqueRenderer.UseDefaultSymbol,
          VisualVariables = ChangeFieldNameInVisualVariables(uniqueRenderer.VisualVariables)
        };

        NewCimRenderer = newCIMUniqueValueRenderer;

        if (uniqueRenderer.ValueExpressionInfo != null && uniqueRenderer.ValueExpressionInfo is CIMExpressionInfo expressionInfo)
        {
          newCIMUniqueValueRenderer.ValueExpressionInfo = new CIMExpressionInfo()
          {
            Expression = ChangeFieldNameInExpression(expressionInfo.Expression),
            Name = expressionInfo.Name,
            ReturnType = expressionInfo.ReturnType,
            Title = expressionInfo.Title
          };

          return new UniqueValueRendererDefinition();
        }
        else
        {
          string[] fieldsNames = uniqueRenderer.Fields;
          CustomField customField;
          List<CustomField> newFieldsNames = new();
          List<string> newNames = new();

          bool hasAssetGroup = _activeAssetGroup != null;
          bool hasAssetType = false;
          bool addAssetGroup = false;

          foreach (string s in fieldsNames)
          {
            customField = GetCustomField(s);

            if (customField.IsAssetGroup)
            {
              hasAssetGroup = true;
            }
            if (customField.IsAssetType)
            {
              hasAssetType = true;
            }

            newFieldsNames.Add(customField);
          }

          if (hasAssetType && !hasAssetGroup)
          {
            newFieldsNames.Insert(0, GetAssetGroupField());
            addAssetGroup = true;
            _activeAssetGroup = newFieldsNames[0].GetAssetGroup(LayerName);
          }

          List<CIMUniqueValueGroup> newGroups = new();
          List<CIMUniqueValueClass> newClasses;
          CIMUniqueValueGroup[] groups = uniqueRenderer.Groups;

          foreach (CIMUniqueValueGroup group in groups)
          {
            CIMUniqueValueClass[] classes = group.Classes;
            if (classes == null)
              continue;
            newClasses = new();
            foreach (CIMUniqueValueClass uniqueValue in classes)
            {
              List<CIMUniqueValue> newValues = new();
              foreach (CIMUniqueValue value in uniqueValue.Values)
              {
                List<string> newFieldValues = new();
                if (addAssetGroup)
                {
                  newFieldValues.Add(LayerName);
                }

                for (int i = 0; i < value.FieldValues.Length; i++)
                {
                  CustomField field = newFieldsNames[i + (addAssetGroup ? 1 : 0)];
                  if (field.IsAssetGroup)
                  {
                    _activeAssetGroup = field.GetAssetGroup(Convert.ToInt32(value.FieldValues[i]));

                    if (_activeAssetGroup != null)
                    {
                      newFieldValues.Add(_activeAssetGroup.Name);
                    }
                    else
                    {
                      newFieldValues.Add(value.FieldValues[i]);
                    }
                  }
                  else if (field.IsAssetType)
                  {
                    if (_activeAssetGroup == null)
                    {
                      CustomField custom = GetAssetGroupField();
                      _activeAssetGroup = custom.GetAssetGroup(LayerName);
                    }

                    if (_activeAssetGroup == null)
                    {
                      newFieldValues.Add(value.FieldValues[i]);
                    }
                    else
                    {
                      AssetType assetType = _activeAssetGroup.GetAssetTypes().FirstOrDefault(a => a.Code == Convert.ToInt32(value.FieldValues[i]));
                      if (assetType == null)
                      {
                        newFieldValues.Add(value.FieldValues[i]);
                      }
                      else
                      {
                        newFieldValues.Add(assetType.Name);
                      }
                    }
                  }
                  else if (field.HasDomain)
                  {
                    newFieldValues.Add(field.GetDomainValue(value.FieldValues[i]));
                  }
                  else
                  {
                    newFieldValues.Add(value.FieldValues[i]);
                  }
                }

                CIMUniqueValue newValue = new()
                {
                  FieldValues = newFieldValues.ToArray()
                };
                newValues.Add(newValue);
              }

              CIMUniqueValueClass newValueClass = new()
              {
                AlternateSymbols = uniqueValue.AlternateSymbols,
                Description = uniqueValue.Description,
                Editable = uniqueValue.Editable,
                Label = uniqueValue.Label,
                Patch = uniqueValue.Patch,
                Visible = uniqueValue.Visible,
                Symbol = ChangeSymbolProperties(uniqueValue.Symbol),
                Values = newValues.ToArray()
              };
              newClasses.Add(newValueClass);
            }

            CIMUniqueValueGroup newGroup = new()
            {
              Classes = newClasses.ToArray()
            };
            newGroups.Add(newGroup);
          }

          List<string> fieldNames = new();
          foreach (var item in newFieldsNames)
          {
            fieldNames.Add(item.NewModelName);
          }

          newCIMUniqueValueRenderer.Groups = newGroups.ToArray();
          newCIMUniqueValueRenderer.Fields = fieldNames.ToArray();

          return new UniqueValueRendererDefinition();
        }
      }
      else if (OriginRenderer is CIMClassBreaksRenderer breakRenderer)
      {

        CIMClassBreaksRenderer newBreakRenderer = breakRenderer.Clone();
        newBreakRenderer.DefaultSymbol = ChangeSymbolProperties(breakRenderer.DefaultSymbol);
        newBreakRenderer.ExclusionClause = ChangeFieldNameInExpression(breakRenderer.ExclusionClause);
        newBreakRenderer.ExclusionLabel = ChangeFieldNameInExpression(breakRenderer.ExclusionLabel);
        newBreakRenderer.ExclusionClause = ChangeFieldNameInExpression(breakRenderer.ExclusionClause);
        newBreakRenderer.ExclusionSymbol = ChangeSymbolProperties(breakRenderer.ExclusionSymbol);
        newBreakRenderer.NormalizationField = GetNewFieldName(breakRenderer.NormalizationField);

        newBreakRenderer.ValueExpressionInfo = new CIMExpressionInfo
        {
          Expression = ChangeFieldNameInExpression(breakRenderer.ValueExpressionInfo.Expression),
          Name = breakRenderer.ValueExpressionInfo.Name,
          ReturnType = breakRenderer.ValueExpressionInfo.ReturnType,
          Title = breakRenderer.ValueExpressionInfo.Title
        };

        newBreakRenderer.VisualVariables = ChangeFieldNameInVisualVariables(breakRenderer.VisualVariables);

        NewCimRenderer = newBreakRenderer;

        return new SimpleRendererDefinition()
        {
          SymbolTemplate = breakRenderer.DefaultSymbol
        };

      }
      else if (OriginRenderer != null)
      {
        NewCimRenderer = CIMObject.FromJson(OriginRenderer.ToJson()) as CIMRenderer;
      }

      return new SimpleRendererDefinition();
    }
    #endregion

    #region Change the source field name and domain name to a new field name and new domain name
    /// <summary>
    /// Set Label and Symbology
    /// </summary>
    /// <param name="OriginLayer">Origin Layer</param>
    /// <param name="DestinationLayer">Destination Layer</param>
    private void SetLabelClassesAndPopupInfos(FeatureLayer OriginLayer, FeatureLayer DestinationLayer)
    {
      ReadOnlyObservableCollection<LabelClass> LabelClasses = OriginLayer.LabelClasses;
      if (LabelClasses == null || LabelClasses.Count == 0)
      { return; }

      CIMFeatureLayer cimDestinationDef = DestinationLayer.GetDefinition() as CIMFeatureLayer;
      List<CIMLabelClass> classes = new();
      foreach (LabelClass labelClass in LabelClasses)
      {
        CIMLabelClass newLabel = new()
        {
          Expression = ChangeFieldNameInExpression(labelClass.Expression),
          ExpressionEngine = labelClass.ExpressionEngine,
          ExpressionTitle = labelClass.Name,
          MaplexLabelPlacementProperties = labelClass.GetMaplexLabelPlacementProperties(),
          MaximumScale = labelClass.MaximumScale,
          MinimumScale = labelClass.MinimumScale,
          Name = labelClass.Name,
          StandardLabelPlacementProperties = labelClass.GetStandardLabelPlacementProperties(),
          UseCodedValue = false,
          Visibility = true,
          TextSymbol = new() { Symbol = labelClass.GetTextSymbol() },
          Priority = labelClass.Priority,
          WhereClause = ChangeFieldNameInWhereClause(labelClass.WhereClause)
        };

        classes.Add(newLabel);
      }

      if (classes.Count == 1 && classes[0].ExpressionTitle != "Class 1")
      {
        classes[0].ExpressionTitle = "Custom";
      }

      if (classes.Count > 0)
      {
        cimDestinationDef.LabelClasses = classes.ToArray();

        DestinationLayer.SetDefinition(cimDestinationDef);
      }

      if (OriginLayer.GetDefinition() is CIMFeatureLayer cimOriginDef)
      {
        if (cimOriginDef is CIMBaseLayer baseOriginLayer)
        {
          CIMPopupInfo popupInfo = ChangeFieldNameInPopupInfo(baseOriginLayer?.PopupInfo);
          if (popupInfo != null)
          {
            if (cimDestinationDef is CIMBaseLayer baseLayer)
            {
              baseLayer.PopupInfo = popupInfo;
              DestinationLayer.SetDefinition(baseLayer);
            }
          }
        }

        if (cimOriginDef is CIMBasicFeatureLayer basicOriginLayer)
        {
          if (cimDestinationDef is CIMBasicFeatureLayer basicDestinationLayer)
          {
            CIMDisplayFilter[] filter = ChangeFieldNameInDisplayFilter(basicOriginLayer?.DisplayFilterChoices);

            if (filter != null)
            {
              basicDestinationLayer.DisplayFilterChoices = filter;
              DestinationLayer.SetDefinition(basicDestinationLayer);
            }
          }

        }
      }
    }

    /// <summary>
    /// Change field name in popup info
    /// </summary>
    /// <param name="Popup">Popup info</param>
    /// <returns>CIMPopupInfo</returns>
    private CIMPopupInfo ChangeFieldNameInPopupInfo(CIMPopupInfo Popup)
    {
      if (Popup == null)
        return null;

      return new CIMPopupInfo()
      {
        ExpressionInfos = ChangeFieldNameInExpressionInfos(Popup.ExpressionInfos),
        FieldDescriptions = ChangeFieldNameInPopupFields(Popup.FieldDescriptions),
        GridLayout = Popup.GridLayout,
        MediaInfos = ChangeFieldNameInMediaInfos(Popup.MediaInfos),
        RelatedRecordSortOrder = Popup.RelatedRecordSortOrder,
        Title = Popup.Title
      };
    }

    /// <summary>
    /// Change field name in expression info
    /// </summary>
    /// <param name="Expressions">Origin expression array</param>
    /// <returns>CIMExpressionInfo Array</returns>
    private CIMExpressionInfo[] ChangeFieldNameInExpressionInfos(CIMExpressionInfo[] Expressions)
    {
      List<CIMExpressionInfo> expressions = new();

      foreach (CIMExpressionInfo expressionInfo in Expressions)
      {
        CIMExpressionInfo info = expressionInfo.Clone();
        info.Expression = ChangeFieldNameInExpression(expressionInfo.Expression);

        expressions.Add(info);
      }

      return expressions.ToArray();
    }

    /// <summary>
    /// Change field name in display filters
    /// </summary>
    /// <param name="DisplayFilterChoices">Origin display filters</param>
    /// <returns>CIMDisplayFilter Array</returns>
    private CIMDisplayFilter[] ChangeFieldNameInDisplayFilter(CIMDisplayFilter[] DisplayFilterChoices)
    {
      if (DisplayFilterChoices == null) return null;

      List<CIMDisplayFilter> filtered = new();

      foreach (CIMDisplayFilter filter in DisplayFilterChoices)
      {
        filtered.Add(new()
        {
          MaxScale = filter.MaxScale,
          MinScale = filter.MinScale,
          Name = filter.Name,
          WhereClause = ChangeFieldNameInWhereClause(filter.WhereClause)
        });
      }

      return filtered.ToArray();
    }

    /// <summary>
    /// Change field name in visual variables
    /// </summary>
    /// <param name="VisualVariables">Source visual variables</param>
    /// <returns>CIMVisualVariable Array</returns>
    private CIMVisualVariable[] ChangeFieldNameInVisualVariables(CIMVisualVariable[] VisualVariables)
    {
      if (VisualVariables == null)
      { return null; }

      List<CIMVisualVariable> newVisualVariables = new();

      foreach (CIMVisualVariable variable in VisualVariables)
      {
        if (variable is CIMColorVisualVariable colorVariable)
        {
          newVisualVariables.Add(new CIMColorVisualVariable
          {
            AuthoringInfo = colorVariable.AuthoringInfo,
            ColorRamp = colorVariable.ColorRamp,
            Expression = ChangeFieldNameInExpression(colorVariable.Expression),
            MaxValue = colorVariable.MaxValue,
            MinValue = colorVariable.MinValue,
            NormalizationField = GetNewFieldName(colorVariable.NormalizationField),
            NormalizationType = colorVariable.NormalizationType,
            PolygonSymbolColorTarget = colorVariable.PolygonSymbolColorTarget,
            ValueExpressionInfo = colorVariable.ValueExpressionInfo == null ? null : new CIMExpressionInfo
            {
              Expression = ChangeFieldNameInExpression(colorVariable.ValueExpressionInfo.Expression),
              Name = colorVariable.ValueExpressionInfo.Name,
              ReturnType = colorVariable.ValueExpressionInfo.ReturnType,
              Title = colorVariable.ValueExpressionInfo.Title
            }
          });
        }
        else if (variable is CIMMultilevelColorVisualVariable colorVariable2)
        {
          newVisualVariables.Add(new CIMMultilevelColorVisualVariable
          {
            AuthoringInfo = colorVariable2.AuthoringInfo,
            ColorRamp = colorVariable2.ColorRamp,
            Levels = colorVariable2.Levels,
            NormalizationField = GetNewFieldName(colorVariable2.NormalizationField),
            NormalizationType = colorVariable2.NormalizationType,
            PolygonSymbolColorTarget = colorVariable2.PolygonSymbolColorTarget,
            ValueExpressionInfo = colorVariable2.ValueExpressionInfo == null ? null : new CIMExpressionInfo
            {
              Expression = ChangeFieldNameInExpression(colorVariable2.ValueExpressionInfo.Expression),
              Name = colorVariable2.ValueExpressionInfo.Name,
              ReturnType = colorVariable2.ValueExpressionInfo.ReturnType,
              Title = colorVariable2.ValueExpressionInfo.Title
            }
          });
        }
        else if (variable is CIMRotationVisualVariable rotationVariable)
        {
          newVisualVariables.Add(new CIMRotationVisualVariable
          {
            AuthoringInfo = rotationVariable.AuthoringInfo,
            NormalToSurface = rotationVariable.NormalToSurface,
            RotationTypeZ = rotationVariable.RotationTypeZ,
            VisualVariableInfoX = new CIMVisualVariableInfo
            {
              Expression = ChangeFieldNameInExpression(rotationVariable.VisualVariableInfoX.Expression),
              RandomMax = rotationVariable.VisualVariableInfoX.RandomMax,
              RandomMin = rotationVariable.VisualVariableInfoX.RandomMin,
              VisualVariableInfoType = rotationVariable.VisualVariableInfoX.VisualVariableInfoType,
              ValueExpressionInfo = rotationVariable.VisualVariableInfoX.ValueExpressionInfo == null ? null : new CIMExpressionInfo
              {
                Expression = ChangeFieldNameInExpression(rotationVariable.VisualVariableInfoX.ValueExpressionInfo.Expression),
                Name = rotationVariable.VisualVariableInfoX.ValueExpressionInfo.Name,
                ReturnType = rotationVariable.VisualVariableInfoX.ValueExpressionInfo.ReturnType,
                Title = rotationVariable.VisualVariableInfoX.ValueExpressionInfo.Title
              }
            },
            VisualVariableInfoY = new CIMVisualVariableInfo
            {
              Expression = ChangeFieldNameInExpression(rotationVariable.VisualVariableInfoY.Expression),
              RandomMax = rotationVariable.VisualVariableInfoY.RandomMax,
              RandomMin = rotationVariable.VisualVariableInfoY.RandomMin,
              VisualVariableInfoType = rotationVariable.VisualVariableInfoY.VisualVariableInfoType,
              ValueExpressionInfo = rotationVariable.VisualVariableInfoY.ValueExpressionInfo == null ? null : new CIMExpressionInfo
              {
                Expression = ChangeFieldNameInExpression(rotationVariable.VisualVariableInfoY.ValueExpressionInfo.Expression),
                Name = rotationVariable.VisualVariableInfoY.ValueExpressionInfo.Name,
                ReturnType = rotationVariable.VisualVariableInfoY.ValueExpressionInfo.ReturnType,
                Title = rotationVariable.VisualVariableInfoY.ValueExpressionInfo.Title
              }
            },
            VisualVariableInfoZ = new CIMVisualVariableInfo
            {
              Expression = ChangeFieldNameInExpression(rotationVariable.VisualVariableInfoZ.Expression),
              RandomMax = rotationVariable.VisualVariableInfoZ.RandomMax,
              RandomMin = rotationVariable.VisualVariableInfoZ.RandomMin,
              VisualVariableInfoType = rotationVariable.VisualVariableInfoZ.VisualVariableInfoType,
              ValueExpressionInfo = rotationVariable.VisualVariableInfoZ.ValueExpressionInfo == null ? null : new CIMExpressionInfo
              {
                Expression = ChangeFieldNameInExpression(rotationVariable.VisualVariableInfoZ.ValueExpressionInfo.Expression),
                Name = rotationVariable.VisualVariableInfoZ.ValueExpressionInfo.Name,
                ReturnType = rotationVariable.VisualVariableInfoZ.ValueExpressionInfo.ReturnType,
                Title = rotationVariable.VisualVariableInfoZ.ValueExpressionInfo.Title
              }
            }
          });
        }
        else if (variable is CIMSizeVisualVariable sizeVariable)
        {
          newVisualVariables.Add(new CIMSizeVisualVariable
          {
            AuthoringInfo = sizeVariable.AuthoringInfo,
            Axis = sizeVariable.Axis,
            DataValues = sizeVariable.DataValues,
            Expression = ChangeFieldNameInExpression(sizeVariable.Expression),
            MaxSize = sizeVariable.MaxSize,
            MaxValue = sizeVariable.MaxValue,
            MinSize = sizeVariable.MinSize,
            MinValue = sizeVariable.MinValue,
            NormalizationField = GetNewFieldName(sizeVariable.NormalizationField),
            NormalizationType = sizeVariable.NormalizationType,
            RandomMax = sizeVariable.RandomMax,
            RandomMin = sizeVariable.RandomMin,
            SizeValues = sizeVariable.SizeValues,
            Target = sizeVariable.Target,
            ValueRepresentation = sizeVariable.ValueRepresentation,
            ValueShape = sizeVariable.ValueShape,
            ValueUnits = sizeVariable.ValueUnits,
            VariableType = sizeVariable.VariableType,
            ValueExpressionInfo = sizeVariable.ValueExpressionInfo == null ? null : new CIMExpressionInfo
            {
              Expression = ChangeFieldNameInExpression(sizeVariable.ValueExpressionInfo.Expression),
              Name = sizeVariable.ValueExpressionInfo.Name,
              ReturnType = sizeVariable.ValueExpressionInfo.ReturnType,
              Title = sizeVariable.ValueExpressionInfo.Title
            }
          });
        }
        else if (variable is CIMTransparencyVisualVariable transparencyVariable)
        {
          newVisualVariables.Add(new CIMTransparencyVisualVariable
          {
            AuthoringInfo = transparencyVariable.AuthoringInfo,
            DataValues = transparencyVariable.DataValues,
            Field = GetNewFieldName(transparencyVariable.Field),
            NormalizationField = GetNewFieldName(transparencyVariable.NormalizationField),
            NormalizationTotal = transparencyVariable.NormalizationTotal,
            NormalizationType = transparencyVariable.NormalizationType,
            TransparencyValues = transparencyVariable.TransparencyValues,
            ValueExpressionInfo = transparencyVariable.ValueExpressionInfo == null ? null : new CIMExpressionInfo
            {
              Expression = ChangeFieldNameInExpression(transparencyVariable.ValueExpressionInfo.Expression),
              Name = transparencyVariable.ValueExpressionInfo.Name,
              ReturnType = transparencyVariable.ValueExpressionInfo.ReturnType,
              Title = transparencyVariable.ValueExpressionInfo.Title
            }
          });
        }
        else
        {
          newVisualVariables.Add(variable);
        }
      }

      return newVisualVariables.ToArray();
    }

    /// <summary>
    /// Change field name in media infos
    /// </summary>
    /// <param name="MediaInfos">Source media infos</param>
    /// <returns>CIMMediaInfo array</returns>
    private CIMMediaInfo[] ChangeFieldNameInMediaInfos(CIMMediaInfo[] MediaInfos)
    {
      if (MediaInfos == null)
        return null;

      List<CIMMediaInfo> newMedias = new();
      foreach (CIMMediaInfo mediaInfo in MediaInfos)
      {
        if (mediaInfo != null)
        {
          if (mediaInfo is CIMAttachmentsMediaInfo attachment)
          {
            newMedias.Add(attachment);
          }
          else if (mediaInfo is CIMBarChartMediaInfo barChart)
          {
            newMedias.Add(new CIMBarChartMediaInfo()
            {
              Title = barChart.Title,
              Caption = barChart.Caption,
              Column = barChart.Column,
              ColumnSpan = barChart.ColumnSpan,
              Fields = ChangeFieldNameInArray(barChart.Fields),
              NormalizeField = GetNewFieldName(barChart.NormalizeField),
              RefreshRate = barChart.RefreshRate,
              RefreshRateUnit = barChart.RefreshRateUnit,
              Row = barChart.Row,
              RowSpan = barChart.RowSpan
            });
          }
          else if (mediaInfo is CIMColumnChartMediaInfo columnChart)
          {
            newMedias.Add(new CIMColumnChartMediaInfo()
            {
              Caption = columnChart.Caption,
              Column = columnChart.Column,
              ColumnSpan = columnChart.ColumnSpan,
              Fields = ChangeFieldNameInArray(columnChart.Fields),
              NormalizeField = GetNewFieldName(columnChart.NormalizeField),
              RefreshRate = columnChart.RefreshRate,
              RefreshRateUnit = columnChart.RefreshRateUnit,
              Row = columnChart.Row,
              RowSpan = columnChart.RowSpan,
              Title = columnChart.Title
            });
          }
          else if (mediaInfo is CIMLineChartMediaInfo lineChart)
          {
            newMedias.Add(new CIMLineChartMediaInfo()
            {
              Caption = lineChart.Caption,
              Column = lineChart.Column,
              ColumnSpan = lineChart.ColumnSpan,
              Fields = ChangeFieldNameInArray(lineChart.Fields),
              NormalizeField = GetNewFieldName(lineChart.NormalizeField),
              RefreshRate = lineChart.RefreshRate,
              RefreshRateUnit = lineChart.RefreshRateUnit,
              Row = lineChart.Row,
              RowSpan = lineChart.RowSpan,
              Title = lineChart.Title
            });
          }
          else if (mediaInfo is CIMPieChartMediaInfo pieChart)
          {
            newMedias.Add(new CIMPieChartMediaInfo()
            {
              Caption = pieChart.Caption,
              Column = pieChart.Column,
              ColumnSpan = pieChart.ColumnSpan,
              Fields = ChangeFieldNameInArray(pieChart.Fields),
              NormalizeField = GetNewFieldName(pieChart.NormalizeField),
              RefreshRate = pieChart.RefreshRate,
              RefreshRateUnit = pieChart.RefreshRateUnit,
              Row = pieChart.Row,
              RowSpan = pieChart.RowSpan,
              Title = pieChart.Title
            });
          }
          else if (mediaInfo is CIMExpressionMediaInfo expression)
          {
            newMedias.Add(new CIMExpressionMediaInfo()
            {
              Column = expression.Column,
              ColumnSpan = expression.ColumnSpan,
              Expression = new CIMExpressionInfo()
              {
                Expression = ChangeFieldNameInExpression(expression.Expression.Expression),
                Name = expression.Expression.Name,
                ReturnType = expression.Expression.ReturnType,
                Title = expression.Expression.Title
              },
              RefreshRate = expression.RefreshRate,
              RefreshRateUnit = expression.RefreshRateUnit,
              Row = expression.Row,
              RowSpan = expression.RowSpan
            });
          }
          else if (mediaInfo is CIMTableMediaInfo table)
          {
            newMedias.Add(new CIMTableMediaInfo
            {
              Caption = table.Caption,
              Column = table.Column,
              ColumnSpan = table.ColumnSpan,
              Fields = ChangeFieldNameInArray(table.Fields),
              RefreshRate = table.RefreshRate,
              RefreshRateUnit = table.RefreshRateUnit,
              Row = table.Row,
              RowSpan = table.RowSpan,
              Title = table.Title,
              UseLayerFields = table.UseLayerFields
            });
          }
          else if (mediaInfo is CIMTextMediaInfo text)
          {
            newMedias.Add(new CIMTextMediaInfo
            {
              Column = text.Column,
              ColumnSpan = text.ColumnSpan,
              RefreshRate = text.RefreshRate,
              RefreshRateUnit = text.RefreshRateUnit,
              Row = text.Row,
              RowSpan = text.RowSpan,
              Text = text.Text
            });
          }
          else
          {
            newMedias.Add(mediaInfo);
          }
        }
      }

      return newMedias.ToArray();
    }

    /// <summary>
    /// Change field name in array
    /// </summary>
    /// <param name="FieldNames">Source field name array</param>
    /// <returns>New fieldsNames array</returns>
    private string[] ChangeFieldNameInArray(string[] FieldNames)
    {
      if (FieldNames == null)
        return null;

      List<string> newFields = new();

      foreach (string field in FieldNames)
      {
        if (!string.IsNullOrEmpty(field))
        {
          string newName = GetNewFieldName(field);
          if (!string.IsNullOrEmpty(newName))
            newFields.Add(newName);
        }
      }

      return newFields.ToArray();
    }

    /// <summary>
    /// Change field name in popup fields
    /// </summary>
    /// <param name="FieldDescriptions">Source CIMPopupFieldDescription array</param>
    /// <returns>New CIMPopupFieldDescription array</returns>
    private CIMPopupFieldDescription[] ChangeFieldNameInPopupFields(CIMPopupFieldDescription[] FieldDescriptions)
    {
      if (FieldDescriptions == null)
        return null;

      List<CIMPopupFieldDescription> fieldsList = new();

      foreach (CIMPopupFieldDescription fieldDescription in FieldDescriptions)
      {
        CIMPopupFieldDescription newDescript = new()
        {
          Alias = fieldDescription.Alias,
          FieldName = GetNewFieldName(fieldDescription.FieldName),
          NumberFormat = fieldDescription.NumberFormat
        };

        fieldsList.Add(newDescript);
      }

      return fieldsList.ToArray();
    }

    /// <summary>
    /// Change field name in primitives
    /// </summary>
    /// <param name="PrimitiveOverrides">Origin CIMPrimitiveOverride array</param>
    /// <returns>New CIMPrimitiveOverride array</returns>
    private CIMPrimitiveOverride[] ChangeFieldNameInPrimitives(CIMPrimitiveOverride[] PrimitiveOverrides)
    {
      if (PrimitiveOverrides == null)
      { return null; }

      List<CIMPrimitiveOverride> newPrimitives = new();

      foreach (var primitiveOverride in PrimitiveOverrides)
      {
        newPrimitives.Add(new CIMPrimitiveOverride()
        {
          Expression = ChangeFieldNameInExpression(primitiveOverride.Expression),
          PrimitiveName = primitiveOverride.PrimitiveName,
          PropertyName = primitiveOverride.PropertyName,
          ValueExpressionInfo = primitiveOverride.ValueExpressionInfo == null ? null :
          new CIMExpressionInfo
          {
            Expression = ChangeFieldNameInExpression(primitiveOverride.ValueExpressionInfo.Expression),
            Name = primitiveOverride.ValueExpressionInfo.Name,
            ReturnType = primitiveOverride.ValueExpressionInfo.ReturnType,
            Title = primitiveOverride.ValueExpressionInfo.Title
          }
        });
      }

      return newPrimitives.ToArray();
    }

    /// <summary>
    /// Change field name in Where clause
    /// </summary>
    /// <param name="OriginWhereClause">Source Where clause</param>
    /// <returns>New Where clause</returns>
    private string ChangeFieldNameInWhereClause(string OriginWhereClause)
    {
      if (string.IsNullOrEmpty(OriginWhereClause))
      {
        return OriginWhereClause;
      }

      int index = OriginWhereClause.IndexOf(" AND ", StringComparison.OrdinalIgnoreCase);
      if (index != -1)
      {
        string returnValue = string.Concat(ChangeFieldNameInWhereClause(OriginWhereClause[..index]), OriginWhereClause.AsSpan(index, 5), ChangeFieldNameInWhereClause(OriginWhereClause[(index + 5)..]));
        return returnValue;
      }

      index = OriginWhereClause.IndexOf(" OR ", StringComparison.OrdinalIgnoreCase);
      if (index != -1)
      {
        string returnValue = string.Concat(ChangeFieldNameInWhereClause(OriginWhereClause[..index]), OriginWhereClause.AsSpan(index, 4), ChangeFieldNameInWhereClause(OriginWhereClause[(index + 4)..]));
        return returnValue;
      }

      index = OriginWhereClause.IndexOf(" IN (", StringComparison.OrdinalIgnoreCase);
      if (index != -1)
      {
        string fieldName = OriginWhereClause[..index].Trim();

        CustomField custom = GetCustomField(fieldName);

        if (custom != null)
        {
          string newExpression = OriginWhereClause.Replace(fieldName, custom.NewModelName);

          index = newExpression.IndexOf(" IN (", StringComparison.OrdinalIgnoreCase);

          if (index != -1)
          {
            index += 5;
            int end = newExpression.IndexOf(')', index);

            string[] values = newExpression[index..end].Split(',');
            List<string> newValues = new();
            for (int i = 0; i < values.Length; i++)
            {
              newValues.Add(custom.GetDomainValue(values[i]));
            }

            newExpression = newExpression[..index];

            foreach (string value in newValues)
            {
              newExpression += string.Format(" '{0}',", value);
            }

            return newExpression[..^2] + ")";
          }

          if (custom.HasDomain)
          {
            index = newExpression.IndexOf('=');
            if (index != -1)
            {
              index += 1;
              string vlaue = newExpression[index..].Trim();

              return newExpression.Replace(vlaue, "'" + custom.GetDomainValue(vlaue) + "'");
            }

          }

          return newExpression;
        }
      }

      return OriginWhereClause;
    }

    /// <summary>
    /// Get new field name
    /// </summary>
    /// <param name="OriginFieldName">Source field name</param>
    /// <returns>New field name</returns>
    private string GetNewFieldName(string OriginFieldName)
    {
      if (string.IsNullOrEmpty(OriginFieldName))
        return OriginFieldName;

      if (OriginFieldName.Contains("shape", StringComparison.OrdinalIgnoreCase))
        return "SHAPE";

      return GetCustomField(OriginFieldName)?.NewModelName;
    }

    /// <summary>
    /// Change field name in expression
    /// </summary>
    /// <param name="OriginExpression">Source expression</param>
    /// <returns>New expression</returns>
    private string ChangeFieldNameInExpression(string OriginExpression)
    {
      if (string.IsNullOrEmpty(OriginExpression))
      {
        return OriginExpression;
      }

      if (OriginExpression[..2] == "//")
      {
        return OriginExpression;
      }

      int index = OriginExpression.IndexOf(" AND ", StringComparison.OrdinalIgnoreCase);
      if (index != -1)
      {
        string returnValue = string.Concat(ChangeFieldNameInExpression(OriginExpression[..index]), OriginExpression.AsSpan(index, 5), ChangeFieldNameInExpression(OriginExpression[(index + 5)..]));
        return returnValue;
      }

      index = OriginExpression.IndexOf(" && ", StringComparison.OrdinalIgnoreCase);
      if (index != -1)
      {
        string returnValue = string.Concat(ChangeFieldNameInExpression(OriginExpression[..index]), OriginExpression.AsSpan(index, 4), ChangeFieldNameInExpression(OriginExpression[(index + 4)..]));
        return returnValue;
      }

      index = OriginExpression.IndexOf(" OR ", StringComparison.OrdinalIgnoreCase);
      if (index != -1)
      {
        string returnValue = string.Concat(ChangeFieldNameInExpression(OriginExpression[..index]), OriginExpression.AsSpan(index, 4), ChangeFieldNameInExpression(OriginExpression[(index + 4)..]));
        return returnValue;
      }

      index = OriginExpression.IndexOf(" || ", StringComparison.OrdinalIgnoreCase);
      if (index != -1)
      {
        string returnValue = string.Concat(ChangeFieldNameInExpression(OriginExpression[..index]), OriginExpression.AsSpan(index, 4), ChangeFieldNameInExpression(OriginExpression[(index + 4)..]));
        return returnValue;
      }

      if (OriginExpression.Trim().Length < 8)
      {
        return OriginExpression;
      }

      index = OriginExpression.IndexOf("+", StringComparison.OrdinalIgnoreCase);
      if (index != -1)
      {
        string returnValue = string.Concat(ChangeFieldNameInExpression(OriginExpression[..index]), "+", ChangeFieldNameInExpression(OriginExpression[(index + 1)..]));
        return returnValue;
      }

      index = OriginExpression.IndexOf("Includes([", StringComparison.OrdinalIgnoreCase);
      while (index != -1)
      {
        int start = OriginExpression.IndexOf("],") + 2;
        int end = OriginExpression.IndexOf(startIndex: start, value: ")");
        string fieldName;

        if (end != -1)
        {
          fieldName = OriginExpression[start..end].Trim();
        }
        else
        {
          fieldName = OriginExpression[start..].Trim();
        }

        CustomField custom = GetCustomField(fieldName);
        if (custom == null)
        {
          string newName = ChangeFieldNameInExpression(fieldName);
          OriginExpression = OriginExpression.Replace(fieldName, newName);
          if (newName.Contains('.'))
          {
            newName = newName[(newName.IndexOf('.') + 1)..];
          }
          custom = GetCustomField(newName);

        }
        else
        {

        }

        if (custom != null)
        {
          string newExpression = "";
          end = index + 10;
          string newValues = OriginExpression[end..(start - 2)];
          string[] Values = newValues.Split(',');
          List<string> Values2 = new();
          foreach (string item in Values)
          {
            if (!string.IsNullOrEmpty(item))
            {
              Values2.Add(custom.GetDomainValue(item));
            }
          }

          if (custom.NewType == FieldType.String)
          {
            foreach (string item in Values2)
            {
              newExpression += string.Format("\"{0}\",", item);
            }
          }
          else
          {
            foreach (string item in Values2)
            {
              newExpression += item + ",";
            }
          }

          newExpression = newExpression[..^1];

          OriginExpression = OriginExpression.Replace(newValues, newExpression);
        }

        index = OriginExpression.IndexOf("Includes([", index + 2, StringComparison.OrdinalIgnoreCase);
      }

      index = OriginExpression.IndexOf("Concatenate", StringComparison.OrdinalIgnoreCase);
      if (index != -1)
      {
        if (OriginExpression.Contains("domainName($feature", StringComparison.OrdinalIgnoreCase) || OriginExpression.Contains("$feature[", StringComparison.OrdinalIgnoreCase))
        {
          int end = OriginExpression.LastIndexOf(']');
          string newExpression = OriginExpression[(index + 13)..end];

          int newIndex = newExpression.IndexOf("domainName($feature", StringComparison.OrdinalIgnoreCase);
          while (newIndex > -1)
          {
            int subEnd = newExpression.IndexOf(')', newIndex);

            string newField = ChangeFieldNameInExpression(newExpression[newIndex..(subEnd + 1)]);
            if ((subEnd + 2) < newExpression.Length)
            {
              if (newIndex == 0)
              {
                newExpression = newField + newExpression[(subEnd + 2)..];
              }
              else
              {
                newExpression = newExpression[..(newIndex - 1)] + newField + newExpression[(subEnd + 2)..];
              }
            }
            else
            {
              newExpression = newExpression[..newIndex] + newField;
            }

            newIndex = newExpression.IndexOf("domainName($feature", StringComparison.OrdinalIgnoreCase);
          }

          newIndex = newExpression.IndexOf("$feature[", StringComparison.OrdinalIgnoreCase);
          while (newIndex > -1)
          {
            int subEnd = newExpression.IndexOf(']', newIndex);

            string newField = ChangeFieldNameInExpression(newExpression[newIndex..(subEnd + 1)]);

            if ((subEnd + 2) < newExpression.Length)
            {
              if (newIndex > 0)
              {
                newExpression = newExpression[..(newIndex - 1)] + newField + newExpression[(subEnd + 2)..];
              }
              else
              {
                newExpression = newField + newExpression[(subEnd + 1)..];
              }
            }
            else
            {
              newExpression = newExpression[..newIndex] + newField;
            }
            newIndex = newExpression.IndexOf("$feature[", StringComparison.OrdinalIgnoreCase);
          }

          return string.Concat(OriginExpression[..(index + 13)], newExpression, OriginExpression[end..]);
        }
      }

      index = OriginExpression.IndexOf("$feature.", StringComparison.OrdinalIgnoreCase);
      while (index != -1)
      {
        index += 9;
        int end = OriginExpression.IndexOf(' ', index);
        string fieldName;
        if (end != -1)
        {
          fieldName = OriginExpression[index..end].Trim();
        }
        else
        {
          fieldName = OriginExpression[index..].Trim();
        }
        CustomField custom = GetCustomField(fieldName);
        if (custom != null)
        {
          string newExpression = OriginExpression.Replace(fieldName, custom.NewModelName);

          OriginExpression = newExpression;
        }
        else
        {
          break;
        }
        index = OriginExpression.IndexOf("$feature.", index, StringComparison.OrdinalIgnoreCase);
      }

      index = OriginExpression.IndexOf("domainName($feature,", StringComparison.OrdinalIgnoreCase);
      while (index != -1)
      {
        int newIndex = index + 21;
        string fieldName = OriginExpression[newIndex..];
        int end = fieldName.IndexOf("'");
        if (end == 0)
        {
          fieldName = fieldName[1..];
          end = fieldName.IndexOf("'");
        }
        fieldName = fieldName[..end];

        CustomField custom = GetCustomField(fieldName);

        if (custom != null)
        {
          end = OriginExpression.IndexOf(')', index);

          string newExpression = OriginExpression[..index] + string.Format("$feature.{0}", custom.NewModelName) + OriginExpression[(end + 1)..];

          index = newExpression.IndexOf(" IN (", StringComparison.OrdinalIgnoreCase);

          if (index != -1)
          {
            index += 5;
            end = newExpression.IndexOf(')');

            string[] values = newExpression[index..end].Split(',');
            List<string> newValues = new();
            for (int i = 0; i < values.Length; i++)
            {
              newValues.Add(custom.GetDomainValue(values[i]));
            }

            newExpression = newExpression[..index];

            foreach (string value in newValues)
            {
              newExpression += string.Format(" '{0}',", value);
            }

            return newExpression[..^2] + ")";
          }

          OriginExpression = newExpression;
        }
        else
        {
          break;
        }

        index = OriginExpression.IndexOf("domainName($feature,", StringComparison.OrdinalIgnoreCase);
      }

      index = OriginExpression.IndexOf("$feature['", StringComparison.OrdinalIgnoreCase);
      while (index != -1)
      {
        int newIndex = index + 10;
        string fieldName = OriginExpression[newIndex..];
        int end = fieldName.IndexOf("'");
        fieldName = fieldName[..end];

        CustomField custom = GetCustomField(fieldName);

        if (custom != null)
        {
          string newExpression = string.Format("$feature.{0}", custom.NewModelName);

          OriginExpression = OriginExpression.Replace(string.Format("$feature['{0}']", fieldName), newExpression);
        }
        else
        {
          break;
        }
        index = OriginExpression.IndexOf("$feature['", StringComparison.OrdinalIgnoreCase);
      }

      index = OriginExpression.IndexOf("[");
      while (index != -1)
      {
        int newIndex = index + 1;
        string fieldName = OriginExpression[newIndex..];
        int end = fieldName.IndexOf("]");
        fieldName = fieldName[..end];

        CustomField custom = GetCustomField(fieldName);

        if (custom != null)
        {
          string newExpression = "[" + custom.NewModelName + "]";

          OriginExpression = OriginExpression.Replace(string.Format("[{0}]", fieldName), newExpression);
        }
        else
        {
          break;
        }
        index = OriginExpression.IndexOf("[", index + 2);
      }

      return OriginExpression;
    }
    #endregion

    #region Change Symbology
    /// <summary>
    /// Change symbol properties
    /// </summary>
    /// <param name="OriginSymbolReference">Source symbol reference</param>
    /// <returns>New symbol reference</returns>
    private CIMSymbolReference ChangeSymbolProperties(CIMSymbolReference OriginSymbolReference)
    {
      if (OriginSymbolReference == null) return null;

      return new CIMSymbolReference()
      {
        Symbol = ChangeSymbol(OriginSymbolReference.Symbol),
        SymbolName = OriginSymbolReference.SymbolName,
        PrimitiveOverrides = ChangeFieldNameInPrimitives(OriginSymbolReference.PrimitiveOverrides)
      };
    }

    /// <summary>
    /// Change Symbol
    /// </summary>
    /// <param name="OriginSymbol">Origin Symbol</param>
    /// <returns>New Symbol</returns>
    private CIMSymbol ChangeSymbol(CIMSymbol OriginSymbol)
    {
      if (OriginSymbol == null) return null;

      if (OriginSymbol is CIMPointSymbol pointSymbol)
      {
        return new CIMPointSymbol()
        {
          Angle = pointSymbol.Angle,
          AngleAlignment = pointSymbol.AngleAlignment,
          Callout = pointSymbol.Callout,
          Effects = pointSymbol.Effects,
          HaloSize = pointSymbol.HaloSize,
          HaloSymbol = (CIMPolygonSymbol)ChangeSymbol(pointSymbol.HaloSymbol),
          PrimitiveName = pointSymbol.PrimitiveName,
          ScaleX = pointSymbol.ScaleX,
          Symbol3DProperties = pointSymbol.Symbol3DProperties,
          SymbolLayers = CreateSymbolSubLayers(pointSymbol.SymbolLayers),
          ThumbnailURI = pointSymbol.ThumbnailURI,
          UseRealWorldSymbolSizes = pointSymbol.UseRealWorldSymbolSizes
        };
      }
      else if (OriginSymbol is CIMPolygonSymbol polygonSymbol)
      {
        return new CIMPolygonSymbol()
        {
          Effects = polygonSymbol.Effects,
          SymbolLayers = CreateSymbolSubLayers(polygonSymbol.SymbolLayers),
          ThumbnailURI = polygonSymbol.ThumbnailURI,
          UseRealWorldSymbolSizes = polygonSymbol.UseRealWorldSymbolSizes
        };
      }
      else if (OriginSymbol is CIMLineSymbol lineSymbol)
      {
        return new CIMLineSymbol()
        {
          Effects = lineSymbol.Effects,
          SymbolLayers = CreateSymbolSubLayers(lineSymbol.SymbolLayers),
          ThumbnailURI = lineSymbol.ThumbnailURI,
          UseRealWorldSymbolSizes = lineSymbol.UseRealWorldSymbolSizes
        };
      }
      else
      {
        return OriginSymbol;
      }
    }

    /// <summary>
    /// Create symbol sublayers
    /// </summary>
    /// <param name="OriginSymbolLayers">Source CIMSymbolLayer array</param>
    /// <returns>New CIMSymbolLayer array</returns>
    private CIMSymbolLayer[] CreateSymbolSubLayers(CIMSymbolLayer[] OriginSymbolLayers)
    {
      List<CIMSymbolLayer> newSymbolLayerList = new();
      foreach (CIMSymbolLayer symbolLayer in OriginSymbolLayers)
      {
        CIMSymbolLayer newSymbolLayer = CreateSymbolLayer(OriginReference: symbolLayer);

        newSymbolLayerList.Add(newSymbolLayer);
      }

      return newSymbolLayerList.ToArray();
    }

    /// <summary>
    /// Create symbol layer
    /// </summary>
    /// <param name="OriginReference">Source CIMSymbolLayer</param>
    /// <returns>New CIMSymbolLayer</returns>
    private CIMSymbolLayer CreateSymbolLayer(CIMSymbolLayer OriginReference)
    {
      if (OriginReference == null)
        return null;

      if (OriginReference is CIMVectorMarker vectorMarker)
      {
        return new CIMVectorMarker()
        {
          AnchorPoint = vectorMarker.AnchorPoint,
          AnchorPointUnits = vectorMarker.AnchorPointUnits,
          AngleX = vectorMarker.AngleX,
          AngleY = vectorMarker.AngleY,
          DominantSizeAxis3D = vectorMarker.DominantSizeAxis3D,
          BillboardMode3D = vectorMarker.BillboardMode3D,
          ClippingPath = vectorMarker.ClippingPath,
          ColorLocked = vectorMarker.ColorLocked,
          Depth3D = vectorMarker.Depth3D,
          Enable = vectorMarker.Enable,
          Frame = vectorMarker.Frame,
          MarkerPlacement = vectorMarker.MarkerPlacement,
          Name = vectorMarker.Name,
          OffsetX = vectorMarker.OffsetX,
          OffsetY = vectorMarker.OffsetY,
          OffsetZ = vectorMarker.OffsetZ,
          Overprint = vectorMarker.Overprint,
          PrimitiveName = vectorMarker.PrimitiveName,
          RespectFrame = vectorMarker.RespectFrame,
          RotateClockwise = vectorMarker.RotateClockwise,
          Rotation = vectorMarker.Rotation,
          ScaleSymbolsProportionally = vectorMarker.ScaleSymbolsProportionally,
          Size = vectorMarker.Size,
          VerticalOrientation3D = vectorMarker.VerticalOrientation3D,
          Effects = OriginReference.Effects,
          MarkerGraphics = ModifyMarkerGraphic(OriginMarkerGraphics: vectorMarker.MarkerGraphics)
        };
      }
      else if (OriginReference is CIMSolidFill solidFill)
      {
        return new CIMSolidFill()
        {
          Color = solidFill.Color,
          Effects = solidFill.Effects,
          ColorLocked = solidFill.ColorLocked,
          Enable = solidFill.Enable,
          Name = solidFill.Name,
          Overprint = solidFill.Overprint,
          PrimitiveName = solidFill.PrimitiveName
        };
      }
      else if (OriginReference is CIMSolidStroke solidStroke)
      {
        return new CIMSolidStroke()
        {
          CapStyle = solidStroke.CapStyle,
          CloseCaps3D = solidStroke.CloseCaps3D,
          Color = solidStroke.Color,
          ColorLocked = solidStroke.ColorLocked,
          Effects = solidStroke.Effects,
          Enable = solidStroke.Enable,
          JoinStyle = solidStroke.JoinStyle,
          LineStyle3D = solidStroke.LineStyle3D,
          MiterLimit = solidStroke.MiterLimit,
          Name = solidStroke.Name,
          Overprint = solidStroke.Overprint,
          PrimitiveName = solidStroke.PrimitiveName,
          Width = solidStroke.Width
        };
      }
      else if (OriginReference is CIMCharacterMarker characterMarker)
      {
        return new CIMCharacterMarker()
        {
          AnchorPoint = characterMarker.AnchorPoint,
          AnchorPointUnits = characterMarker.AnchorPointUnits,
          AngleX = characterMarker.AngleX,
          AngleY = characterMarker.AngleY,
          DominantSizeAxis3D = characterMarker.DominantSizeAxis3D,
          BillboardMode3D = characterMarker.BillboardMode3D,
          CharacterIndex = characterMarker.CharacterIndex,
          ColorLocked = characterMarker.ColorLocked,
          Depth3D = characterMarker.Depth3D,
          Effects = characterMarker.Effects,
          Enable = characterMarker.Enable,
          FontFamilyName = characterMarker.FontFamilyName,
          FontStyleName = characterMarker.FontStyleName,
          FontType = characterMarker.FontType,
          FontVariationSettings = characterMarker.FontVariationSettings,
          MarkerPlacement = characterMarker.MarkerPlacement,
          Name = characterMarker.Name,
          OffsetX = characterMarker.OffsetX,
          OffsetY = characterMarker.OffsetY,
          OffsetZ = characterMarker.OffsetZ,
          Overprint = characterMarker.Overprint,
          PrimitiveName = characterMarker.PrimitiveName,
          RespectFrame = characterMarker.RespectFrame,
          RotateClockwise = characterMarker.RotateClockwise,
          Rotation = characterMarker.Rotation,
          ScaleSymbolsProportionally = characterMarker.ScaleSymbolsProportionally,
          ScaleX = characterMarker.ScaleX,
          Size = characterMarker.Size,
          Symbol = (CIMPolygonSymbol)ChangeSymbol(characterMarker.Symbol),
          VerticalOrientation3D = characterMarker.VerticalOrientation3D
        };
      }
      else if (OriginReference is CIMPictureMarker pictureMarker)
      {
        return new CIMPictureMarker()
        {
          AnchorPoint = pictureMarker.AnchorPoint,
          AnchorPointUnits = pictureMarker.AnchorPointUnits,
          AngleX = pictureMarker.AngleX,
          AngleY = pictureMarker.AngleY,
          AnimatedSymbolProperties = pictureMarker.AnimatedSymbolProperties,
          DominantSizeAxis3D = pictureMarker.DominantSizeAxis3D,
          BillboardMode3D = pictureMarker.BillboardMode3D,
          ColorLocked = pictureMarker.ColorLocked,
          ColorSubstitutions = pictureMarker.ColorSubstitutions,
          Depth3D = pictureMarker.Depth3D,
          Effects = pictureMarker.Effects,
          OffsetZ = pictureMarker.OffsetZ,
          Enable = pictureMarker.Enable,
          InvertBackfaceTexture = pictureMarker.InvertBackfaceTexture,
          MarkerPlacement = pictureMarker.MarkerPlacement,
          Name = pictureMarker.Name,
          OffsetX = pictureMarker.OffsetX,
          OffsetY = pictureMarker.OffsetY,
          Overprint = pictureMarker.Overprint,
          PrimitiveName = pictureMarker.PrimitiveName,
          RotateClockwise = pictureMarker.RotateClockwise,
          Rotation = pictureMarker.Rotation,
          ScaleX = pictureMarker.ScaleX,
          Size = pictureMarker.Size,
          TextureFilter = pictureMarker.TextureFilter,
          TintColor = pictureMarker.TintColor,
          URL = pictureMarker.URL,
          VerticalOrientation3D = pictureMarker.VerticalOrientation3D
        };

      }
      else if (OriginReference is CIMHatchFill hatchFill)
      {
        return new CIMHatchFill()
        {
          ColorLocked = hatchFill.ColorLocked,
          Effects = hatchFill.Effects,
          Enable = hatchFill.Enable,
          LineSymbol = (CIMLineSymbol)ChangeSymbol(hatchFill.LineSymbol),
          Name = hatchFill.Name,
          OffsetX = hatchFill.OffsetX,
          OffsetY = hatchFill.OffsetY,
          Overprint = hatchFill.Overprint,
          PrimitiveName = hatchFill.PrimitiveName,
          Rotation = hatchFill.Rotation,
          Separation = hatchFill.Separation
        };
      }
      else
      {

      }

      return OriginReference;
    }

    /// <summary>
    /// Modify marker graphic
    /// </summary>
    /// <param name="OriginMarkerGraphics">Source CIMMarkerGraphic array</param>
    /// <returns>New CIMMarkerGraphic array</returns>
    private CIMMarkerGraphic[] ModifyMarkerGraphic(CIMMarkerGraphic[] OriginMarkerGraphics)
    {
      if (OriginMarkerGraphics == null)
      { return null; }

      List<CIMMarkerGraphic> newGraphics = new();

      foreach (CIMMarkerGraphic markerGraphic in OriginMarkerGraphics)
      {
        CIMMarkerGraphic newMarker = new()
        {
          Geometry = markerGraphic.Geometry,
          PrimitiveName = markerGraphic.PrimitiveName,
          TextString = markerGraphic.TextString,
          Symbol = ChangeSymbol(markerGraphic.Symbol)
        };
        newGraphics.Add(newMarker);
      }

      return newGraphics.ToArray();
    }
    #endregion

    #region Others
    /// <summary>
    /// Get Asset Group custom field
    /// </summary>
    /// <returns>Custom field</returns>
    private CustomField GetAssetGroupField()
    {
      return _activeCustomFields.FirstOrDefault(a => a.IsAssetGroup);
    }

    /// <summary>
    /// Get custom field
    /// </summary>
    /// <param name="FieldName">Field name</param>
    /// <param name="UseNewName">Use new name</param>
    /// <returns>Custom field</returns>
    private CustomField GetCustomField(string FieldName, bool UseNewName = false)
    {
      if (string.IsNullOrEmpty(FieldName))
      {
        return null;
      }

      CustomField custom = UseNewName ? _activeCustomFields.FirstOrDefault(a => a.CompareWithNewName(FieldName)) : _activeCustomFields.FirstOrDefault(a => string.Compare(a.OriginName, FieldName, StringComparison.OrdinalIgnoreCase) == 0);

      if (custom == null)
      {
        custom = _activeCustomFields.FirstOrDefault(a => a.CompareWithNewName(FieldName));
      }
      return custom;
    }

    /// <summary>
    /// Increase step and change message in cancelable progressor
    /// </summary>
    /// <param name="ProgressorCancelable">Cancelable progressor source</param>
    /// <param name="Message">Message to display</param>
    /// <returns>True if canceled</returns>
    internal bool CancelOrIncreaseProgressor(CancelableProgressorSource ProgressorCancelable, string Message)
    {
      if (ProgressorCancelable.Progressor.CancellationToken.IsCancellationRequested)
      {
        _canceledStatus = "Canceled by user";
        return true;
      }
      else if (!string.IsNullOrEmpty(_canceledStatus))
      {
        return true;
      }

      ProgressorCancelable.Progressor.Value += 1;
      ProgressorCancelable.Progressor.Status = (ProgressorCancelable.Progressor.Value * 100 / ProgressorCancelable.Progressor.Max) + @" % Completed";
      ProgressorCancelable.Progressor.Message = string.Format("Step {0} – {1}", ProgressorCancelable.Progressor.Value, Message);

      System.Diagnostics.Debug.WriteLine(string.Format("\n{0}  // {1} ", ProgressorCancelable.Progressor.Message, ProgressorCancelable.Progressor.Status));

      _canceledStatus = "";
      return false;
    }

    /// <summary>
    /// Get the number of operations to execute
    /// Number of features to create
    /// Number of layers to create - optional
    /// Aggregation table to create and fill - optional
    /// </summary>
    /// <returns>Total of operation</returns>
    private uint GetNumberOfOperation()
    {
      _diagramLayer = GetDiagramLayerFromMap(_originMap);
      if (_diagramLayer == null)
      {
        _canceledStatus = "The active map is not a diagram map";
        {
          return 0;
        }
      }

      int allLayers = _diagramLayer.GetLayersAsFlattenedList().Count; // export layer
      int mainLayers = _diagramLayer.Layers.Count; // create and fill table

      return (uint)(allLayers + mainLayers + (_createMap ? 3 : 0) + (_exportAggregation ? 2 : 0));
    }
    #endregion
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  internal class ExportPane_ShowButton : Button
  {
    protected override void OnClick()
    {
      ExportPaneViewModel.Show();
    }

  }
}

