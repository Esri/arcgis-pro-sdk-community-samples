/*

   Copyright 2018 Esri

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

namespace SimplePointPluginTest
{
  /// <summary>
  /// SimplePointPluginTest implements a custom plugin datasource for reading csv files.  SimplePointPluginTest is an add-in that allows to access the custom datasource.  SimplePointPlugin contains the actual custom plugin datasource implementation to access csv data from within ArcGIS Pro. 
  /// </summary>
  /// <remarks>
  /// 1. This solution is using the **RBush NuGet**.  If needed, you can install the NuGet from the "NuGet Package Manager Console" by using this script: "Install-Package RBush".
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data 
  /// 1. The data used in this sample is located in this folder 'C:\Data\SimplePointPlugin\SimplePointData'
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. In ArcGIS Pro create a new Map.
  /// 1. In Visual Studio set a break point inside the TestCsv1.OnClick code-behind.
  /// 1. In ArcGIS Pro click on the 'Debug Add-in Code' button.
  /// 1. You can now step through the code showing how to use a custom plugin in code.
  /// 1. In ArcGIS Pro click on the 'Add Plugin Data to Map' button.
  /// 1. The test data is now added to the current map.
  /// 1. Use the test data on the map or via the attribute table.
  /// ![UI](Screenshots/screen2.png)  
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("SimplePointPluginTest_Module"));
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
