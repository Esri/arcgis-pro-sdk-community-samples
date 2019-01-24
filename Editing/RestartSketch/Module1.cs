//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

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

namespace RestartSketch
{
    /// <summary>
    /// This sample provides an edit sketch context menu to restart the sketch from the last vertex.
    /// </summary>
    /// <remarks>
    /// The main use case for this functionality is to start creating a new feature relative to somewhere else such as a distance and bearing from a road intersection or a distance along a stream.
    /// To use this add-in:
    /// 1. Download the Community Sample data (see under the "Resources" section for downloading sample data).  The sample data contains a project called "FeatureTest.aprx" with data suitable for this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\FeatureTest\FeatureTest.aprx" is available.
    /// 1. Open this solution in Visual Studio 2015.
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to open ArCGIS Pro. ArcGIS Pro will open.  
    /// 1. Open the "C:\Data\FeatureTest\FeatureTest.aprx" project.
    /// 1. Create an edit sketch representing the offset from an origin. E.g. start the sketch at an intersection and digitize a segment with direction and distance constraints.
    /// 1. Start editing on the "TestLines" feature layer.  Place the first vertex by snapping to a given point and right click to chose a direction.
    /// ![UI](Screenshots/Screen1.png)
    /// 1. Right click to set a given distance.
    /// ![UI](Screenshots/Screen2.png)
    /// 1. A new vertex has now been placed in a given direction and distance from a starting location.
    /// 1. Now I want to start my 'real' sketch at this vertex (with given direction and distance from a starting location).
    /// 1. Right click to display the sketch context menu and click 'Restart Sketch'.
    /// ![UI](Screenshots/Screen3.png)
    /// 1. The sketch is restarted from the last sketch point.
    /// ![UI](Screenshots/Screen4.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("RestartSketch_Module"));
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
