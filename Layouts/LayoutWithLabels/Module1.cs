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

namespace LayoutWithLabels
{
		/// <summary>
		/// This sample shows how to create a layout with custom labeling for a selected set of polygon features.  In order to do this the sample is using the following functionality:
		/// * A layer file containing a 'labeled' polygon layer is loaded by the add-in and dynamically added to the map
		/// * The add-in then reads a csv file containing some Ids of polygon features that have to be labeled with 'customized' text strings
		/// * The labels (using call-out labels) are placed on the map (using the dynamically loaded 'labeled' polygon layer
		/// * A control can then be used to re-position the labels on the map
		/// * The add-in adds a layout that contains the labeled map and a tabular list of all labeled features.
		/// </summary>
		/// <remarks>
		/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
		/// 1. Make sure that the Sample data is unzipped in c:\data 
		/// 1. The project used for this sample is 'C:\Data\LocalGovernment\LocalGovernment.aprx'
		/// 1. In Visual Studio click the Build menu. Then select Build Solution.
		/// 1. Launch the debugger to open ArcGIS Pro.
		/// 1. ArcGIS Pro will open, select the LocalGovernment.aprx project
		/// 1. Click on the Add-In tab and the click the 'Label Selected Parcels' button.
		/// ![UI](Screenshots/Screenshot1.png)
		/// 1. Click the "load Layer Package" button on the "SelectParcelsToLabel" dockpane.  This loads a layer file  containing a 'labeled' polygon layer and dynamically adds it to the map
		/// ![UI](Screenshots/Screenshot2.png)
		/// 1. Now that the "Read from File" button is enabled, click the "Read from File" button in order to load a sample csv file that contains records that are identified by using a unique parcel Id field.
		/// 1. The loaded features are displayed in a table on the "SelectParcelsToLabel" dockpane and highlighted and labeled on the map using the 'labeled' polygon layer.
		/// ![UI](Screenshots/Screenshot3.png)
		/// 1. You can use the "Label leader line length" control on the "SelectParcelsToLabel" dockpane to manipulate the leader line length.
		/// ![UI](Screenshots/Screenshot4.png)
		/// 1. Finally to generate the layout select a 'Map Layout" and click the "Make Layout" button.
		/// ![UI](Screenshots/Screenshot5.png)
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
								return _this ?? (_this = (Module1)FrameworkApplication.FindModule("LayoutWithLabels_Module"));
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
