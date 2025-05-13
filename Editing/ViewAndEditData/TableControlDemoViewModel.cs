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
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Editing.Controls;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Internal.Catalog;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework.Controls;
using System.Windows.Controls;
using ArcGIS.Desktop.Mapping.Events;
using System.Windows.Input;
using System.Windows;

namespace ViewAndEditData
{
  internal class TableControlDemoViewModel : DockPane
  {
    private const string _dockPaneID = "ViewAndEditData_TableControlDemo";
    private Inspector _featureInspector = null;
    private EmbeddableControl _inspectorViewModel = null;
    private UserControl _inspectorView = null;
    internal MapMember _mapMember = null;
    public TableControlDemoViewModel()
    {
      // create a new instance for the inspector
      _featureInspector = new Inspector();
      // create an embeddable control from the inspector class to display on the pane
      var icontrol = _featureInspector.CreateEmbeddableControl();
      // get view and viewmodel from the inspector
      InspectorViewModel = icontrol.Item1;
      InspectorView = icontrol.Item2;
      //Subscribe to events
      ProjectWindowSelectedItemsChangedEvent.Subscribe(OnProjectWindowSelectedItem);
      TOCSelectionChangedEvent.Subscribe(OnTOCSelectionChanged);
    }

    #region Event handlers
    /// <summary>
    /// Event handler for the TOC selection changed event. This is called when the user selects a layer or table in the TOC.
    /// </summary>
    /// <param name="args"></param>
    private void OnTOCSelectionChanged(MapViewEventArgs args)
    {
      var map = MapView.Active.Map;
      if (map == null) return;
      //Get the selected layer or table
      var selectedLayer = MapView.Active.GetSelectedLayers().OfType<FeatureLayer>().FirstOrDefault();
      if (selectedLayer != null)
      {
        _mapMember = selectedLayer;
        Heading = $"MapMember: {selectedLayer.Name}";
      }
      else //table?
      {
        var selectedTable = MapView.Active.GetSelectedStandaloneTables().FirstOrDefault();
        if (selectedTable != null)
        {
          _mapMember = selectedTable;
          Heading = $"MapMember: {selectedTable.Name}";
        }
        else
        {
          _mapMember = null;
          Heading = "Data Viewer";
        }
      }
      // check if it's supported by the TableControl
      if (!TableControlContentFactory.IsMapMemberSupported(_mapMember))
      {
        _mapMember = null;
        return;
      }

      // create the content
      var tableContent = TableControlContentFactory.Create(_mapMember);
      // assign it
      if (tableContent != null)
      {
        this.TableContent = tableContent;
        if (MyTableControl != null) 
        {
          //Subscribe to the TableContentLoaded event
          MyTableControl.TableContentLoaded += MyTableControl_TableContentLoaded;
        }
      }
    }
    /// <summary>
    /// Event handler for the ProjectWindowSelectedItemsChangedEvent. This is called when the user selects an item in the Catalog window.
    /// </summary>
    /// <param name="args"></param>
    private void OnProjectWindowSelectedItem(ProjectWindowSelectedItemsChangedEventArgs args)
    {
      if (args.IProjectWindow.SelectionCount > 0)
      {
        // get the first selected item
        var item = args.IProjectWindow.SelectedItems.First();

        // check if it's supported by the TableControl
        if (!TableControlContentFactory.IsItemSupported(item))
        {
          IsExternalDatasource = Visibility.Hidden;
          return;
        }
        _selectedItem = item;
        _mapMember = null; //clear the map member
        // create the content
        var tableContent = TableControlContentFactory.Create(_selectedItem);
        Heading = $"External Datasource: {_selectedItem.Name}";
        ExternalDatasourceText = $"Add external datasource {_selectedItem.Name} to the map to view or edit using the Inspector.";
        // assign it
        if (tableContent != null)
        {
          this.TableContent = tableContent;
          if (MyTableControl != null)
          {
            //Subscribe to the TableContentLoaded event
            MyTableControl.TableContentLoaded += MyTableControl_TableContentLoaded;
          }
        }
      }
    }

    internal async void MyTableControl_TableContentLoaded(object sender, EventArgs e)
    {
      var tableControl = sender as TableControl;
      if (tableControl == null) return;
      //Display the Find tool on the table control
      tableControl.Find();
      IList<long> oids = new List<long>();
      //Is there any selected features in the table control?
      await QueuedTask.Run( ()=> {
        oids = tableControl.GetSelectedObjectIds().ToList();
        //hide inspector when no features are selected
        if (oids.Count ==0) InspectorVisibility = Visibility.Hidden; 
        //it is a map member with selections, so inspector is updated 
        if (oids.Count > 0 && _mapMember != null) 
        {
          AttributeInspector.Load(_mapMember, oids.FirstOrDefault());
        }
      });
      # region Set visibility of the inspector and external datasource text
      if (_mapMember == null)
      {
        IsExternalDatasource = Visibility.Visible;
      }
      else //map member
      {
        //hide external datasource text when map member is selected 
        IsExternalDatasource = Visibility.Hidden;  
        if (oids.Count > 0) //map member, with selections
        {
          InspectorVisibility = Visibility.Visible;
        }
        else //map member, no selections
          InspectorVisibility = Visibility.Hidden; //hide inspector when no features are selected
      }
      #endregion

      tableControl.SelectedRowsChanged += MyTableControl_SelectedRowsChanged;
    }

