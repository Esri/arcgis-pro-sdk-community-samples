/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Mapping.Events;
using System.Windows.Threading;
using ArcGIS.Core.Events;

namespace WorkingWithQueryDefinitionFilters
{
    /// <summary>
    /// This sample demonstrates working with Definition Query filters.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro. 
    /// 1. ArcGIS Pro will open. 
    /// 1. Open any project that contains a Map or Local Scene with feature layers and/or standalone tables.
    /// 1. Right click on any feature layer or standalone table. Select the Definition Query Filters button on the context menu.
    /// ![UI](screenshots/contextMenu.png)
    /// 1. The Definition Query Filters dockpane will open up.
    /// ![UI](screenshots/DefinitionFiltersDockpane.png)
    /// 1. If the selected feature layer has Definition queries, you can see them listed in this dockpane.
    /// 1. You can create a  new definition query for the layer, activate a different definition query or delete an existing query.
    /// 1. If the selected feature layer does not have any definition queries, you will be able to create a new Definition query.
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;
        public DefineQueryDefinitionFiltersViewModel DefFilterVM;
        private const string _dockPaneID = "WorkingWithQueryDefinitionFilters_DefineQueryDefinitionFilters";
        public SubscriptionToken _mapMembePropChangedEventtoken;
        internal SubscriptionToken _layersAddedEventoken;
        internal SubscriptionToken _layersRemovedEventoken;
        internal SubscriptionToken _tablesAddedEventoken;
        internal SubscriptionToken _tablesRemovedEventoken;
        public bool ActiveFilterExists = false;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("WorkingWithQueryDefinitionFilters_Module"));
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
            //ArcGIS.Desktop.Mapping.Events.MapMemberPropertiesChangedEvent.Unsubscribe(DefFilterVM.OnMapMemberPropertiesChanged);
            return true;
        }

        protected override bool Initialize()
        { 
            return base.Initialize();
        }

        protected override void Uninitialize()
        {
            ArcGIS.Desktop.Mapping.Events.MapMemberPropertiesChangedEvent.Unsubscribe(DefFilterVM.OnMapMemberPropertiesChanged);
            ArcGIS.Desktop.Mapping.Events.LayersAddedEvent.Unsubscribe(DefFilterVM.OnLayersAdded);
            return;
        }

        #endregion Overrides
        
        /// <summary>
        /// We have to ensure that GUI updates are only done from the GUI thread.
        /// </summary>
		public void ActionOnGuiThread(Action theAction)
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
}

