//   Copyright 2015 Esri
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

namespace CustomPopup
{
  /// <summary>
  /// This sample shows how to author custom pop-up content to display in a pop-up window. 
  /// In this example we are generating html and javascript code using the Google Charts api to create rich and interactive content in the pop-up. 
  /// This example also shows how to add your own commands to the bottom of the pop-up window. 
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 2. Click Start button to open ArcGIS Pro.
  /// 3. ArcGIS Pro will open. 
  /// 4. Open a map view. Click on the Add-In tab on the ribbon.
  /// 5. Within this tab there is a Custom Pop-up tool. Click it to activate the tool.
  /// 6. In the map click and drag a box around the features you want to identify.
  /// 7. The pop-up window should display and you should see a table showing the values for all the visible numeric fields in the layer. 
  /// It will also display a pie chart for those same fields.
  /// 8. As you click through the pop-up results the content is being generated dynamically for each feature.
  /// 9. The pop-up window also has a custom command "Show statistics" at the bottom of the window that when clicked shows additional information about the feature.
  ///
  ///![UI](screenshots/Popup.png)
  /// </remarks>
  internal class PopupModule : Module
  {
    private static PopupModule _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static PopupModule Current
    {
      get
      {
        return _this ?? (_this = (PopupModule)FrameworkApplication.FindModule("CustomPopup_Module"));
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
