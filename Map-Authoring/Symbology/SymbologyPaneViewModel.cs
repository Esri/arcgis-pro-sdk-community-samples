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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Symbology
{
    internal class SymbologyPaneViewModel : DockPane
    {
        private const string _dockPaneID = "Symbology_SymbologyPane";
        private static readonly object LockStyleItems = new object();
       

        protected SymbologyPaneViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(MyCustomStyleItems, LockStyleItems);            
            lock (LockStyleItems)
            {
                MyCustomStyleItems.Clear();
            }
            SelectedGeometry = SymbolGeometries.FirstOrDefault();
            CreateStyleProjectItem();
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

        #region Public properties
        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Custom Symbols";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        private ObservableCollection<CustomSymbolStyleItem> _myCustomStyleItems = new ObservableCollection<CustomSymbolStyleItem>();
        /// <summary>
        /// Collection of Symbol Items. 
        /// </summary>
        public ObservableCollection<CustomSymbolStyleItem> MyCustomStyleItems
        {
            get
            {                
                return _myCustomStyleItems;
            }

            set
            {
                SetProperty(ref _myCustomStyleItems, value, () => MyCustomStyleItems);
            }
        }

        private ObservableCollection<string> _symbolGeometries = new ObservableCollection<string>
        {
            "Point",
            "Line",
            "Polygon",
            "Mesh"
        };
        /// <summary>
        /// Collection of available symbol geometries
        /// </summary>
        public ObservableCollection<string> SymbolGeometries
        {
            get
            {               
                return _symbolGeometries;
            }
            set { SetProperty(ref _symbolGeometries, value, () => SymbolGeometries); }
        }

        private string _selectedGeometry;
        /// <summary>
        /// The selected symbol geometry type
        /// </summary>
        public string SelectedGeometry
        {
            get
            {
                //Initialize(); This crashes
                return _selectedGeometry;
            }
            set
            {                
                SetProperty(ref _selectedGeometry, value, () => SelectedGeometry);
                Initialize();
            }            
        }

        public StyleItemType styleItemType = StyleItemType.PointSymbol;

        #endregion


        #region private methods and properties

        private IDictionary<GeometryType, string> _geometriesDictionary = new Dictionary<GeometryType, string>
        {
            { GeometryType.Point, "Point" },
            { GeometryType.Polyline, "Line"},
            {GeometryType.Polygon, "Polygon" },
            {GeometryType.Multipatch, "Mesh" }
        };

        private static string _styleFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\AppData\Roaming\ESRI\ArcGISPro\MyCustomSymbols.stylx";
        private static StyleProjectItem _styleProjectItem = null;

        /// <summary>
        /// Initialize the list box with the symbol items
        /// </summary>
        /// <returns></returns>
        private async Task Initialize()
        {
            lock (LockStyleItems)
            {
                MyCustomStyleItems.Clear();
            }
            var myGeometryTypeKey = _geometriesDictionary.FirstOrDefault(x => x.Value == SelectedGeometry).Key;
            var symbolsMapping = await GetSymbolDataMapping(myGeometryTypeKey);                                   
            foreach (var symbol in symbolsMapping)
            {            
                var symbolStyleItem = await CreateSymbolStyleItem(symbol.Key, symbol.Value); //define the symbol
                lock (LockStyleItems)
                {

                    MyCustomStyleItems.Add(new CustomSymbolStyleItem(symbolStyleItem, symbol.Value));
                }
                //Add styleItem to StyleProject Item
                bool styleItemExists = await DoesStyleItemExists(symbolStyleItem.ItemType, symbol.Value);
                if ((styleItemExists == false) && (_styleProjectItem != null))
                    await AddStyleItemToStyle(_styleProjectItem, symbolStyleItem);
            }
        } 

        /// <summary>
        /// Creates a SymbolStyleItem from a symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private Task<SymbolStyleItem> CreateSymbolStyleItem(CIMSymbol symbol, string data)
        {
            var symbolStyleItem = QueuedTask.Run(() =>
            {
                SymbolStyleItem sItem = null;
                try
                {
                    sItem = new SymbolStyleItem() //define the symbol
                    {
                        Symbol = symbol,
                        ItemType = styleItemType,
                        //Category = "Shapes",
                        Name = data,
                        Key = data,
                        Tags = data,                        
                    };
                }
                catch (Exception ex)
                {

                }                
                return sItem;
            });
            return symbolStyleItem;
        }
        /// <summary>
        /// Gets all the symbols available for a particular geometry
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private async Task<Dictionary<CIMSymbol, string>> GetSymbolDataMapping(GeometryType geometry)
        {           
            var myDictionary = new Dictionary<CIMSymbol, string>();

            switch (geometry)
            {
                case GeometryType.Point:
                    styleItemType = StyleItemType.PointSymbol;
                    myDictionary = await MyPointSymbology.GetAllPointSymbolsAsync();
                    break;
                case GeometryType.Polygon:
                    styleItemType = StyleItemType.PolygonSymbol;
                    myDictionary = await MyPolygonSymbology.GetAllPolygonSymbolsAsync();
                    break;
                case GeometryType.Polyline:
                    styleItemType = StyleItemType.LineSymbol;
                    myDictionary = await MyLineSymbology.GetAllLineSymbolsAsync();
                    break;
                case GeometryType.Multipatch:
                    styleItemType = StyleItemType.MeshSymbol;
                    myDictionary = await MeshSymbology.GetAll3DSymbolsAsync();
                    break;
            }
            return myDictionary;
        }
        /// <summary>
        /// Create a styleProject item.
        /// </summary>
        /// <returns></returns>
        private static async Task CreateStyleProjectItem()
        {
            if (_styleProjectItem?.PhysicalPath == null)
            {
                await QueuedTask.Run(() =>
                {
                    if (File.Exists(_styleFilePath)) //check if the file is on disc. Add it to the project if it is.
                        Project.Current.AddStyle(_styleFilePath);
                    else //else create the style item  
                    {
                        if (_styleProjectItem != null)
                            Project.Current.RemoveStyle(_styleProjectItem.Name); //remove style from project                           
                        Project.Current.CreateStyle($@"{_styleFilePath}");
                    }
                });
                var styleItemsContainer = Project.Current.GetItems<StyleProjectItem>(); //gets all Style Project Items in the current project
                _styleProjectItem = styleItemsContainer.FirstOrDefault(s => s.Name.Contains("MyCustomSymbols"));
            }
        }

        /// <summary>
        /// Adds a styleitem to a style.
        /// </summary>
        /// <param name="styleProjectItem"></param>
        /// <param name="cimSymbolStyleItem"></param>
        /// <returns></returns>
        private Task AddStyleItemToStyle(StyleProjectItem styleProjectItem, SymbolStyleItem cimSymbolStyleItem)
        {
            return QueuedTask.Run(() =>
            {
                if (styleProjectItem == null || cimSymbolStyleItem == null)
                {
                    return;
                }

                styleProjectItem.AddItem(cimSymbolStyleItem);
            });
        }

        public Task<bool> DoesStyleItemExists(StyleItemType styleItemType, string key)
        {
            var styleItemExists =  QueuedTask.Run(() =>
            {

                bool styleItem = false;
                //Search for a specific point symbol in style
                SymbolStyleItem item = (SymbolStyleItem)_styleProjectItem?.LookupItem(styleItemType, key);
                if (item == null)
                    styleItem =  false;
                else
                    styleItem = true;
                return styleItem;
            });
            return styleItemExists;
        }

        #endregion
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class SymbologyPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            SymbologyPaneViewModel.Show();
        }
    }
}
