/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace CustomCatalogContext
{
	/// <summary>
	/// Shows how to modify a sampling of different context menus for the ArcGIS Pro Catalog (dockpane) context menu
	/// </summary>
	/// <remarks>
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open. 
	/// 1. Open any project file. Click on the Catalog Dockpane tab selector or if the Catalog Dockpane is not displayed, use the View tab on the Pro ribbon and select the 'Catalog Pane'.
	/// ![UI](Screenshots/Screen1.png)  
	/// 1. Right click on either the 'Maps' or 'Styles' nodes to use the 'Custom Catalog Button' menu option.
	/// 1. In the OnClick code behind the context of the clicked on IProjectWindow(s) is retrieved from the FrameworkApplication.ActiveWindow property.
	/// 1. The IProjectWindow's SelectedItems property to access each of the selected items which in turn can be cast to the respective project item depending on what "type" of item it is (i.e. map, style, gdb, etc)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CustomCatalogContext_Module"));
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
