/*

   Copyright 2018 Esri

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
using ArcGIS.Desktop.Mapping;
using System.Collections.ObjectModel;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;
using System.IO;
using ArcGIS.Desktop.Core;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Mapping;

namespace TextSymbols
{
    internal class TextSymbolsGalleryViewModel : DockPane
    {
        private const string _dockPaneID = "Labeling_LabelGallery";
        private static readonly object _lockStyleItems = new object();
        private static readonly object _lockLayers = new object();
        protected TextSymbolsGalleryViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(CustomTextStyles, _lockStyleItems);
            BindingOperations.EnableCollectionSynchronization(ActiveMapLayers, _lockLayers);
            _applyLabelCommand = new RelayCommand(() => ApplyLabel(), () => true);
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
        protected override async Task InitializeAsync()
        {
            await InitAsync();
            return;
        }
        #region Public properties
        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Custom Text Symbols";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
        private ObservableCollection<BasicFeatureLayer> _activeMapLayers = new ObservableCollection<BasicFeatureLayer>();
        /// <summary>
        /// Collection of feature layers in the active map
        /// </summary>
        public ObservableCollection<BasicFeatureLayer> ActiveMapLayers
        {
            get { return _activeMapLayers; }
            set { SetProperty(ref _activeMapLayers, value, () => ActiveMapLayers); }
        }
        private BasicFeatureLayer _selectedLayer;
        /// <summary>
        /// The selected feature layer
        /// </summary>
        public BasicFeatureLayer SelectedLayer
        {
            get { return _selectedLayer; }
            set { SetProperty(ref _selectedLayer, value, () => SelectedLayer); }
        }
        
        private bool _isLabelVisible = true;
        /// <summary>
        /// Checks if the label visibility for the layer should be set
        /// </summary>
        public bool IsLabelVisible
        { get { return _isLabelVisible; }
          set { SetProperty(ref _isLabelVisible, value, () => IsLabelVisible); }
        }

        private ObservableCollection<SymbolStyleItem> _customTextStyles = new ObservableCollection<SymbolStyleItem>();
        /// <summary>
        /// Collection of custom text symbols
        /// </summary>
        public ObservableCollection<SymbolStyleItem> CustomTextStyles
        {
            get { return _customTextStyles; }
            set { SetProperty(ref _customTextStyles, value, () => CustomTextStyles); }
        }

        private SymbolStyleItem _selectTextStyle;
        /// <summary>
        /// Selected Text symbol
        /// </summary>
        public SymbolStyleItem SelectedTextStyle
        {
            get { return _selectTextStyle; }
            set { SetProperty(ref _selectTextStyle, value, () => SelectedTextStyle); }
        }
        private Visibility _dockpaneVisible = Visibility.Visible;
        /// <summary>
        /// Controls the visible state of the controls on the dockpane
        /// </summary>
        /// <remarks>Dockpane visibility controlled from the module</remarks>
        public Visibility DockpaneVisible
        {
            get
            {
                return _dockpaneVisible;
            }
            set { SetProperty(ref _dockpaneVisible, value, () => DockpaneVisible); }
        }
        #endregion

        private ICommand _applyLabelCommand;
        public ICommand ApplyLabelCommand
        {
            get { return _applyLabelCommand; }
        }

        public async Task InitAsync()
        {            
            if (MapView.Active == null)
                return;
            await CreateStyleProjectItemAsync();
            ActiveMapLayers = new ObservableCollection<BasicFeatureLayer>();
            CustomTextStyles = new ObservableCollection<SymbolStyleItem>();
            //Initialize the collection of layers
            foreach (BasicFeatureLayer lyr in MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>())
            {
                lock(_lockLayers)
                    ActiveMapLayers.Add(lyr);
            }
              
            if (ActiveMapLayers.Count > 0)
                SelectedLayer = ActiveMapLayers[0];
            if (CustomTextStyles.Count > 0)
                return;
            //Get the custom text symbols into a dictionary
            var allTextSymbols = await TextSymbols.GetAllTextSymbolsAsync();
            //Add the text symbols to a collection of text symbols
            foreach (var customTextItem in allTextSymbols)
            {
                var symbolStyleItem = await CreateSymbolStyleItem(customTextItem.Key, customTextItem.Value); //define the symbol
                                                                                                             //CustomTextStyles.Add(new CustomTextSymbolStyleItem(symbolStyleItem, customTextItem.Value));
                lock(_lockStyleItems)
                    CustomTextStyles.Add(symbolStyleItem);
                //Add styleItem to StyleProject Item
                bool styleItemExists = await DoesStyleItemExists(symbolStyleItem.ItemType, customTextItem.Value);
                if ((styleItemExists == false) && (_styleProjectItem != null))
                    await AddStyleItemToStyle(_styleProjectItem, symbolStyleItem);
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
                if (styleProjectItem == null || cimSymbolStyleItem == null|| styleProjectItem.IsReadOnly)
                {
                    return;
                }
                try
                {
                    styleProjectItem.AddItem(cimSymbolStyleItem);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}. Unable to add {cimSymbolStyleItem.Name}. Key is {cimSymbolStyleItem.Key}");
                }
                
            });
        }

        public Task<bool> DoesStyleItemExists(StyleItemType styleItemType, string key)
        {
            var styleItemExists = QueuedTask.Run(() =>
            {

                bool styleItem = false;
                //Search for a specific point symbol in style
                SymbolStyleItem item = (SymbolStyleItem)_styleProjectItem?.LookupItem(styleItemType, key);
                if (item == null)
                    styleItem = false;
                else
                    styleItem = true;
                return styleItem;
            });
            return styleItemExists;
        }
        private static string _styleFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\AppData\Roaming\ESRI\ArcGISPro\MyCustomTextSymbols.stylx";
        private static StyleProjectItem _styleProjectItem = null;
        /// <summary>
        /// Create a styleProject item.
        /// </summary>
        /// <returns></returns>
        private static Task CreateStyleProjectItemAsync()
        {
            if (_styleProjectItem == null)
            {
                return QueuedTask.Run(() =>
                {
                    if (File.Exists(_styleFilePath)) //check if the file is on disc. Add it to the project if it is.
                        Project.Current?.AddStyle(_styleFilePath);
                    else //else create the style item                            
                        Project.Current.CreateStyle($@"{_styleFilePath}");
                });
            }
            else
            {
                var styleItemsContainer = Project.Current.GetItems<StyleProjectItem>(); //gets all Style Project Items in the current project
                _styleProjectItem = styleItemsContainer.FirstOrDefault(s => s.Name.Contains("MyCustomTextSymbols"));
                return Task.FromResult(0);
            }
        }
        private Task ApplyLabelFeatureLayerAsync()
        {          
            return QueuedTask.Run(() =>
            {
                //Get the layer's definition
                var lyrDefn = SelectedLayer.GetDefinition() as CIMFeatureLayer;
                //Get the label classes - we need the first one
                var listLabelClasses = lyrDefn.LabelClasses.ToList();
                var theLabelClass = listLabelClasses.FirstOrDefault();
                //Place all labels horizontally
                theLabelClass.StandardLabelPlacementProperties.LineLabelPosition.Horizontal = true;
                //Set the label classes' symbol to the custom text symbol
                theLabelClass.TextSymbol.Symbol = SelectedTextStyle.Symbol;
                lyrDefn.LabelClasses = listLabelClasses.ToArray(); //Set the labelClasses back
                SelectedLayer.SetDefinition(lyrDefn);

                //set the label's visiblity
                if (IsLabelVisible)
                    (SelectedLayer as FeatureLayer).SetLabelVisibility(true);
            });
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
                        ItemType = StyleItemType.TextSymbol,
                        Name = data,
                        Key = data,
                        Tags = data,
                    };
                }
                catch (Exception)
                {

                }
                return sItem;
            });
            return symbolStyleItem;
        }
        private async void ApplyLabel()
        {
            if (SelectedLayer is FeatureLayer)
                await ApplyLabelFeatureLayerAsync();               
            else if (SelectedLayer is AnnotationLayer)
                await EditAnnotationSymbolAsync();            
        }
        private Task EditAnnotationSymbolAsync()
        {
            return QueuedTask.Run(() => {

                EditOperation op = new EditOperation();
                op.Name = "Change annotation graphic";

                //At 2.1 we must use an edit operation Callback...
                op.Callback(context => {
                    RowCursor rowCursor;
                    if (SelectedLayer.GetSelection().GetCount() == 0) //The Selected layer has no selection
                        rowCursor = SelectedLayer.GetTable().Search(null, false);
                    else //Selection exists
                    {
                        var oidList = SelectedLayer.GetSelection().GetObjectIDs();
                        var oid = SelectedLayer.GetTable().GetDefinition().GetObjectIDField();
                        QueryFilter qf = new QueryFilter()
                        {
                            WhereClause = string.Format("({0} in ({1}))", oid, string.Join(",", oidList))
                        };
                        //Cursor must be non-recycling. Use the table ~not~ the layer..i.e. "GetTable().Search()"
                        //SelectedLayer is ~your~ Annotation layer
                        rowCursor = SelectedLayer.GetTable().Search(qf, false);
                    }
                    while (rowCursor.MoveNext())
                    {
                        var af = rowCursor.Current as AnnotationFeature;
                        var graphic = af.GetGraphic() as CIMTextGraphic;
                        graphic.Symbol = SelectedTextStyle.Symbol.MakeSymbolReference();
                        // update the graphic
                        af.SetGraphic(graphic);
                        // store is required
                        af.Store();
                        //refresh layer cache
                        context.Invalidate(af);
                    }

                }, SelectedLayer.GetTable());

                op.Execute();
                //set the label's visiblity
                if (IsLabelVisible)
                    (SelectedLayer as AnnotationLayer).SetVisibility(true);
            });
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class LabelGallery_ShowButton : Button
    {
        protected override void OnClick()
        {
            TextSymbolsGalleryViewModel.Show();
        }
    }
}
