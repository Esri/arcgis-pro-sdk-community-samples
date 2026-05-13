/*

   Copyright 2026 Esri

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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Connectivity
{
  /// <summary>
  /// This sample is used to demonstrate how to use the ArcGIS.Core.SystemCore.Connectivity class to determine the connectivity status of the machine running ArcGIS Pro. It also demonstrates how to listen for connectivity changes by subscribing to the ConnectivityChanged event on the Connectivity class.
  /// The results are displayed in a DockPane, which is opened by clicking the "Connectivity" button on the "Add-In" tab of the ArcGIS Pro ribbon. The DockPane displays whether the machine is a mobile device or desktop, whether it is in airplane mode, and the internet connection status. The icons and text update when connectivity changes are detected.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio, click the Build menu, then select Build Solution to build the solution and make sure there are no errors.
  /// 1. Start debugging by clicking the Start button or pressing F5. This will open ArcGIS Pro.
  /// 1. In ArcGIS Pro, open the "Add-In" tab on the ribbon and click the "Connectivity" button to open the DockPane.
  /// 1. Observe the connectivity status displayed in the DockPane. Try changing your connectivity status (e.g., turn on airplane mode, disconnect from the internet) and observe how the DockPane updates to reflect the changes.
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("Connectivity_Module");

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
