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
using System.Windows.Documents;
using System.Windows.Input;

namespace PublishFeatureLyrPython
{
  /// <summary>
  /// Create an ArcGIS Pro SDK Add-in that has Python scripts embedded.  This adds your Add-in’s Python script toolbox to Pro’s GP toolboxes.
  /// Once the Add-in is registered in ArcGIS Pro, you can run the Add-in’s Python scripts through ArcGIS Pro’s ‘GP Toolboxes’.
  /// This sample also demonstrates how to call the embedded Python script programmatically with parameters
  /// </summary>
  /// <remarks>
  /// 1. This solution file includes an example python script named MySharing.pyt which is included as 'Content' (Build action) in the add-in.
  /// 1. The python script is stored in the .\Toolboxes\toolboxes folder and when this add-in is loaded in ArcGIS Pro the python script is available a script tool under the ArcGIS Pro Geoprocessing toolbox.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. You can run the python script from the Geoprocessing toolbox as shown here.
  /// ![UI](Screenshots/Screen2.png)
  /// ![UI](Screenshots/Screen3.png)
  /// ![UI](Screenshots/Screen4.png)
  /// ![UI](Screenshots/Screen5.png)
  /// ![UI](Screenshots/Screen6.png)
  /// 1. You can also run the python script from code as implemented in the 'Publish as Web Layer' button under the 'Custom Publishing' tab (RunPyScriptButton.cs).
  /// ![UI](Screenshots/Screen7.png)
  /// ![UI](Screenshots/Screen8.png)
  /// ![UI](Screenshots/Screen9.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("PublishFeatureLyrPython_Module");

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
