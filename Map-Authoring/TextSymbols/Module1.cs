/*

   Copyright 2019 Esri

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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Mapping;
using System.Windows;

namespace TextSymbols
{
    /// <summary>
    /// This sample creates custom text symbol items that you can use to label feature layers and Annotation layers.  
    /// These text symbol items are stored in a personal style file in your project that you can re-use. 
    /// Community Sample data (see under the "Resources" section for downloading sample data) has a TextSymbols.ppkx 
    /// project package that contains Annotation and Feature layers that can be used for this sample. 
    /// This project package is can be found under C:\Data\TextSymbols folder. Alternatively,
    /// you can also use any Annotation and Feature layers to work with this sample.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open C:\Data\TextSymbols\TextSymbols.ppkx or any project file that has a map with feature layers and/or Annotation layers. 
    /// 1. Activate the map.
    /// 1. In the Add-in tab, click the "Text Symbols Gallery" button.
    /// 1. In the Text Symbols Gallery dockpane, you can see the custom text symbols available in this sample. 
    /// ![UI](screenshots/LabelGalery.png) 
    /// 1. Select a text symbol.
    /// 1. Select the Layer you want to label from the collection of feature layers in the combo box.
    /// 1. Click the Label button to apply the lables. 
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Labeling_Module"));
            }
        }

        #region Overrides
        protected override bool Initialize()
        {
            //Subscribe to event to see if the Label gallery context is set
            ProjectOpenedEvent.Subscribe(OnProjectOpened);
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            DrawCompleteEvent.Subscribe(OnDrawComplete);
            LayersAddedEvent.Subscribe(OnLayersAdded);
            return base.Initialize();
        }

        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
        {
            CheckContext();
        }

        private void OnLayersAdded(LayerEventsArgs obj)
        {
            CheckContext();
        }

        private  void OnDrawComplete(MapViewEventArgs obj)
        {
            CheckContext();
        }

        private void OnProjectOpened(ProjectEventArgs obj)
        {
            CheckContext();
        }

        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides
        private void CheckContext()
        {
            var vm = FrameworkApplication.DockPaneManager.Find("Labeling_LabelGallery") as TextSymbolsGalleryViewModel;

            if (vm == null)
                return;

            if (MapView.Active == null || MapView.Active?.Map == null || MapView.Active?.Map.IsScene == null)
            {
                vm.DockpaneVisible = Visibility.Collapsed;
                return;
            }

            if (MapView.Active?.Map?.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any() != true)
            {
                vm.DockpaneVisible = Visibility.Collapsed;
                return;
            }
            vm.DockpaneVisible = Visibility.Visible;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            vm.InitAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
