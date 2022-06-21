//Copyright 2020 Esri

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
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.DeviceLocation;
using ArcGIS.Desktop.Core.DeviceLocation.Events;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.DeviceLocation;


namespace DeviceTracker
{
  internal class SourceProperties : ViewModelBase
  {
    internal void Clear()
    {
      AntennaHeight = "";
      BaudRate = "";
      Port = "";
      StopBits = "";
      DataBits = "";
      Parity = "";
    }

    internal void Populate(string port, string baudRate, string antennaHeight, string dataBits, string parity, string stopBits)
    {
      Port = port;
      BaudRate = baudRate;
      AntennaHeight = antennaHeight;
      DataBits = dataBits;
      Parity = parity;
      StopBits = stopBits;
    }

    private string _Port;
    public string Port
    {
      get => _Port;
      set => SetProperty(ref _Port, value);
    }

    private string _BaudRate;
    public string BaudRate
    {
      get => _BaudRate;
      set => SetProperty(ref _BaudRate, value);
    }

    private string _AntennaHeight;
    public string AntennaHeight
    {
      get => _AntennaHeight;
      set => SetProperty(ref _AntennaHeight, value);
    }

    private string _DataBits;
    public string DataBits
    {
      get => _DataBits;
      set => SetProperty(ref _DataBits, value);
    }

    private string _Parity;
    public string Parity
    {
      get => _Parity;
      set => SetProperty(ref _Parity, value);
    }

    private string _StopBits;
    public string StopBits
    {
      get => _StopBits;
      set => SetProperty(ref _StopBits, value);
    }
  }

  internal class GNSSPropertiesViewModel : DockPane
  {
    private const string _dockPaneID = "DeviceTracker_GNSSProperties";

