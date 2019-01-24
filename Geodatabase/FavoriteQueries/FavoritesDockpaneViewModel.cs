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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace FavoriteQueries
{
    internal class FavoritesDockpaneViewModel : DockPane
    {
        /// <summary>
        /// This is a Map of the Name of the ArcGIS.Core.Data Table/FeatureClass backing the Layer to the List 
        /// if Queries are set up for the corresponding Table/FeatureClass
        /// Since FavoriteQuery is an abstract class, SubClass the FavoriteQuery for the specific case
        /// </summary>
        readonly Dictionary<string, List<FavoriteQuery>> _queryMap = new Dictionary<string, List<FavoriteQuery>>
        {
            {"Crimes", new List<FavoriteQuery> { new UnClassifiedCrimesQuery{Name = "Unclassified Crimes"}, new ClassifiedCrimesQuery {Name = "Classified Crimes"} }}
        }; 

        private const string DockPaneId = "FavoriteQueries_FavoritesDockpane";

        protected FavoritesDockpaneViewModel() { }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId);

            pane?.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Favorite Queries";

        private ObservableCollection<string> _queries;
        private ObservableCollection<string> _layers;
        private ObservableCollection<object> _featureData;
        private string _selectedQuery;
        private string _selectedLayer;
        private int _resultCount;
        private ICommand _cmdWork;
        private ICommand _cmdDropDownLayers;
        private ICommand _cmdDropDownQueries;

        public ObservableCollection<string> Queries
        {
            get { return _queries; }
            set
            {
                _queries = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("Queries"));
            }
        }

        public ObservableCollection<string> Layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("Layers"));
            }
        }

        public ObservableCollection<object> FeatureData
        {
            get { return _featureData; }
            set
            {
                _featureData = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("FeatureData"));
            }
        }

        public string SelectedQuery
        {
            get { return _selectedQuery; }
            set
            {
                SetProperty(ref _selectedQuery, value, () => SelectedQuery);
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
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        
        /// <summary>
        /// Based on the Query Selected, the FeatureData (bound to the DataGrid) is populated with the results of the Query
        /// </summary>
        public ICommand CmdWork
        {
            get
            {
                return _cmdWork ?? (_cmdWork = new RelayCommand(() =>
                {
                    QueuedTask.Run(() =>
                    {
                        try
                        {
                            using (
                                Table table =
                                    (MapView.Active.Map.Layers.First(layer => layer.Name.Equals(SelectedLayer)) as
                                        FeatureLayer)
                                        .GetTable())
                            {
                                FeatureData = !_queryMap.ContainsKey(table.GetName())
                                    ? new ObservableCollection<object>()
                                    : new ObservableCollection<object>(
                                        _queryMap[table.GetName()].First(query => query.Name.Equals(SelectedQuery))
                                            .Execute(table));
                            }
                            ResultCount = FeatureData.Count;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($@"Query error: {ex}");
                            ResultCount = 0;
                        }
                    });
                }, () => SelectedQuery != null));
            }
        }

        /// <summary>
        /// This method will populate the Layers (bound to the LayersComboBox) with all the Feature Layers present in the Active Map View
        /// </summary>
        public ICommand CmdDropDownLayers
        {
            get
            {
                return _cmdDropDownLayers ?? (_cmdDropDownLayers = new RelayCommand(() =>
                {
                    Layers =
                        new ObservableCollection<string>(
                            MapView.Active.Map.Layers.Where(layer => layer is FeatureLayer).Select(layer => layer.Name));
                }, () => MapView.Active.Map.Layers != null));
            }
        }

        /// <summary>
        /// Based on the selected layer name, the query Map is used to populate the Queries Combobox
        /// </summary>
        public ICommand CmdDropDownQueries
        {
            get
            {
                return _cmdDropDownQueries ?? (_cmdDropDownQueries = new RelayCommand(() =>
                {
                    QueuedTask.Run(() =>
                    {
                        var featureLayer =
                            MapView.Active.Map.Layers.First(layer => layer.Name.Equals(SelectedLayer)) as FeatureLayer;
                        if (featureLayer == null) return;
                        using (var table = featureLayer.GetTable())
                        {
                            Queries = !_queryMap.ContainsKey(SelectedLayer)
                                ? new ObservableCollection<string>()
                                : new ObservableCollection<string>(_queryMap[table.GetName()].Select(query => query.Name));
                        }
                    });
                }, true));
            }
        }
        
    }

    internal class ClassifiedCrimesQuery : CrimesQuery
    {
        public override List<object> Execute(Table table)
        {
            var queryFilter = new QueryFilter
            {
                WhereClause = "Offense_Type > 0",
                //PrefixClause = "DISTINCT",
                PostfixClause = "ORDER BY Offense_Type",
                SubFields = "Address, Major_Offense_Type, Offense_Type"
            };
            return CrimesQuery.PopulateResultData(table, queryFilter);
        }
    }

    internal class UnClassifiedCrimesQuery : CrimesQuery
    {
        public override List<object> Execute(Table table)
        {
            var queryFilter = new QueryFilter
            {
                WhereClause = "Offense_Type = 0",
                //PrefixClause = "DISTINCT",
                PostfixClause = "ORDER BY Offense_Type",
                SubFields = "Address, Major_Offense_Type, Offense_Type"
            };
            return CrimesQuery.PopulateResultData(table, queryFilter);
        }
    }

    /// <summary>
    /// This class represents a Favorite Query
    /// The Execute method is supposed to Execute the query desired and return a list of objects to be populated in the grid
    /// </summary>
    internal abstract class FavoriteQuery
    {
        public abstract List<object> Execute(Table table);
        public string Name { get; set; }
    }

    internal abstract class CrimesQuery : FavoriteQuery
    {
        protected static List<object> PopulateResultData(Table table, QueryFilter queryFilter)
        {
            var list = new List<object>();
            IReadOnlyList<Subtype> subtypes = table.GetDefinition().GetSubtypes();
            using (RowCursor rowCursor = table.Search(queryFilter, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row current = rowCursor.Current)
                    {
                        var subtypeValue = Convert.ToInt32(current["Offense_Type"]);
                        var sMayorOffenseType = Convert.ToString(current["Major_Offense_Type"]);
                        var sAddress = Convert.ToString(current["Address"]);
                        var theSubtype = subtypes.First(subtype => (subtype.GetCode() == subtypeValue));
                        var sOffenseType = theSubtype.GetName();
                        list.Add(new CrimeData
                        {
                            MajorOffenseType = sMayorOffenseType,
                            Address = sAddress,
                            OffenseType = sOffenseType
                        });
                    }
                }
            }
            return list;
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class FavoritesDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            FavoritesDockpaneViewModel.Show();
        }
    }

    internal class CrimeData
    {
        public string Address { get; set; }
        public string MajorOffenseType { get; set; }
        public string OffenseType { get; set; }
    }
}
