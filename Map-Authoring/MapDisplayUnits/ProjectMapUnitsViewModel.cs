/*

   Copyright 2020 Esri

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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.UnitFormats;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace MapUnits
{
  internal class ProjectMapUnitsViewModel : DockPane
  {
    private const string _dockPaneID = "MapUnits_ProjectMapUnits";

    protected ProjectMapUnitsViewModel() {
      ListOfProjectUnits.Clear();
      var units = Enum.GetValues(typeof(UnitFormatType));
      foreach (var unit in units)
      {
        ListOfProjectUnits.Add(unit.ToString());
      }
      SelectedUnit = ListOfProjectUnits[0];
      SceneVisibility = Visibility.Hidden;
      UpdateMapCollection();
    }

    protected override Task InitializeAsync()
    {

      return base.InitializeAsync();
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
    private string _heading = "Set Default Display Units for this project";
    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }

    private string _mapHeading = "Change Map Display Units";
    public string MapHeading
    {
      get { return _mapHeading; }
      set
      {
        SetProperty(ref _mapHeading, value, () => MapHeading);
      }
    }

    private ObservableCollection<string> _listOfProjectUnits = new ObservableCollection<string>();
    public ObservableCollection<string> ListOfProjectUnits
    {
      get {

        return _listOfProjectUnits;
      }

    }
    private string _selectedUnit;
    /// <summary>
    /// Location, Distance, Angular, etc
    /// </summary>
    public string SelectedUnit
    {
      get { return _selectedUnit; }
      set
      {
        SetProperty(ref _selectedUnit, value, () => SelectedUnit);
        UpdateDisplayUnits(); //gets all the "Display Units" for the "Unit Format Type" selected. Unit Format types are: Distance, Location, Angular, etc.
      }
    }
    private static readonly object _displayUnitFormatItemsLock = new object();
    private ObservableCollection<DisplayUnitFormatItem> _displayUnitFormatItems = new ObservableCollection<DisplayUnitFormatItem>();
    public ObservableCollection<DisplayUnitFormatItem> DisplayUnitFormatItems
    {
      get { return _displayUnitFormatItems; }

    }

    private DisplayUnitFormatItem _selectedDisplayUnitFormatItem;
    public DisplayUnitFormatItem SelectedDisplayUnitFormatItem
    {
      get { return _selectedDisplayUnitFormatItem; }
      set
      {
        SetProperty(ref _selectedDisplayUnitFormatItem, value, () => SelectedDisplayUnitFormatItem);
      }
    }
    private ObservableCollection<Map> _listOfMaps = new ObservableCollection<Map>();
    public ObservableCollection<Map> ListOfMaps
    {
      get { return _listOfMaps; }

    }

    private Map _selectedMap;
    public Map SelectedMap
    {
      get { return _selectedMap; }
      set
      {
        SetProperty(ref _selectedMap, value, () => SelectedMap);
        //Map selection changed. So get the list of available Location units for the new map. 
        //Applicable for Map and Scene.
        GetAvailableDisplayUnits();

        //If selected map is scene, get the list of available Elevation units for the new scene also
        //Scene's have "Location" units and "Elevation Units". Elevation units use the "Distance" UnitFormatType.
        if (SelectedMap.IsScene)
        {
          SceneVisibility = Visibility.Visible;
          GetAvailableElevationUnits();
        }
        else
        {
          SceneVisibility = Visibility.Hidden;
        }                 
        //GetMapDisplayUnitFormatAsync();
        MapSpatialReference = SelectedMap?.SpatialReference.Name;
      }
    }
    private string _mapSpatialReference;
    public string MapSpatialReference
    {
      get {
        
        return _mapSpatialReference;  }
      set
      {
        SetProperty(ref _mapSpatialReference, value, () => MapSpatialReference);
      }
    }

    private ObservableCollection<DisplayUnitFormat> _mapAvailableDisplayUnits = new ObservableCollection<DisplayUnitFormat>();

    public ObservableCollection<DisplayUnitFormat> MapAvailableDisplayUnits
    {
      get { 
        
        return _mapAvailableDisplayUnits; }
    }

    private DisplayUnitFormat _selectedMapAvailableDisplayUnit;

    /// <summary>
    /// Selected Display Unit in the collection of "available display units" for a given map
    /// </summary>
    public DisplayUnitFormat SelectedMapAvailableDisplayUnit
    {
      get { return _selectedMapAvailableDisplayUnit; }
      set { 
        SetProperty(ref _selectedMapAvailableDisplayUnit, value, () => SelectedMapAvailableDisplayUnit);
        //Compare the Map's Display Unit with the select display unit. If they are different, enable the Apply button.
        CanChangeMapDU = _mapDisplayUnitFormat?.UnitCode == SelectedMapAvailableDisplayUnit?.UnitCode ? false : true;
      }
    }
    private ObservableCollection<DisplayUnitFormat> _sceneAvailableElevUnits = new ObservableCollection<DisplayUnitFormat>();
    public ObservableCollection<DisplayUnitFormat> SceneAvailableElevUnits
    {
      get { return _sceneAvailableElevUnits; }

    }

    private DisplayUnitFormat _selectedSceneAvailableElevUnit;
    public DisplayUnitFormat SelectedSceneAvailableElevUnit
    {
      get { return _selectedSceneAvailableElevUnit; }
      set
      {
        SetProperty(ref _selectedSceneAvailableElevUnit, value, () => SelectedSceneAvailableElevUnit);
        CanChangeSceneEU = _sceneElevationUnitFormat?.UnitCode == SelectedSceneAvailableElevUnit?.UnitCode ? false : true;
      }
    }
    private Visibility _sceneVisibility;

    public Visibility SceneVisibility
    {
      get { return _sceneVisibility; }
      set
      {
        SetProperty(ref _sceneVisibility, value, () => SceneVisibility);
      }
    }

    private ICommand _makeDefaultCommand;
    public ICommand MakeDefaultCommand
    {
      get {
        _makeDefaultCommand = new RelayCommand(() => MakeDisplayUnitAsDefault(), IsNotDefaultDisplayFormat );
        return _makeDefaultCommand; 
      }
    }
    private bool IsNotDefaultDisplayFormat()
    {
      return SelectedDisplayUnitFormatItem.IsDefaultFormat ? false : true;
    }

    private void MakeDisplayUnitAsDefault()
    {
      if (SelectedDisplayUnitFormatItem == null) return;
      SelectedDisplayUnitFormatItem.SetDefaultDisplayUnit();
    }
    private ICommand _changeMapDisplayUnitCommand;
    public ICommand ChangeMapDisplayUnitCommand
    {
      get
      {
        _changeMapDisplayUnitCommand = new RelayCommand(() => ChangeMapDisplayUnit(), CanChangeMapDisplayUnit);
        return _changeMapDisplayUnitCommand;
      }
    }

    //We check if the Selected Map's Display Unit is same as the one selected in the drop down. 
    //If they are the same, Apply button should be disabled.
    //If different, Apply button should be enabled.
    public bool CanChangeMapDisplayUnit()
    {
      return CanChangeMapDU;
    }

    public void ChangeMapDisplayUnit()
    {
      if (SelectedMap == null) return;
      if (SelectedMapAvailableDisplayUnit == null) return; //No DisplayUnitFormat selected.
      //Change the selected map's display unit.
      QueuedTask.Run( () => SelectedMap.SetLocationUnitFormat(SelectedMapAvailableDisplayUnit));
      CanChangeMapDU = false; //Map's Display Unit will match the selected display unit. Since we just made the change.

    }

    private ICommand _changeSceneDisplayUnitCommand;
    private DisplayUnitFormat _mapDisplayUnitFormat;
    private DisplayUnitFormat _sceneElevationUnitFormat;
    public bool CanChangeMapDU;
    public bool CanChangeSceneEU;
    public ICommand ChangeSceneDisplayUnitCommand
    {
      get
      {
        _changeSceneDisplayUnitCommand = new RelayCommand(() => ChangeSceneDisplayUnit(), CanChangeSceneDisplayUnit);
        return _changeSceneDisplayUnitCommand;
      }
    }

    public bool CanChangeSceneDisplayUnit()
    {
      return CanChangeSceneEU;
    }

    public void ChangeSceneDisplayUnit()
    {
      if (SelectedMap == null) return;
      if (SelectedSceneAvailableElevUnit == null) return; //No DisplayUnitFormat selected.
      //Change the selected scene's elevation unit.
      QueuedTask.Run(() => SelectedMap.SetElevationUnitFormat(SelectedSceneAvailableElevUnit));
      CanChangeSceneEU = false; //Scene's Elevation Unit will match the selected unit. Since we just made the change.
    }    
    /// <summary>
    /// Given a selected map, get the list of available map location unit formats.  
    /// This will be displayed in the combo box. The map's display unit can be changed to any of these location unit values.
    /// </summary>
    private async void GetAvailableDisplayUnits()
    {
      //Clear the current available units
      MapAvailableDisplayUnits.Clear();
      var units = await QueuedTask.Run(() =>
      {
        //Gets the list of available map location unit formats for the given map.
        return SelectedMap.GetAvailableLocationUnitFormats();
      });
      //Update combo box binding collection to the units for the selected map.
      foreach (var item in units)
      {
        MapAvailableDisplayUnits.Add(item);
      }
      //Gets the current map location unit format for the current project
      _mapDisplayUnitFormat = await QueuedTask.Run(() =>
      {
         return SelectedMap.GetLocationUnitFormat();
      });
      //Set the location unit combo box item to the map's location unit format (Meters, decimal degree, etc)
      SelectedMapAvailableDisplayUnit = MapAvailableDisplayUnits.FirstOrDefault(u => u.UnitCode == _mapDisplayUnitFormat.UnitCode);
      //Should the Apply button be enabled?
      CanChangeMapDU = false; //They will match
    }
    /// <summary>
    /// Given a selected scene, get the list of available elevation unit formats.  
    /// This will be displayed in the combo box. The scene's elevation unit can be changed to any of these location unit values.
    /// </summary>
    private async void GetAvailableElevationUnits()
    {
      //Clear the current available units
      SceneAvailableElevUnits.Clear();
      var units = await QueuedTask.Run( () => {
        //Gets the list of available Scene Elevation unit formats for the given scene.
        return SelectedMap.GetAvailableElevationUnitFormats();
      });
      //Update combo box binding collection to the units for the selected scene.
      foreach (var item in units)
      {
        SceneAvailableElevUnits.Add(item);
      }
      //Gets the current scene elevation unit format for the current project
      _sceneElevationUnitFormat = await QueuedTask.Run( () => {
        return SelectedMap.GetElevationUnitFormat();
      });
      SelectedSceneAvailableElevUnit = SceneAvailableElevUnits.FirstOrDefault(e => e.UnitCode == _sceneElevationUnitFormat.UnitCode);
      //Should the Apply button be enabled?
      CanChangeSceneEU = false;
    }
    /// <summary>
    /// Gets all the "Display Units" for the "Unit Format Type" selected. 
    /// Unit Format types are: Distance, Location, Angular, etc.
    /// Each Format type has "DisplayUnitFormats". Distance has meters, foot, etc.
    /// </summary>
    public async void UpdateDisplayUnits()
    {
      List<DisplayUnitFormatItem> items = new List<DisplayUnitFormatItem>(); //DisplayUnitFormatItem is custom type for this sample.
      if (SelectedUnit == null) return;
      await QueuedTask.Run(() => {
        //Getting the UnitFormatType object from the selected item in the combo box.  
        UnitFormatType UnitFormatTypeValue = (UnitFormatType)Enum.Parse(typeof(UnitFormatType), SelectedUnit, true);
        //Get the list of DisplayUnitFormats in the current project for the given ArcGIS.Desktop.Core.UnitFormats.UnitFormatType
        var unitFormats = DisplayUnitFormats.Instance.GetProjectUnitFormats(UnitFormatTypeValue);
        //Gets the default DisplayUnitFormat in the current project for the given ArcGIS.Desktop.Core.UnitFormats.UnitFormatType.
        var defaultUnitFormat = DisplayUnitFormats.Instance.GetDefaultProjectUnitFormat(UnitFormatTypeValue);
        foreach (var unitFormat in unitFormats)
        {
          //is the unit format the default for this type?
          bool IsUnitFormatDefault = defaultUnitFormat.UnitCode == unitFormat.UnitCode ? true : false;          
          items.Add(new DisplayUnitFormatItem(UnitFormatTypeValue, unitFormat, IsUnitFormatDefault));
        }
      });
      if (items == null) return;
      DisplayUnitFormatItems.Clear();
      foreach (var item in items)
      {
        DisplayUnitFormatItems.Add(item);
      }
    }     
    public async void UpdateMapCollection()
    {
      var projectMapItems = Project.Current.GetItems<MapProjectItem>();
      if (projectMapItems == null) return;
      var projectMaps = await QueuedTask.Run(() =>
      {
        List<Map> maps = new List<Map>();
        foreach (var item in projectMapItems)
        {
          maps.Add(item.GetMap());
        }
        return maps;
      });
      if (projectMaps.Count == 0) return;
      ListOfMaps.Clear();
      foreach (var map in projectMaps)
      {
        ListOfMaps.Add(map);        
      }
      SelectedMap = ListOfMaps[0];      
    }    
  }
  

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class ProjectMapUnits_ShowButton : Button
  {
    protected override void OnClick()
    {
      ProjectMapUnitsViewModel.Show();
    }
  }
}
