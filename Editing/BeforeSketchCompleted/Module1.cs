//   Copyright 2020 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BeforeSketchCompleted
{
  /// <summary>
  /// This sample demonstrates the use of the BeforeSketchCompleted event to modify the sketch geometry before the sketch is completed.
  /// The example sets the Z values of the sketch to the specified elevation surface regardless of the current Z environment and any existing Z values the sketch may have.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a folder called 'C:\Data\Configurations\Projects' with sample data required for this solution.  Make sure that the Sample data (specifically CommunitySampleData-3D-mm-dd-yyyy.zip) is unzipped into c:\data and c:\data\PolyTest is available.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Start the debugger to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open. 
  /// 1. Open the Pro project file: PolyTest.aprx in the C:\Data\PolyTest\ folder or another map containing Z aware editable data and an elevation surface called 'Ground' Alternatively create a new map with Z aware editable data (e.g. map notes) and add an elevation source.
  /// 1. Open the add-in tab and click on the BeforeSketchCompleted button in the Sketch Events group
  /// 1. On the edit tab, click on the Create button in the Features group to display the create features pane.
  /// 1. Create a feature using a construction tool (the default polygon or line tool where applicable).
  /// 1. Select and examine the Z values on the newly created feature via the attributes pane and switch to the geometry tab. The Z values should reflect the surface Z values.
  /// ![UI](Screenshots/Screen1.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("BeforeSketchCompleted_Module"));
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