    protected GNSSPropertiesViewModel()
    {
      CurrentTabIndex = 0;

      _gnssItems = new List<TabControl>();
      _gnssItems.Add(new TabControl() { Text = "GNSS Source" });
      _gnssItems.Add(new TabControl() { Text = "GNSS Location" });

      CurrentSource = new SourceProperties();
      InputSource = new SourceProperties();

      GetDeviceSourceData();
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

    private List<TabControl> _gnssItems;
    public List<TabControl> GNSSItems => _gnssItems;

    private int _CurrentTabIndex;
    public int CurrentTabIndex
    {
      get => _CurrentTabIndex;
      set
      {
        SetProperty(ref _CurrentTabIndex, value);
        NotifyPropertyChanged(nameof(IsGNSSSourceTab));
        NotifyPropertyChanged(nameof(IsGNSSLocationTab));
      }
    }
    public bool IsGNSSSourceTab => (CurrentTabIndex == 0);
    public bool IsGNSSLocationTab => (CurrentTabIndex == 1);


    #region Location Source

    public SourceProperties CurrentSource { get; set; }
    public SourceProperties InputSource { get; set; }

    private string _SourceAccuracy;
    public string SourceAccuracy
    {
      get => _SourceAccuracy;
      set => SetProperty(ref _SourceAccuracy, value);
    }

    private string _InputSourceAccuracy;
    public string InputSourceAccuracy
    {
      get => _InputSourceAccuracy;
      set => SetProperty(ref _InputSourceAccuracy, value);
    }

    private void ClearCurrentSourceDetails()
    {
      CurrentSource.Clear();
      SourceAccuracy = "";
    }
    private void GetDeviceSourceData()
    {
      var service = DeviceLocationService.Instance;
      if (service == null)
        return;

      var src = service.GetSource();
      if (src == null)
      {
        ClearCurrentSourceDetails();
        return;
      }

      var dlProps = service.GetProperties();
      SourceAccuracy = dlProps.AccuracyThreshold.ToString();


      if (src is SerialPortDeviceLocationSource portSrc)
      {
        CurrentSource.Populate(portSrc.ComPort,
                portSrc.BaudRate.ToString(),
                portSrc.AntennaHeight.ToString(),
                portSrc.DataBits.ToString(),
                portSrc.Parity.ToString(),
                portSrc.StopBits.ToString()
                );
      }
    }

    private RelayCommand _CloseSourceCmd;
    public ICommand CloseSourceCmd => _CloseSourceCmd ?? (_CloseSourceCmd = new RelayCommand(() => CloseSource(), DoesSourceExist));

    private bool DoesSourceExist
    {
      get
      {
        var service = DeviceLocationService.Instance;

        var src = service.GetSource();
        if (src == null)
          return false;

        return true;

        //isConnected = service.IsDeviceConnected();
      }
    }

    private async void CloseSource()
    {
      var service = DeviceLocationService.Instance;

      var src = service.GetSource();
      if (src == null)
        return;

      bool isConnected = service.IsDeviceConnected();

      await QueuedTask.Run(() =>
      {
        service.Close();
      });

      isConnected = service.IsDeviceConnected();

      // test it succeeded?
      src = service.GetSource();
      if (src == null)
        ClearCurrentSourceDetails();
    }

    private RelayCommand _OpenSourceCmd;
    public ICommand OpenSourceCmd => _OpenSourceCmd ?? (_OpenSourceCmd = new RelayCommand(() => OpenSource(), true));

    private async void OpenSource()
    {
      if (string.IsNullOrEmpty(InputSource.Port))
      {
        MessageBox.Show("Please enter source details. A ComPort is required.");
        return;
      }
      var service = DeviceLocationService.Instance;

      int iValue;
      double dValue;

      var newSrc = new SerialPortDeviceLocationSource();
      newSrc.ComPort = InputSource.Port;

      if (int.TryParse(InputSource.BaudRate, out iValue))
        newSrc.BaudRate = iValue;

      if (double.TryParse(InputSource.AntennaHeight, out dValue))
        newSrc.AntennaHeight = dValue;

      //newSrc.DataBits = 6; //  8;
      //newSrc.Parity = System.IO.Ports.Parity.Odd;   // none
      //newSrc.StopBits = System.IO.Ports.StopBits.OnePointFive; // .One;
      // etc

      // include source accuracy 
      DeviceLocationProperties props = null;
      if (double.TryParse(InputSourceAccuracy, out dValue))
      {
        props = new DeviceLocationProperties();
        props.AccuracyThreshold = dValue;
      }
      var src = service.GetSource();

      try
      {
        await QueuedTask.Run(() =>
        {
          // close any existing source
          if (src != null)
            service.Close();

          // open the new one
          service.Open(newSrc, props);

          GetDeviceSourceData();
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private RelayCommand _UpdateSourceCmd;
    public ICommand UpdateSourceCmd => _UpdateSourceCmd ?? (_UpdateSourceCmd = new RelayCommand(() => UpdateSource()));    // DoesSourceExist

    private async Task UpdateSource()
    {
      var service = DeviceLocationService.Instance;
      var src = service.GetSource();
      if (src == null)
        return;

      await QueuedTask.Run(() =>
      {
        int iValue;
        double dValue;
        if (src is SerialPortDeviceLocationSource portSrc)
        {
          if (int.TryParse(InputSource.BaudRate, out iValue))
            portSrc.BaudRate = iValue;

          if (double.TryParse(InputSource.AntennaHeight, out dValue))
            portSrc.AntennaHeight = dValue;

          //newSrc.DataBits = 6; //  8;
          //newSrc.Parity = System.IO.Ports.Parity.Odd;   // none
          //newSrc.StopBits = System.IO.Ports.StopBits.OnePointFive; // .One;

          service.UpdateSource(portSrc);
        }


        // update properties
        if (double.TryParse(InputSourceAccuracy, out dValue))
        {
          var dlProps = service.GetProperties();
          dlProps.AccuracyThreshold = dValue;

          service.UpdateProperties(dlProps);
        }
      });

      // refresh data on the dockpane
      GetDeviceSourceData();
    }

    private RelayCommand _GetSourcePropertiesCmd;
    public ICommand GetSourcePropertiesCmd => _GetSourcePropertiesCmd ?? (_GetSourcePropertiesCmd = new RelayCommand(() => GetSourceProperties(), DoesSourceExist));

    private void GetSourceProperties()
    {
      // refresh data on the dockpane
      GetDeviceSourceData();
    }

    private RelayCommand _EnableSourceCmd;
    public ICommand EnableSourceCmd => _EnableSourceCmd ?? (_EnableSourceCmd = new RelayCommand(() => EnableSource(), DoesSourceExist));

    private async void EnableSource()
    {
        var msg = await QueuedTask.Run(() =>
        {
          string message = null;
          try
          {
            bool enabled = MapDeviceLocationService.Instance.IsDeviceLocationEnabled;
            MapDeviceLocationService.Instance.SetDeviceLocationEnabled(!enabled);
          }
          catch (InvalidOperationException e)
          {
            message = e.Message;
          }
          return message;
        });
      if (msg != null) MessageBox.Show(msg);
    }


    private RelayCommand _ConnectSourceEventsCmd;
    public ICommand ConnectSourceEventsCmd => _ConnectSourceEventsCmd ?? (_ConnectSourceEventsCmd = new RelayCommand(() => ConnectSourceEvents()));

    private void ConnectSourceEvents()
    {
      if (_tokenDeviceLocationPropertiesUpdated == null)
        _tokenDeviceLocationPropertiesUpdated = DeviceLocationPropertiesUpdatedEvent.Subscribe(OnDeviceLocationPropertiesUpdated);
      if (_tokenDeviceLocationSourceChanged == null)
        _tokenDeviceLocationSourceChanged = DeviceLocationSourceChangedEvent.Subscribe(OnChangedDeviceLocationSource);
    }

    private RelayCommand _DisconnectSourceEventsCmd;
    public ICommand DisconnectSourceEventsCmd => _DisconnectSourceEventsCmd ?? (_DisconnectSourceEventsCmd = new RelayCommand(() => DisconnectSourceEvents()));

    private void DisconnectSourceEvents()
    {
      if (_tokenDeviceLocationPropertiesUpdated != null)
        DeviceLocationPropertiesUpdatedEvent.Unsubscribe(_tokenDeviceLocationPropertiesUpdated);
      if (_tokenDeviceLocationSourceChanged != null)
        DeviceLocationSourceChangedEvent.Unsubscribe(_tokenDeviceLocationSourceChanged);

      _tokenDeviceLocationPropertiesUpdated = null;
      _tokenDeviceLocationSourceChanged = null;
    }

    private SubscriptionToken _tokenDeviceLocationPropertiesUpdated;
    private SubscriptionToken _tokenDeviceLocationSourceChanged;

    internal void OnDeviceLocationPropertiesUpdated(DeviceLocationPropertiesUpdatedEventArgs args)
    {
      var properties = args.DeviceLocationProperties;
      var threshold = properties.AccuracyThreshold;

      RecordEvent("OnDeviceLocationUpdated", threshold.ToString());
      //RecordEvent("OnDeviceLocationUpdated", "");
    }

    internal void OnChangedDeviceLocationSource(DeviceLocationSourceChangedEventArgs args)
    {
      var src = args.DeviceLocationSource;
      string comPort = "";
      if (src is SerialPortDeviceLocationSource portSrc)
        comPort = portSrc.ComPort;

      RecordEvent("OnChangedDeviceLocationSource", comPort);
    }

    private RelayCommand _ClearLogCmd;
    public ICommand ClearLogCmd => _ClearLogCmd ?? (_ClearLogCmd = new RelayCommand(() => ClearLog()));

    private void ClearLog()
      => ClearEntries();

    #endregion

    private RelayCommand _ClearLocLogCmd;
    public ICommand ClearLocLogCmd => _ClearLocLogCmd ?? (_ClearLocLogCmd = new RelayCommand(() => ClearLocLog()));

    private void ClearLocLog()
      => ClearLocEntries();

    #region eventLog

    private static readonly object _lock = new object();
    private List<string> _entries = new List<string>();

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

    private void ClearEntries()
    {
      lock (_lock)
      {
        _entries.Clear();
      }
      NotifyPropertyChanged(nameof(EventLog));
    }

    private void AddEntry(string entry)
    {
      lock (_lock)
      {
        _entries.Add($"{entry}");
      }
      NotifyPropertyChanged(nameof(EventLog));
    }

    private void RecordEvent(string eventName, string entry)
    {
      var dateTime = DateTime.Now.ToString("G");
      AddEntry($"{dateTime}: {eventName} {entry}");
    }


    #endregion

    #region Locations

    private bool _LocationOptions_ShowLocation;
    public bool LocationOptions_ShowLocation
    {
      get => _LocationOptions_ShowLocation;
      set => SetProperty(ref _LocationOptions_ShowLocation, value);
    }

    private bool _LocationOptions_KeepAtCenter;
    public bool LocationOptions_KeepAtCenter
    {
      get => _LocationOptions_KeepAtCenter;
      set => SetProperty(ref _LocationOptions_KeepAtCenter, value);
    }

    private bool _LocationOptions_TrackUp;
    public bool LocationOptions_TrackUp
    {
      get => _LocationOptions_TrackUp;
      set => SetProperty(ref _LocationOptions_TrackUp, value);
    }

    private RelayCommand _UpdateMapLocationOptionsCmd;
    public ICommand UpdateMapLocationOptionsCmd => _UpdateMapLocationOptionsCmd ?? (_UpdateMapLocationOptionsCmd = new RelayCommand(() => UpdateMapLocationOptions()));

    private async void UpdateMapLocationOptions()
    {
      var mapService = MapDeviceLocationService.Instance;

      var options = mapService.GetDeviceLocationOptions();
      options.DeviceLocationVisibility = LocationOptions_ShowLocation;
      options.NavigationMode = LocationOptions_KeepAtCenter ? MappingDeviceLocationNavigationMode.KeepAtCenter : MappingDeviceLocationNavigationMode.None;
      options.TrackUpNavigation = LocationOptions_TrackUp;

      var error = await QueuedTask.Run(() =>
      {
        try
        {
          mapService.SetDeviceLocationOptions(options);
        }
        catch (InvalidOperationException e)
        {
          return e.Message;
        }

        return "";
      });
      if (!string.IsNullOrEmpty(error))
      {
        MessageBox.Show(error);
      }
    }

    private RelayCommand _ZoomPanToLocationCmd;
    public ICommand ZoomPanToLocationCmd => _ZoomPanToLocationCmd ?? (_ZoomPanToLocationCmd = new RelayCommand(() => ZoomPanToLocation()));

    private async void ZoomPanToLocation()
    {
      var mapService = MapDeviceLocationService.Instance;

      var error = await QueuedTask.Run(() =>
      {
        try
        {
          mapService.ZoomOrPanToCurrentLocation(true);
        }
        catch (InvalidOperationException e)
        {
          return e.Message;
        }

        return "";
      });
      if (!string.IsNullOrEmpty(error))
        MessageBox.Show(error);
    }

    private RelayCommand _AddLastLocationCmd;
    public ICommand AddLastLocationCmd => _AddLastLocationCmd ?? (_AddLastLocationCmd = new RelayCommand(() => AddLastLocation()));

    private async Task AddLastLocation()
    {
      // add the last known device location to the graphics layer
      var map = MapView.Active?.Map;
      if (map == null)
      {
        MessageBox.Show("Must have an active map.");
        return;
      }

      var graphicsLayer = map.GetLayersAsFlattenedList().OfType<GraphicsLayer>().FirstOrDefault();
      if (graphicsLayer == null)
      {
        MessageBox.Show("Please add a graphics layer to your map.");
        return;
      }
      // ensure there is a device
      var service = DeviceLocationService.Instance;
      var source = service.GetSource();
      if (source == null)
      {
        MessageBox.Show("No device source.  Connect to a device.");
        return;
      }

      // dont await
      bool validPoint = await QueuedTask.Run(() =>
      {
        // get the last location
        var pt = service.GetCurrentSnapshot()?.GetPositionAsMapPoint();
        if ((pt == null) || (pt.IsEmpty))
          return false;

        // create symbol
        var ptSymbol = SymbolFactory.Instance.ConstructPointSymbol(CIMColor.CreateRGBColor(125, 125, 0), 10, SimpleMarkerStyle.Triangle);
        graphicsLayer.AddElement(pt, ptSymbol);
        graphicsLayer.ClearSelection();
        return true;
      });

      if (!validPoint)
        MessageBox.Show("no valid point was retrieved");
    }


    private RelayCommand _GetLocationsCmd;
    public ICommand GetLocationsCmd => _GetLocationsCmd ?? (_GetLocationsCmd = new RelayCommand(() => GetLocations()));

    private void GetLocations()
    {
      DeviceUtils.ClearDeviceLocations();
      SnapshotChangedEvent.Subscribe(OnSnapshotChanged);
    }

    private RelayCommand _DisconnectLocationsCmd;
    public ICommand DisconnectLocationsCmd => _DisconnectLocationsCmd ?? (_DisconnectLocationsCmd = new RelayCommand(() => DisconnectLocationEvents()));

    private void DisconnectLocationEvents()
    {
      SnapshotChangedEvent.Unsubscribe(OnSnapshotChanged);
    }

    private void OnSnapshotChanged(SnapshotChangedEventArgs args)
    {
      if (args == null)
        return;

      var snapshot = args.Snapshot as NMEASnapshot;
      if (snapshot == null)
        return;

      QueuedTask.Run(() =>
      {
        var pt = snapshot.GetPositionAsMapPoint();
        if (pt.IsEmpty || pt == null)
          return;

        string location = "X:" + pt.X + "; Y: " + pt.Y + ";Z " + pt.Z;
        RecordLocEvent("OnSnapshotChanged", location);
        DeviceUtils.AddDeviceLocation(pt);
      });
    }
    private RelayCommand _AddPolylineCmd;
    public ICommand AddPolylineCmd => _AddPolylineCmd ?? (_AddPolylineCmd = new RelayCommand(() => AddPolyline()));

    private bool HasLocations
    {
      get
      {
        var enumPoints = DeviceUtils.DeviceLocations;
        return enumPoints.Count > 0;
      }
    }
  
    private void AddPolyline()
    {
      var map = MapView.Active?.Map;
      if (map == null)
      {
        MessageBox.Show("Must have an active map.");
        return;
      }
      var graphicsLayer = map.GetLayersAsFlattenedList().OfType<GraphicsLayer>().FirstOrDefault();
      if (graphicsLayer == null)
      {
        MessageBox.Show("Please add a graphics layer to your map.");
        return;
      }
      // ensure there is a device
      var service = DeviceLocationService.Instance;
      var source = service.GetSource();
      if (source == null)
      {
        MessageBox.Show("No device source.  Connect to a device.");
        return;
      }

      // get the most recent locations ... up to a maximum of 10 
      var enumPoints = DeviceUtils.DeviceLocations;
      if (enumPoints.Count == 0)
      {
        MessageBox.Show("No locations retrieved.");
        return;
      }

      // build a polyline
      var polyline = PolylineBuilderEx.CreatePolyline(enumPoints, AttributeFlags.None);
      if (polyline.IsEmpty)
      {
        MessageBox.Show("No locations retrieved.");
        return;
      }

      // add to graphics layer

      // dont await
      QueuedTask.Run(() =>
      {
        // create symbology - red dash-dot line
        var lineSymbol = SymbolFactory.Instance.ConstructLineSymbol(CIMColor.CreateRGBColor(255, 0, 0), 3, SimpleLineStyle.DashDot);
        graphicsLayer.AddElement(polyline, lineSymbol);
        graphicsLayer.ClearSelection();
      });
    }
    #endregion

    #region Location eventLog

    private static readonly object _locLock = new object();
    private List<string> _locEntries = new List<string>();

    public string LocEventLog
    {
      get
      {
        string contents = "";
        lock (_locLock)
        {
          contents = string.Join("\r\n", _locEntries.ToArray());
        }
        return contents;
      }
    }

    private void ClearLocEntries()
    {
      lock (_locLock)
      {
        _locEntries.Clear();
      }
      NotifyPropertyChanged(nameof(LocEventLog));
    }

    private void AddLocEntry(string entry)
    {
      lock (_locLock)
      {
        _locEntries.Add($"{entry}");
      }
      NotifyPropertyChanged(nameof(LocEventLog));
    }

    private void RecordLocEvent(string eventName, string entry)
    {
      var dateTime = DateTime.Now.ToString("G");
      AddLocEntry($"{dateTime}: {eventName} {entry}");
    }

    #endregion
  }


  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class GNSSProperties_ShowButton : Button
  {
    protected override void OnClick()
    {
      GNSSPropertiesViewModel.Show();
    }
  }
}
