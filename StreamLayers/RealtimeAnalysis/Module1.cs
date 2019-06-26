/*

   Copyright 2019 Esri

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

namespace RealtimeAnalysis
{
	/// <summary>
	/// This sample illustrates real-time analysis. Real-time API allows you to integrate real-time to any part of ArcGIS Pro - whether you just want to show some information on a control or perform a very complex spatial analysis by executing geoprocessing tools.   
	/// </summary>
	/// <remarks>
	/// Using Bing Maps API: To use the Bing Maps APIs, you must have a (Bing Maps Key)[https://msdn.microsoft.com/en-us/library/dd877180.aspx].
	/// Note: When you use the Bing Maps APIs with a Bing Maps Key, usage transactions are logged. See Understanding (Bing Maps Transactions)[https://msdn.microsoft.com/en-us/library/ff859477.aspx] for more information.
	/// Creating a Bing Maps Key
	/// 1. Go to the Bing Maps Dev Center at https://www.bingmapsportal.com/. 
	/// ** If you have a Bing Maps account, sign in with the Microsoft account that you used to create the account or create a new one.For new accounts, follow the instructions in (Creating a Bing Maps Account)[https://msdn.microsoft.com/en-us/library/gg650598.aspx].
	/// 2. Select Keys under My Account.
	/// 3. Provide the following information to create a key:
	/// ** Application name: Required.The name of the application.
	/// ** Application URL: The URL of the application.
	/// ** Key type: Required. Select the key type that you want to create.You can find descriptions of key and application types (here)[https://www.microsoft.com/maps/create-a-bing-maps-key.aspx].
	/// ** Application type: Required. Select the application type that best represents the application that will use this key.You can find descriptions of key and application types (here)[https://www.microsoft.com/maps/create-a-bing-maps-key.aspx].  
	/// 4.	Type the characters of the security code, and then click Create. The new key displays in the list of available keys.Use this key to authenticate your Bing Maps application as described in the documentation for the Bing Maps API you are using.
	///  
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
	/// 1. Make sure that the Sample data is unzipped in c:\data 
	/// 1. The project used for this sample is 'C:\Data\StreamLayers\RealtimeAnalysis.aprx'.  
	/// 1. Once opened this solution in Visual Studio, open Module1.cs and update value for BingKey variable
	/// 1. This solution is using the **Newtonsoft.Json NuGet**.  If needed, you can install the NuGet from the "NuGet Package Manager Console" by using this script: "Install-Package Newtonsoft.Json".
	/// 1. Click the Build menu.Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open, open the 'C:\Data\StreamLayers\RealtimeAnalysis.aprx' project.
	/// 1. Open a map from the Catalog pane.
	/// 1. i.e. open the '02. ExploratoryAnalysis - Geo-fence' map.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Activate the Real-time Analysis tab.
	/// 1. Click on a button that matches with the map name(e.g. if you open a map named '02. ExploratoryAnalysis - Geo-fence', click on the button named '02. Geo-fence'). This subscribes to a layer real-time feed.
	/// 1. Click on this button again to Unsubscribe from the real-time feed.
	/// ![UI](Screenshots/Geofence_Notification_FullScreen.gif)
	/// </remarks>
	internal class Module1 : Module
	{
		private static Module1 _this = null;

		internal const string BingKey = @""; //<Add you bing key>";

		private double _x = -122.333964;
		private double _y = 47.602038;

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current
		{
			get
			{
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Realtime_Module"));
			}
		}

		public double Lat
		{
			get
			{
				return _y;
			}
			set
			{
				_y = value;
			}
		}

		public double Long
		{
			get
			{
				return _x;
			}
			set
			{
				_x = value;
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
