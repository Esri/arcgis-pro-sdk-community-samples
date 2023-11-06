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
using ArcGIS.Desktop.Internal.Core;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenItemDialogBrowseFilter
{
    public static class DialogBrowseFilters
    {
        public static void NewFilterPolygonFileGDB()
        {
            //Create and use a new filter to view Polygon feature classes in a file GDB.
            //The browse filter is used in an OpenItemDialog.

            BrowseProjectFilter bf = new BrowseProjectFilter
            {
                //Name the filter
                Name = "Polygon feature class in FGDB"
            };
            //Add typeID for Polygon feature class
            bf.AddCanBeTypeId("fgdb_fc_polygon");
            //Allow only File GDBs
            bf.AddDontBrowseIntoFlag(BrowseProjectFilter.FilterFlag.DontBrowseFiles);
            bf.AddDoBrowseIntoTypeId("database_fgdb");
            //Display only folders and GDB in the browse dialog
            bf.Includes.Add("FolderConnection");
            bf.Includes.Add("GDB");
            //Does not display Online places in the browse dialog
            bf.Excludes.Add("esri_browsePlaces_Online");

            //Display the filter in an Open Item dialog
            OpenItemDialog aNewFilter = new OpenItemDialog
            {
                Title = "Open Polygon Feature classes",
                InitialLocation = @"C:\Data",
                MultiSelect = false,
                BrowseFilter = bf               
            };
            bool? ok = aNewFilter.ShowDialog();
        }

        public static void NewFilterForCustomItem()
        {

            //Create and use a dialog filter to view a Custom item
            //The browse filter is used in an OpenItemDialog.

            BrowseProjectFilter bf = new BrowseProjectFilter();
            //Name the filter
            bf.Name = "\"customItem\" files";
            //Add typeID for Filters_ProGPXItem custom item
            bf.AddCanBeTypeId("Filters_ProGPXItem");
            //Does not allow browsing into files
            bf.AddDontBrowseIntoFlag(BrowseProjectFilter.FilterFlag.DontBrowseFiles);
            //Display only folders and GDB in the browse dialog
            bf.Includes.Add("FolderConnection");
            //Does not display Online places in the browse dialog
            bf.Excludes.Add("esri_browsePlaces_Online");

            //Display the filter in an Open Item dialog
            OpenItemDialog aNewFilter = new OpenItemDialog
            {
                Title = "Open \"customItem\"",
                InitialLocation = @"C:\Data",
                MultiSelect = false,
                BrowseFilter = bf
            };
            bool? ok = aNewFilter.ShowDialog();

        }
        public static void NewFilterCompositeFilter()
        {

            //Create and use a Composite Filter that displays "lyr" or "lyrx" files.
            //The composite filter is used in an OpenItemDialog.
            BrowseProjectFilter compositeFilter = new BrowseProjectFilter();
            compositeFilter.AddFilter(BrowseProjectFilter.GetFilter("esri_browseDialogFilters_layers_lyr"));
            compositeFilter.AddFilter(BrowseProjectFilter.GetFilter("esri_browseDialogFilters_layers_lyrx"));
            compositeFilter.Includes.Add("FolderConnection");
            compositeFilter.Excludes.Add("esri_browsePlaces_Online");
            //Display the filter in an Open Item dialog
            OpenItemDialog op = new OpenItemDialog
            {
                Title = "Open LYR or LYRX files",
                InitialLocation = @"C:\Data",
                MultiSelect = false,
                BrowseFilter = compositeFilter
            };
            bool? ok = op.ShowDialog();
        }

        public static void NewFilterFromDAMLDeclaration()
        {
            //Create and use a filter defined in DAML. The filter displays line feature classes in a file GDB.
            //The browse filter is used in an OpenItemDialog.

            var bf = new BrowseProjectFilter("NewLineFeatures_Filter");
            //Display the filter in an Open Item dialog
            OpenItemDialog op = new OpenItemDialog
            {
                Title = "Open Line Feature classes",
                InitialLocation = @"C:\Data",
                MultiSelect = false,
                BrowseFilter = bf
            };
            bool? ok = op.ShowDialog();
        }

        public static void ModifyExistingProLyrxFilter()
        {
            //Creates a new browse filter from Pro's "esri_browseDialogFilters_layers_lyrx" browse filter to display lyr files also.
            //The browse filter is used in an OpenItemDialog.
            BrowseProjectFilter lyrXLyrGeneral = new BrowseProjectFilter("esri_browseDialogFilters_layers_lyrx");
            lyrXLyrGeneral.Name = "Layer Files (LYRX) and Layer Files (LYR)";
            lyrXLyrGeneral.AddCanBeTypeId("layer_general");
            lyrXLyrGeneral.AddDontBrowseIntoFlag(BrowseProjectFilter.FilterFlag.DontBrowseFiles);
            //Display only folders and GDB in the browse dialog
            lyrXLyrGeneral.Includes.Add("FolderConnection");
            lyrXLyrGeneral.Includes.Add("GDB");
            //Does not display Online places in the browse dialog
            lyrXLyrGeneral.Excludes.Add("esri_browsePlaces_Online");
            //Display the filter in an Open Item dialog
            OpenItemDialog aNewFilter = new OpenItemDialog
            {
                Title = "Open LyrX and Lyr General files",
                InitialLocation = @"C:\Data",
                MultiSelect = false,
                BrowseFilter = lyrXLyrGeneral
            };
            bool? ok = aNewFilter.ShowDialog();
        }

        public static void UseProFilterGeodatabases()
        {
            //Create a browse filter that uses Pro's "esri_browseDialogFilters_geodatabases" filter.
            //The browse filter is used in an OpenItemDialog.
            BrowseProjectFilter bf = new BrowseProjectFilter("esri_browseDialogFilters_geodatabases");
            //Display the filter in an Open Item dialog
            OpenItemDialog aNewFilter = new OpenItemDialog
            {
                Title = "Open Geodatabases",
                InitialLocation = @"C:\Data",
                MultiSelect = false,
                //Set the BrowseFilter property to Pro's Geodatabase filter.
                BrowseFilter = bf
            };
            bool? ok = aNewFilter.ShowDialog();
        }

        public static void ModifyAddMapToDisplayCustomItem()
        {
            IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("esri_mapping_addDataButton");
            var command = wrapper as ICommand; 

            if ((command != null) && command.CanExecute(null))
                command.Execute(null);
        }
        

        public static void DisplayOpenItemDialog(BrowseProjectFilter browseProjectFilter, string dialogName)
        {
            OpenItemDialog aNewFilter = new OpenItemDialog
            {
                Title = dialogName,
                InitialLocation = @"C:\Data",
                MultiSelect = false,
                //Use BrowseFilter property to specify the filter 
                //you want the dialog to display
                BrowseFilter = browseProjectFilter
            };
            bool? ok = aNewFilter.ShowDialog();
        }
    }
}
