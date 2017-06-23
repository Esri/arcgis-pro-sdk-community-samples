// Copyright 2017 Esri

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
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace RasterInspector
{
    /// <summary>
    /// This sample show how to:
    /// * Retrieve raster values
    /// </summary>
    /// <remarks>
    /// 1. This solution is using the **Gu.Wpf.DataGrid2D**.  If needed, you can install the Nuget from the "Nuget Package Manager Console" by using this script: "Install-Package Gu.Wpf.DataGrid2D".
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open a project containing a raster layer. Add a raster dataset if needed.
    /// 1. Select the raster layer in the table of contents.
    /// 1. Switch to th Add-In tab.
    /// 1. Click the pixel inspector tool and move the mouse pointer across the raster dataset.
    /// ![UI](Screenshots/PixelInspector.png)
    /// </remarks>
    internal class RasterModule : Module
    {
        private static RasterModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static RasterModule Current
        {
            get
            {
                return _this ?? (_this = (RasterModule)FrameworkApplication.FindModule("RasterInspector_Module"));
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


        /// <summary>
        /// Utility function to enable an action to run on the UI thread (if not already)
        /// </summary>
        /// <param name="action">the action to execute</param>
        /// <returns></returns>
        internal static Task RunOnUIThread(Action action)
        {
            if (OnUIThread)
            {
                action();
                return Task.FromResult(0);
            }
            else
                return Task.Factory.StartNew(action, System.Threading.CancellationToken.None, TaskCreationOptions.None, QueuedTask.UIScheduler);
        }

        /// <summary>
        /// Determines if the application is currently on the UI thread
        /// </summary>
        private static bool OnUIThread
        {
            get
            {
                if (FrameworkApplication.TestMode)
                    return QueuedTask.OnWorker;
                else
                    return System.Windows.Application.Current.Dispatcher.CheckAccess();
            }
        }
    }
}
