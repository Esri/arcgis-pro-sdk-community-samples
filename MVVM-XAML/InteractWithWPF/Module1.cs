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

namespace InteractWithWPF
{
  /// <summary>
  /// This sample demonstrates how to interact with WPF controls in ArcGIS Pro's UI.  Sometimes ArcGIS Pro UI elements are not accessible via the ArcGIS Pro API and in some of those cases WPF controls can be used to modify the UI.  
  /// Indeed the "Project" button is not a standard Framework Plugin (button) like the other tabs on the ArcGIS Pro ribbon.  But, since ArcGIS Pro is a WPF application, and we can 'spy' the WPF visual tree (use Visual Studio to do so) to find which control implements the "Project" button.   It turns out that the "Project" control's name is "appButton" and the type of the control is "ActiproSoftware.Windows.Controls.Bars.RibbonApplicationButton".  Once ArcGIS Pro is open you can use the code in this add-in to update the "Project" button to "New Caption".
  /// Note: since we can't access any ActiProSoftware resources the code is using reflection to make the .Content property update.
  /// </summary>
  /// <remarks>
  /// 1. In Visual studio rebuild the solution.
  /// 1. Debug the add-in.
  /// 1. Load any ArcGIS project.
  /// 1. Click on the "Add-in" tab. 
  /// 1. Click on the "Update Project Button Caption" button to change the caption of the "Project" button to "New Caption".
  /// ![UI](Screenshots/Screen1.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("InteractWithWPF_Module");

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
