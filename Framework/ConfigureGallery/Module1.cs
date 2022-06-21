/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

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
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace ConfigureGallery
{
  /// <summary>
  /// This sample shows how to use custom categories to configure buttons and tools into a gallery on Pro's ribbon.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to open ArcGIS Pro.
  /// 1. Open any project. Click on the Configure Gallery tab on the ribbon.
  /// 1. Within this tab there is an inline gallery that hosts a collection of controls. Controls that are registered within A custom category are displayed in this gallery.
  ///  This tab also contains other controls that are placed directly on the ribbon, outside the gallery. These controls are not registered with the category.
  /// ![UI](screenshots/configuregallery.png)
  /// 1. New custom categories are defined in DAML. Notice the following code snippet in the config.daml that defines a new category called AcmeCustom_AnalysisTools:
  /// ```xml
  /// &lt;categories&gt;
  ///  &lt;!--Step 1--&gt;
  ///  &lt;!--Create a new category to hold new commands in a a Gallery--&gt;
  ///  &lt;insertCategory id = "AcmeCustom_AnalysisTools" &gt;&lt;/ insertCategory &gt;
  /// &lt;/ categories &gt;
  /// ```
  /// 1. Controls are registered to a category using DAML. In Config.daml, here is a code snippet that registers a control with the AcmeCustom_AnalysisTools category:
  /// ```xml
  ///  &lt;button id="ConfigureGallery_Buttons_AcmeCommand1" caption="Command 1" 
  ///            categoryRefID="AcmeCustom_AnalysisTools" 
  ///            className="ConfigureGallery.Buttons.AcmeCommand1" ...&gt;
  ///      &lt;tooltip heading = "Tooltip Heading" &gt;
  ///       Command 1&lt;disabledText /&gt;
  ///      &lt;/tooltip&gt;
  ///      &lt;content version = "1.0" group="Group A" /&gt;
  ///  &lt;/button&gt;
  /// ```
  /// 1. The gallery is built by finding all the controls registered in the custom category using the Categories.GetComponentElements method. You can also 
  /// define custom attributes in the content tag of the control entry in the config.daml. In this scenario we have defined a version and group attributes
  /// which are used to define how the gallery looks when dropped down. 
  /// ![UI](screenshots/GalleryDropdown.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ConfigureGallery_Module"));
      }
    }
    private bool _isRunning = false;
    internal async Task RunMe(string label)
    {
      if (_isRunning)
        return;
      _isRunning = true;
      //Show progress
      var progDialog = new ProgressDialog($"Running {label}...");
      progDialog.Show();

      await Task.Delay(2000);

      progDialog.Hide();
      _isRunning = false;
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
