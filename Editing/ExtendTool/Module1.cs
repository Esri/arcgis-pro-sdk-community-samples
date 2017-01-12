//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ExtendTool
{
    /// <summary>
    /// This sample creates a sketch tool that emulates the Extend functionality from ArcMap. It demonstrates creating a point sketch tool, working with geometry and performing the edit through an edit operation.
    /// </summary>
    /// <remarks>
    /// To use this sample:
    /// 1. Build or debug the sample through Visual Studio.
    /// 2. In Pro, select a feature that you wish to extend an existing line too.
    /// 3. Select the Extend tool from the samples group in the edit modify features pane.
    /// 4. Select one feature (either a polyline or polygon feature) to extend a line to
    /// ![UI](Screenshots/Screen1.png)
    /// 5. Click on a line to extend to the selected feature (make sure the alignment is such that the line can be extended)
    /// ![UI](Screenshots/Screen2.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ExtendTool_Module"));
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
