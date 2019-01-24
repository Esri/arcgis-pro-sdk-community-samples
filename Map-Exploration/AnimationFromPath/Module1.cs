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

namespace AnimationFromPath
{
  /// <summary>
  /// This sample allows creation of animation from path. A couple of different
  /// options have been proovided including View Along, Top-Down, Face-Target etc.
  /// The add-in also allows for setting Z-Offset, Duration and Custom Pitch.
  /// The Set Target tool can be used to specify a target that the camera will always
  /// face in the created animation. Set-target tool is used with the Face-Target view mode.
  /// In addition to the view you would like, the add-in also provides three different
  /// options for creating the keyframes. You can choose to:
  /// - Keyframes along path - creates fewer keyframes but tries to keep you on the path 
  ///   while avoiding sharp turns at corners
  /// - Keyframes every N seconds - creates a keyframe at the time-spacing specified
  /// - Keyframes only at vertices - creates a keyframe at each line vertex
  /// </summary>
  /// <remarks>
  ///1. In Visual Studio click the Build menu. Then select Build Solution.
  ///2. Click Start button to open ArcGIS Pro.
  ///3. ArcGIS Pro will open. 
  ///4. Open a scene or map view and a line feature class
  ///5. Select a line feature
  ///6. On the ADD-IN tab choose options under Animation from Path group and create keyframes
  ///NOTE - the selected line geometry is used for creating the keyframes. This means that
  /// for a 2D line feature, the keyframes will be created at zero height + any Z-Offset you
  /// specified in the options on the ADD-IN tab
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("AnimationFromPath_Module"));
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
