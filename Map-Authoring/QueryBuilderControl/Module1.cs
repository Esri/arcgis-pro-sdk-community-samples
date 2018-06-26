/*

   Copyright 2018 Esri

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

namespace QueryBuilderControl
{
    /// <summary>
    /// This sample provides an illustration of how to use the QueryBuilderControl.  This add-in contains a dockPane which hosts the QueryBuilderControl.  
    /// The dockPane is used to view and modify the definition query for feature layers and standalone tables.  Use the Expression property to determine the 
    /// current and complete SQL expression. 
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open.
    /// 1. Open an existing project.
    /// 1. Click on the ADD-IN TAB.
    /// 1. Click on the *Show DefinitionQueryDockPane* button. 
    /// 1. The Definition Query dock pane will open up.
    /// ![UI](Screenshots/QueryBuilderDockPane.png)
    /// 1. Select a feature layer or standalone table in the TOC. The dock pane will display the definition query for the highlighted TOC item. 
    /// 1. Use the query builder control to update the definition query for the highlighted TOC item. 
    /// 1. Use the Apply button to write the updated definition query to the highlighted layer.
    /// ![UI](Screenshots/QueryBuilderDockPane_Layer.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("QueryBuilderControl_Module"));
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
