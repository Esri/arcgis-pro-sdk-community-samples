//   Copyright 2014 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace ExportWeb3DObjectResource
{
    /// <summary>
    /// This sample provides a new tab and controls that allows to export 3D marker symbol layers to web 3D object resources
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Either open and existing project (for example: "Interacting with Maps.aprx" included in the sample dataset) or create a new scene that has point feature layer(s) symbolized with 3D marker symbols
    /// 1. Select one or more point feature layers in the Contents pane.  If you used "Interacting with Maps.aprx", display the 3D map and select the Tree feature layer. 
    /// 1. EXPORT WEB 3D OBJECT tab will be shown on the ribbon
    /// 1. Click the Export button
    /// 1. In the dialog that comes up, specify the output location of JSON files that will be created by the export process
    /// 1. Once the output location is specified, each 3D marker symbol layer in the selected feature layers will be exported as a separate .json file on disk in the output location
    /// 1. These exported .json files can be used in the Esri 3D JavaScript API
    /// ![UI](Screenshots/Screen.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ExportWeb3DObjectResource_Module"));
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
