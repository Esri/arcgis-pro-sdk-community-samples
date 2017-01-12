/*

   Copyright 2017 Esri

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
using ArcGIS.Desktop.Mapping;

namespace LayerPopups
{   /// <summary>
    /// This sample illustrates how to customize ArcGIS Pro's Layer Pop-up screen.  The sample shows how to define layer popups that will be persisted in the layer's CIM definition and defines a set of 'helper' classes modelled after the similar pattern ArcGIS.Desktop.Mapping implements for Renderer definitions. Popups can be defined that incorporate text, field values, images, charts, etc.
    /// </summary>
    /// <remarks>
    /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a project called 'AdminSample.aprx' that includes data that is used by this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Admin" is available.
    /// 1. Open this solution in Visual Studio 2015.  
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
    /// 1. Open the project "AdminSample.aprx" in the "C:\Data\Admin\" folder since this project contains data referenced by the sample code.
    /// 1. Click on the Add-in tab and see that three buttons in a 'Layer Popup' group were added.
	/// ![UI](Screenshots/Screen1.png)  
    /// 1. Click on any of the US state polygons to see the 'standard' layer popup. 
	/// ![UI](Screenshots/Screen2.png)  
	/// 1. Close the popup.  
    /// 1. Click the 'SimplePopup' button and click on any of the US state polygons to see the 'simple' layer popup.  Notice the changed title and the additional text above the field data.  
	/// ![UI](Screenshots/Screen3.png)  
	/// 1. Close the popup.  
    /// 1. Click the 'AdvancedPopup' button and click on any of the US state polygons to see the 'advanced' layer popup.  Notice the additional graphs on the bottom of the layer popup window.  
	/// ![UI](Screenshots/Screen4.png)  
	/// 1. Close the popup.
    /// 1. Click the 'ResetPopup' button to reset the layer popup to its original view.  
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("LayerPopups_Module"));
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
