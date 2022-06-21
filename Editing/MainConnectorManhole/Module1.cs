//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

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
  /// 1. The data for this sample is available in the CommunitySampleData-3D-05-11-2022.zip download (see link below). After unzipping the content, use the project foudn in this location:  c:\Data\MainManhole\MainManhole.ppkx
  /// 1. The sample requires a main (line), connector (line) and manhole (point) 3D layers in a scene. You can use the "scene" in this project to run the add-in.
  /// 1. The demonstration shows using the sketch to draw on a surface with the resulting features created underneath and connecting.
  /// ![UI](Screenshots/Screen.png)
  /// 1. The following screenshot is an example of the expected output.
  /// ![UI](Screenshots/OutputExample1.png)  
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
