/*

   Copyright 2017 Esri

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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;

namespace SymbolLookup
{
    internal class SymbolLookupDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "SymbolLookup_SymbolLookupDockpane";
        private static readonly object _lock = new object();
        protected SymbolLookupDockpaneViewModel() {
            BindingOperations.EnableCollectionSynchronization(_allFeatureItems, _lock);
            _copySymbolJSONCmd = new RelayCommand(() => CopyJSONToClipboard(JSONText));
         
        }

        protected override async Task InitializeAsync()
        {
            //Subscribe to MapSelectionChangedEvent
            MapSelectionChangedEvent.Subscribe(OnMapSelectionChangedEvent);
            //Subscribe to ActiveMapViewChangedEvent 
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChangedEvent);
            await UpdateSymbolList();           

            return;
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
        private string _heading = "Symbol Lookup";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        private ObservableCollection<FeatureItem> _allFeatureItems = new ObservableCollection<FeatureItem>();
        /// <summary>
        /// Represents the Symbol list collection
        /// </summary>
        public ObservableCollection<FeatureItem> FeatureItems
        {
            get { return _allFeatureItems; }
           
        }

        private Visibility _dockpaneVisibility;
        /// <summary>
        /// Controls the visible state of the controls on the dockpane
        /// </summary>
        public Visibility DockpaneVisibility
        {
            get { return _dockpaneVisibility; }
            set { SetProperty(ref _dockpaneVisibility, value, () => DockpaneVisibility); }
        }

        private FeatureItem _selectedFeatureItem;
        /// <summary>
        /// Represents the selected feature item.
        /// </summary>
        public FeatureItem SelectedFeatureItem
        {
            get { return _selectedFeatureItem; }
            set {                
                SetProperty(ref _selectedFeatureItem, value, () => SelectedFeatureItem);
                //Control property settings
                if (SelectedFeatureItem == null)
                {
                    JSONText = string.Empty;
                    SymbolImageSource = null;
                    LayerName = string.Empty;
                    DockpaneVisibility = Visibility.Collapsed;
                    return;
                }
                JSONText = SelectedFeatureItem?.MapCIMSymbol.ToJson();
                SymbolImageSource = SelectedFeatureItem?.SymbolImageSource;
                LayerName = SelectedFeatureItem?.LayerName;
                DockpaneVisibility = Visibility.Visible;
            }
        }
        private ImageSource _img;
        /// <summary>
        /// Represents the selected feature item's symbol source
        /// </summary>
        public ImageSource SymbolImageSource
        {
            get { return _img; }
            set { SetProperty(ref _img, value, () => SymbolImageSource); }

        }

        private string _jsonText;
        /// <summary>
        /// Represents the selected feature item's symbol's JSON
        /// </summary>
        public string JSONText
        {
            get { return _jsonText; }
            set { SetProperty(ref _jsonText, value, () => JSONText); }
        }
        
        private ICommand _selectFeaturesCmd;
        /// <summary>
        /// The button command for the Selected features tool
        /// </summary>
        public ICommand SelectFeaturesCommand
        {
            get
            {
                return _selectFeaturesCmd ?? (_selectFeaturesCmd = new RelayCommand(() => {
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_selectByRectangleTool"); //Pro's select by rectangle tool
                }));
            }
        }
        private RelayCommand _copySymbolJSONCmd;
        /// <summary>
        /// Represents the Copy to Clipboard command
        /// </summary>
        public ICommand CopySymbolJSONCmd
        {
            get { return _copySymbolJSONCmd; }
        }

        
        private string _layerName;
        /// <summary>
        /// Represent's the selected feature item's layer name.
        /// </summary>
        public string LayerName
        {
            get { return _layerName; }
            set
            {
                SetProperty(ref _layerName, value, () => LayerName);
            }
        }
        #endregion
        /// <summary>
        /// Represents the Copy to Clipboard method
        /// </summary>
        public static void CopyJSONToClipboard(string clipBoardString)
        {
            Clipboard.SetText(clipBoardString);
        }
        /// <summary>
        /// Event handler when the active MapView changes
        /// </summary>
        /// <param name="obj"></param>
        private void OnActiveMapViewChangedEvent(ActiveMapViewChangedEventArgs obj)
        {
            if (obj.IncomingView == null)
            {
                FeatureItems.Clear();
                return;
            }
            //Clear the list first
            if (FeatureItems?.Count > 0)
            {
                FeatureItems.Clear();
            }
                
            UpdateSymbolList();
        }
        /// <summary>
        /// Event handler when the selected features changes
        /// </summary>
        /// <param name="obj"></param>

        private void OnMapSelectionChangedEvent(MapSelectionChangedEventArgs obj)
        {
            //Clear the list first
            if (FeatureItems?.Count > 0)
            {
                FeatureItems.Clear();
            }
            UpdateSymbolList();
        }
        /// <summary>
        /// Updates the listbox with the feature/Symbol list
        /// </summary>
        /// <returns></returns>
		internal Task UpdateSymbolList()
		{

			if (MapView.Active?.Map == null)
			{
				return Task.FromResult(0);
			}
			return QueuedTask.Run(() =>
			{
				var activeMapView = MapView.Active;
				if (activeMapView == null)
					return;
				var features = activeMapView.Map.GetSelection().ToDictionary();
				foreach (var kvp in features)
				{
					if (kvp.Key is FeatureLayer)
					{
						var lyr = kvp.Key as FeatureLayer;
						if (lyr.CanLookupSymbol())
						{
							var symbol = lyr.LookupSymbol(kvp.Value[0], activeMapView);
              if (symbol == null) continue;
							lock (_lock)
								FeatureItems.Add(new FeatureItem(lyr, kvp.Value[0], symbol));
						}
					}
				}
				
				var defSelectionAction = (Action)(() =>
				{
					SelectedFeatureItem = FeatureItems.Count > 0 ? FeatureItems[0] : null;
				});
				ActionOnGuiThread(defSelectionAction);
			
			});
        }
        /// <summary>
        /// We have to ensure that GUI updates are only done from the GUI thread.
        /// </summary>
		private void ActionOnGuiThread(Action theAction)
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
   
   

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class SymbolLookupDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            SymbolLookupDockpaneViewModel.Show();
        }
    }
}
