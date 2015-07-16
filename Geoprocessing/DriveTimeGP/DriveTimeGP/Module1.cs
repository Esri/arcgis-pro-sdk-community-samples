using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace DriveTimeGP
{
    /// <summary>
    /// This samples shows how to call the esri drive time geoprocessing service @ http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Network/ESRI_DriveTime_US/GPServer/CreateDriveTimePolygons 
    /// and then displays the drive time areas for 3 times.
    /// </summary>
    /// <remarks>    
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open any project file but make sure that you have a base map. 
    /// 1. Active a map view.
    /// 1. Click on the Add-in tab on the ribbon and then on the "DriveTimeGPTool" button.
    /// 1. The sketch tool (point) is now enabled - click on the map to get drive times for the click location.
    /// 1. Wait for the GP task to finish and you can view the updated map and popup.
    /// ![UI](Screenshots/DriveTime.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("DriveTimeGP_Module"));
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
