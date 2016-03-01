//   Copyright 2015 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace CustomSymbolPicker
{
    /// <summary>
    /// This sample provides a new dock pane with options for searching for symbols in styles in current project, 
    /// build a gallery with preview images for returned results and this sample also allows picking a symbol from this custom gallery
    /// to apply to the currently selected feature layer in the Contents pane
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open.
    /// 4. Either create a new blank project OR open an existing project.
    /// 5. Make sure the project you are working with has a map or scene with feature layer (point, line or polygon).
    /// 6. Click on the ADD-IN TAB.
    /// 7. Click on the Custom Symbol Picker button in the Carto group.
    /// 8. The Custom Symbol Picker dock pane will open up.
    /// 9. Select the type of symbol to search for (Point, Line, Polygon).
    /// 10. Select the style to search - the drop-down shows all the styles in the current project.
    /// 11. Type in a search term - multiple search terms can be separated with a space. For example: "red circle" will return results for "red AND circle".
    /// 12. Either hit ENTER or click on the Search button.
    /// 13. Symbol gallery will be populated with search results.
    /// 14. Click on a feature layer in Contents pane for which the symbol has to be updated.
    /// 15. Click on a symbol in the Custom Symbol Picker gallery - this will update the symbol for the feature layer currently selected in Contents pane.
    /// ![SymbolPicker](Screenshots/SymbolPicker.png)
    /// </remarks>
    internal class SymbolPickerModule : Module
    {
        private static SymbolPickerModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static SymbolPickerModule Current
        {
            get
            {
                return _this ?? (_this = (SymbolPickerModule)FrameworkApplication.FindModule("CustomSymbolPicker_Module"));
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        /// <summary>
        /// Generic implementation of ExecuteCommand to allow calls to
        /// <see cref="FrameworkApplication.ExecuteCommand"/> to execute commands in
        /// your Module.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected override Func<Task> ExecuteCommand(string id)
        {

            //replace generic implementation with custom logic
            //etc as needed for your Module
            var command = FrameworkApplication.GetPlugInWrapper(id) as ICommand;
            if (command == null)
                return () => Task.FromResult(0);
            if (!command.CanExecute(null))
                return () => Task.FromResult(0);

            return () =>
            {
                command.Execute(null); // if it is a tool, execute will set current tool
                return Task.FromResult(0);
            };
        }
        #endregion Overrides

    }
}
