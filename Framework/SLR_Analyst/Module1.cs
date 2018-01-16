/*

   Copyright 2018 Esri

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

namespace SLR_Analyst
{
	/// <summary>
	/// This demonstration illustrates custom ArcGIS Pro map exploration functionality provided through a Pro SDK add-in.  The scenario uses a sample Pro add-in, Sea Level Rise Analyst, which can assist urban planning and public safety organizations in quickly identifying areas affected by sea level rise within a study area in Miami Beach, Florida.  
	/// The general concept of the add-in is based on the [NOAA Sea Level Rise Viewer](https://coast.noaa.gov/slr/) web application.	The add-in and dataset allow for identification and selection from three layers – land use, parcels and streets.  
	/// The custom add-in demonstrates three main Pro SDK capabilities:
	/// -	Interaction with and selection of layers within the project from a custom add-in pane.
	/// -	Dynamic creation of a temporary report pane with results in text and chart form, and charts which leverage a custom tooltip which allows for better data exploration.
	/// -	Basic text reporting of code attributes within a selection set.  
	/// 
	/// DATA SOURCES AND DESCRIPTION (Refer to Data Distribution Permission statement at the end of this document)
	/// The dataset is comprised of data clipped for the study area from the following sources:
	/// - Land use, Parcel and Street data layers provided by [Miami-Dade County GIS Open Data site](http://gis-mdc.opendata.arcgis.com/)
	/// - Sea Level Rise layers for 1 – 6 feet provided by [NOAA SLR data site](https://coast.noaa.gov/slrdata/)
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
	/// 1. Make sure that the Sample data is unzipped in c:\data 
	/// 1. The project used for this sample is 'C:\Data\SLR_Analyst\SLR_Analyst_Data.ppkx'
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. This solution is using the **System.Windows.Controls.DataVisualization.Toolkit Nuget**.  If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package System.Windows.Controls.DataVisualization.Toolkit".
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open, open the SLR_Analyst_Data.ppkx project
	/// 1. Once the project is open and loaded, the first step is to open the custom SLR Analyst pane. To do this, select the Add-In tab and press the Show SLR Tools add-in button to open the SLR Analyst dockpane.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Use your choice of navigation to zoom to an extent within the study area where you can clearly see land use and parcel polygons.  Zooming to 1:6,000 scale should work well initially.
	/// 1. Choose layer(s) to select and then choose sea level rise via the slider 
	/// 1. Click 'Run Selection' to run the analysis
	/// ![UI](Screenshots/Screen2.png)
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("SLR_Analyst_Module"));
			}
		}

		#region Static Module functions

		public static SLR_PaneViewModel CreateNewSlrPane (string paneTitle, 
									string paneConfiguration, string reportText,
									bool ckbLandUseChecked, bool ckbParcelChecked, bool ckbStreetChecked, 
									IList<KeyValueWithTooltip> luKv, 
									IList<KeyValueWithTooltip> pKv, 
									IList<KeyValueWithTooltip> sKv)
		{
			var slrPaneViewModule = SLR_PaneViewModel.Create();
			if (slrPaneViewModule != null)
			{
				slrPaneViewModule.PaneTitle = paneTitle;
				slrPaneViewModule.PaneConfiguration = paneConfiguration;
				slrPaneViewModule.UpdateLandUse(luKv);
				slrPaneViewModule.UpdateParcel(pKv);
				slrPaneViewModule.UpdateStreet(sKv);
				slrPaneViewModule.CkbLandUseChecked = ckbLandUseChecked;
				slrPaneViewModule.CkbParcelChecked = ckbParcelChecked;
				slrPaneViewModule.CkbStreetChecked = ckbStreetChecked;
				slrPaneViewModule.ReportText = reportText;
				slrPaneViewModule.UpdateVisibility();
			}
			return slrPaneViewModule;
		}

		#endregion


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
