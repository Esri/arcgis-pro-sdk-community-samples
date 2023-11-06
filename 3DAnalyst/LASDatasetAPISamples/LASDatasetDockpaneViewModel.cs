/*

   Copyright 2023 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Analyst3D;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Core;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LASDatasetAPISamples
{
  /// <summary>
  /// Represents the ViewModel for the LASDatasetDockpane.
  /// </summary>
  internal class LASDatasetDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "LASDatasetAPISamples_LASDatasetDockpane";
    private static readonly object _lockObject = new object();
    private static readonly object _lockCC = new object();
    private static readonly object _lockReturnValues = new object();
    private static readonly object _lockFlags = new object();
    protected LASDatasetDockpaneViewModel()
    {
      BindingOperations.EnableCollectionSynchronization(LASLayersInMap, _lockObject);
      BindingOperations.EnableCollectionSynchronization(UniqueClassCodesInLayer, _lockCC);
      BindingOperations.EnableCollectionSynchronization(AllReturnValues, _lockReturnValues);
      BindingOperations.EnableCollectionSynchronization(AllClassificationFlags, _lockFlags);
      ArcGIS.Desktop.Mapping.Events.MapMemberPropertiesChangedEvent.Subscribe(OnMapMemberPropertiesChanged);
      ArcGIS.Desktop.Mapping.Events.ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
      ArcGIS.Desktop.Mapping.Events.LayersAddedEvent.Subscribe(OnLayersAdded);
      ArcGIS.Desktop.Mapping.Events.LayersRemovedEvent.Subscribe(OnLayersRemoved);
    }



    /// <summary>
    /// Triggers when the Dockpane becomes visible/hidden in the UI.
    /// </summary>
    /// <param name="isVisible"></param>
    protected override void OnShow(bool isVisible)
    {
      if (isVisible)
      {
        // Get the LAS layers in the map
        var map = MapView.Active?.Map;

        var lasLayers = map?.GetLayersAsFlattenedList().OfType<LasDatasetLayer>();
        DisplayLASLayerExistsMessage = map == null || lasLayers.Count() == 0 ? Visibility.Hidden : Visibility.Visible;
        DisplayNoLasLayerMessage = DisplayLASLayerExistsMessage == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

        if (map == null || lasLayers.Count() == 0)
          return;

        LASLayersInMap.Clear();

        foreach (var lasLayer in lasLayers)
        {
          LASLayersInMap.Add(lasLayer);
        }

        SelectedLASLayer = LASLayersInMap.FirstOrDefault();
      }
      base.OnShow(isVisible);
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
    #region Binding properties

    private Visibility _displayLASLayerExistsMessage = Visibility.Hidden;
    public Visibility DisplayLASLayerExistsMessage
    {
      get => _displayLASLayerExistsMessage;
      set => SetProperty(ref _displayLASLayerExistsMessage, value);
    }

    private Visibility _displayNoLasLayerMessage = Visibility.Hidden;
    public Visibility DisplayNoLasLayerMessage
    {
      get => _displayNoLasLayerMessage;
      set => SetProperty(ref _displayNoLasLayerMessage, value);
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "LAS Filter";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    private ObservableCollection<LasDatasetLayer> _lasLayersInMap = new ObservableCollection<LasDatasetLayer>();
    public ObservableCollection<LasDatasetLayer> LASLayersInMap
    {
      get => _lasLayersInMap;
      set => SetProperty(ref _lasLayersInMap, value);
    }

    private LasDatasetLayer _selectedLASLayer;
    public LasDatasetLayer SelectedLASLayer
    {
      get => _selectedLASLayer;
      set
      {
        SetProperty(ref _selectedLASLayer, value);
        _ = UpdateFilterSettingsAsync(SelectedLASLayer);
      }
    }
    private ObservableCollection<CustomLASFilterDisplayItem> _uniqueClassCodesInLayer = new ObservableCollection<CustomLASFilterDisplayItem>();
    public ObservableCollection<CustomLASFilterDisplayItem> UniqueClassCodesInLayer
    {
      get => _uniqueClassCodesInLayer;
      set => SetProperty(ref _uniqueClassCodesInLayer, value);
    }
    private CustomLASFilterDisplayItem _selectedClassCodes;
    public CustomLASFilterDisplayItem SelectedClassCodes
    {
      get => _selectedClassCodes;
      set {
        SetProperty(ref _selectedClassCodes, value);
      } 
    }

    private ObservableCollection<CustomLASFilterDisplayItem> _allReturnValues = new ObservableCollection<CustomLASFilterDisplayItem>();
    public ObservableCollection<CustomLASFilterDisplayItem> AllReturnValues
    {
      get => _allReturnValues;
      set => SetProperty(ref _allReturnValues, value);
    }
    private CustomLASFilterDisplayItem _selectedReturnValues;
    public CustomLASFilterDisplayItem SelectedReturnValues
    {
      get => _selectedReturnValues;
      set => SetProperty(ref _selectedReturnValues, value);
    }

    private ObservableCollection<CustomLASFilterDisplayItem> _allClassificationFlags = new ObservableCollection<CustomLASFilterDisplayItem>();
    public ObservableCollection<CustomLASFilterDisplayItem> AllClassificationFlags
    {
      get => _allClassificationFlags;
      set => SetProperty(ref _allClassificationFlags, value);
    }
    private CustomLASFilterDisplayItem _selectedClassificationFlags;
    public CustomLASFilterDisplayItem SelectedClassificationFlags
    {
      get => _selectedClassificationFlags;
      set => SetProperty(ref _selectedClassificationFlags, value);
    }

    private ImageSource _displayFilterImage = Application.Current.Resources["DisplayManualTiePoints32"] as ImageSource;
    public ImageSource DisplayFilterImage
    {
      get => _displayFilterImage;
      set => SetProperty(ref _displayFilterImage, value);
    }
    private ImageSource _retrievePointsImage = Application.Current.Resources["LidarSession32"] as ImageSource;
    public ImageSource RetrievePointsImage
    {
      get => _retrievePointsImage;
      set => SetProperty(ref _retrievePointsImage, value);
    }
    #endregion
    #region Utility members
    private List<int> _ccInLayer;
    private List<LasReturnType> _rvInLayer;
    private List<string> _flags = new List<string>()
    {
      "Not Flagged", "Key Points", "Synthetic Points", "Withheld Points"
    };
    /// <summary>
    /// Called when the map member properties change, when a new LAS layer is selected. 
    /// </summary>
    /// <param name="lasLayer"></param>
    /// <remarks>This method reads the LAS layer, gathers the values for Classification Codes, return value and classification flags and updates the UI</remarks>
    /// <returns></returns>
    private async Task UpdateFilterSettingsAsync(LasDatasetLayer lasLayer)
    {
      if (lasLayer == null)
        return;
      DisplayLASLayerExistsMessage =  Visibility.Visible;
      DisplayNoLasLayerMessage =  Visibility.Hidden;

      await QueuedTask.Run(() =>
      {
        _ccInLayer = lasLayer.GetDisplayFilter().ClassCodes; //the filter values for class codes in the layer
        UniqueClassCodesInLayer.Clear();
 
        using  (var lasDataset = lasLayer.GetLasDataset())
        {
          foreach (var cc in lasDataset.GetUniqueClassCodes()) //Get the unique class codes in the layer
          {
            //When the _ccInLayer is empty, all the class codes are displayed - this is a little idiosyncrasy of the CIM.
            bool isCCDisplayed = _ccInLayer.Count == 0 ? true : _ccInLayer.Contains(cc); //Is the class code applied in the filter?
            UniqueClassCodesInLayer.Add(new CustomLASFilterDisplayItem(cc, isCCDisplayed));
          }

          AllReturnValues.Clear();
          _rvInLayer = lasLayer.GetDisplayFilter().Returns; //the filter values for return values in the layer
          foreach (var rv in lasDataset.GetUniqueReturns())
          {
            //When the _rvInLayer is empty, all the return values are displayed - this is a little idiosyncrasy of the CIM.
            bool isRVDisplayed = _rvInLayer.Count == 0 ? true : _rvInLayer.Contains(rv);
            AllReturnValues.Add(new CustomLASFilterDisplayItem(rv, isRVDisplayed));
          }
        }
        AllClassificationFlags.Clear();
        bool sytheticPoint = lasLayer.GetDisplayFilter().SyntheticPoints;
        bool keyPoint = lasLayer.GetDisplayFilter().KeyPoints;
        bool overlapPoint = lasLayer.GetDisplayFilter().OverlapPoints;
        bool withheldPoint = lasLayer.GetDisplayFilter().WithheldPoints;
        bool notFlagged = lasLayer.GetDisplayFilter().NotFlagged;
        foreach (var flagTpye in _flags)
        {
          bool isFlagChecked = false;
          if (flagTpye == "Not Flagged")
            isFlagChecked = notFlagged;
          if (flagTpye == "Key Points")
            isFlagChecked = keyPoint;         
          if (flagTpye == "Synthetic Points")
            isFlagChecked = sytheticPoint;
          if (flagTpye == "Withheld Points")
            isFlagChecked = withheldPoint;
          
          AllClassificationFlags.Add(new CustomLASFilterDisplayItem(flagTpye, isFlagChecked));
        }
      });
    }
    #endregion
    #region Commands

    public RelayCommand CmdApplyDisplayFilter
    {
      get
      {
        return new RelayCommand(() => ApplyDisplayFilter(), true);
      }
    }
    /// <summary>
    /// Applies the display filter to the selected LAS layer
    /// </summary>
    private void ApplyDisplayFilter()
    {
      QueuedTask.Run(() =>
      {        
        if (SelectedLASLayer != null)
        {
          LasPointDisplayFilter lasPointDisplayFilter = new LasPointDisplayFilter();
          lasPointDisplayFilter.Returns = GetLasReturnTypes();
          lasPointDisplayFilter.ClassCodes = GetLasClassCodes();
          lasPointDisplayFilter.KeyPoints = IsKeyPointChecked;
          lasPointDisplayFilter.SyntheticPoints = IsSyntheticPointsChecked;
          lasPointDisplayFilter.OverlapPoints = IsOverlapPointsChecked;
          lasPointDisplayFilter.WithheldPoints = IsWithheldPointsChecked;
          lasPointDisplayFilter.NotFlagged = IsNotFlaggedChecked;
          SelectedLASLayer.SetDisplayFilter(lasPointDisplayFilter);

        }
      });
    }

    public RelayCommand CmdRetrievePoints
    {
      get
      {
        return new RelayCommand(() => ActivateRetrievePointsTool(), true);
      }
    }

    private void ActivateRetrievePointsTool()
    {
      FrameworkApplication.SetCurrentToolAsync("LASDatasetAPISamples_RetrievePointsUsingFilterTool");
    }

    #endregion
    #region Event handlers
    /// <summary>
    /// Event Handler when the Map Member properties change
    /// </summary>
    /// <param name="args"></param>
    private void OnMapMemberPropertiesChanged(MapMemberPropertiesChangedEventArgs args)
    {
      var lasLayer = args.MapMembers.OfType<LasDatasetLayer>().FirstOrDefault();
      if (lasLayer == null)
        return;
      _ = UpdateFilterSettingsAsync(lasLayer);
    }

    private void OnLayersRemoved(LayerEventsArgs args)
    {
      var layersRemoved = args.Layers;
      foreach (var layerRemoved in layersRemoved)
      {
        var lasLayer = layerRemoved as LasDatasetLayer;
        if (lasLayer == null)
          continue;
        if (LASLayersInMap.Contains(lasLayer))
          LASLayersInMap.Remove(lasLayer);
      }

      if (LASLayersInMap.Count == 0)
      {
        DisplayNoLasLayerMessage = Visibility.Visible;
        DisplayLASLayerExistsMessage = Visibility.Hidden;
        return;
      }
      SelectedLASLayer = LASLayersInMap.FirstOrDefault();
    }

    private void OnLayersAdded(LayerEventsArgs args)
    {
      var layersAdded = args.Layers;
      foreach (var layerAdded in layersAdded)
      {
        var lasLayer = layerAdded as LasDatasetLayer;
        if (lasLayer == null)
          continue;
        if (!LASLayersInMap.Contains(lasLayer))
          LASLayersInMap.Add(lasLayer);
      }
      if (LASLayersInMap.Count > 0)
      {
        DisplayNoLasLayerMessage = Visibility.Hidden;
        DisplayLASLayerExistsMessage = Visibility.Visible;
      }
      SelectedLASLayer = LASLayersInMap.FirstOrDefault();
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
    {
      // Get the LAS layers in the map
      var map = args.IncomingView?.Map;

      var lasLayers = map?.GetLayersAsFlattenedList().OfType<LasDatasetLayer>();
      DisplayLASLayerExistsMessage = map == null || lasLayers.Count() == 0 ? Visibility.Hidden : Visibility.Visible;
      DisplayNoLasLayerMessage = DisplayLASLayerExistsMessage == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;

      if (map == null || lasLayers.Count() == 0)
        return;

      LASLayersInMap.Clear();

      foreach (var lasLayer in lasLayers)
      {
        LASLayersInMap.Add(lasLayer);
      }

      SelectedLASLayer = LASLayersInMap.FirstOrDefault();
    }

    #endregion
    /// <summary>
    /// Gets the class codes that are checked in the UI
    /// </summary>
    /// <returns></returns>
    public List<int> GetLasClassCodes()
    {
      var classCodes = new List<int>();
      foreach (var cc in UniqueClassCodesInLayer)
      {
        if (cc.IsChecked)
          classCodes.Add(cc.ClassCode);

      }
      return classCodes;
    }
    /// <summary>
    /// Gets the return types that are checked in the UI
    /// </summary>
    /// <returns></returns>
    public List<LasReturnType> GetLasReturnTypes()
    {
      var returns = new List<LasReturnType>();
      foreach (var item in AllReturnValues)
      {
        if (item.IsChecked)
          returns.Add(item.LasReturnType);
      }
      return returns;
    }

    private bool _isKeyPoints;
    /// <summary>
    /// Is the Key Points checked in the UI
    /// </summary>
    public bool IsKeyPointChecked
    {
      get
      {
        foreach (var item in AllClassificationFlags)
        {
          if (item.ItemType == CustomItemType.Flags && item.Name == "Key Points" && item.IsChecked)
          {
            return true;
          }
        }
        return false;
      }
      set => SetProperty(ref _isKeyPoints, value, () => IsKeyPointChecked);
    }
    private bool _isOverlapPoints;
    /// <summary>
    /// Is the Overlap Points checked in the UI
    /// </summary>
    public bool IsOverlapPointsChecked
    {
      get
      {
        foreach (var item in AllClassificationFlags)
        {
          if (item.ItemType == CustomItemType.Flags && item.Name == "Overlap Points" && item.IsChecked)
          {
            return true;
          }
        }
        return false;
      }
      set => SetProperty(ref _isOverlapPoints, value, () => IsOverlapPointsChecked);
    }

    private bool _isSyntheticPoints;
    /// <summary>
    /// Is the Synthetic Points checked in the UI
    /// </summary>
    public bool IsSyntheticPointsChecked
    {
      get
      {
        foreach (var item in AllClassificationFlags)
        {
          if (item.ItemType == CustomItemType.Flags && item.Name == "Synthetic Points" && item.IsChecked)
          {
            return true;
          }
        }
        return false;
      }
      set => SetProperty(ref _isSyntheticPoints, value, () => IsSyntheticPointsChecked);
    }

    private bool _isWithheldPoints;
    /// <summary>
    /// Is the Withheld Points checked in the UI
    /// </summary>
    public bool IsWithheldPointsChecked
    {
      get
      {
        foreach (var item in AllClassificationFlags)
        {
          if (item.ItemType == CustomItemType.Flags && item.Name == "Withheld Points" && item.IsChecked)
          {
            return true;
          }
        }
        return false;
      }
      set => SetProperty(ref _isWithheldPoints, value, () => IsWithheldPointsChecked);
    }

    private bool _isNotFlagged;
    public bool IsNotFlaggedChecked
    {
      get
      {
        foreach (var item in AllClassificationFlags)
        {
          if (item.ItemType == CustomItemType.Flags && item.Name == "Not Flagged" && item.IsChecked)
          {
            return true;
          }
        }
        return false;
      }
      set => SetProperty(ref _isNotFlagged, value, () => IsNotFlaggedChecked);
    }
  }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class LASDatasetDockpane_ShowButton : Button
    {
      protected override void OnClick()
      {
        LASDatasetDockpaneViewModel.Show();
      }
    }
 
}
