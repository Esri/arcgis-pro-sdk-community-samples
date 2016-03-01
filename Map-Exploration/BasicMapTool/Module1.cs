using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace BasicMapTool
{
    /// <summary>
    /// Provides a basic map tool that can be Copy/Paste inherited as a starting point for more
    /// sophisticated tool development.It has all the basic features such as key handling, simple mouse click
    /// implementation, and an associated embeddable control that can be moved around the Map View.
    /// </summary>
    /// <remarks>   
    /// 1. Open this solution in Visual Studio 2015.  
    /// 1. Click the build menu and select Build Solution.
    /// 1. Click the Start button to open ArCGIS Pro.  ArcGIS Pro will open.
    /// 1. Open any project or a blank map.
    /// 1. Click on the Add-in tab and see that a 'Show Coordinates' button was added.
    /// 1. Click the 'Show Coordinates' button and click anywhere on your map pane.
    /// 1. The embeddable control showing the current coordinates is displayed.
    /// ![UI](Screenshots/Screen1.png)
    /// 1. Use the cursor keys to pan the map.
    /// </remarks>
    internal class Module1 : Module {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current {
            get {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("BasicMapTool_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload() {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
