using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
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

namespace AttributeCustomDockpane
{
	/// <summary>
	/// This sample shows a custom dockpane to display attribute columns and geometry for a single feature.
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
	/// 1. Make sure that the Sample data is unzipped in c:\data 
	/// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro opens, select the FeatureTest.aprx project
	/// 1. Activate the Add-in tab on the ArcGIS Pro ribbon and click the 'Select Polygon Feature' button.
	/// 1. Navigate on the map to a polygon feature and click on anywhere on the polygon feature to inspect that feature.
	/// ![UI](Screenshots/Screenshot1.png)
	/// 1. The 'Show Attributes' dockpane displays the attributes and the geometry for the selected feature.
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("AttributeCustomDockpane_Module"));
			}
		}

		#region Static Properties

		private static Inspector _attributeInspector = null;

		internal static Inspector AttributeInspector
		{
			get { return _attributeInspector; }
			set { _attributeInspector = value; }
		}

		private static ShowAttributeViewModel _attributeViewModel = null;
		
		internal static ShowAttributeViewModel AttributeViewModel
		{
			get { return _attributeViewModel; }
			set { _attributeViewModel = value; }
		}

		#endregion Static Properties

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
