//   Copyright 2016 Esri
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
  /// To install this add-in:
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 
  /// The main use case for this functionality is to start creating a new feature relative to somewhere else such
  /// as a distance and bearing from a road intersection or a distance along a stream.
  /// To use:
  /// 1. Create an edit sketch representing the offset from an origin. E.g. start the sketch at an intersection and digitize a segment with direction and distance constraints.
  /// 2. Right click to display the sketch context menu and click 'Restart Sketch'.
  /// 3. The sketch is restarted from the last sketch point.
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
