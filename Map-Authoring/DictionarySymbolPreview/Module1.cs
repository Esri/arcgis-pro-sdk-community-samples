/*

   Copyright 2016 Esri

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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace DictionarySymbolPreview {

    /// <summary>
    /// Preview the Mil2525d symbol set that ships with ArcGIS Pro. Includes the 
    /// entities, modifiers, and coded domains exported from the <a href="https://github.com/Esri/joint-military-symbology-xml">JMSML</a> 
    /// (joint military symbology library) schema. These values are used to configure the symbol preview sample.
    /// </summary>
    /// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a dataset called 'MilitaryOverlay' with sample data for use by this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\MilitaryOverlay" is available.
    /// 1. Open this solution in Visual Studio 2015.  
    /// 1. Click the build menu and select Build Solution.
    /// 1. This solution is using the **Extended.Wpf.Toolkit Nuget**.  If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package Extended.Wpf.Toolkit".
    /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
    /// 1. Open the project "MilitaryOverlay.aprx" in the "C:\Data\MilitaryOverlay" folder since this project some military symbolgy sample data.
    /// 1. You can also add layers using the feature classes in the <i>MilitaryOverlay.gdb</i> that is distributed with the SDK sample data.
    /// 1. Change the 'Appearance'->'Symbology' to "Dictionary" if it is not automatically set.
    /// 1. Click on the ADD-IN tab
    /// 1. Click on the "Show Mil2525d Symbol Preview" button.
    /// 1. The Mil2525d Symbol Preview dockpane opens.
    /// 1. Pick from the available symbol sets and change symbol attributes to preview different Mil2525d symbols
    /// ![UI](Screenshots/Screen1.png)
    /// 1. The code window at the bottom shows the CSharp code you would need to make the corresponding CIMSymbol in your code.
    /// ![UI](Screenshots/Screen2.png)
    /// 1. Use the selection tool to select a feature from any of the layers.
    /// 1. The selected feature's attributes will be used to configure the Symbol Preview.
    /// ![UI](Screenshots/Screen3.png) 
    /// 
    /// TODO:
    /// o Support 256x256 Preview area for the symbol
    /// o Support editing the selected feature attributes with the Symbol Preview settings
    /// 
    /// <para><b>Note:</b></para>
    /// The JMSML schema and associated tools to generate the Mil2525d data can be found online
    /// <a href="https://github.com/Esri/joint-military-symbology-xml">here</a> on github. 
    /// The sample can be used against any ESRI military features data set for the MIL2525d specification. Sample 
    /// data is included with the Pro samples but other example data can be found 
    /// <a href="https://github.com/Esri/military-features-data">here</a>.
    /// 
    /// </remarks>
    internal class Module1 : Module {
        private static Module1 _this = null;
        private SymbolPreviewViewModel _symbolPreview = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("DictionarySymbolPreview_Module"));
            }
        }

        /// <summary>
        /// When overridden in a derived class, gives the custom Module a chance to initialize itself and return its status to the calling Framework.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A custom Module has two opportunities to initialize itself: its class constructor and its Initialize method. The Framework calls both 
        ///             functions whenever a Module is loaded. Modules load either explicitly with <c>FrameworkApplication.FindModule</c> or implicitly whenever
        ///             any of their DAML elements (Panes, DockPanes, Controls, etc) are loaded. For example, when a DockPane or a Button on a Ribbon Tab is created, 
        ///             their parent module will automatically load if it hasn't already done so.
        /// </para>
        /// <para>
        /// The Initialize method has the added benefit of returning whether the initialization was successful
        ///             or not. If initialization fails, the Framework immediately calls <c>Uninitialize</c>.
        /// </para>
        /// </remarks>
        /// <returns/>
        protected override bool Initialize() {
            //Listen for feature selected events
            ArcGIS.Desktop.Mapping.Events.MapSelectionChangedEvent.Subscribe(async (mse) => {
                foreach (var kvp in mse.Selection) {
                    if ((kvp.Key as BasicFeatureLayer) == null)
                        continue;
                    //Is a feature selected? Is it a Mil2525 feature?
                    if (kvp.Value.Count > 0 && await HasMil2525Attributes((BasicFeatureLayer) kvp.Key)) {
                        if (_symbolPreview == null) {
                            _symbolPreview = FrameworkApplication.DockPaneManager.Find(
                                SymbolPreviewViewModel._dockPaneID) as SymbolPreviewViewModel;
                        }
                        //Set the first feature as the selected feature
                        if (_symbolPreview.IsVisible) {
                            _symbolPreview.SetSelected((BasicFeatureLayer)kvp.Key, kvp.Value[0]);
                        }
                    }
                }
            });
            return true;
        }
        /// <summary>
        /// Check if the given layer supports the Mil2525d spec.
        /// </summary>
        /// <remarks>Currently checks for symbolset</remarks>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static Task<bool> HasMil2525Attributes(BasicFeatureLayer layer) {
            return QueuedTask.Run(() => {
                var fdescs = layer.GetFieldDescriptions();
                foreach (var fdesc in fdescs) {
                    if (fdesc.Name.ToLower() == "symbolset")
                        return true;
                }
                return false;
            });
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
