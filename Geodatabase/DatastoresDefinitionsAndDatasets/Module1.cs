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

namespace DatastoresDefinitionsAndDatasets
{
	/// <summary>
	/// This sample illustrates the use of Datastore, Definitions, and Datasets of the Geodatabase API.
	/// </summary>
	/// <remarks>
	/// 1. In Visual studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open, select any project
	/// 1. Open the Add-in tab and click the "Datastore Datasets and Definitions" button to open "Datastore Datasets and Definitions" dockpane.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Select a Datastore Type from the drop down and then enter a data path that contains the selected datastore.
	/// 1. Data Path samples are: 
	/// 1. For "File Geodatabase": any folder with a .GDB extension for example: C:\Data\FeatureTest\FeatureTest.gdb
	/// 1. For "Enterprise Geodatabase": select a Database Connection file with the .SDE file extension.
	/// 1. For "Feature Service": https://sdk5.esri.com/server/rest/services/Alameda/FeatureServer 
	/// 1. For "Sqlite Database": any folder with a .SQLITE? extension
	/// 1. For "Shape File": any folder that contains shape files with a .SHP extension
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Click the "Load Metadata" button to retrieve the metadata from the specified data source.
	/// 1. The "Data Type" drop down and the "Datasets" list are updated to reflect the content of the selected data source.
	/// 1. Select a "Data Type" drop down and a "Dataset" to view some of the datasource's metadata.
	/// ![UI](Screenshots/Screen3.png)
	/// ![UI](Screenshots/Screen4.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("DatastoresDefinitionsAndDatasets_Module"));
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
