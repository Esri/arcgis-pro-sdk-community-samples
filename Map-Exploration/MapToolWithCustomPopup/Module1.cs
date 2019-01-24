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

namespace CustomPopup
{
    /// <summary>
    /// This sample shows how to author custom pop-up content to display in a pop-up window. 
    /// In this example we are generating html and javascript code using the Google Charts api to create rich and interactive content in the pop-up. 
    /// This example also shows how to add your own commands to the bottom of the pop-up window. 
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open the "C:\Data\ElectionData\Election.aprx" project.
    /// 1. Click on the Add-In tab on the ribbon.
    /// 1. Within this tab there is a Custom Pop-up tool. Click on the button to activate the tool.
    /// 1. On the map click and drag a box around the features you want to examine in the custom pop-up.
    /// 1. The pop-up window should display and you should see a table showing the values for all the visible numeric fields in the layer. 
    /// 1. The custom pop-up also displays a pie chart for those same fields.
    /// ![UI](screenshots/5MapTool2D-2.png)
    /// 1. As you click through the pop-up results the content is being generated dynamically for each feature.
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CustomPopup_Module"));
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
