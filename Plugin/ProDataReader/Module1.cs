/*

   Copyright 2017 Esri

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

namespace ProDataReader
{
	/// <summary>
	/// ProDataReader implements a three plugin datasources to read the following:
	/// - Classic ArcGIS Personal Geodatabase: A personal geodatabase is a Microsoft Access database that can store, query, and manage both spatial and nonspatial data.  ProMdbPluginDatasource implements read-only access to personal geodatabase feature class data.
	/// - Jpg photos with GPS metadata: smart phone and digital cameras have the option to capture GPS information when a photo is taken.  ProJpgPluginDatasource allows to access these GPS enable photos as a read-only feature class.
	/// - Gpx data: GPX (the GPS eXchange Format) is a data format for exchanging GPS data between programs and implemented by many GPS tracking devices. ProGpxPluginDatasource allows to access Gpx files as a read-only feature class. 
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
	/// 1. Make sure that the Sample data is unzipped in c:\data 
	/// 1. The data used in this sample is located in this folder 'C:\Data\TestPersonalGdb'
	/// 1. Also in order to access Microsoft Access database files you need to download a 64 bit driver which can be downloaded from Microsoft here: https://www.microsoft.com/en-us/download/details.aspx?id=13255 
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. In ArcGIS Pro create a new Map using the Empty Map Template.
	/// 1. Open the Catalog Dockpane and add new Folder Connection to connect this folder 'C:\Data\TestPersonalGdb'
	/// ![UI](Screenshots/Screen1.png)  
	/// 1. On the Catalog Dockpane open the TestPersonalGdg folder to explore the ProMdb Project Item which allows you to 'drill-down' to the feature class content of ArcGIS personal geodatabases.
	/// 1. Open the TestPersonalGdg folder to find the TestPGdb.arcgismdb personal geodatabase.  The Personal Geodatabase file extension is usually .mdb, however, ArcGIS Pro doesn't allow project custom items with that extension, hence the renaming of the file extension to ArcGISMdb.
	/// ![UI](Screenshots/Screen2.png)  
	/// 1. In the source code look at the ProMdbProjectItem class, which is used to implement the TestPGdb.arcgismdb node.
	/// ![UI](Screenshots/Screen3.png)  
	/// 1. Under the TestPGdb.arcgismdb node you can see all point/line/polygon feature classes of the personal geodatabase.  In source code, the ProMdbTable class is used to prepresent each table.
	/// 1. Right clicking on the TestPGdb.arcgismdb node allows to add TestPGdb.arcgismdb as a Project Item.  In the source code this is done in AddToProject button class.
	/// 1. Right clicking on any of the feature classes allows the feature class to be added to the current map.  In the source code this is done in the AddToCurrentMap button class.
	/// 1. After you add a ProMdbTable item to the current map, the ProMdbPluginDatasource plug-in is used to convert the MS access table content into a feature class that can be added to a map and displayed as an attribute table.
	/// ![UI](Screenshots/Screen5.png) 
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ProDataReader_Module"));
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
