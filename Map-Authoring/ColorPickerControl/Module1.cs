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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ColorPickerControl
{
	/// <summary>
	/// This sample shows how to use the Pro API's ColorPicker Control from a Dockpane.
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
	/// 1. Make sure that the Sample data is unzipped in c:\data
	/// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
	/// 1. In Visual Studio click the Build menu.Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open, select the FeatureTest.aprx project
	/// 1. The first polygon feature layer in your project's TOC is used to demonstrate the ColorPicker Control.
	/// 1. In Add-in tab, click the "Show Polygon ColorPicker" button.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. If the selected polygon feature has a simple renderer the ColorPicker control shows the fill color for the simple renderer.
	/// 1. Click on the ColorPicker Control and select a different color
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Click on 'Change Fill Color' button to apply the color to the simple polygon renderer.
	/// ![UI](Screenshots/Screen3.png)
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ColorPickerControl_Module"));
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
