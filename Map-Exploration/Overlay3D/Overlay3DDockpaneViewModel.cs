/*

   Copyright 2018 Esri

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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
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
using ArcGIS.Desktop.Mapping;


namespace Overlay3D
{
  internal class Overlay3DDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "Overlay3D_Overlay3DDockpane";
    private List<string> SystemStyle3Ds = new List<string> { "3D Vegetation - Realistic", "3D Vegetation - Thematic" };

    private readonly ObservableCollection<string> _treeNames = new ObservableCollection<string>();
    private string _selectedTreeName;
    private string _theSelectedTreeName;
    private int _theSelectedTreeCount;

    private ObservableCollection<CustomSymbolStyleItem> _symbolStyleItems = new ObservableCollection<CustomSymbolStyleItem>();
    private CustomSymbolStyleItem _selectedSymbolStyleItem;

    private ObservableCollection<string> _systemStyles;
    private string _selectedSystemStyle;

    private object _lock = new object();

    private double _progressValue = 1;
    private double _maxProgressValue = 100;

    private List<IDisposable> _flushList = new List<IDisposable>();

    protected Overlay3DDockpaneViewModel()
    {
      _selectedSystemStyle = SystemStyle3Ds[0];
      _systemStyles = new ObservableCollection<string>(SystemStyle3Ds);
      BindingOperations.EnableCollectionSynchronization(TreeNames, _lock);
      BindingOperations.EnableCollectionSynchronization(SystemStyles, _lock);
      BindingOperations.EnableCollectionSynchronization(SymbolStyleItems, _lock);
    }

    public ICommand CmdRefresh
    {
      get
      {
        return new RelayCommand(() =>
        {
          QueuedTask.Run(() =>
          {
            var queryFilter = new QueryFilter
            {
              // not working: PostfixClause = "GROUP BY Name ORDER BY Name",   
              // not working: SubFields = "Name, Count(*) as Cnt"
              SubFields = "Name"
            };
            lock (_lock) TreeNames.Clear();
            FeatureLayer feat = null;
            Dictionary<string, int> countByName = new Dictionary<string, int>();
            try
            {
              feat = MapView.Active.Map.Layers.First
                (layer => layer.Name.Equals("Trees")) as FeatureLayer;
              using (var rowCursor = feat.GetFeatureClass().Search(queryFilter))
              {
                var nameFieldIdx = rowCursor.FindField("Name");
                // not working: var countFieldIdx = rowCursor.FindField("Cnt");
                while (rowCursor.MoveNext())
                {
                  using (var row = rowCursor.Current)
                  {
                    var treeName = row[nameFieldIdx].ToString();
                    if (countByName.ContainsKey(treeName))
                    {
                      countByName[treeName]++;
                    }
                    else
                    {
                      countByName.Add(treeName, 1);
                    }
                    // not working: var treeName = $@"{row[nameFieldIdx].ToString()} ({row[countFieldIdx].ToString()})";
                    //lock (_lock)
                    //  TreeNames.Add(treeName);
                    EnableTreeNameSelection = true;
                  }
                }
                lock (_lock)
                {
                  foreach (var key in countByName.Keys)
                  {
                    TreeNames.Add($@"{key} ({countByName[key].ToString()})");
                  }
                }
              }
            }
            catch (Exception ex)
            {
              System.Diagnostics.Debug.WriteLine($@"CmdRefresh error: {ex.ToString()}");
            }
            if (feat == null)
            {
              MessageBox.Show(@"Wrong dataset use ""Interacting with Maps / Portland 3D map""");
            }
          });
        }, true);
      }
    }

    /// <summary>
    /// collection of tree names.  tree name is used to find matching 3 D- symbology.
    /// which in turn is used for Overlay graphic rendering
    /// </summary>
    public ObservableCollection<string> TreeNames
    {
      get { return _treeNames; }
    }

    /// <summary>
    /// Holds the selected tree name
    /// </summary>
    public string SelectedTreeName
    {
      get { return _selectedTreeName; }
      set
      {
        SetProperty(ref _selectedTreeName, value, () => SelectedTreeName);
        EnableSymbolSelection = !string.IsNullOrEmpty(_selectedTreeName);
        _theSelectedTreeCount = 0;
        _theSelectedTreeName = string.Empty;
        if (EnableSymbolSelection)
        {
          var idx = _selectedTreeName.IndexOf('(');
          _theSelectedTreeName = _selectedTreeName.Substring(0, idx - 1);
          var endIdx = _selectedTreeName.IndexOf(')', idx);
          int.TryParse(_selectedTreeName.Substring(idx + 1, endIdx - idx - 1), out _theSelectedTreeCount);
        }
        RefreshSymbolStyleItems(_selectedSystemStyle, _theSelectedTreeName);
      }
    }

    private bool _enableSymbolSelection = false;
    public bool EnableSymbolSelection
    {
      get { return _enableSymbolSelection; }
      set
      {
        SetProperty(ref _enableSymbolSelection, value, () => EnableSymbolSelection);
      }
    }

    private bool _enableTreeNameSelection = false;
    public bool EnableTreeNameSelection
    {
      get { return _enableTreeNameSelection; }
      set
      {
        SetProperty(ref _enableTreeNameSelection, value, () => EnableTreeNameSelection);
      }
    }

    private bool _enableEnableUpdateStatus = false;
    public bool EnableEnableUpdateStatus
    {
      get { return _enableEnableUpdateStatus; }
      set
      {
        SetProperty(ref _enableEnableUpdateStatus, value, () => EnableTreeNameSelection);
      }
    }


    /// <summary>
    /// collection of SystemStyles.  SystemStyle is used to find matching 3 D- symbology.
    /// which in turn is used for Overlay graphic rendering
    /// </summary>
    public ObservableCollection<string> SystemStyles
    {
      get { return _systemStyles; }
    }

    /// <summary>
    /// Holds the selected SystemStyles
    /// </summary>
    public string SelectedSystemStyle
    {
      get { return _selectedSystemStyle; }
      set
      {
        SetProperty(ref _selectedSystemStyle, value, () => SelectedSystemStyle);
        RefreshSymbolStyleItems(_selectedSystemStyle, _theSelectedTreeName);
      }
    }

    private void RefreshSymbolStyleItems(string selectedSystemStyle, string selectedTreeName)
    {
      lock (_lock) SymbolStyleItems.Clear();
      if (string.IsNullOrEmpty(_selectedSystemStyle)
        || string.IsNullOrEmpty(_theSelectedTreeName))
        return;
      try
      {
        QueuedTask.Run(() =>
        {
          GetPointSymbols(selectedSystemStyle, selectedTreeName);
          //if (symbols.Count > 0) UpdateStyles(symbols);
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error refreshing Symbol Style Items: {ex.ToString()}");
      }
    }

    /// <summary>
    /// collection of symbol style items for a given tree name
    /// </summary>
    public ObservableCollection<CustomSymbolStyleItem> SymbolStyleItems
    {
      get { return _symbolStyleItems; }
    }

    /// <summary>
    /// Holds the selected symbol style
    /// </summary>
    public CustomSymbolStyleItem SelectedSymbolStyleItem
    {
      get { return _selectedSymbolStyleItem; }
      set
      {
        SetProperty(ref _selectedSymbolStyleItem, value, () => SelectedSymbolStyleItem);
        if (_selectedSymbolStyleItem == null) return;
        if (string.IsNullOrEmpty(_theSelectedTreeName)) return;
        QueuedTask.Run(async () =>
        {
          var symbol = GetPointSymbol(_selectedSystemStyle, _selectedSymbolStyleItem.SymbolName);
          var lyr = MapView.Active.Map.Layers.First(layer => layer.Name.Equals("Trees")) as FeatureLayer;
          IDictionary<long, Geometry> geoms = new Dictionary<long, Geometry>();

          string updateText = $@"Adding Symbols for {_theSelectedTreeCount} '{_theSelectedTreeName}' trees to the MapView using Overlay Graphics";
          ProgressUpdate(updateText, 1, _theSelectedTreeCount);
          if (lyr != null)
          {
            QueryFilter qf = new QueryFilter()
            {
              SubFields = "objectid, name, Shape",
              WhereClause = $@"name = '{_theSelectedTreeName}'"
            };
            using (var rowCursor = lyr.GetFeatureClass().Search(qf))
            {
              while (rowCursor.MoveNext())
              {
                using (var row = rowCursor.Current as Feature)
                {
                  var pnt = row.GetShape().Clone() as MapPoint;
                  var projBottomPnt = GeometryEngine.Instance.Project(pnt, MapView.Active.Map.SpatialReference) as MapPoint;
                  var surfaceZ = await MapView.Active.Map.GetZsFromSurfaceAsync(projBottomPnt);
                  var z = (surfaceZ.Geometry as MapPoint).Z;
                  projBottomPnt = MapPointBuilderEx.CreateMapPoint(projBottomPnt.X, projBottomPnt.Y, z, projBottomPnt.SpatialReference);
                  var projTopPnt = MapPointBuilderEx.CreateMapPoint(projBottomPnt.X, projBottomPnt.Y, projBottomPnt.Z + 50, projBottomPnt.SpatialReference);
                  IList<MapPoint> pnts = new List<MapPoint>() { projBottomPnt, projTopPnt };
                  geoms.Add (row.GetObjectID(), PolylineBuilderEx.CreatePolyline (pnts, AttributeFlags.None));
                }
              }
            }
          }
          if (symbol != null)
          {
            var iCnt = 0;
            foreach (var oid in geoms.Keys)
            {
              ProgressUpdate(updateText, ++iCnt, _theSelectedTreeCount);
              // add the 3D geometry to the graphic overlay using layer and object id
              // _flushList.Add(MappingExtensions.AddOverlay(MapView.Active, lyr, oid, symbol.Symbol.MakeSymbolReference()));

              // add the 3D geometry but use the elevated point as the geometry
              var theLine = geoms[oid] as Polyline;
              var theBottom = theLine.Points[0];
              _flushList.Add(MappingExtensions.AddOverlay(MapView.Active, theBottom, symbol.Symbol.MakeSymbolReference()));

              // add the text to the graphic overlay
              //define the text symbol
              var textSymbol = new CIMTextSymbol();
              var cimLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.RedRGB, 2, SimpleLineStyle.Solid);

              //define the text graphic
              var textGraphic = new CIMTextGraphic();
              //Create a simple text symbol
              textSymbol = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.BlackRGB, 12, "Corbel", "Regular");
              //Sets the geometry of the text graphic
              textGraphic.Shape = geoms[oid];
              //Sets the text string to use in the text graphic
              textGraphic.Text = "This is my line";
              _flushList.Add(MappingExtensions.AddOverlay(MapView.Active, textGraphic));
              
              //Sets symbol to use to draw a line going straight up
              _flushList.Add(MappingExtensions.AddOverlay(MapView.Active, geoms[oid],  cimLineSymbol.MakeSymbolReference()));
            }
          }
        });
      }
    }

    #region UpdateText / progress

    /// <summary>
    /// Gets the value to set on the progress
    /// </summary>
    public double ProgressValue
    {
      get
      {
        return _progressValue;
      }
      set
      {
        SetProperty(ref _progressValue, value, () => ProgressValue);
      }
    }

    /// <summary>
    /// Gets the max value to set on the progress
    /// </summary>
    public double MaxProgressValue
    {
      get
      {
        return _maxProgressValue;
      }
      set
      {
        SetProperty(ref _maxProgressValue, value, () => MaxProgressValue);
      }
    }

    private string _UpdateStatus;
    /// <summary>
    /// UpdateStatus Text
    /// </summary>
    public string UpdateStatus
    {
      get
      {
        return _UpdateStatus;
      }
      set
      {
        SetProperty(ref _UpdateStatus, value, () => UpdateStatus);
      }
    }

    private string _ProgressText;
    /// <summary>
    /// Progress bar Text
    /// </summary>
    public string ProgressText
    {
      get
      {
        return _ProgressText;
      }
      set
      {
        SetProperty(ref _ProgressText, value, () => ProgressText);
      }
    }

    private string _previousText = string.Empty;
    private int _iProgressValue = -1;
    private int _iProgressMax = -1;

    private void ProgressUpdate(string sText, int iProgressValue, int iProgressMax)
    {
      if (System.Windows.Application.Current.Dispatcher.CheckAccess())
      {
        if (_iProgressMax != iProgressMax) MaxProgressValue = iProgressMax;
        else if (_iProgressValue != iProgressValue)
        {
          ProgressValue = iProgressValue;
          ProgressText = (iProgressValue == iProgressMax) ? "Done" : $@"{(iProgressValue * 100 / iProgressMax):0}%";
        }
        if (sText != _previousText) UpdateStatus = sText;
        _previousText = sText;
        _iProgressValue = iProgressValue;
        _iProgressMax = iProgressMax;
      }
      else
      {
        ProApp.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
          (Action)(() =>
          {
            if (_iProgressMax != iProgressMax) MaxProgressValue = iProgressMax;
            else if (_iProgressValue != iProgressValue)
            {
              ProgressValue = iProgressValue;
              ProgressText = (iProgressValue == iProgressMax) ? "Done" : $@"{(iProgressValue * 100 / iProgressMax):0}%";
            }
            if (sText != _previousText) UpdateStatus = sText;
            _previousText = sText;
            _iProgressValue = iProgressValue;
            _iProgressMax = iProgressMax;
          }));
      }
    }

    /// <summary>
    /// Clears any added graphics
    /// </summary>
    public ICommand CmdFlush
    {
      get
      {
        return new RelayCommand(() =>
        {
          //QueuedTask.Run(() =>
          //{
          foreach (var disp in _flushList)
          {
            disp?.Dispose();
          }

          _flushList.Clear();
          //});
        }, true);
      }
    }

    #endregion UpdateText / Progress

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
    /// Call from the MCT
    /// </summary>
    /// <param name="systemStyle"></param>
    /// <param name="symbolStyleName"></param>
    /// <returns></returns>
    private void GetPointSymbols(string systemStyle, string symbolStyleName)
    {
      // check if we need to load the 3D Vegetation - Realistic symbols
      var style3DProjectItem = Project.Current.GetItems<StyleProjectItem>().Where(p => p.Name == systemStyle).FirstOrDefault();
      if (style3DProjectItem == null)
      {
        Project.Current.AddStyle(systemStyle);
        style3DProjectItem = Project.Current.GetItems<StyleProjectItem>().Where(p => p.Name == systemStyle).FirstOrDefault();
      }
      if (style3DProjectItem == null)
      {
        throw new Exception($@"Unable to load this style: {systemStyle}");
      }
      var symbols = style3DProjectItem.SearchSymbols(StyleItemType.PointSymbol, "");
      foreach (var symbol in symbols)
      {
        lock (_lock)
        {
          _symbolStyleItems.Add(new CustomSymbolStyleItem(symbol, symbol.Key));
        }
      }
    }

    /// <summary>
    /// Call from the MCT
    /// </summary>
    /// <param name="systemStyle3D">3D system style category name</param>
    /// <param name="symbolStyleName"></param>
    /// <returns></returns>
    private static SymbolStyleItem GetPointSymbol(string systemStyle3D, string symbolStyleName)
    {
      // check if we need to load the 3D Vegetation - Realistic symbols
      var style3DProjectItem = Project.Current.GetItems<StyleProjectItem>().Where(p => p.Name == systemStyle3D).FirstOrDefault();
      if (style3DProjectItem == null)
      {
        Project.Current.AddStyle(systemStyle3D);
        style3DProjectItem = Project.Current.GetItems<StyleProjectItem>().Where(p => p.Name == systemStyle3D).FirstOrDefault();
      }
      if (style3DProjectItem == null) return null;
      return style3DProjectItem.SearchSymbols(StyleItemType.PointSymbol, symbolStyleName).FirstOrDefault();
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _txtHeading = "Map with Tree 3D layer";
    public string TxtHeading
    {
      get { return _txtHeading; }
      set
      {
        SetProperty(ref _txtHeading, value, () => TxtHeading);
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class Overlay3DDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      Overlay3DDockpaneViewModel.Show();
    }
  }
}
