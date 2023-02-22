/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ArcGISProConfig
{
  /// <summary>
  /// This sample shows how to remote start and remote control ArcGIS Pro from an ArcGIS Corehost application.
  /// This sample is comprised of two samples:
  /// - Corehost **server application** that starts and controls the ArcGIS Pro client: ArcGISProConfigCoreHost
  /// - ArcGIS Pro **client application** that is started and controlled: ArcGISProConfig
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the "Resources" section for downloading sample data).  Make sure that the Sample data is unzipped in c:\data and "C:\Data\FeatureTest\FeatureTest.aprx" is available.
  /// 1. Open both solutions in Visual Studio: ArcGISProConfigCoreHost and ArcGISProConfig
  /// 1. Rebuild both solutions.
  /// 1. Make sure this project is available: "C:\Data\FeatureTest\FeatureTest.aprx" or change the path in the Corehost application.
  /// 1. Run the client application: ArcGISProConfigCoreHost
  /// 1. Note that the AppDomain is modified on startup to resolve the Assembly Paths for ArcGIS.Core.dll and ArcGIS.CoreHost.dll by using the ArcGIS Pro installation location.
  /// 1. The ArcGISProConfigCoreHost console application starts ArcGIS Pro with the "ArcGISProConfig" Managed Configuration
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Once ArcGISProConfig has started and is ready, ArcGISProConfigCoreHost sends the project path: "C:\Data\FeatureTest\FeatureTest.aprx" to be opened
  /// 1. ArcGISProConfig opens the project
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Once the output stops press any key to close the application.  
  /// 1. Close the "ArcGISProConfig" client session (ArcGIS Pro) using the User Interface.  Note that closing can be automated as well.
  /// 1. The "ArcGISProConfigCoreHost" Corehost application terminates after ArcGIS Pro has been closed.
  /// ![UI](Screenshots/Screen3.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ArcGISProConfig_Module");

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