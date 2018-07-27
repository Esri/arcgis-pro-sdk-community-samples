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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

namespace AddInInfoManager
{
	internal class ShowDamlVersion : Button
	{
		protected override void OnClick()
		{
			try { 
				// 1. get the filename of this .esriAddinX file
				var fileName = AddIn.GetAddInId();
				// 2. get the config.daml content from the esriAddinX file
				var versionTuple = AddIn.GetConfigDamlAddInInfo(fileName);
				MessageBox.Show($@"Version: {versionTuple.Version} desktopVersion: {versionTuple.DesktopVersion} Id: {Module1.Id}");
			}
			catch (Exception ex)
			{
				// problem
				MessageBox.Show($@"Unable to parse config.daml: {ex.Message}");
			}
		}
	}
}
