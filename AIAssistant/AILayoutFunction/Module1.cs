/*

   Copyright 2026 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace AILayoutFunction
{
  /// <summary>
  /// Demonstrates how to add custom behavior to the ArcGIS Pro Assistant using the AIAssistantExtension and AIAssistantFunction attributes.
  /// </summary>
  /// <remarks>
  /// 1. This sample is based on the instructions found at: https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Create-an-AI-Assistant-Function  
  /// 2. (Optional) Download the Community Sample Data (see the 'Resources' section for instructions). This data can be used to test the add-in with prepackaged map documents.  
  /// 3. In Visual Studio, click the **Build** menu and select **Build Solution**.  
  /// 4. Launch the debugger to open ArcGIS Pro.  
  /// 5. Use an existing project or create a new one in ArcGIS Pro with some content.  
  /// 6. Click the **Help** tab. You should see the **AI Assistant** button.  
  ///      ![UI](screenshots/AIAssistantButton.png)
  /// 7. Click the button to open the AI Assistant pane, where you can interact with the assistant.  
  /// 8. Type "Create a layout using the current map" in the AI Assistant pane and press **Enter** (or click the arrow).  
  /// 9. You can also try: "Create a layout for the current map with a legend, a scale bar, a north arrow, and title it 'Enhanced layout'", then press **Enter** (or click the arrow).  
  /// 10. Experiment with different combinations of parameters to confirm the functionality works as intended.
  /// </remarks>

  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("AILayoutFunction_Module");

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
