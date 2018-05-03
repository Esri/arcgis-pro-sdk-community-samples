using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace ContentFileExplorer
{
	/// <summary>
	/// This sample illustrates how to search for shapefiles and geodatabases in the file system and then add any of those discovered layer sources to the current map view.
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
	/// 1. Make sure that the Sample data is unzipped in c:\data 
	/// 1. In Visual studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open, select any project and open a map view.
	/// 1. Click the 'File Explorer' button on the Add-In tab to open the 'File Explorer' dock pane.
	/// 1. Click the 'Refresh' button on the 'File Explorer' dock pane to refresh the content of the file explorer tree view.  Drill down through the tree view and click on any layer source to add the data as a new layer to the current map view.
	/// ![UI](Screenshots/Screen1.png)
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ContentFileExplorer_Module"));
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
