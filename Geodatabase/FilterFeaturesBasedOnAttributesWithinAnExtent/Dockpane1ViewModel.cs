//   Copyright 2017 Esri
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Framework.Dialogs;

namespace FilterFeaturesBasedOnAttributesWithinAnExtent
{
  internal class Dockpane1ViewModel : DockPane
  {
    private const string _dockPaneID = "ProAppModule3_Dockpane1";

    protected Dockpane1ViewModel()
    {
      BindingOperations.EnableCollectionSynchronization(FeatureData, _collectionLock);
      BindingOperations.EnableCollectionSynchronization(Fields, _collectionLock);

      TOCSelectionChangedEvent.Subscribe(UpdateFields);
      SelectedLayer = @"Select Feature Layer in TOC";
    }

    /// <summary>
    /// Make sure there is a feature layer selected
    /// Update the Fields Combobox with the Fields corresponding to the Layer Selected
    /// </summary>
    /// <param name="args"></param>
    private void UpdateFields(MapViewEventArgs args)
    {
      if (args.MapView == null) return;
      if (args.MapView.GetSelectedLayers().Count == 0)
      {
        SelectedLayer = @"Select Feature Layer in TOC";
        return;
      }
      var selectedLayer = args.MapView.GetSelectedLayers()[0];
      if (!(selectedLayer is FeatureLayer))
      {
        SelectedLayer = @"Select Feature Layer in TOC";
        return;
      }
      SelectedLayer = args.MapView.GetSelectedLayers()[0].Name;
      _featureLayer = selectedLayer as FeatureLayer;
      QueuedTask.Run(() =>
      {
        using (var table = _featureLayer.GetTable())
        {
          var newFields = new ObservableCollection<string>(table.GetDefinition().GetFields().Select(field => field.Name));
          lock (_collectionLock)
          {
            Fields.Clear();
            foreach (var field in newFields) Fields.Add(field);
          }
        }
      });
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      var pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      pane?.Activate();
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Highlight Features";

    private FeatureLayer _featureLayer;
    private ObservableCollection<string> _fields = new ObservableCollection<string>();
    private ObservableCollection<FeatureData> _featureData = new ObservableCollection<FeatureData>();
    private readonly object _collectionLock = new object();
    private string _selectedLayer;
    private string _selectedField;
    private string _fieldValue;
    private int _resultCount = 0;
    private ICommand _cmdWork;

    public ObservableCollection<string> Fields
    {
      get { return _fields; }
      set
      {
        _fields = value;
        NotifyPropertyChanged(new PropertyChangedEventArgs("Fields"));
      }
    }

    public ObservableCollection<FeatureData> FeatureData
    {
      get { return _featureData; }
      set
      {
        _featureData = value;
        NotifyPropertyChanged(new PropertyChangedEventArgs("FeatureData"));
      }
    }

    public string SelectedField
    {
      get { return _selectedField; }
      set
      {
        SetProperty(ref _selectedField, value, () => SelectedField);
      }
    }

    public string FieldValue
    {
      get { return _fieldValue; }
      set
      {
        SetProperty(ref _fieldValue, value, () => FieldValue);
      }
    }

    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }

    public int ResultCount
    {
      get { return _resultCount; }
      set
      {
        SetProperty(ref _resultCount, value, () => ResultCount);
      }
    }

    public string SelectedLayer
    {
      get { return _selectedLayer; }
      set
      {
        SetProperty(ref _selectedLayer, value, () => SelectedLayer);
      }
    }

    /// <summary>
    /// Get the selected Feature Layer and the corresponding FeatureClass
    /// If there are any features selected for that layer, Zoom to the extent and use the extent for a Spatial Query
    /// If there are no feature selected, perform a normal query on all the features for that FeatureClass
    /// List the selected Object Ids in the datagrid by assigning them to FeatureData property (bound to the datagrid)
    /// </summary>
    public ICommand CmdWork
    {
      get
      {
        return _cmdWork ?? (_cmdWork = new RelayCommand(() =>
        {
          try
          {
            if (MapView.Active.GetSelectedLayers().Count == 0) return;
            var selectedLayer = MapView.Active.GetSelectedLayers()[0];
            var theField = GetFieldByName(SelectedField).Result;
            if (!(selectedLayer is FeatureLayer)) return;
            var featureLayer = selectedLayer as FeatureLayer;
            QueuedTask.Run(() =>
                    {
                  using (var table = featureLayer.GetTable())
                  {
                    var quote = theField.FieldType == FieldType.String ? "'" : "";
                    var whereClause = $"{SelectedField} = {quote}{FieldValue}{quote}";
                    using (var mapSelection = featureLayer.GetSelection())
                    {
                      QueryFilter queryFilter;
                      if (mapSelection.GetCount() > 0)
                      {
                        Envelope envelope = null;
                        using (var cursor = mapSelection.Search())
                        {
                          while (cursor.MoveNext())
                          {
                            using (var feature = cursor.Current as Feature)
                            {
                              envelope = envelope == null
                                          ? feature.GetShape().Extent
                                          : envelope.Union(feature.GetShape().Extent);
                            }
                          }
                        }
                        queryFilter = new SpatialQueryFilter
                        {
                          FilterGeometry = new EnvelopeBuilder(envelope).ToGeometry(),
                          SpatialRelationship = SpatialRelationship.Contains,
                          WhereClause = whereClause
                        };
                      }
                      else
                      {
                        queryFilter = new QueryFilter { WhereClause = whereClause };
                      }
                      try
                      {
                        lock (_collectionLock) FeatureData.Clear();
                        using (var rowCursor = table.Search(queryFilter, false))
                        {
                          while (rowCursor.MoveNext())
                          {
                            using (var current = rowCursor.Current)
                            {
                              var offenseType = Convert.ToInt32(current["Offense_Type"]);
                              var sMayorOffenseType =
                                          Convert.ToString(current["Major_Offense_Type"]);
                              var sAddress = Convert.ToString(current["Address"]);
                              var objectId = current.GetObjectID();
                              lock (_collectionLock)
                                FeatureData.Add(new FeatureData
                                {
                                  ObjectId = objectId,
                                  MajorOffenseType = sMayorOffenseType,
                                  Address = sAddress,
                                  OffenseType = offenseType
                                });
                            }
                          }
                        }
                        ResultCount = FeatureData.Count;
                      }
                      catch (Exception ex)
                      {
                        MessageBox.Show($@"Query error: {ex}");
                        ResultCount = 0;
                      }
                    }
                  }
                });
          }
          catch (Exception ex)
          {
            MessageBox.Show($@"Query error: {ex}");
            ResultCount = 0;

          }
        }, true));
      }
    }

    private static Task<Field> GetFieldByName(string fieldName)
    {
      var selectedLayer = MapView.Active.GetSelectedLayers()[0];
      var featureLayer = selectedLayer as FeatureLayer;
      return QueuedTask.Run(() =>
      {
        Field resultField = null;
        if (featureLayer != null)
        {
          using (var table = featureLayer.GetTable())
          {
            resultField = table.GetDefinition().GetFields().FirstOrDefault(field => field.Name == fieldName);
          }
        }
        return resultField;
      });
    }
  }

  internal class FeatureData
  {
    public long ObjectId { get; set; }
    public string MajorOffenseType { get; set; }
    public int OffenseType { get; set; }
    public string Address { get; set; }
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
