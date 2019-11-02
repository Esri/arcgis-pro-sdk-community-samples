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

namespace ProSqlExpressReader
{
	/// <summary>
	/// ProSqlExpressReader implements a three plugin datasources to read the following:
	/// - Classic ArcGIS Personal Geodatabase: A personal geodatabase is a Microsoft Access database that can store, query, and manage both spatial and nonspatial data.  ProSqlExpressPluginDatasource implements read-only sql to personal geodatabase feature class data.
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
	/// 1. Make sure that the Sample data is unzipped in c:\data 
	/// 1. The data used in this sample is located in this folder 'C:\Data\PluginData\SQLExpressData'
	/// 1. In order to run this sample you have to install SQL Server Express, which can be downloaded and installed for free from Microsoft here: https://www.microsoft.com/en-us/download/details.aspx?id=13255.  The SQL Server version supported is 2017 or newer.  
	/// 1. Once SQL Server Express is installed and runnung, use 'Microsoft SQL Server Management Studio 18' to 'attach' the following database files:
	/// 1. C:\Data\PluginData\SQLExpressData\TestSqlExpress.mdf and C:\Data\PluginData\SQLExpressData\FDTestSQLExpress.mdf.   
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. In ArcGIS Pro open this project: 'C:\Data\PluginData\SqlExpress\SqlExpress.aprx'
	/// 1. Open the Catalog Dockpane and open Folder Connection to to drill down to this folder: 'SQLExpressData'
	/// ![UI](Screenshots/Screen1.png)  
	/// 1. The folder 'SQLExpressData' is linked to 'C:\Data\PluginData\SQLExpressData' this folder contains a file called 'SqlExpress.sqlexpress' with a file extension that triggers the 'ProDataProjectItem' Custom Project Item class. 'SqlExpress.sqlexpress' in turn contains lines of SQL Server RDMS conenction strings that are read and processed by the Custom Project Item class in order to explore the content of each database connection.
	/// 1. Open the 'SqlExpress.sqlexpress' to find two connection strings to two databases: TestSqlExpress, FdTestSqlExpress
	/// 1. Please note that each database contains tables (and feature class tables) that where copied from a personal (access) geodatabase.  The following 3 tables were also copied in order to manage feature datasets and spatial references: GDB_Items", GDB_GeomColumns, GDB_SpatialRefs.  
	/// 1. Back in ArcGIS Pro | Catalog dockpane you can see that each connection string is displayed with its database name and a drill down list comprised of tables, featureclasses, and feature datasets.
	/// ![UI](Screenshots/Screen2.png)  
	/// 1. In the source code look at the ProSqlProjectItem class, which is used to implement the "sqlexpress" file extension node in the Catalog dockpane.
	/// 1. Under the FdTestSqlExpress node you can see all a table and feature datasets containing point, line, and polygon feature classes of the personal geodatabase.  In source code, the ProDataSubItem class is used to prepresent each table node in the catalog browser.
	/// 1. Right clicking on any of the feature classes or feature datasets allows the feature class(es) to be added to the current map.  In the source code this is done in the AddToCurrentMap button class.
	/// ![UI](Screenshots/Screen3.png)  
	/// 1. After you add a ProSqlTable item to the current map, the ProSqlExpressPluginDatasource plug-in is used to convert the sql table content into a feature class that can be added to a map 
	/// ![UI](Screenshots/Screen4.png) 
	/// 1. You can also view the attribute table for the plugindatasource featureclass that you added to your map
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ProSqlExpressReader_Module"));
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
