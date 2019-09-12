//   Copyright 2019 Esri
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
using ArcGIS.Desktop.Editing.Events;

namespace G2GEvents
{
  internal class Dockpane1ViewModel : DockPane
  {
    private const string _dockPaneID = "G2GEvents_Dockpane1";

    protected Dockpane1ViewModel() { AddEntry("Click 'Start Events' to start listening to ground to grid events"); }
    
    private ICommand _startStopCmd;
    private ICommand _clearCmd = null;
    private bool _listening = false;

    private CIMGroundToGridCorrection _current_CIMg2g;
    private CIMGroundToGridCorrection _copy_CIMg2g;

    private List<string> _entries = new List<string>();

    private static readonly object _lock = new object();
    private bool _firstStart = true;

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
    private string _heading = "Ground to Grid Events";

    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }

    private void AddEntry(string entry)
    {
      lock (_lock)
      {
        _entries.Add($"{entry}");
      }
      NotifyPropertyChanged(nameof(EventLog));
    }

    private void ClearEntries()
    {
      lock (_lock)
      {
        _entries.Clear();
      }
      NotifyPropertyChanged(nameof(EventLog));
    }

    public ICommand ClearTextCmd
    {
      get
      {
        if (_clearCmd == null)
          _clearCmd = new RelayCommand(() => ClearEntries());
        return _clearCmd;
      }
    }

    public string EventLog
    {
      get
      {
        string contents = "";
        lock (_lock)
        {
          contents = string.Join("\r\n", _entries.ToArray());
        }
        return contents;
      }
    }

    private void RecordEvent(string eventName, string entry)
    {
      var dateTime = DateTime.Now.ToString("G");
      lock (_lock)
      {
        _entries.Add($"{dateTime}: {eventName} {entry}");
      }
      NotifyPropertyChanged(nameof(EventLog));
    }

    public string ButtonText => _listening ? "Stop Events" : "Start Events";

    #region RegisterUnregister

    public ICommand StartStopCmd => _startStopCmd ?? (_startStopCmd = new RelayCommand(StartStop));

    private async Task StartStop()
    {
      if (_firstStart)
      {
        ClearEntries();
        _firstStart = false;
      }
      if (_listening)
      {
        _listening = Unregister();
        AddEntry("Not listening");
      }
      else
      {
        _listening = await Register();
        AddEntry("Listening");
      }
      NotifyPropertyChanged("ButtonText");
    }

    private async Task<bool> Register()
    {
      //Get the active map view.
      var mapView = MapView.Active;
      if (mapView?.Map == null)
        return false;

      _current_CIMg2g = await mapView.Map.GetGroundToGridCorrection();

      if (_current_CIMg2g != null)
        _copy_CIMg2g = _current_CIMg2g.CreateCopy(); //make a copy of the G2G right before listening for events
      else
      {
        _current_CIMg2g = new CIMGroundToGridCorrection();
        _copy_CIMg2g = new CIMGroundToGridCorrection();
      }

      GroundToGridEvent.Subscribe(GroundToGridEventMethod);
      return true;
    }

    private bool Unregister()
    {
      //Original comment - "events have to be unregistered on the same thread they were registered on...hence the use of the Queued Task"

      GroundToGridEvent.Unsubscribe(GroundToGridEventMethod);
      return false;
    }
    #endregion RegisterUnregister

    // cached properties
    //public bool G2GEnabled { get; private set; }
    public bool UseDirectionOffset { get; private set; }
    public bool UseScale { get; private set; }
    public double DirectionOffset { get; private set; }
    public GroundToGridScaleType ScaleType { get; private set; }
    public double ConstantScaleFactor { get; private set; }
    public double CombinedScaleFactorForSketch { get; private set; }


    private void GroundToGridEventMethod(GroundToGridEventArgs e)
    {

      bool changed;
      if (e.PropertyName != null)
        changed = ShowProperty(e.PropertyName);
      else
      {
        changed = ShowProperty(nameof(GroundToGridCorrection.Enabled));
        changed |= ShowProperty(nameof(GroundToGridCorrection.UseDirection));
        changed |= ShowProperty(nameof(GroundToGridCorrection.UseScale));
        changed |= ShowProperty(nameof(GroundToGridCorrection.Direction));
        changed |= ShowProperty(nameof(GroundToGridCorrection.ScaleType));
        changed |= ShowProperty(nameof(GroundToGridCorrection.ConstantScaleFactor));
      }
      if (changed)
        AddEntry("---------- Change detected. ----------");
      else
        AddEntry("---------- No change. ----------");
    }

