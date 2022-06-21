/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Core;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Text;


namespace PortalInfoListAllFedServers
{
	/// <summary>
	/// Basics - This is the sample for PortalManagement SDK (ArcGIS.Desktop.Core.ArcGISPortal &amp; ArcGIS.Desktop.Core.ArcGISPortalManager) and EsriHttpClient.
	/// Usage -
	/// PortalInfoDockPane is to be triggered after clicking on the "Button1".
	/// If the Active portal is arcgis online, print basic portal info (1).
	/// When the Active portal is portal, print portal info and fed servers etc. (1,2,3)
	/// You can either manually sign in to the active portal, or wait till this program prompts you for user name and password.
	/// </summary>
	internal class Button1 : Button
	{
		protected override void OnClick()
		{
			PortalInfoDockpaneViewModel.Show();
		}
	}
}
