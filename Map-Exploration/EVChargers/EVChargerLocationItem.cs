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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EVChargers
{
  /// <summary>
  /// Represents each charger location
  /// </summary>
  internal class EVChargerLocationItem : PropertyChangedBase
  {
    private string _stationName;
    public string StationName
    {
      get => _stationName;
      set => SetProperty(ref _stationName, value);
    }

    private string _address;
    public string Address
    {
      get => _address;
      set => SetProperty(ref _address, value);
    }

    private string _city;
    public string City
    {
      get => _city;
      set => SetProperty(ref _city, value);
    }

    private string _state;
    public string State
    {
      get => _state;
      set => SetProperty(ref _state, value);
    }

    private string _zip;
    public string Zip
    {
      get => _zip; set => SetProperty(ref _zip, value);
    }

    private long _oid;
    public long OID
    {
      get => _oid; set => SetProperty(ref _oid, value);
    }
    private long _countOfLevel1;
    public long CountOfLevel1
    {
      get => _countOfLevel1;
      set => SetProperty(ref _countOfLevel1, value);
    }

    private long _countOfLevel2;
    public long CountOfLevel2
    {
      get => _countOfLevel2;
      set => SetProperty(ref _countOfLevel2, value);
    }

    private long _countOfDCFast;
    public long CountOfDCFast
    {
      get => _countOfDCFast;
      set => SetProperty(ref _countOfDCFast, value);
    }

    private string _connectors;
    /// <summary>
    /// Represents the connectors available in each charge location. 
    /// Tesla: Tesla uses the same connector for level 1, level 2 and DC fast charge. 
    /// J1772: Every non-Tesla level 1 or level 2 charging station sold in North America utilizes the J1772 connector.
    /// J1772COMBO: The CCS connector uses the J1772 charging inlet, and adds two more pins below. It “combines” the J1772 connector with the high speed charging pins, which is how it got its name.
    /// CHAdeMO: CHAdeMo was developed by the Japanese utility Tepco. It is the official standard in Japan, and virtually all DC fast chargers in Japan use a CHAdeMO connector. 
    /// It’s different in North America, where the only manufacturers currently selling electric vehicles that use the CHAdeMO connector are Nissan and Mitsubishi.
    /// </summary>
    /// <remarks>The dataset strings all the connectors available in a charger location in one field. The values are separated by a space.</remarks>
    public string Connectors
    {
      get => _connectors;
      set
      {
        SetProperty(ref _connectors, value);
        if (value == null) return;
        var connectorTypes = value.Split(' ').ToList();
        if (connectorTypes.Contains("TESLA"))
          IsConnectorTesla = Visibility.Visible;
        
        if (connectorTypes.Contains("J1772COMBO"))
          IsConnectorCombo = Visibility.Visible; 

        if (connectorTypes.Contains("CHADEMO"))
          IsConnectorChademo = Visibility.Visible; 

        if (connectorTypes.Contains("J1772"))
          IsConnectorj1772 = Visibility.Visible; ;                
      }
    }
    private Visibility _isConnectorTesla = Visibility.Collapsed;
    public Visibility IsConnectorTesla
    {
      get => _isConnectorTesla;
      set => SetProperty(ref _isConnectorTesla, value);

    }

    private Visibility _isConnectorCombo = Visibility.Collapsed;
    public Visibility IsConnectorCombo
    {
      get => _isConnectorCombo;
      set => SetProperty(ref _isConnectorCombo, value);
    }

    private Visibility _isConnectorChademo = Visibility.Collapsed;
    public Visibility IsConnectorChademo
    {
      get => _isConnectorChademo;
      set => SetProperty(ref _isConnectorChademo, value);
    }

    private Visibility _isConnectorJ1772 = Visibility.Collapsed;
    public Visibility IsConnectorj1772
    {
      get => _isConnectorJ1772;
      set => SetProperty(ref _isConnectorJ1772, value);
    }

    private ICommand _zoomToChargerLocationCommand;
    public ICommand ZoomToChargerLocationCommand
    {
      get
      {
        _zoomToChargerLocationCommand = new RelayCommand(() => ZoomToChargerLocation(), true);
        return _zoomToChargerLocationCommand;
      }
    }
    /// <summary>
    /// Method that gets called when you click on a charger item
    /// </summary>
    public void FlashAndZoomToFeature() //invoked when you click on a item
    {
      if (MapView.Active != null)
      {
        MapView.Active.FlashFeature(Module1.Current.EVChargersFeatureLayer, OID);
        var selectionDictionary = new Dictionary<FeatureLayer, List<long>>();
        selectionDictionary.Add(Module1.Current.EVChargersFeatureLayer, new List<long> { OID });
        QueuedTask.Run( () => {
          //MapView.Active.Map?.SetSelection(SelectionSet.FromDictionary(selectionDictionary), SelectionCombinationMethod.New);
          MapView.Active.ZoomTo(SelectionSet.FromDictionary(selectionDictionary));
        });
      }   
    }
    /// <summary>
    /// This is called by the zoom button for each item
    /// </summary>
    public void ZoomToChargerLocation()
    {
      if (MapView.Active != null)
      {
        var selectionDictionary = new Dictionary<FeatureLayer, List<long>>();
        selectionDictionary.Add(Module1.Current.EVChargersFeatureLayer, new List<long> { OID });
        QueuedTask.Run(() =>  {
          MapView.Active.ZoomTo(SelectionSet.FromDictionary(selectionDictionary), TimeSpan.FromSeconds(1));
          MapView.Active.FlashFeature(Module1.Current.EVChargersFeatureLayer, OID);
        });
      }
    }
  }
}
