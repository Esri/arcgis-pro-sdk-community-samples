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

namespace GetLineOfSight
{
  /// <summary>
  /// This sample illustrates how to calculate the line of sight between an observer point and a target point. The line of sight is calculated based on a TIN dataset.  
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data
  /// 1. The project used for this sample is 'C:\Data\3DAnalyst\3DLayersMap.ppkx'
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select the 3DLayersMap.ppkx project package.
  /// 1. Activate the LineOfSight scene.
  /// 1. This scene has the following layers in the TOC:
  /// * squaw_tin: TIN Dataset
  /// * LosResults_Input: Line feature layer input for the Line of Sight calculation.
  /// * LosResults: Line Feature Layer that will store the Line of sight result in two components - Visible and Invisible(if there is an obstruction point) sections. Visible section is symbolized in red and Invisible section is symbolized in green.
  /// * LosObstructionPoints: Point feature layer that represents the obstruction points detected in the line of sight.
  /// 1. Select the 'Edit' tab on the ArcGIS Pro ribbon and 'Create' new features
  /// 1. On the 'Create Features' pane select the LosResults_Input feature layer to see the 'Line of Sight' tool
  /// ![UI](Screenshots/LineOfSightTool.png)      
  /// 1. Select and activate the tool and see the Options page displaying the various parameters for the Line of Sight Tool.
  /// ![UI](Screenshots/LineOfSightOptions.png)      
  /// 1. Enter the parameters you require.
  /// 1. Sketch a line on the TIN in the Scene.
  /// 1. See a "Line of Sight" profile created for the sketched line. If there is an obstruction, you will also see this point symbolized in red. In this case, the line of sight will have two components - Visible section is symbolized in red and Invisible section is symbolized in green.
  /// ![UI](Screenshots/LineOfSightAndObstructionPoint.png)   
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("GetLineOfSight_Module");

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
