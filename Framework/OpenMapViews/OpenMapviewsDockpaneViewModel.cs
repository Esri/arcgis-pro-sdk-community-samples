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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Mapping;
using System.Windows.Data;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using System.Windows.Input;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Threading;

namespace OpenMapViews
{
    internal class OpenMapviewsDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "OpenMapViews_OpenMapviewsDockpane";

        /// <summary>
        /// used to lock collections for use by multiple threads
        /// </summary>
        private readonly object _lockCollection = new object();
        private ObservableCollection<MapviewItem> _listOfMapviews = new ObservableCollection<MapviewItem>();
        private MapviewItem _selectedMapview;

        private string _temporaryPath = string.Empty;

        /// <summary>
        /// CTor
        /// </summary>
        public OpenMapviewsDockpaneViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(ListOfMapviews, _lockCollection);

            // create a simulated list of mapviews
            // either when a project is opened or if a project is already open
            ProjectOpenedEvent.Subscribe((ProjectEventArgs e) => { CreateMapviewItemsAsync(); });
            if (Project.Current != null) CreateMapviewItemsAsync();

            ProjectClosedEvent.Subscribe((ProjectEventArgs e) =>
            {
                // cleanup temp folder
                if (!string.IsNullOrEmpty(_temporaryPath)) System.IO.Directory.Delete(_temporaryPath, true);
                _temporaryPath = string.Empty;
            });
        }

        public ObservableCollection<MapviewItem> ListOfMapviews
        {
            get { return _listOfMapviews; }
        }

        /// <summary>
        /// Because we need a type of 'index' that allows create a set of maps and 
        /// corresponding mapviews.  The 'index' is then displayed in a datagrid on a dockpane.
        /// Selecting an item on the datagrid will either show or hide the associated mapview pane
        /// </summary>
        private void CreateMapviewItemsAsync()
        {
            if (Project.Current == null)
            {
                lock (_lockCollection)
                {
                    ListOfMapviews.Clear();
                }
                return;
            }
            IEnumerable<GDBProjectItem> gdbProjectItems = Project.Current.GetItems<GDBProjectItem>();
            QueuedTask.Run(() =>
            {
                lock (_lockCollection)
                {
                    ListOfMapviews.Clear();
                }
                bool bFound = false;
                if (string.IsNullOrEmpty(_temporaryPath)) _temporaryPath = System.IO.Path.GetTempPath();
                foreach (GDBProjectItem gdbProjectItem in gdbProjectItems)
                {
                    using (Datastore datastore = gdbProjectItem.GetDatastore())
                    {
                        //Unsupported datastores (non File GDB and non Enterprise GDB) will be of type UnknownDatastore
                        if (datastore is UnknownDatastore)
                            continue;

                        Geodatabase geodatabase = datastore as Geodatabase;
                        // Use the geodatabase.
                        try
                        {
                            var statesFeature = geodatabase.GetDefinition<FeatureClassDefinition>("states");
                            var shortNameIdx = statesFeature.FindField("STATE_ABBR");
                            var fipsNumberIdx = statesFeature.FindField("STATE_FIPS");
                            var geoIdx = statesFeature.FindField(statesFeature.GetShapeField());
                            if (shortNameIdx >= 0 && fipsNumberIdx >= 0)
                            {
                                using (FeatureClass fcStates = geodatabase.OpenDataset<FeatureClass>(statesFeature.GetName()))
                                {
                                    var fcRowCursor = fcStates.Search(new QueryFilter { SubFields= "STATE_ABBR,STATE_FIPS,Shape", WhereClause = "1 = 1", PostfixClause = "order by STATE_ABBR" });
                                    while (fcRowCursor.MoveNext())
                                    {
                                        using (Row row = fcRowCursor.Current)
                                        {
                                            var mapName = row[shortNameIdx].ToString();
                                            var mapNumber = Convert.ToUInt16(row[fipsNumberIdx]);
                                            var geometry = row[geoIdx] as Geometry;
                                            lock (_lockCollection)
                                            {
                                                if (!ListOfMapviews.Any((x) => x.MapName == mapName))
                                                {
                                                    ListOfMapviews.Add(new MapviewItem
                                                    {
                                                        MapName = mapName,
                                                        MapNumber = mapNumber,
                                                        Extent = geometry.Extent.Expand(1.1, 1.1, true)
                                                    });
                                                }
                                            }
                                            bFound = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // no state feature class found
                        }
                    }
                }
                if (!bFound)
                {
                    lock (_lockCollection)
                    {
                        ListOfMapviews.Add(new MapviewItem { MapName = "No 'States' Feature class" });
                    }
                }
            });
        }

        /// <summary>
        /// a map view item was selected -> create the Map and display the map pane
        /// </summary>
        public MapviewItem SelectedMapview
        {
            get { return _selectedMapview; }
            set
            {
                SetProperty(ref _selectedMapview, value, () => SelectedMapview);
                if (_selectedMapview != null)
                {
                    System.Diagnostics.Debug.WriteLine("SelectedMapview start");
                    CreateAndViewMapPane(_selectedMapview);
                    System.Diagnostics.Debug.WriteLine("SelectedMapview end");
                }
            }
        }

        /// <summary>
        /// Creates a new mapviewpane (and a map as well if need-be) or closes the mapviewpane if it already exists
        /// </summary>
        /// <param name="selectedMapview"></param>
        private async void CreateAndViewMapPane(MapviewItem selectedMapview)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("CreateAndViewMapPane start");
                var newMapName = selectedMapview.MapviewName;
                {
                    var mapPanes = ProApp.Panes.OfType<IMapPane>();
                    foreach (Pane pane in mapPanes)
                    {
                        if (pane.Caption == newMapName)
                        {
                            ProApp.Panes.ClosePane(pane.InstanceID);
                            UpdateHasMapView(selectedMapview.MapName, false);
                            return;
                        }
                    }
                }
                var newExtent = selectedMapview.Extent;
                // we need to find or create a map with a given selectedMapview.MapviewName
                // 1: does the map already exist?
                MapProjectItem mapProjItem = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item =>
                                                                item.Name.Equals(newMapName));
                if (mapProjItem != null)
                {
                    // we found the existing MapProjectItem
                    var mapPanes = ProApp.Panes.OfType<IMapPane>();
                    foreach (Pane pane in mapPanes)
                    {
                        if (pane.Caption == newMapName)
                        {
                            ProApp.Panes.ClosePane(pane.InstanceID);
                            UpdateHasMapView(selectedMapview.MapName, false);
                            return;
                        }
                    }
                    //Opening the map in a mapview
                    await QueuedTask.Run(async () =>
                    {
                        await ProApp.Panes.CreateMapPaneAsync(mapProjItem.GetMap());
                    });
                    UpdateHasMapView(selectedMapview.MapName, true);
                }
                else
                {
                    await CreateMapThruCopy(newMapName, newExtent);
                    UpdateHasMapView(selectedMapview.MapName, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An error occurred in CreateAndViewMapPane: {ex.ToString()}");
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine("CreateAndViewMapPane end");
            }
        }

        /// <summary>
        /// Takes a map as a template and makes a copy of that map with a given new map name and extent
        /// </summary>
        /// <param name="newMapName"></param>
        /// <param name="newExtent"></param>
        /// <returns></returns>
        private async Task CreateMapThruCopy(string newMapName, Envelope newExtent)
        {
            // we have to create a new Map from any existing Map and then
            // add it as MapProjectItem item
            MapProjectItem mapTemplateProjItem = Project.Current.GetItems<MapProjectItem>().FirstOrDefault();
            var newMapPane = await QueuedTask.Run(async () =>
            {
                var mapTemplate = mapTemplateProjItem.GetMap();
                var theNewMap = MapFactory.Instance.CopyMap(mapTemplate);
                var defMap = theNewMap.GetDefinition();
                defMap.DefaultExtent = newExtent;
                defMap.Name = newMapName;
                theNewMap.SetDefinition(defMap);
                return await ProApp.Panes.CreateMapPaneAsync(theNewMap);
            });
        }

        /// <summary>
        /// Checks or Unchecks the "HasViewPane" checkbox.  Then clears the current selection
        /// </summary>
        /// <param name="MapName"></param>
        /// <param name="newHasMapView"></param>
        private void UpdateHasMapView (string MapName, bool newHasMapView)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke((Action)(() =>
               {
                   lock (_lockCollection)
                   {
                       if (SelectedMapview != null)
                       {
                           System.Diagnostics.Debug.WriteLine("UpdateHasMapView start");
                           SelectedMapview.HasViewPane = newHasMapView;
                           // clear the selection
                           SelectedMapview = null;
                           System.Diagnostics.Debug.WriteLine("UpdateHasMapView end");
                       }
                   }
               }));
        }

        #region Helpers

        /// <summary>
        /// Replaces the class content of a given json class tag
        /// </summary>
        /// <param name="json"></param>
        /// <param name="className"></param>
        /// <param name="newClassContent"></param>
        /// <param name="fileName">Used in case of exception</param>
        /// <returns>updated json string</returns>
        private string ReplaceJsonClass(string json, string className, string newClassContent, string fileName)
        {
            var searchPattern = $@"""{className}"" : {{";
            var idx = json.IndexOf(searchPattern);
            if (idx < 0) throw new Exception($"A format problem was encounter in the mapx file: {fileName}");
            idx--;
            idx += searchPattern.Length;
            var idxEnd = FindClosingTag (json, idx);
            var changedContent = json.Remove(idx, idxEnd - idx);
            return changedContent.Insert(idx, newClassContent);
        }

        private int FindClosingTag(string json, int idx)
        {
            var idxCloseTag = idx;
            var idxEnd = json.Length;
            var indent = 0;
            while (idxCloseTag < idxEnd)
            {
                switch (json[idxCloseTag])
                {
                    case '{':
                        indent++;
                        break;
                    case '}':
                        indent--;
                        if (indent == 0)
                        {
                            // closing tag
                            return idxCloseTag + 1;
                        }
                        break;
                }
                idxCloseTag++;
            }
            return idxCloseTag;
        }

        /// <summary>
        /// Replaces the string content of a given json tag
        /// </summary>
        /// <param name="json"></param>
        /// <param name="stringName"></param>
        /// <param name="newStringContent"></param>
        /// <param name="fileName">Used in case of exception</param>
        /// <returns>updated json string</returns>
        private string ReplaceJsonString(string json, string stringName, string newStringContent, string fileName)
        {
            var searchPattern = $@"""{stringName}"" : """;
            var idx = json.IndexOf(searchPattern);
            if (idx < 0) throw new Exception($"A format problem was encounter in the mapx file: {fileName}");
            idx += searchPattern.Length;
            var idxEnd = json.IndexOf(@"""", idx);
            var changedContent = json.Remove(idx, idxEnd - idx);
            return changedContent.Insert(idx, newStringContent);
        }

        #endregion Helpers
        
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
        private string _heading = "Select to show/hide view Map";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class OpenMapviewsDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            OpenMapviewsDockpaneViewModel.Show();
        }
    }
}
