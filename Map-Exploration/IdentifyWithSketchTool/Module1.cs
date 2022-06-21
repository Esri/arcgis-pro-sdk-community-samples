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

namespace IdentifyWithSketchTool
{
    /// <summary>
    /// This sample shows how to: 
    /// * Add a custom sketch tool to work with map views
    /// * Use the sketch to create a new selection and zoom to the result in 2D or 3D
    /// * Create a custom identify tool for 2D and 3D
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Launch the debugger to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open a project with a map view that has feature layers.
    /// 1. Click on the Sketch tab and then on the 'Select And Zoom' button.
    /// 1. On your map view 'sketch' (using the rubber band rectangle) an area containing features
    /// 1. Features will be selected and the map view will be zoom to the selection area's extent
    /// 1. Next click the 'Custom Identify' button and you will see the 'Identify Result' popup
    /// ![UI](Screenshots/2DScreen.png)    
    /// 1. Now open a project that contains a scene with 3D features
    /// 1. Click on the Sketch tab and then on the 'Select And Zoom' button
    /// 1. On your 3D map view 'sketch' (using the rubber band rectangle) an area containing features
    /// 1. Features will be selected and the map view will be zoom to the selection area's extent
    /// 1. Next click the 'Custom Identify' button and you will see the 'Identify Result' pop-up
    /// ![UI](Screenshots/3DScreen.png)    
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("SketchTool_Module"));
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
