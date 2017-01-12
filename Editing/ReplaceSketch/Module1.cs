//   Copyright 2017 Esri
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

namespace ReplaceSketch
{
    /// <summary>
    /// This sample adds the ReplaceSketch command to the sketch context menu. 
    /// This allows you to add the shape of a line or polygon to the edit sketch by right clicking on the feature and choosing this command.
    /// It is equivalent to the ArcMap editing sketch context menu item.
    /// </summary>
    /// <remarks>
    /// To install this add-in:
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 
    /// The replace sketch functionality is useful when you want to create a sketch from an underlying feature.
    /// For example you may want to split a polygon with an underlying road or stream. 
    /// To do this you would:
    /// 2. Select a polygon to split.
    /// 3. Activate the editor split tool.
    /// ![UI](Screenshots/Screenshot1.png)  
    /// 4. Right-click over a whole line feature that passes through the polygon
    /// ![UI](Screenshots/Screenshot2.png)  
    /// 5. And select ReplaceSketch.
    /// ![UI](Screenshots/Screenshot3.png)  
    /// 6. Continue or adjust the sketch as necessary then finish the sketch to use it as the splitting line.
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
