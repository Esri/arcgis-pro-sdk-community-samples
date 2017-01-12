/*

   Copyright 2017 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;

namespace QueryDefDataQuery
{
    internal class QueryDefDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "QueryDefDataQuery_QueryDefDockpane";
        private ICommand _cmdSingleQuery;
        private ICommand _cmdRelateQuery;
        private int _resultCount;
        private DataTable _resultData;

        protected QueryDefDockpaneViewModel() { }

        public int ResultCount
        {
            get { return _resultCount; }
            set
            {
                SetProperty(ref _resultCount, value, () => ResultCount);
            }
        }

        public DataTable ResultData
        {
            get { return _resultData; }
            set
            {
                SetProperty(ref _resultData, value, () => ResultData);
            }
        }
        
        public ICommand CmdSingleQuery
        {
            get { return _cmdSingleQuery ?? (_cmdSingleQuery = new RelayCommand(() => SingleQueryDef(), true)); }
        }

        public ICommand CmdRelateQuery
        {
            get { return _cmdRelateQuery ?? (_cmdRelateQuery = new RelayCommand(() => RelateQueryDef(), true)); }
        }

        #region GeoDatabase

        public async Task SingleQueryDef()
        {
            try
            {
                ResultCount = 0;
                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    var featLayer =
                        (MapView.Active.Map.Layers.First(layer => layer.Name.Equals("Portland Precincts")) as
                            FeatureLayer);
                    if (featLayer == null) return;
                    using (var tbl = featLayer.GetTable())
                    {
                        using (var datastore = tbl.GetDatastore())
                        {
                            if (datastore is UnknownDatastore) return;
                            var geodatabase = datastore as Geodatabase;
                            if (geodatabase == null) return;
                            var queryDef = new QueryDef
                            {
                                Tables = "Portland_PD_Precincts",
                                SubFields = "Name",
                                WhereClause = "Name <> ''"
                            };
                            var result = new DataTable("results");
                            result.Columns.Add("Precinct Name", typeof (string));
                            using (var rowCursor = geodatabase.Evaluate(queryDef, false))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        var feature = row as Feature;
                                        var rowResult = result.NewRow();
                                        rowResult[0] = Convert.ToString(row[0]);
                                        result.Rows.Add(rowResult);
                                    }
                                }
                            }
                            ResultCount = result.Rows.Count;
                            ResultData = result;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Error in single table querydef: {ex}");
            }
        }

        public async Task RelateQueryDef()
        {
                try
                {
                    ResultCount = 0;
                    await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                    {
                        var featLayer =
                            (MapView.Active.Map.Layers.First(layer => layer.Name.Equals("Portland Precincts")) as
                                FeatureLayer);
                        if (featLayer == null) return;
                        using (var tbl = featLayer.GetTable())
                        {
                            using (var datastore = tbl.GetDatastore())
                            {
                                if (datastore is UnknownDatastore) return;
                                var geodatabase = datastore as Geodatabase;
                                if (geodatabase == null) return;
                                var queryDef = new QueryDef
                                {
                                    Tables = "Portland_PD_Precincts,Police_Stations",
                                    SubFields = "Name,Address,Zip,Website",
                                    WhereClause = "Name <> '' and Name = Precinct"
                                };
                                var result = new DataTable("results");
                                result.Columns.Add("Precinct Name", typeof(string));
                                result.Columns.Add("Address", typeof(string));
                                result.Columns.Add("Zip", typeof(string));
                                result.Columns.Add("Website", typeof(string));
                                using (var rowCursor = geodatabase.Evaluate(queryDef, false))
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Row row = rowCursor.Current)
                                        {
                                            var feature = row as Feature;
                                            var rowResult = result.NewRow();
                                            for (var i = 0; i < 4; i++)
                                                rowResult[i] = Convert.ToString(row[i]);
                                            result.Rows.Add(rowResult);
                                        }
                                    }
                                }
                                ResultCount = result.Rows.Count;
                                ResultData = result;
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($@"Error in single table querydef: {ex}");
                }
            }
        #endregion

        #region UI Management
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
        private string _heading = "QueryDef Dockpane";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
        #endregion 
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class QueryDefDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            QueryDefDockpaneViewModel.Show();
        }
    }
}
