//   Copyright 2015 Esri
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
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
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
        Dictionary<string, List<FavoriteQuery>> queryMap = new Dictionary<string, List<FavoriteQuery>>
        {
            {"Pool_Permits", new List<FavoriteQuery>{ new UnInspectedPoolsQuery{Name = "Uninspected Pools"}, new AboveGroundPoolsQuery{Name = "Above Ground Pools"} }}
        }; 

        private const string _dockPaneID = "FavoriteQueries_FavoritesDockpane";

        protected FavoritesDockpaneViewModel() { }

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
        private string _heading = "Favorite Queries";

        private ObservableCollection<string> queries;
        private ObservableCollection<string> layers;
        private ObservableCollection<Object> featureData;
        private string selectedQuery;
        private string selectedLayer;

        public ObservableCollection<string> Queries
        {
            get { return queries; }
            set
            {
                queries = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("Queries"));
            }
        }

        public ObservableCollection<string> Layers
        {
            get { return layers; }
            set
            {
                layers = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("Layers"));
            }
        }

        public ObservableCollection<Object> FeatureData
        {
            get { return featureData; }
            set
            {
                featureData = value;
                NotifyPropertyChanged(new PropertyChangedEventArgs("FeatureData"));
            }
        }

        public string SelectedQuery
        {
            get { return selectedQuery; }
            set
            {
                SetProperty(ref selectedQuery, value, () => SelectedQuery);
            }
        }
        public string SelectedLayer
        {
            get { return selectedLayer; }
            set
            {
                SetProperty(ref selectedLayer, value, () => SelectedLayer);
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
        /// This method will populate the Layers (bound to the LayersComboBox) with all the Feature Layers present in the Active Map View
        /// </summary>
        public void UpdateLayers()
        {
            Layers = new ObservableCollection<string>(MapView.Active.Map.Layers.Where(layer => layer is FeatureLayer).Select(layer => layer.Name));
        }

        /// <summary>
        /// Based on the selected layer name, the query Map is used to populate the Queries Combobox
        /// </summary>
        /// <param name="selectedItem"></param>
        public void UpdateQueries(string selectedItem)
        {
            QueuedTask.Run(() =>
            {
                using (Table table = (MapView.Active.Map.Layers.First(layer => layer.Name.Equals(selectedItem)) as FeatureLayer).GetTable())
                {
                    Queries = !queryMap.ContainsKey(selectedItem) ? new ObservableCollection<string>() : new ObservableCollection<string>(queryMap[table.GetName()].Select(query => query.Name));
                }
            });
        }

        /// <summary>
        /// Based on the Query Selected, the FeatureData (bound to the DataGrid) is populated with the results of the Query
        /// </summary>
        public void DoWork()
        {
            QueuedTask.Run(() =>
            {
                using (Table table = (MapView.Active.Map.Layers.First(layer => layer.Name.Equals(SelectedLayer)) as FeatureLayer).GetTable())
                {
                    FeatureData = !queryMap.ContainsKey(table.GetName()) ? new ObservableCollection<object>() : new ObservableCollection<object>(queryMap[table.GetName()].First(query => query.Name.Equals(SelectedQuery)).Execute(table));
                }
            });
        }
    }

    internal class AboveGroundPoolsQuery : PoolsQuery
    {
        public override List<Object> Execute(Table table)
        {
            var queryFilter = new QueryFilter
            {
                WhereClause = "Has_Pool = 1 AND Pool_Type = 2",
                PrefixClause = "DISTINCT",
                PostfixClause = "ORDER BY Pool_Type",
                SubFields = "Address, APN, Pool_Type"
            };
            return PoolsQuery.PopulatePoolData(table, queryFilter);
        }
    }

    internal class UnInspectedPoolsQuery : PoolsQuery
    {
        public override List<Object> Execute(Table table)
        {
            var queryFilter = new QueryFilter
            {
                WhereClause = "Has_Pool = 1 AND Is_Inspected = 1",
                PrefixClause = "DISTINCT",
                PostfixClause = "ORDER BY Pool_Type",
                SubFields = "Address, APN, Pool_Type"
            };
            return PoolsQuery.PopulatePoolData(table, queryFilter);
        }
    }

    /// <summary>
    /// This class represents a Favorite Query
    /// The Execute method is supposed to Execute the query desired and return a list of objects to be populated in the grid
    /// </summary>
    internal abstract class FavoriteQuery
    {
        public abstract List<Object> Execute(Table table);
        public string Name { get; set; }
    }

    internal abstract class PoolsQuery : FavoriteQuery
    {
        protected static List<object> PopulatePoolData(Table table, QueryFilter queryFilter)
        {
            var list = new List<Object>();
            IReadOnlyList<Subtype> subtypes = table.GetDefinition().GetSubtypes();
            using (RowCursor rowCursor = table.Search(queryFilter, false))
            {
                while (rowCursor.MoveNext())
                {
                    using (Row current = rowCursor.Current)
                    {
                        list.Add(new PoolData
                        {
                            APN = Convert.ToString(current["APN"]),
                            Address = Convert.ToString(current["Address"]),
                            PoolType = subtypes.First(subtype => subtype.GetCode().Equals(current["Pool_Type"])).GetName(),
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

    internal class PoolData
    {
        public string Address { get; set; }
        public string APN { get; set; }
        public string PoolType { get; set; }
    }
}
