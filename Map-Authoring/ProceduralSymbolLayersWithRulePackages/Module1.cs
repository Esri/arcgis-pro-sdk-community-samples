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
    /// 1. This sample requires the Building Footprint layer. 
    /// 1. Download the [ArcGISProSampleBuildings.ppkx Project package from ArcGIS Online](http://www.arcgis.com/sharing/rest/content/items/a0aa60303a39476688b599a6ce842afb/data).
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. This solution is using the Newtonsoft.Json Nuget. If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package Newtonsoft.Json".
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. Open the downloaded ArcGISProSampleBuildings.ppkx Project package.
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
