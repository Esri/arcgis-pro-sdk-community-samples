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

namespace DockpaneWithComboDropdown
{
  /// <summary>
  /// This MVVM / XAML sample shows how to implement ComboBoxes on the ArcGIS Pro Ribbon and in an ArcGIS Pro API Dockpane using MVVM, where the combobox dropdown is populates from a dictionary.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio build the solution and run the debugger to open ArcGIS Pro.
  /// 1. In ArcGIS Pro open any project.  
  /// 1. Select the 'Add-in' tab, and then click on 'C ombobox Demo DockPane' to open the test dockpane.
  /// 1. Select a State from the dropdown or Capital from the dropdown to view the selection messagebox.
  /// ![Screen1](Screenshots/Screenshot1.png)
  /// </remarks>
  internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("DockpaneWithComboDropdown_Module");

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
