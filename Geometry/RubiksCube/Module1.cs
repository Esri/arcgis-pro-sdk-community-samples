/*

   Copyright 2020 Esri

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

namespace RubiksCube
{
  /// <summary>
  /// This sample illustrates creating a multipatch, applying materials and applying textures using the MultipatchBuilderEx class. 
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains required data for this sample add-in.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open.
  /// 1. Open the "C:\Data\MultipatchBuilderEx\MultipatchBuilderExCubeDemo.aprx" project which contains the required data needed for this sample.
  /// 1. From the "Add-in" tab select "Create Cube".
  /// ![UI](Screenshots/RibbonUI.png)
  /// 1. A multipatch cube will be created.
  /// ![UI](Screenshots/BlankCube.png)
  /// 1. From the "Add-in" tab select "Apply Materials". The cube will have materials applied.
  /// ![UI](Screenshots/CubeWithMaterials.png)
  /// 1. From the "Add-in" tab select "Apply Textures".  The cube will have the rubik's cube image applied as a texture.
  /// ![UI](Screenshots/CubeWithTextures.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    internal static long CubeMultipatchObjectID = -1;


    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("RubiksCube_Module"));
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
