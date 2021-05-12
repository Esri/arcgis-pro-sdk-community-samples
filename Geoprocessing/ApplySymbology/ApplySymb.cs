using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace ApplySymbology
{
  /// <summary>
  /// This sample shows how to move Symbology from one layer to another layer using the 'Apply Symbology From Layer' GeoProcessing tool.  
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the "Resources" section for downloading sample data).  The sample data contains a project called "FeatureTest.aprx" with data suitable for this sample.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Interacting with Maps" is available. 
  /// 1. In Visual studio click the Build menu. Then select Build Solution and debug the solution.
  /// 1. ArcGIS Pro will open, select and open the "C:\Data\Interacting with Maps\Interacting with Maps.aprx" project.
  /// 1. Open the 'Portland Crimes' map, select the 'Crimes' layer, and use the 'New Layer File' button on the 'Share' tab to save the 'Crimes' layer as lyrx file, called 'Crimes.lyrx'.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Create a new empty 2D Map and then open the catalog dockpane and add the 'Crimes' feature class to the current map.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. Open the Add-in tab and click the "Move Symbology" button to start the 'Apply Symbology From Layer' GeoProcessing tool.
  /// ![UI](Screenshots/Screen3.png)
  /// 1. When prompted to select the 'Symbology Input Layer' browse and select the 'Crimes.lyrx' file saved in the pervious step.
  /// 1. After the GeoProcessing tool completes, note that the 'Crimes' symbology has been updated.
  /// ![UI](Screenshots/Screen4.png)
  /// </remarks>
  internal class ApplySymb : Module
    {
        private static ApplySymb _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static ApplySymb Current
        {
            get
            {
                return _this ?? (_this = (ApplySymb)FrameworkApplication.FindModule("ApplySymbology_Module"));
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
