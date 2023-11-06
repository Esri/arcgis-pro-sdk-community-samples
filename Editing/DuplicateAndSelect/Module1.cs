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

namespace DuplicateAndSelect
{
  /// <summary>
  /// This sample provides a set of controls which guide the user through a data quality assurance(QA) review workflow, with tools for visually reviewing and notating datasets based on their accuracy.
  /// </summary>
  /// <remarks>
  /// 1. Open this solution in Visual Studio.
  /// 1. Click the build menu and select Build Solution.
  /// 1. Click the Start button to run the solution.  ArcGIS Pro will open.
  /// 1. Open an existing Project the contains a Map showing a FeatureLayer and a StandAloneTable.
  /// 1. Click on the "Add-in" tab and note the "Feature Duplicate and Select" and "Row Duplicate and Select" button groups.
  /// ![UI](Screenshot/Screenshot1.png)
  /// 1. Click the "Attributes" button to bring up the "Attributes" dockpane and make sure that the "selection" tab is selected.
  /// 1. Select one Feature and one Row (in the Standalone Table).
  /// ![UI](Screenshot/Screenshot2.png)
  /// 1. Click the "Duplicate Add Selection" button to create a duplicate of the first selected feature and add the newly created feature to the existing selection.
  /// ![UI](Screenshot/Screenshot3.png)
  /// 1. Next to perform the same operation on a Standalone Table
  /// ![UI](Screenshot/Screenshot4.png)
  /// 1. Click the "Table Dupl. Add Selection" button to duplicate the first selected row and add the newly created row to the selected set.
  /// ![UI](Screenshot/Screenshot5.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("DuplicateAndSelect_Module");

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
