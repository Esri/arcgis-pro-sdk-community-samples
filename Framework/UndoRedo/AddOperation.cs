//   Copyright 2018 Esri
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
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace PreRel_UndoRedo
{
    /// <summary>
    /// Button to illustrate how operations can be added to different OperationManagers. 
    /// </summary>
    /// <remarks>
    /// In ArcGIS Pro does not contain a single application undo/redo stack. In general, undo operations are organized per Pane and DockPane.
    /// 
    /// See <see cref="ArcGIS.Desktop.Framework.OperationManager"/> for additional information on the OperationManager and undo/redo stacks.
    /// </remarks>
    internal class AddOperation : Button
    {
        protected override async void OnClick()
        {
            Operation op = new MySampleOperation();

            // OPTION 1 = add the operation to a specific dockpane operation manager  (in this case, the sampleDockPane)
            DockPane pane = FrameworkApplication.DockPaneManager.Find(SampleDockPaneViewModel._dockPaneID);
            if (pane == null)
                return;

            SampleDockPaneViewModel vm = pane as SampleDockPaneViewModel;
            if (vm.OperationManager != null)
                await vm.OperationManager.DoAsync(op);


            // OR  OPTION 2 - add the operation to the active panes operation manager
            //Pane pane = FrameworkApplication.Panes.ActivePane;
            //if ((pane != null) && (pane.OperationManager != null))
            //    await pane.OperationManager.DoAsync(op);                

            // OR  OPTION 3 - add the operation to the active maps operation manager
            //if ((MappingModule.ActiveMapView != null) && (MappingModule.ActiveMapView.Map != null))
            //    await MappingModule.ActiveMapView.Map.OperationManager.DoAsync(op);

        }
    }
}
