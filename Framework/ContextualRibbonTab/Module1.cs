/*

   Copyright 2023 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ContextualRibbonTab
{
  /// <summary>
  /// This sample illustrates how to control the visibility of a Tab control on the ArcGIS Pro Ribbon.
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  Make sure that the Sample data is unzipped in c:\data.
  /// 1. Open this solution in Visual Studio.
  /// 1. Click the build menu and select Build Solution.
  /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
  /// 1. Open the project "FeatureTest.aprx" in the "C:\Data\FeatureTest" folder.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Note that loading a project for which the name starts with 'Feature' the 'Contextual Tab' is enabled.
  /// 1. Next load any other project (for which the name doesn't start with 'Feature') 
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Note that now the 'Contextual Tab' is disabled (hidden).
  /// </remarks>
  internal class Module1 : Module
  {

    private static Module1 _this = null;

    public const string SpecialProjectStateID = "special_project_state";

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ContextualRibbonTab_Module");

    #region Check opened Project

    /// <summary>
    /// Called when the Module is initialized.
    /// </summary>
    /// <returns></returns>
    protected override bool Initialize() 
    {
      if (FrameworkApplication.State.Contains(SpecialProjectStateID))
      {
        FrameworkApplication.State.Deactivate(SpecialProjectStateID); //deactivates the state
      }
      // subscribe to Project opened event
      ProjectOpenedEvent.Subscribe(OnProjectOpened);
      return base.Initialize();
    }

    /// <summary>
    /// Project Opened event handler
    /// </summary>
    /// <param name="obj"></param>
    private void OnProjectOpened(ProjectEventArgs obj) 
    {
      var enableContextState = obj.Project.Name.StartsWith("feature", StringComparison.OrdinalIgnoreCase);
      // check if the project name starts with 'Feature'
      if (enableContextState)
      {
        // activate the UI
        FrameworkApplication.State.Activate(SpecialProjectStateID); //activates the state
      }
      else
      {
        // deactivate the UI
        if (FrameworkApplication.State.Contains(SpecialProjectStateID))
        {
          FrameworkApplication.State.Deactivate(SpecialProjectStateID); //deactivates the state
        }
      }
      MessageBox.Show($"{obj.Project.Name} has opened - Context State is now {(enableContextState ? "Enabled" : "Disabled")}");
    }

    /// <summary>
    /// unsubscribe to the project opened event
    /// </summary>
    protected override void Uninitialize()
    {
      // unsubscribe
      ProjectOpenedEvent.Unsubscribe(OnProjectOpened);
      return;
    }

    #endregion

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
