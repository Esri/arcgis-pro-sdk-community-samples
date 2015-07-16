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

namespace GeoprocessingExecuteAsync
{
    /// <summary>
    /// This sample adds a tool button to Addin Tab. You click on the BufferGeometryTool button and draw a line
    /// on the map. The tool then passes the line geometry to Geoprocessing ExecuteToolAsync
    /// method which draws a buffer around that line.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. 
    /// 4. Open a map view and zoom in to your area of interest. 
    /// 5. Click on BufferGeometryTool button on the ADD-IN TAB and then click on several points on the map to draw a line.
    /// 5. Double-click completes the line and the ExecuteAsync tool is called with this line as the input to Buffer (Analysis Tools toolbox) tool.
    /// ![UI](Screenshots/Screen.png)
    /// 6. Once the execution of the Buffer tool is complete, the buffered polygon as added to display.
    /// ![UI](Screenshots/Screen2.png)
    /// </remarks>
    internal class GPExecuteModule : Module
    {
        private static GPExecuteModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static GPExecuteModule Current
        {
            get
            {
                return _this ?? (_this = (GPExecuteModule)FrameworkApplication.FindModule("GeoprocessingExecuteAsync_Module"));
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

        /// <summary>
        /// Generic implementation of ExecuteCommand to allow calls to
        /// <see cref="FrameworkApplication.ExecuteCommand"/> to execute commands in
        /// your Module.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected override Func<Task> ExecuteCommand(string id)
        {

            //TODO: replace generic implementation with custom logic
            //etc as needed for your Module
            var command = FrameworkApplication.GetPlugInWrapper(id) as ICommand;
            if (command == null)
                return () => Task.FromResult(0);
            if (!command.CanExecute(null))
                return () => Task.FromResult(0);

            return () =>
            {
                command.Execute(null); // if it is a tool, execute will set current tool
                return Task.FromResult(0);
            };
        }
        #endregion Overrides

    }
}
