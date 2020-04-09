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
  /// ProDataReader implements two plugin datasources to allowing Pro viewing of the following formats:
  /// - Jpg photos with GPS metadata: smart phone and digital cameras have the option to capture GPS information when a photo is taken.  ProJpgPluginDatasource allows to access these GPS enable photos as a read-only feature class.
  /// - Gpx data: GPX (the GPS eXchange Format) is a data format for exchanging GPS data between programs and implemented by many GPS tracking devices. ProGpxPluginDatasource allows to access Gpx files as a read-only feature class. 
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data 
  /// 1. The data used in this sample is located in this folder 'C:\Data\PluginData' and 'C:\Data\PluginDataLinkTo'
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. In ArcGIS Pro open this project: 'C:\Data\PluginDataLinkTo\TestPlugin\TestPlugin.aprx'
  /// 1. Open the Catalog Dockpane and open Folder Connection to drill down to this folder: 'PluginDataLinkTo'
  /// ![UI](Screenshots/Screen1.png)  
  /// 1. The folder 'PluginDataLinkTo' is linked to 'C:\Data\PluginDataLinkTo' this folder contains a file called 'LinkToCustomPluginData.xlnk' with a file extension '.xlnk' that triggers the 'ProDataProjectItem' Custom Project Item class. 'LinkToCustomPluginData.xlnk' in turn contains all the folder paths which are scanned and processed by the Custom Project Item class.
  /// 1. Open the 'LinkToCustomPluginData.xlnk' folder to find various GPX and JPG file data sources. 
  /// ![UI](Screenshots/Screen2.png)  
  /// 1. In the source code look at the ProDataProjectItem class, which is used to implement the LinkToCustomPluginData.xlnk node in the Catalog dockpane.
  /// 1. The ProDataProjectItem class is triggered by an entry in config.daml specifically the 'insertComponent' tag for id: ProDataReader_ProDataReader.  Under the 'content' tag you find the attribute fileExtension="xlnk" which triggers the instantiation of the ProDataProjectItem class whenever a file with this extension (case sensitive) is encountered.  
  /// 1. &lt;B&gt;Note: the fileExtension attribute is case sensitive, hence only file extensions matching a case sensitive compare with the fileExtension attribute will work&lt;/B&gt;
  /// ![UI](Screenshots/Screen3.png)  
  /// 1. Under the 'LinkToCustomPluginData.xlnk' node you can see '2019-March-31-Hike' which represents a line feature class (GPS Track) and 'Berlin-Devsummit-17' which represents a point feature datasets containing the point location where the photo was taken.  In source code, the ProDataSubItem class is used to prepresent each node in the catalog browser.
  /// 1. Right clicking on any of the feature classes or the 'MiscPictures Jpeg Images' image folder node allows the feature class(es) to be added to the current map.  In the source code this is done in the AddToCurrentMap button class.
  /// 1. After you add an item to the current map, the ProPluginDatasource plug-in is used to convert the source data to a feature class that can be added to a map and displayed as an attribute table.
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
