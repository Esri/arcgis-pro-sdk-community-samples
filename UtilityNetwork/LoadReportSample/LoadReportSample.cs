//   Copyright 2018 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace LoadReportSample
{
	/// <summary>
	/// This add-in demonstrates the creation of a simple electric distribution report.  It traces downstream from a given point and adds up the count of customers and total load per phase.  This sample is meant to be a demonstration on how to use the Utility Network portions of the SDK.  The report display is rudimentary.  Look elsewhere in the SDK for better examples on how to display data.  Rather than coding special logic to pick a starting point, this sample leverages the existing Set Trace Locations tool that is included with the product.  That tool writes rows to a table called UN_Temp_Starting_Points, which is stored in the default project workspace.  This sample reads rows from that table and uses them as starting points for our downstream trace.
	/// 
	/// Utility network SDK samples require a utility network service to run.  For the Load Report sample, you will need to do the following: 
	/// * Configure a utility network database and service using the ArcGIS for Electric data model.  Instructions for setting up and configuring this data are located on the [ArcGIS for Electric website](http://solutions.arcgis.com/electric/help/electric-utility-network-configuration/). 
	/// * Run the ConfigureLoadReportData.py python script to complete the configuration for this sample.
	/// Of course, the constants inside the source files can be changed to accomodate different data models. 
	/// </summary>
	/// <remarks>
	/// 1. In Visual Studio click the Build menu.  Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open.
	/// 1. Open a map view that contains at least one Feature Layer whose source points to a Feature Class that participates in a utility network.
	/// 1. Select a feature layer or subtype group layer that participates in a utility network or a utility network layer
	/// 1. Click on the SDK Samples tab on the Utility Network tab group
	/// 1. Click on the Starting Points tool to create a starting point on the map
	/// 1. Click on the Create Load Report tool
	/// </remarks>
	internal class LoadReportSample : Module
	{
		private static LoadReportSample _this = null;


		public static LoadReportSample Current
		{
			get
			{
				return _this ?? (_this = (LoadReportSample)FrameworkApplication.FindModule("LoadReportSample_Module"));
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
