using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;

namespace GetRecentPortalProjects
{
  /// <summary>
  /// This sample illustrates how to retrieve recently opened projects. This includes projects that are opened from the local machine and from a portal. The sample also shows how to open a project from the recent projects list.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Launch the debugger to run this configuration.
  /// 1. The configuration's custom start up page will display.
  /// 1. This page will list the most recent projects that have been opened in ArcGIS Pro. The list will include projects that are opened from the local machine and from a portal.
  /// ![UI](screenshots/RecentProjectsList.png)    
  /// 1. Select any project to open it. Note: If you pick a portal project to open, the active portal must match the portal that the project was opened from. 
  /// 1. Use the Browse button on the start up page to browse to other projects. The open item dialog displayed has a composite filter that will show local projects and projects on your current portal.
  /// ![UI](screenshots/OpenItemDialog.png)   
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("GetRecentPortalProjects_Module");

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