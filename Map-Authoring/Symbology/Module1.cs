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

namespace Symbology
{
    /// <summary>
    /// This sample creates custom point, line and polygon symbol items that you can use to render feature layers.  
    /// These symbol items are stored in a personal style file in your project that you can re-use.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open any project file. 
    /// 1. In Add-in tab, click the "Show Symbology pane" button.
    /// 1. In the Symbology pane, you can see the custom Point, Line and Polygon symbols available in this sample. Select
    /// each of these items in the combo box to see the available symbols.
    /// ![UI](Screenshots/SymbologyPane.png)  
    /// 1. In the project, access the Style project items available in the current project using the Catalog Pane.
    /// 1. Click the MyCustomSymbols style project item. This is a personal style file created by the sample. It comprises all the symbols you saw in the Symbology pane.
    /// ![UI](Screenshots/CustomStyleProjectItem.png)  
    /// 1. You can now use these custom symbols to render any feature layer.
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here.
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Symbology_Module"));
            }
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
