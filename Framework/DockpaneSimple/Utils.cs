//Copyright 2015 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace DockpaneSimple
{
    internal class Utils
    {
        /// <summary>
        /// Given a map project item's path (URL) this method opens a map if it has not already been opened
        /// this method is important since some functionality like bookmarks are not accessible if the map
        /// has not been opened and activated
        /// </summary>
        /// <param name="url"></param>
        internal static async void OpenAndActivateMap(string url)
        {
            Map map = MapView.Active.Map;

            bool bAlreadyOpen = false;

            // if not null - it may be open
            if (map != null)
            {
                // see if its already open
                //IList<IMapPane> mapPanes = MappingModule.GetMapPanes(map);
                PaneCollection mapPanes = FrameworkApplication.Panes;
                if ((mapPanes != null) && (mapPanes.Count > 0))
                {
                    bAlreadyOpen = true;

                    // activate the first one
                    Pane pane = mapPanes[0] as Pane;
                    if (pane != FrameworkApplication.Panes.ActivePane)
                        pane.Activate();
                }
            }

            // if not open - open it
            if (!bAlreadyOpen)
            {
                var mapItem = Project.Current.Items.FirstOrDefault(i => i.Path == url) as MapProjectItem;
                map = mapItem.GetMap();
                await ProApp.Panes.CreateMapPaneAsync(map);                
            }
        }

        /// <summary>
        /// Get the ICommand interface for a given typed DAML representation like for example: DAML.Button.esri_core_showProjectDockPane
        /// or the string itself as for example "esri_core_contentsDockPane"
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the specified commandId parameter didn't yield a valid ICommand</exception>
        /// <param name="commandId">Id of the command: use the typed DAML representation if possible to prevent errors i.e. DAML.Button.esri_core_showProjectDockPane or the string itself "esri_core_contentsDockPane" </param>
        /// <returns>ICommand if an ICommand interface exists otherwise an exception is thrown</returns>
        internal static ICommand GetICommand(string commandId)
        {
            // get the command's plug-in wrapper
            var iCommand = FrameworkApplication.GetPlugInWrapper(commandId) as ICommand;
            if (iCommand == null)
            {
                throw new ArgumentException("No such command id: {0} returns an ICommand interface", commandId);
            }
            return iCommand;
        }

        /// <summary>
        /// Block the current thread's execution until either a condition becomes true or a timeout expires
        /// </summary>
        /// <remarks>
        /// Usage: the example below blocks the current thread until the current project is available 
        /// or the time-out occurred
        /// await Utils.BlockUntil(() => ProjectModule.CurrentProject != null);
        /// if (ProjectModule.CurrentProject != null) {
        ///     // this thread no has access to the current project
        /// }
        /// else {
        ///     // this thread still has no access to the current project
        /// }
        /// </remarks>
        /// <param name="pred">Specify a function that will eventually return true; once this function returns true BlockUntil will exit</param>
        /// <param name="maxTimeoutInMilliSeconds">optional: once this timeout occurs the function exists even if the predicate is still false; the default is 2000 milliseconds</param>
        /// <param name="delayInterval">optional: time interval yielded to other thread between checking of the specified 'pred' function; default is 500 milliseconds</param>
        /// <returns>void</returns>
        public static async Task BlockUntil(Func<bool> pred, int maxTimeoutInMilliSeconds = 2000, int delayInterval = 500)
        {
            var iTotalTime = 0;
            while (!pred() && iTotalTime < maxTimeoutInMilliSeconds)
            {
                await Task.Delay(500);
                iTotalTime += delayInterval;
            }
        }

        /// <summary>
        /// utility function to enable an action to run on the UI thread (if not already)
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
        /// determines if the application is currently on the UI thread
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
