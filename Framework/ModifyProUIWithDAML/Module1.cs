using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace ModifyProUIWithDAML
{
    /// <summary>
    /// This sample demonstrates how to modify the Pro UI using DAML
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.  
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open.
    /// 1. Open any project with at least one feature layer.
    /// 1. Notice the following customizations made to the Pro UI:
    /// * The Bookmarks button has been removed in the Navigate group on the Map tab
    /// ![RemoveButton](screenshots/DeleteCoreButton.png) 
    /// * A new button has been inserted into the Navigate group on the Map Tab
    /// ![NewButton](screenshots/NewButtonCoreGroup.png) 
    /// * With any Map view active, right click on a feature layer in the TOC. Notice the New Button context menu item added.
    /// ![New Menu](screenshots/NewMenuItemInContextMenu.png)
    /// * Click the Project tab to access Pro's backstage.  Notice the missing Open and New project tabs.  A new tab called "Geocode" has been inserted above the Save project button.
    /// ![New Backstage tab](screenshots/BackstageDeleteExistingTabsInsertNewTabs.png)
    /// * Click the Project tab to access Pro's backstage. Click the Options tab to display the Options Property Sheet.  Notice the new "Sample Project Settings" property page inserted within the Project group.
    /// ![New Backstage tab](screenshots/PropertySheetOptionsDialog.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ModifyProUIWithDAML_Module"));
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
