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
using System.Threading.Tasks;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Mapping.Events;

namespace SymbolLookup
{
    /// <summary>
    /// This sample demonstrate how to get the JSON representation for a symbol.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution. 
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open any project file that contains feature layers. Activate the mapview with the feature layers.
    /// 1. In the Add-In tab, click the SymbolLookup button. This will display the Symbol Lookup dockpane.
    /// 1. The Select Features button in this dockpane is Pro's Select By Rectangle button. Click this button to activate the select features tool and select some features.
    /// 1. This will populate the listbox in the dockpane listing the selected features, the symbol for the features and the first OID of each features.
    /// 1. Notice the preview symbol and the JSON representation of the selected symbol.
    /// ![UI](screenshots/symbollookup.png)  
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;
        private const string _dockPaneID = "SymbolLookup_SymbolLookupDockpane";
        public bool IsToolSelection
        {
            get;set;
        }
        private static SymbolLookupDockpaneViewModel _dockPane;
        internal static SymbolLookupDockpaneViewModel SymbolLookupVM
        {
            get
            {
                if (_dockPane == null)
                {
                    _dockPane = FrameworkApplication.DockPaneManager.Find(_dockPaneID) as SymbolLookupDockpaneViewModel;
                }
                return _dockPane;
            }
        }
      /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("SymbolLookup_Module"));
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
