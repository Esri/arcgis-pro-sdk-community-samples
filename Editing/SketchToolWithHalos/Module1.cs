// Copyright 2024 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace SketchToolWithHalos
{
  /// <summary>
  /// This sample illustrates how to build a sketch tool that has a number of halos of specific distances that follow the cursor.  
  /// The halos are implemented as an embeddable control that is displayed as a SketchTip. 
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data
  /// 1. The project used for this sample is 'C:\Data\Interacting with Maps\Interacting with Maps.aprx'
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select the Interacting with Maps.aprx project
  /// 1. Select the 'Add-In' tab on the ArcGIS Pro ribbon and activate the 'Sketch Tool with Halos' tool.
  /// ![UI](Screenshots/SketchToolHalos.png)      
  /// 1. Move your mouse around and notice how the halos follow the cursor. Zoom in and out and notice how the halos adjust to the different map scale. 
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("SketchToolWithHalos_Module");

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
