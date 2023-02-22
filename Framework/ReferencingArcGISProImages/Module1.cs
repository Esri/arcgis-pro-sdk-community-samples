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

namespace ReferencingArcGISProImages
{  /// <summary>
   /// ReferencingArcGISProImages shows how to reference ArcGIS Pro images (icons etc.) from your Add-in.
   /// </summary>
   /// <remarks>
   /// 1. In Visual Studio click the Build menu. Then select Build Solution.
   /// 1. Click Start button to open ArcGIS Pro.
   /// 1. In ArcGIS Pro open any map.
   /// 1. In ArcGIS Pro click on the 'Reference Icons' tab to view different alternatives to reference ArcGIS Pro icons (https://github.com/esri/arcgis-pro-sdk/wiki/DAML-ID-Reference-Icons).
   /// 1. You can look at the config.daml to see how ArcGIS Pro icons are referenced from within config.daml
   /// 1. Check ReferenceFromXAML.xaml on how to reference ArcGIS Pro icons from XAML
   /// 1. Check ReferenceFromXAMLViewModel.cs on how to reference ArcGIS Pro icons from code-behind
   /// ![UI](Screenshots/Screen1.png)  
   /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("ReferencingArcGISProImages_Module");

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
