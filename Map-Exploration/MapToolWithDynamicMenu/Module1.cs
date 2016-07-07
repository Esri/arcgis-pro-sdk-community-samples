using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace MapToolWithDynamicMenu
{
    /// <summary>
    /// This sample shows how to author a dynamic pop-up menu as an extension of a map exploration tool. 
    /// In this example a mouse click on a map that contains a high density points is used to pop-up a dynamic menu showing all point features intersecting the mouse click area.  The dynamic pop-up can then be used to select a single feature. 
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open the "C:\Data\Interacting with Maps\Interacting with Maps.aprx" project.
    /// 1. Click on the Add-In tab on the ribbon.
    /// 1. Within this tab there is a **Show Selection Pop-up** tool. Click on the tool button to activate the tool.
    /// 1. On the map click on an area with high feature point density as shown here.
    /// ![UI](screenshots/6MapTool2D.png)
    /// 1. The pop-up menu show all features that intersect the point click area. 
    /// 1. Click on of the features. 
    /// 1. Verify that the feature was really selected.
    /// ![UI](screenshots/6MapTool2D-2.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MapToolWithDynamicMenu_Module"));
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
