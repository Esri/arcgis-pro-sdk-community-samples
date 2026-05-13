//   Copyright 2026 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReplaceSketch
{
  /// <summary>
  /// This sample adds the ReplaceSketch command to the sketch context menu. 
  /// This allows you to add the shape of a line or polygon to the edit sketch by right clicking on the feature and choosing this command.
  /// It is equivalent to the ArcMap editing sketch context menu item.
  /// The replace sketch functionality is useful when you want to create a sketch from an underlying feature.
  /// For example you may want to split a polygon using an underlying road or stream geometry. 
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data(see under the "Resources" section for downloading sample data).  Make sure that the Sample data is unzipped in c:\data and "C:\Data\FeatureTest\FeatureTest.aprx" is available.
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.  
  /// 1. Launch the debugger to open ArcGIS Pro.  
  /// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'  .  
  /// 1. In ArcGIS Pro select the FeatureTest.aprx project.  
  /// 1. Select the polygon to split that already has a line going through the polygon and activate the editor split tool to 'split' the polygon.
  /// ![UI](Screenshots/Screenshot1.png)  
  /// 1. Right-click over the line since clicking close to the line will select that line for the 'sketch replacement'.  
  /// ![UI](Screenshots/Screenshot2.png)  
  /// 1. And select ReplaceSketch this will replace your current sketched geometry with the geometry of the polyline under the right mouse click.
  /// ![UI](Screenshots/Screenshot3.png)  
  /// 1. Continue or adjust the sketch as necessary then finish the sketch to use it as the splitting line.
  /// ![UI](Screenshots/Screenshot4.png)  
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("testContextMenu_Module"));
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
