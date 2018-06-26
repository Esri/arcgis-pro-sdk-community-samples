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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;

namespace ProceduralSymbolLayersWithRulePackages
{
    /// <summary>
    /// This sample demonstrates rendering a polygon feature layer with a City Engine rule package. The procedural symbol layer that is created is then saved as an item in a personal style in the project.
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a map package called 'ArcGISProSampleBuildings.ppkx' which is required for this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\RulePackages" is available.
    /// 1. Open this solution in Visual Studio.  
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
    /// 1. Open the map package "ArcGISProSampleBuildings.ppkx" located in the "C:\Data\RulePackages" folder since this project contains all required data.
    /// ![UI](Screenshots/Screen1.png)
    /// 1. Click the ADD-IN tab in ArcGIS Pro.
    /// 1. Click the Procedural Symbol button. This will open the ProceduralSymbol dockpane.
    /// 1. There are 3 City Engine rule packages available in this dockpane. Select any rule package thumbnail in the gallery.
    /// ![UI](Screenshots/Screen2.png)
    /// 1. The building footprint feature layer will be rendered with this rule package.  
    /// ![UI](Screenshots/Screen3.png)
    /// 1. In your project, a BuildingStyles Style project item will be created. This will contain the procedural symbol used to render the feature layer.
    /// Note: In the TOC, you will notice that the Building Footprint layer will not have a thumbnail for the generated symbol. 
    /// You can fix this by using the Symbology Dockpane and clicking the camera button to create a Thumbnail. Click Apply to apply the image to the TOC.
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ProceduralSymbolLayersWithRulePackages_Module"));
            }
        }

        /// <summary>
        /// Feature Layer that has specific fields that are mapped to the attributes in the rule packages used in this sample.
        /// </summary>
        public static FeatureLayer BuildingFootprintLayer { get; private set; }

        protected override bool Initialize()
        {
            //subscribe to events to determine if the procedural symbol dockpane should be visible.
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            MapViewInitializedEvent.Subscribe(OnMapViewInitialized);
            MapMemberPropertiesChangedEvent.Subscribe(OnMapMemberPropertiesChanged);            
            MapClosedEvent.Subscribe(OnMapClosed);
            return base.Initialize();
        }

        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
        {
            CheckContext();
        }
        private void OnMapViewInitialized(MapViewEventArgs args)
        {
            CheckContext();
        }

        private void OnMapClosed(MapClosedEventArgs args)
        {
            CheckContext();
        }

        private void OnMapMemberPropertiesChanged(MapMemberPropertiesChangedEventArgs args)
        {
            //check only if the BuildFootprint layer's property is being changed and if any change is happening to the 3D or 2D Layers group.
            if (args.EventHints.Contains(MapMemberEventHint.SceneLayerType) && args.MapMembers.Contains(BuildingFootprintLayer))
                CheckContext();
        }

        /// <summary>
        /// Check if the Dockpane should be made visible
        /// </summary>
        private void CheckContext()
        {
            var vm = FrameworkApplication.DockPaneManager.Find("ProceduralSymbolLayersWithRulePackages_ProceduralSymbolWithRulePackages") as ProceduralSymbolWithRulePackagesViewModel;

            if (vm == null)
                return;
            
            if (MapView.Active == null || MapView.Active?.Map == null || !MapView.Active.Map.IsScene)
            {                
                vm.DockpaneVisible = Visibility.Collapsed;
                return;
            }

            BuildingFootprintLayer = MapView.Active?.Map?.GetLayersAsFlattenedList()
                .OfType<FeatureLayer>()
                .FirstOrDefault(lyr => lyr.Name == "BuildingFootprints");
           
            vm.DockpaneVisible = BuildingFootprintLayer == null || BuildingFootprintLayer.SceneLayerType != SceneLayerType.SceneLayer3D  ? Visibility.Collapsed : Visibility.Visible;

        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            MapViewInitializedEvent.Unsubscribe(OnMapViewInitialized);
            MapMemberPropertiesChangedEvent.Unsubscribe(OnMapMemberPropertiesChanged);
            MapClosedEvent.Unsubscribe(OnMapClosed);
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
