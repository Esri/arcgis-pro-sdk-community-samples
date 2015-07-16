//   Copyright 2015 Esri
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

namespace MainConnectorManhole
{
    /// <summary>This sample creates a sketch tool that creates multiple features in 3D. It demonstrates a line sketch tool, working with 3D geometry, and edit templates.</summary>
    /// <remarks>
    /// 1. The sample was initially created for internal demonstrations to show feature construction in 3D. 
    /// 1. It requires the main (line), connector (line) and manhole (point) 3D layers in a scene. 
    /// 1. The demonstration shows using the sketch to draw on a surface with the resulting features created underneath and connecting.
    /// ![UI](Screenshots/Screen.png)
    /// 1. The following two screenshots are examples of the expected output.
    /// ![UI](Screenshots/OutputExample1.png)
    /// ![UI](Screenshots/OutputExample2.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MainConnectorManhole_Module"));
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
