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

namespace OpenItemDialogBrowseFilter
{
    ///<summary>
    ///This sample demonstrates working with browse dialog filters.  
    ///</summary>
    ///<remarks>
    ///Using the sample:
    ///1. In Visual Studio click the Build menu. Then select Build Solution.
    ///1. Click Start button to open ArcGIS Pro.
    ///1. ArcGIS Pro will open. 
    ///1. Open any project.
    ///1. Click the Dialog filters tab on the Pro ribbon.
    ///1. Notice the in-line gallery that displays a collection of Dialog filters.
    ///1. Click the gallery expander to view the entire gallery. The filters in this sample are grouped into two categories: Custom Pro browse filters and Built-in browse filters.
    ///![UI](screenshots/FilterGallery.png)
    ///1. Clicking on each filter will display Pro's OpenItemDialog set to that specific filter.
    ///1. The table below offers a brief explanation of these filters showcased in this sample.  
    ///
    ///|Browse dialog image with filter| Filter type| Description|
    ///|------------- |:-------------:| -----:|
    ///|![Polygon FGDB](screenshots/PolygonFGDB.png)    | Custom Pro BrowseProjectFilter<br/>**Polygon FGDB**: Show Polygon File GDB|
    ///|![Custom Item](screenshots/CustomItem.png)    | Custom Pro BrowseProjectFilter<br/>**Custom Item**: Show files with the "customItem" file extension. Sample file located in the [community sample data](https://github.com/Esri/arcgis-pro-sdk-community-samples#samples-data). After you extract the contents of the zip file, the file can be found in the folder C:\Data\BrowseDialogFilters|
    ///|![Composite Filter](screenshots/compositeFilter.png)    | Custom Pro Browse BrowseProjectFilter<br/>**Composite Filter**: Show Composite Filter File GDB|
    ///|![Line FGDB](screenshots/lineFGDB.png)    | Custom Pro BrowseProjectFilter<br/>**Line FGDB**: Show Line File GDB|
    ///|![DAML Filter](screenshots/DAMLFilter.png)    | Custom Pro BrowseProjectFilter<br/>**DAML Filter**: Line File GDB filter created from DAML definition files |
    ///|![Lyr and Lyrx](screenshots/LyrLyrx.png)  | Built-in Pro BrowseProjectFilter<br/>**Lyr and Lyrx**:Show Lyrx and lyr files     |
    ///|![Geodatabase](screenshots/Geodatabase.png)  | Built-in Pro BrowseProjectFilter<br/>**Geodatabase**: Show all Geodatabases|
    ///|![Custom Item](screenshots/AddToMapCustomItem.png)  | Built-in Pro BrowseProjectFilter<br/>**Custom Item**: Browse custom item in Pro's Add to map dialog|    
    ///1. Click the "Browse Filter Spy+" button choice at the bottom of the in-line gallery or on the Pro ribbon.  
    ///1. The Browse Filter Spy+ dialog allows you to choose between 3 options - Pro Browse Filters, Pro FilterFlags and ProTypeIDs.
    ///    * **Pro Browse Filters**: Using this choice you can see all the Browse Filters defined in DAML. This includes Pro's built in filters and those defined in Add-ins (config.daml). 
    ///    You can select any of the browse filters and see its DAML Definition below. The Open Filter button will display Pro's OpenItemDialog set to that specific filter. 
    ///    * **Pro FilterFlags**: Using this choice you can see the enum values for BrowseProjectFilter.FilterFlag. Additionally, when you select any filterFlag, 
    ///    you can see the typeIDs that participate in that filter flag.
    ///    * **ProTypeIDs**: This choice allows you to see all the TypeIDs defined in Pro. This includes the typeIDs for custom items created by Add-ins. 
    ///    Additionally, when you select a TypeID in the data grid, you can see the DAML definition of the type ID. This will allow you to see the filterFlags used by that typeID.
    ///![UI](screenshots/ProFilters.png)
    ///</remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;
        internal static void ToggleFiltersGalleryView() => _toggleFiltersGalleryView?.Invoke(); //To call on my button's OnClick
        private static Action _toggleFiltersGalleryView = null;
        internal static void SetToggleFiltersGalleryView(Action a)
        {
            _toggleFiltersGalleryView = a;
        }
        

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Filters_Module"));
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
