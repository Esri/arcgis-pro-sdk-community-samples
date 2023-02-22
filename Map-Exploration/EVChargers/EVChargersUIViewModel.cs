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
using ActiproSoftware.Windows.Controls.Ribbon.Controls;
using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;


namespace EVChargers
{
  /// <summary>
  /// Represents the embeddable control that has the Search filter for the charging stations.
  /// </summary>
  internal class EVChargersUIViewModel : EmbeddableControl
  {
    public EVChargersUIViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) {
      Module1.Current.EVViewModel = this; 
      //Clear the charger and Connector collections.
      //Update these collections
      ChargersTypes.Clear();
      ConnectorTypes.Clear();
      foreach (var keyValuePair in Defaults.ChargerFieldNameTypeMapping)
      {
        var newChargerItem = new ChargerItem { ChargerName = keyValuePair.Key };
        ChargersTypes.Add(newChargerItem);
        newChargerItem.PropertyChanged += ChargerItem_PropertyChanged; //Event handler
      }
      SelectedChargerItem = ChargersTypes[0];
      foreach (var kvp in Defaults.ConnectorValueTypesMapping)
      {
        var connectorItem = new ConnectorTypeItem { ConnectorName = kvp.Key };
        ConnectorTypes.Add(connectorItem);
        connectorItem.PropertyChanged += ConnectorItem_PropertyChanged; //event handler
      }
      SelectedConnector = ConnectorTypes[0];      
    }
    public async override Task OpenAsync()
    {
      Module1.Current.EVChargersFeatureLayer = MapView.Active.Map.Layers.FirstOrDefault(n => n.Name == "EVChargers") as FeatureLayer;
      if (Module1.Current.EVChargersFeatureLayer == null) return;
      int numberOfRecords = await QueuedTask.Run( () => {
        Module1.Current.EVChargersFeatureLayer.RemoveAllDefinitionQueries(); //remove existing queries
        int numRows = 0; //get the record count of the layer
        using (var cursor = Module1.Current.EVChargersFeatureLayer.GetFeatureClass().Search())
        {
          while (cursor.MoveNext())
            numRows++;
        }
        return numRows;
      });
      NoOfRecords = numberOfRecords;
    }
    //Triggered when the state of any connector item changes
    //This is how we figure out if a connector item is selected, unselected. Or if "All" items is selected, etc.
    private void ConnectorItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      System.Diagnostics.Debug.WriteLine("ConnectorItems have been selected event");
      var sb = new StringBuilder("Selected Connectors:");
      if (e.PropertyName == nameof(ConnectorTypeItem.IsSelected))
      {
        var connectorItem = sender as ConnectorTypeItem;
        if (connectorItem.IsSelected) //an item was turned on
        {
          SelectedConnector = connectorItem;
          if (connectorItem.ConnectorName == "All")
          {
            SelectedConnectors.Clear();
            foreach (var item in ConnectorTypes)
            {
              SelectedConnectors.Add(connectorItem);
              //this will trigger event again.   
              item.IsSelected = true;    
            }
          }
          else //only one item was selected
          {
            connectorItem.IsSelected = true;
            if (!SelectedConnectors.Contains(connectorItem))
             SelectedConnectors.Add(connectorItem);
          }

        }

        else //item was turned off
        {
          if (connectorItem.ConnectorName == "All") //all need to be turned off
          {
            SelectedConnectors.Clear();
            foreach (var item in ConnectorTypes)
            {
              item.IsSelected = false;
            }
          }
          else //only one item needs to be turned off
          {
            connectorItem.IsSelected = false;
            SelectedConnectors.Remove(connectorItem);
          }

        }
      }      
      foreach (var item in SelectedConnectors)
      {
        sb.AppendLine(item.ConnectorName);
      }
      sb.AppendLine($"Count: {SelectedConnectors.Count}");
      //MessageBox.UpdateResultsDockpane($"{sb.ToString()}");
    }
    //Triggered when the state of any charger item changes
    //This is how we figure out if a charger item is selected, unselected. Or if "All" items is selected, etc.
    private void ChargerItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      System.Diagnostics.Debug.WriteLine("ChargeItems event");
      if (e.PropertyName == nameof(ChargerItem.IsSelected))
      {
        var chargerItem = sender as ChargerItem;
        
        if (chargerItem.IsSelected) //an item was turned on
        {
          SelectedChargerItem = chargerItem;
          if (chargerItem.ChargerName == "All")
          {
            SelectedChargers.Clear();
           
            foreach (var item in ChargersTypes)
            {
              SelectedChargers.Add(item);
              //this will trigger event again.
              item.IsSelected = true;
              
            }
            
          }
          else //only one item was selected
          {
            chargerItem.IsSelected = true;
            if (!SelectedChargers.Contains(chargerItem))
               SelectedChargers.Add(chargerItem);
          }
            
        }

        else //item was turned off
        {
          if (chargerItem.ChargerName == "All") //all need to be turned off
          {
            SelectedChargers.Clear();
            foreach (var item in ChargersTypes)
            {
              item.IsSelected = false;
            }
          }
          else //only one item needs to be turned off
          {
            chargerItem.IsSelected = false;
            SelectedChargers.Remove(chargerItem);
          }
            
        }               
      }
      var sb = new StringBuilder("ChargerTypes:");
      foreach (var item in ChargersTypes)
      {
        sb.AppendLine(item.ChargerName);
      }
      sb.AppendLine($"Count: {ChargersTypes.Count}");
    }

    /// <summary>
    /// Text shown in the control.
    /// </summary>
    private string _text = "Electric Vehicle Charging Station Locations";
    public string Text
    {
      get => _text;
      set => SetProperty(ref _text, value);
    }

    private double _controlWidth;
    public double ControlWidth
    {
      get => _controlWidth;
      set
      {
        SetProperty(ref _controlWidth, value);
      }
    }
    private double _controlHeight;
    public double ControlHeight
    {
      get => _controlHeight;
      set
      {
        SetProperty(ref _controlHeight, value);
      }
    }

    private ObservableCollection<ChargerItem> _chargerTypes = new ObservableCollection<ChargerItem>();
    public ObservableCollection<ChargerItem> ChargersTypes
    {
      get => _chargerTypes;
      set => SetProperty(ref _chargerTypes, value);
    }

    private ObservableCollection<ChargerItem> _selectedChargers = new ObservableCollection<ChargerItem>();
    public ObservableCollection<ChargerItem> SelectedChargers
    { 
      get => _selectedChargers;
      set
      {
        SetProperty(ref _selectedChargers, value);       
        
      }
    }

    private ChargerItem _selectedChargerItem;
    public ChargerItem SelectedChargerItem
    {
      get => _selectedChargerItem;
      set => SetProperty(ref _selectedChargerItem, value);
    }

    private ObservableCollection<ConnectorTypeItem> _connectorTypes = new ObservableCollection<ConnectorTypeItem>();
    public ObservableCollection<ConnectorTypeItem> ConnectorTypes
    {
      get => _connectorTypes;
      set => SetProperty(ref _connectorTypes, value);
    }

    private ConnectorTypeItem _selectedConnector;
    public ConnectorTypeItem SelectedConnector
    {
      get => _selectedConnector;
      set => SetProperty(ref _selectedConnector, value);
    }

    private ObservableCollection<ConnectorTypeItem> _selectedConnectors = new ObservableCollection<ConnectorTypeItem>();
    public ObservableCollection<ConnectorTypeItem> SelectedConnectors
    {
      get => _selectedConnectors;
      set
      {
        SetProperty(ref _selectedConnectors, value);

      }
    }
    private string _searchLocation;
    public string SearchLocation
    {
      get => _searchLocation;
      set => SetProperty(ref _searchLocation, value);
    }
    private int _NoOfRecords;
    public int NoOfRecords
    {
      get => _NoOfRecords;
      set => SetProperty(ref _NoOfRecords, value);
    }

    private ICommand _applyFilters;
    public ICommand ApplyFiltersCommand
    {
      get {
        _applyFilters = new RelayCommand(() => ApplyFilters(), true);
        return _applyFilters;
      }
    }

    private async void ApplyFilters()
    {
      NoOfRecords = 0;
      Module1.EVChargerLocationItems.Clear();
      Module1.DisplayResultsVM?.SearchLocationResults.Clear();
      var result = await QueuedTask.Run<(List<EVChargerLocationItem> locationsFound, int rowCount)>( () =>
      {
        //Connector type query build
        string connectorFieldName = "USER_EV_Connector_Types";
        int connectorIdx = -1;
        using (var table = Module1.Current.EVChargersFeatureLayer.GetTable())
        {
          var def = table.GetDefinition();
          connectorIdx = def.FindField("USER_EV_Connector_Types"); //connector type field.
        }
        if (connectorIdx == -1) return (null, 0);
        //USER_EV_Connector_Types LIKE '%J1772%' And USER_EV_DC_Fast_Count >= '1' And USER_EV_Level2_EVSE_Num >= 1
        List<string> connectorQueries = new List<string>(); //used for Query

        foreach (var item in SelectedConnectors)
        {
          if (item.ConnectorName == "All") continue;
          connectorQueries.Add($"{connectorFieldName} LIKE '%{Defaults.ConnectorValueTypesMapping[item.ConnectorName]}%'");
        }
        //charger type query
        List<string> chargerQueries = new List<string>();
        
        foreach (var item in SelectedChargers)
        {
          if (item.ChargerName == "All") continue;
          chargerQueries.Add($"{Defaults.ChargerFieldNameTypeMapping[item.ChargerName]} >= 1");
        }
        //Start building the complete query string now
        string whereClause = string.Empty; 
        string connectorQueryStrings = string.Empty; 
        string chargerQueryStrings = string.Empty; 
        string addressQueryString = string.Empty;
        //Build connectorQueryStrings
        if (connectorQueries.Count > 0)
        {
          connectorQueryStrings = $"({string.Join(" Or ", connectorQueries.ToArray())})";
          whereClause = connectorQueryStrings;
        }        
        //Build Charger queries
        if (chargerQueries.Count > 0 )
        {
          chargerQueryStrings = $"({string.Join(" Or ", chargerQueries.ToArray())})";
          whereClause = string.IsNullOrEmpty(whereClause) ? chargerQueryStrings : $"{whereClause} And {chargerQueryStrings}";
        }        
        //Add address to queries
        if (!string.IsNullOrEmpty(SearchLocation))
        {
          whereClause = string.IsNullOrEmpty(whereClause) ?   $"Address LIKE '%{SearchLocation}%'" : $"{whereClause} And Address LIKE '%{SearchLocation}%'";
        }
        var oidField = Module1.Current.EVChargersFeatureLayer.GetFeatureClass().GetDefinition().GetObjectIDField();

        QueryFilter qf = new QueryFilter()
        {
          WhereClause =  whereClause,
          SubFields = $"{oidField},USER_Station_Name, USER_Street_Address, USER_City, USER_State, USER_ZIP, USER_EV_Connector_Types, USER_EV_Level1_EVSE_Num, USER_EV_Level2_EVSE_Num, USER_EV_DCFast_Count"
        };
        Module1.Current.EVChargersFeatureLayer.SetDefinitionQuery(qf.WhereClause);
        //Count the records
        int numRows = 0;
        using (var cursor = Module1.Current.EVChargersFeatureLayer.GetFeatureClass().Search(qf))
        {
          while (cursor.MoveNext())
          {
            numRows++;
            using (var record = cursor.Current)
            {
              var stationName = record["USER_Station_Name"]?.ToString();
              var streetAddres = record["USER_Street_Address"]?.ToString();
              var city = record["USER_City"]?.ToString();
              var state = record["USER_State"]?.ToString();
              var zip = record["USER_ZIP"]?.ToString();
              var oid = Convert.ToInt64(record[oidField]);
              var level1Count = Convert.ToInt64(record["USER_EV_Level1_EVSE_Num"]);
              var level2Count = Convert.ToInt64(record["USER_EV_Level2_EVSE_Num"]);
              var dcFastCount = Convert.ToInt64(record["USER_EV_DCFast_Count"]);
              var connectors = record["USER_EV_Connector_Types"]?.ToString();
              Module1.EVChargerLocationItems.Add(new EVChargerLocationItem { 
                StationName = stationName, 
                Address = streetAddres, 
                City = city, 
                State = state, 
                Zip = zip, 
                OID = oid,
                CountOfDCFast = dcFastCount,
                CountOfLevel1 = level1Count,
                CountOfLevel2 = level2Count,
                Connectors = connectors
              });
            }
          }            
        }
        System.Diagnostics.Debug.WriteLine($"Number of records = {numRows}");
        MapView.Active.ZoomTo(Module1.Current.EVChargersFeatureLayer, false, TimeSpan.FromSeconds(1));
        return (Module1.EVChargerLocationItems, numRows); //TODO: How to return one thing? I don't need to return the charger locations
      });
      NoOfRecords = result.rowCount;
      DisplayResultsViewModel.UpdateResultsDockpane(); //Display the results in the Display Dockpane.
    }
    /// <summary>
		/// We have to ensure that GUI updates are only done from the GUI thread.
		/// </summary>
		public void ActionOnGuiThread(Action theAction)
    {
      if (System.Windows.Application.Current.Dispatcher.CheckAccess())
      {
        //We are on the GUI thread
        theAction();
      }
      else
      {
        //Using the dispatcher to perform this action on the GUI thread.
        ProApp.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, theAction);
      }
    }
  }
}
