//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace PreRel_UndoRedo
{
    /// <summary>
    /// This sample demonstrates how to add operations onto the undo/redo stack.   
    /// </summary>
    /// <remarks>
    /// ArcGIS Pro does not contain a single undo/redo operation stack for the application; it has multiple undo/redo stacks. Each pane and dockpane decides how it's 
    /// own operations are managed. In many cases, each pane and dockpane has it's own OperationManager; however in some cases they may 
    /// elect to share a single OperationManager.  For example, operations added to map A are not visible to map B (becuase they have different
    /// OperationManagers), but two panes showing the same map will show the same operations as they share the same OperationManager. 
    /// 
    /// When a window becomes active, its OperationManager is requested and connected to the undo/redo user interface. 
    /// <para>
    /// This sample contains a dockpane which has its own OperationManager.   The two buttons at the top of the dockpane illustrate how to 
    /// create an undo/redo operation and add it to the OperationManager.  The third button at the top of the dockpane illustrates a 
    /// compositeOperation which allows many operations to be grouped into a single composite operation.   The 4 buttons at the bottom of the 
    /// dockpane manipulate the undo/redo stack - performing undo and redo actions; remove undo actions and clearing the stacks. 
    /// </para>
    /// <para>
    /// 1. Open this solution in Visual Studio 2013.  
    /// 2. Click the Build menu. Then select Build Solution.
    /// 3. Click Start button to open ArcGIS Pro. ArcGIS Pro will open.
    /// 4. Open a project containing data.  
    /// 5. Click on the Add-in tab and see that 2 buttons are added to a Undo_Redo group.
    /// 6. Click the "Show Sample DockPane" button in the Undo_Redo group.  The Sample dockpane will be displayed
    /// 7. Ensure that a map is open.  
    /// 8. Use the Fixed Zoom In and Fixed Zoom Out buttons to see zoom in and zoom out operations added to the undo stack for the sample dockpane.
    /// 9. Use the Undo and Redo buttons to undo and redo the operations.  Use the Remove Operation button to pop an operation (without undoing it).  
    ///  Use the Clear All Operations button to clear all the operations of a particular category from the stack.    
    /// </para>
    /// ![UI](Screenshots/Screen.png)
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
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("PreRel_UndoRedo_Module"));
            }
        }


    }
}
