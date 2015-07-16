using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace CommandFilter {
    /// <summary>
    /// This sample shows how to implement and register a command filter. Any ICommand (IPluginWrapper);
    /// can be filtered. Simply return false from the filter to prevent execution of the relevant command
    /// </summary>    
    /// <remarks>    
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open any project file. 
    /// 1. Click on the Add-in tab on the ribbon and then on the "Show Filter Dockpane" button.  
    /// 1. Command Filter Dock Pane opens.  
    /// 1. Check the 'Filtering is ON' checkbox now click any command button on the ArcGIS Pro ribbon.  Including the "Show Filter Dockpane" button.  
    /// 1. See the 'Command clicked:' list showing the command text.  
    /// 1. Use the 'Popup Messagebox Filter' to show a message box when the command is intercepted.  
    /// ![UI](Screenshots/Usage.png)
    /// </remarks>
    public class Module1 : Module {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current {
            get {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CommandFilter_Module"));
            }
        }
        /// <summary>
        /// Formats a URI given an image name and an optional Image path component.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static Uri PackUriForResource(string resourceName, string folderName = "Images") {
            string asm = System.IO.Path.GetFileNameWithoutExtension(
                System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            string uriString = folderName.Length > 0
                ? string.Format("pack://application:,,,/{0};component/{1}/{2}", asm, folderName, resourceName)
                : string.Format("pack://application:,,,/{0};component/{1}", asm, resourceName);
            return new Uri(uriString, UriKind.Absolute);
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
