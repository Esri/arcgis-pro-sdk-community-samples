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

namespace MultipatchBuilderEx
{
  /// <summary>
  /// This sample demonstrates how to construct multipatch geometries using the MultipatchBuilderEx class found in the ArcGIS.Core.Geometry namespace. 
  /// The MultipatchBuilderEx class also allows you to modify properties of a multipatch including the materials and textures.
  /// This add-in contains a number of buttons which create and modify multipatch features. 
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
  /// 1. Make sure that the Sample data is unzipped in C:\Data
  /// 1. Before you run the sample verify that the project "C:\Data\MultipatchTest\MultipatchTest.aprx" is present since this is required to run the sample.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open.
  /// 1. Open the "C:\Data\MultipatchTest\MultipatchTest.aprx" project.
  /// 1. Click on the ADD-IN TAB.
  /// 1. Click on the *Create Triangle Multipatch* button. 
  /// 1. A new multiaptch feature will be created.
  /// ![UI](Screenshots/Multipatch.png)
  /// 1. Click on the "Apply Materials" button.
  /// 1. The created multipatch feature will now have materials applied to it's faces. 
  /// ![UI](Screenshots/MultipatchMaterial.png)
  /// 1. Click on the "Apply Textures" button.
  /// 1. The created multipatch feature will now have textures applied to it's faces. 
  /// ![UI](Screenshots/MultipatchTexture.png)
  /// 1. Click on the "Add Multipatch to Overlay" button.
  /// 1. A new multiaptch geometry will be added to the overlay using a mesh symbol.
  /// ![UI](Screenshots/MultipatchOverlay.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MultipatchBuilderEx_Module"));
      }
    }

    internal static long MultipatchOID = -1;

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