    internal async void MyTableControl_SelectedRowsChanged(object sender, EventArgs e)
    {
      var tableControl = sender as TableControl;
      if (tableControl == null) return;
      IList<long> oids = new List<long>();
      await QueuedTask.Run(() =>
      {
        oids = tableControl.GetSelectedObjectIds().ToList();
        if (oids.Count> 0 && _mapMember != null) {
          AttributeInspector.Load(_mapMember, oids.FirstOrDefault());
        }
      });
# region Set visibility of the inspector and external datasource text
      if (_mapMember == null)
      {
        IsExternalDatasource = Visibility.Visible;
      }
      else //map member
      {
        IsExternalDatasource = Visibility.Hidden; //hide external datasource text when map member is selected
        if (oids.Count > 0) //map member, with selections
        {
          InspectorVisibility = Visibility.Visible;
        }
        else //map member, no selections
          InspectorVisibility = Visibility.Hidden; //hide inspector when no features are selected
      }
      #endregion
    }
    #endregion Event handlers

    #region Binding properties
    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Data Viewer";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
    private string _externalDatasourceText = "External Datasource"; 
    public string ExternalDatasourceText
    {
      get => _externalDatasourceText;
      set => SetProperty(ref _externalDatasourceText, value);
    }
    private TableControlContent _tableContent;
    public TableControlContent TableContent
    {
      get => _tableContent;
      set => SetProperty(ref _tableContent, value);
    }

    #endregion
    #region Visibility properties
    private Visibility _isDataSelected = Visibility.Hidden;
    public Visibility IsDataSelected
    {
      get { return _isDataSelected; }
      set => SetProperty(ref _isDataSelected, value);
    }
    private Visibility _isExternalDatasource = Visibility.Hidden;
    public Visibility IsExternalDatasource
    {
      get => _isExternalDatasource;

      set => SetProperty(ref _isExternalDatasource, value);
    }

    private Visibility _inspectorVisibility = Visibility.Hidden;
    public Visibility InspectorVisibility
    {
      get => _inspectorVisibility;
      set => SetProperty(ref _inspectorVisibility, value, () => InspectorVisibility);
    }
    #endregion
    private TableControl MyTableControl
    {
      get
      {
        // use the Content property of the TableControlContent to get the TableControl
        var visualTreeRoot = this.Content as DependencyObject;
        return visualTreeRoot.GetChildOfType<TableControl>();
      }
    }
    private Item _selectedItem;


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

    #region Commands
    public ICommand ApplyCommand
    {
      get
      {
        return new RelayCommand(() =>
        {
          // Save any updates to the database
          _featureInspector.ApplyAsync();
          Project.Current.SaveEditsAsync(); //saves the edits immediately
        });
      }
    }
    public ICommand CancelCommand
    {
      get
      {
        return new RelayCommand(() =>
        {
          // Undo any editing
          _featureInspector.Cancel();
        });
      }
    }
    #endregion Commands

    #region Attributes

    #endregion Attributes

    /// <summary>
    /// Access to the view model of the inspector
    /// </summary>
    public EmbeddableControl InspectorViewModel
    {
      get { return _inspectorViewModel; }
      set
      {
        if (value != null)
        {
          _inspectorViewModel = value;
          _inspectorViewModel.OpenAsync();

        }
        else if (_inspectorViewModel != null)
        {
          _inspectorViewModel.CloseAsync();
          _inspectorViewModel = value;
        }
        NotifyPropertyChanged(() => InspectorViewModel);
      }
    }
    /// <summary>
    /// Property for the inspector UI.
    /// </summary>
    public UserControl InspectorView
    {
      get { return _inspectorView; }
      set
      {
        SetProperty(ref _inspectorView, value, () => InspectorView);
      }
    }
    /// <summary>
    /// Property containing an instance for the inspector.
    /// </summary>
    public Inspector AttributeInspector
    {
      get { return _featureInspector; }
      set
      {
        SetProperty(ref _featureInspector, value, () => AttributeInspector);
      }
    }
   
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class TableControlDemo_ShowButton : ArcGIS.Desktop.Framework.Contracts.Button
  {
    protected override void OnClick()
    {
      TableControlDemoViewModel.Show();
    }
  }
}
