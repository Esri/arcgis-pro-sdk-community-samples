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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace PreRel_UndoRedo
{
  /// <summary>
  /// Dockpane that illustrates how to manage an OperationManager and add undo/redo operations to it.  
  /// </summary>
  /// <remarks>
  /// The dockpane contains buttons that 
  /// - adds a simple zoom in operation to the undo stack
  /// - adds a simple zoom out operation to the undo stack
  /// - adds a composite operation to the undo stack
  /// - undoes an operation from the undo stack
  /// - redoes an operation from the redo stack
  /// - removes an operation from the undo stack
  /// - clears all operations of a specific category from the undo and redo stacks
  /// </remarks>
    internal class SampleDockPaneViewModel : DockPane
    {
        internal static string _dockPaneID = "PreRel_UndoRedo_SampleDockPane";


    /// <summary>
    /// Operation manager for the dockpane
    /// </summary>
        private OperationManager _operationManager = new OperationManager();
        public override OperationManager OperationManager
        {
      get { return _operationManager; }
        }

       
    /// <summary>
    /// constructor for the dockpane viewmodel. 
    /// </summary>
        protected SampleDockPaneViewModel() 
        {
          _fixedZoomInCmd = new RelayCommand(() => FixedZoomIn(), () => true);
          _fixedZoomOutCmd = new RelayCommand(() => FixedZoomOut(), () => true);

      _compositeZoomInCmd = new RelayCommand(() => CompositeZoomIn(), () => true);

          _undoCmd = new RelayCommand(() => Undo(), () => CanUndo);
          _redoCmd = new RelayCommand(() => Redo(), () => CanRedo);

          _removeOperationCmd = new RelayCommand(() => RemoveOperation(), () => CanUndo);
          _clearOperationsCommand = new RelayCommand(() => ClearOperations(), () => CanUndo || CanRedo);
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "My DockPane";
        public string Heading
        {
            get { return _heading; }
      set { SetProperty(ref _heading, value, () => Heading); }
        }

        #region Zoom Commands

    /// <summary>
    /// Composite zoom in command, binds to a button in the view
    /// </summary>
    private RelayCommand _compositeZoomInCmd;
    public ICommand CompositeZoomInCommand
    {
       get { return _compositeZoomInCmd; }
    }
          
    /// <summary>
    /// Fixed zoom in command, binds to a button in the view
    /// </summary>
        private RelayCommand _fixedZoomInCmd;
        public ICommand FixedZoomInCommand
        {
          get { return _fixedZoomInCmd; }
        }

    /// <summary>
    /// fixed zoom out command, binds to a button in the view
    /// </summary>
        private RelayCommand _fixedZoomOutCmd;
        public ICommand FixedZoomOutCommand
        {
          get { return _fixedZoomOutCmd; }
        }


    /// <summary>
    /// Action for the composite zoom in button.  Performs a 3x zoom in.
    /// </summary>
    /// <returns>A Task that represents the CompositeZoomIn method</returns>
    private Task CompositeZoomIn()
    {
      // composite operations need to be created on the worker thread
      return QueuedTask.Run(() =>
      {
        this.OperationManager.CreateCompositeOperation(() =>
        {
          for (int idx = 0; idx < 3; idx++)
          {
            MyZoomOperation op = new MyZoomOperation(true);
            this.OperationManager.Do(op);
          }
        }, "Zoom In 3x");
      });
    }

    /// <summary>
    /// Action for the fixed zoom in button
    /// </summary>
    /// <returns>A Task that represents the FixedZoomIn method</returns>
        private async Task FixedZoomIn()
        {
          MyZoomOperation op = new MyZoomOperation(true);
          await this.OperationManager.DoAsync(op);
        }

        private async Task FixedZoomOut()
        {
          MyZoomOperation op = new MyZoomOperation(false);
          await this.OperationManager.DoAsync(op);
        }
        #endregion

        #region Undo/Redo Commands

    /// <summary>
    /// Determines if the undo button can be enabled.  It should be enabled if operations exist on the undo stack of the OperationManager.
    /// </summary>
    /// <returns>returns True if operations exist</returns>
        internal bool CanUndo
        {
          get 
          {
            if (OperationManager != null)
             return OperationManager.CanUndo;

            return false;
          }
        }

    /// <summary>
    /// Determines if the redo button can be enabled. It should be enabled if operations exist on the redo stack of the OperationManager 
    /// </summary>
        private bool CanRedo
        {
          get 
          {
            if (OperationManager != null)
             return OperationManager.CanRedo;

            return false;
          }
        }
    
    /// <summary>
    /// The Undo command; binds to a button on the view
    /// </summary>
        private RelayCommand _undoCmd;
        public ICommand UndoCommand
        {
          get { return _undoCmd; }
        }

    /// <summary>
    ///  The Redo command; binds to a button on the view
    /// </summary>
        private RelayCommand _redoCmd;
        public ICommand RedoCommand
        {
          get { return _redoCmd; }
        }

    /// <summary>
    /// Action for the undo button; performs the undo action on the OperationManager
    /// </summary>
    /// <returns>A Task that represents the Undo method</returns>
        private async Task Undo()
        {
          if (OperationManager != null)
          {
            if (OperationManager.CanUndo)
              await OperationManager.UndoAsync();
          }
        }

    /// <summary>
    /// Action for the redo button; performs the redo action on the OperationManager
    /// </summary>
    /// <returns>A Task that represents the Redo method</returns>
        private async Task Redo()
        {
          if (OperationManager != null)
          {
            if (OperationManager.CanRedo)
              await OperationManager.RedoAsync();
          }
        }

    /// <summary>
    /// The RemoveOperation command; binds to a button on the view
    /// </summary>
        private RelayCommand _removeOperationCmd;
        public ICommand RemoveOperationCommand
        {
          get { return _removeOperationCmd; }
        }

    /// <summary>
    /// The Clear Operations command; binds to a button on the view
    /// </summary>
        private RelayCommand _clearOperationsCommand;
        public ICommand ClearOperationsCommand
        {
          get { return _clearOperationsCommand; }
        }

    /// <summary>
    /// Action for the RemoveOperation button; pops the most recent operation from the undo stack (without undoing it)
    /// </summary>
        private void RemoveOperation()
        {
          if (OperationManager != null)
          { 
            // find all the operations of my category
            List<Operation> ops = OperationManager.FindUndoOperations(o => o.Category == PreRel_UndoRedo.Category);
        // remove the most recent (this is the one at the top of the list)
            if ((ops != null) && (ops.Count > 0))
          OperationManager.RemoveUndoOperation(ops[0]);
          }
        }

    /// <summary>
    /// Action for the ClearOperations button; clears the undo and redo stacks of a specific category of operations
    /// </summary>
        private void ClearOperations()
        {
          if (OperationManager != null)
          {
            OperationManager.ClearUndoCategory(PreRel_UndoRedo.Category);
            OperationManager.ClearRedoCategory(PreRel_UndoRedo.Category);
          }
        }
        #endregion

    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class SampleDockPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            SampleDockPaneViewModel.Show();
        }
    }
}
