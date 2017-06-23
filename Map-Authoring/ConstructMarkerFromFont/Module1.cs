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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;

namespace ConstructMarkerFromFont
{
    /// <summary>
    /// This sample demonstrate how to create a CIMMarker from a font file and use it to render a point feature layer.
    /// The marker is then added to a personal style item in the current project.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution. 
    /// 1. This solution is using the **Extended.Wpf.Toolkit Nuget**.  If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package Extended.Wpf.Toolkit".
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open any project file that contains a point feature layer. 
    /// 1. Right click on the point feature layer to bring up the context menu.
    /// 1. Select the "Construct Marker from Fonts" menu item.
    /// 1. This will open the "Construct Marker from Fonts" dockpane.
    /// 1. Select a font, style and size and click Apply to render the point feature layer with the selected character.
    /// 1. If you check the "Add selected marker to a personal FontMarker style" option, the selected marker will be added to a FontMarker style in the project.
    /// ![UI](ScreenShots/ConstructMarkerFromFonts.png)  
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;
        private const string StateId = "construct_marker_singlePointFeatureLayerSelectedState";


        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ConstructMarkerFromFont_Module"));
            }
        }

        /// <summary>
        /// Called by Framework whenever this Module is loaded.
        /// </summary>
        /// <returns></returns>
        protected override bool Initialize()
        {
            //Subscribe to the TOC selection changed event to find out if a Point feature layer is selected. 
            //This will activate the context menu item to open the "Construct Marker from Fonts" dockpane. 
            TOCSelectionChangedEvent.Subscribe((args) =>
            {
                Construct_MarkerViewModel vm = FrameworkApplication.DockPaneManager.Find("ConstructMarkerFromFont_Construct_Marker") as Construct_MarkerViewModel;
                if (GetSelectedPointLayers(args.MapView).Count() == 1)
                {
                    //activate state
                    FrameworkApplication.State.Activate(StateId); //Activates the custom state when the context is set.

                    if (vm != null)
                    {
                        vm.DockpaneVisible = Visibility.Visible; 
                        vm.GetSelectedPointFeatureLayer();
                    }
                   
                }
                else
                {
                    FrameworkApplication.State.Deactivate(StateId);
                    if (vm != null)
                        vm.DockpaneVisible = Visibility.Collapsed;
                }                
            });
            return base.Initialize();
        }

        private IEnumerable<FeatureLayer> GetSelectedPointLayers(MapView mapview)
        {
            var layers =
                mapview.GetSelectedLayers()
                    .OfType<FeatureLayer>()
                    .Where(fl => fl.ShapeType == esriGeometryType.esriGeometryPoint);
            return layers;
        }

        #region Overrides
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

    }
}