    bool ShowProperty(string propertyName)
    {
      switch (propertyName)
      {
        case nameof(GroundToGridCorrection.CombinedScaleFactorForSketch):
          // do something because CombinedScaleFactorForSketch property changed
          double dCombinedScaleFactor = GroundToGridCorrection.CombinedScaleFactorForSketch;
          if (dCombinedScaleFactor == CombinedScaleFactorForSketch)
            return false;
          AddEntry("Sketch combined scale factor: " + Convert.ToString(dCombinedScaleFactor));
          return true;
        case nameof(GroundToGridCorrection.Enabled):
          // do something because Enabled property changed
          var newEnabled = GroundToGridCorrection.Enabled;
          if (newEnabled == Enabled)
            return false;
          AddEntry("Ground to Grid was previously turned " + (Enabled ? "ON" : "OFF"));
          Enabled = newEnabled;
          AddEntry("Ground to Grid is now turned " + (Enabled ? "ON" : "OFF"));
          AddEntry(".");
          return true;
        case nameof(GroundToGridCorrection.UseDirection):
          // do something because UseDirection property changed
          var newUseDirectionOffset = GroundToGridCorrection.UseDirection;
          if (newUseDirectionOffset == UseDirectionOffset)
            return false;
          AddEntry("Direction offset was previously turned " + (UseDirectionOffset ? "ON" : "OFF"));
          UseDirectionOffset = newUseDirectionOffset;
          AddEntry("Direction offset is now turned " + (UseDirectionOffset ? "ON" : "OFF"));
          AddEntry(".");
          return true;
        case nameof(GroundToGridCorrection.UseScale):
          // do something because UseScale property changed
          var newUseScale = GroundToGridCorrection.UseScale;
          if (newUseScale == UseScale)
            return false;
          AddEntry("Constant scale factor was previously turned " + (UseScale ? "ON" : "OFF"));
          UseScale = newUseScale;
          AddEntry("Constant scale factor is now turned " + (UseScale ? "ON" : "OFF"));
          AddEntry(".");
          return true;
        case nameof(GroundToGridCorrection.Direction):
          // do something because Direction property changed
          var newDirectionOffset = GroundToGridCorrection.Direction;
          if (newDirectionOffset == DirectionOffset)
            return false;
          var AngConv = ArcGIS.Core.SystemCore.DirectionUnitFormatConversion.Instance;
          var ConvDef = new ArcGIS.Core.SystemCore.ConversionDefinition()
          {
            DirectionTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar,
            DirectionUnitsIn = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees,
            DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.Polar,
            DirectionUnitsOut = ArcGIS.Core.SystemCore.DirectionUnits.DegreesMinutesSeconds //TODO: get this from the Project default angular units
          };
          AddEntry("Direction offset changed from : " + AngConv.ConvertToString(DirectionOffset, 0, ConvDef));
          DirectionOffset = newDirectionOffset;
          AddEntry("Direction offset changed to : " + AngConv.ConvertToString(DirectionOffset, 0, ConvDef));
          AddEntry(".");
          return true;
        case nameof(GroundToGridCorrection.ScaleType):
          var newScaleType = GroundToGridCorrection.ScaleType;
          if (newScaleType == ScaleType)
            return false;
          AddEntry("Scale was previously set to " + (ScaleType == GroundToGridScaleType.ConstantFactor ? "constant factor." : "be computed using elevation."));
          ScaleType = newScaleType;
          AddEntry("Scale is now set to " + (ScaleType == GroundToGridScaleType.ConstantFactor ? "constant factor." : "be computed using elevation."));
          return true;
        case nameof(GroundToGridCorrection.ConstantScaleFactor):
          // do something because ConstantScaleFactor property changed
          var newConstantScaleFactor = GroundToGridCorrection.ConstantScaleFactor;
          if (newConstantScaleFactor == ConstantScaleFactor)
            return false;
          AddEntry("Constant scale factor changed from : " + ConstantScaleFactor.ToString());
          ConstantScaleFactor = newConstantScaleFactor;
          AddEntry("Constant scale factor changed to : " + ConstantScaleFactor.ToString());
          AddEntry(".");
          return true;
        default:
          return false;
      }

    }

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
