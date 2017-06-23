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

namespace Renderer
{
    /// <summary>
    /// This sample renders feature layers with various types of renderers. There are examples for the following types of renderers in this sample:
    ///   * Simple Renderers
    ///   * Unique Value Renderer
    ///   * Class breaks renderers
    ///       * Graduated color to define quantities
    ///       * Graduated symbols to define quantities
    ///       * Unclassed color gradient to define quantities
    ///   * Proportional Renderer
    ///   * Heat map renderer
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open any project file that contains points, polygon and line feature layers. 
    /// 1. In Add-in tab, click the "Apply Renderer" button.
    /// 1. The first point feature layer in your project's TOC will be rendered with an "Unique Value Renderer".
    /// To experiment with the various renderers available with this sample, follow the steps below:
    /// 1. Stop debugging Pro.
    /// 1. In Visual Studio's solution explorer, open the ApplyRenderer.cs file. This is the class file for the Apply Renderer button.
    /// 1. The OnClick method in the ApplyRenderer.cs file gets the first point feature layer in your project and then applies the Unique Value Renderer to it.<br/>
    ///    **You can modify the code in the OnClick method in this file to use one of the various renderers available in this sample and/or select any feature layer in your project.**
    ///     ```c#  
    ///     protected async override void OnClick()  
    ///     {  
    ///       //TODO: This line below gets the first point layer in the project to apply a renderer.  
    ///       //You can modify it to use other layers with polygon or line geometry if needed.  
    ///       var lyr = MapView.Active.Map.GetLayersAsFlattenedList().OfType&lt;FeatureLayer&gt;().FirstOrDefault(s => s.ShapeType == esriGeometryType.esriGeometryPoint);  
    ///       //TODO: Modify this line below to experiment with the different renderers  
    ///       await UniqueValueRenderers.UniqueValueRenderer(lyr);  
    ///     }  
    ///     ```
    /// 1. After modifying the OnClickMethod build the solution and click the start button to open Pro.  
    /// 1. Open any project and test the Apply Renderer button again.
    /// ![UI](screenshots/Renderers.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Renderer_Module"));
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
