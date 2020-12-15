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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace ParcelFabricAPI
{
  /// <summary>
  /// ParcelFabricAPI shows the following Parcel Fabric API capabilities:
  /// 1. Create a Record – a new Record feature in the Records feature class
  /// 1. Setting the created Record as the active record
  /// 1. Copy lines from an existing import feature class selection of non-fabric features into a parcel type called "Tax" in a parcel fabric. Parcel seeds are created automatically.
  /// 1. Build the active record
  /// 1. Copy lines from an existing selection of "Tax" parcels into a parcel type called “Lot” in the same parcel fabric. Parcel seeds are created automatically.
  /// 1. Build the active record
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data). The sample data contains an ArcGIS Pro project and data to be used for this sample. Make sure that the Sample data is unzipped in c:\data and c:\Data\ParcelFabric is available.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. You can run the add-in using the debugger, but to see its full functionality you should run the add-in wihtout the debugger first since some of the functionality like Progress Dialogs are not supported when running ArcGIS Pro from the debugger.
  /// 1. Open the project 'c:\Data\ParcelFabric\Island\ParcelIsland.aprx'.  
  /// 1. Select the 'Parcels' tab, and then click on 'Import Plat' to bring up the 'Import Plat' dockpane.
  /// ![Parcel Fabric Dockpane](Screenshots/Screenshot1.png)  
  /// 1. Use the 'Import Plat' dockpane to select the zone, section, and plat to import:
  /// ![Import Selection](Screenshots/Screenshot2.png)  
  /// 1. Click the 'Import Plat to Tax' button to start the import process.
  /// ![Start Import Process](Screenshots/Screenshot3.png)  
  /// 1. After the import completed you can identify some of the Tax records that were created by the import.
  /// ![Import Process](Screenshots/Screenshot4.png)  
  /// 1. Click the 'Copy Tax to Lot' button in order to copy the imported Tax records into 'Lot' parcels. 
  /// 1. After the copy completed you can identify some of the Lot records that were created by the import.
  /// ![Import Process](Screenshots/Screenshot5.png)
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ParcelLayerAPI_Module"));
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
