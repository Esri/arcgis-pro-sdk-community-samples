// Copyright 2017 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       http://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace ConstructionToolWithOptions
{
  /// <summary>
  /// This sample illustrates how to build a construction tool with options allowing users to provide parameters at run-time. This pattern is new at ArcGIS Pro 2.0.  
  /// Two samples are included.
  /// The first is the BufferedLineTool.  The line sketch geoemtry is buffered by a user defined distance to create a polygon feature.
  /// The second sample is the CircleTool.  A user defined radius is used to create a circular arc with the point sketch geometry as the centroid.  This tool is registered with both the esri_editing_construction_polyline and esri_editing_construction_polygon categories allowing both polyline and polygon features to be created.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in c:\data
  /// 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
  /// 1. In Visual Studio click the Build menu.Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open, select the FeatureTest.aprx project
  /// 1. Select the 'Edit' tab on the ArcGIS Pro ribbon and 'Create' new features
  /// 1. On the 'Create Features' pane select the test polygon feature layer to see the 'Buffered Line' tool
  /// ![UI](Screenshots/ConstructionToolOptions_1.png)      
  /// 1. Select the tool and see the Options page displaying the buffer distance
  /// ![UI](Screenshots/ConstructionToolOptions_2.png)      
  /// 1. Enter a buffer distance and sketch a line.See a buffer of the sketched line used to generate a new polygon feature.
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ConstructionToolWithOptions_Module"));
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
