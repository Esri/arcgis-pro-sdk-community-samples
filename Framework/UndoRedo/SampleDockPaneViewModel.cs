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
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace PreRel_UndoRedo
{
    internal class SampleDockPaneViewModel : DockPane
    {
        internal static string _dockPaneID = "PreRel_UndoRedo_SampleDockPane";

        // add an operation manager
        private OperationManager _operationManager = new OperationManager();
        public override OperationManager OperationManager
        {
            get
            {
                return _operationManager;
            }
        }

        
        protected SampleDockPaneViewModel() 
        {
          _fixedZoomInCmd = new RelayCommand(() => FixedZoomIn(), () => true);
          _fixedZoomOutCmd = new RelayCommand(() => FixedZoomOut(), () => true);

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
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        #region Zoom Commands

        private RelayCommand _fixedZoomInCmd;
        public ICommand FixedZoomInCommand
        {
          get { return _fixedZoomInCmd; }
        }

        private RelayCommand _fixedZoomOutCmd;
        public ICommand FixedZoomOutCommand
        {
          get { return _fixedZoomOutCmd; }
        }

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

        internal bool CanUndo
        {
          get 
          {
            if (OperationManager != null)
             return OperationManager.CanUndo;

            return false;
          }
        }

        private bool CanRedo
        {
          get 
          {
            if (OperationManager != null)
             return OperationManager.CanRedo;

            return false;
          }
        }
        private RelayCommand _undoCmd;
        public ICommand UndoCommand
        {
          get { return _undoCmd; }
        }

        private RelayCommand _redoCmd;
        public ICommand RedoCommand
        {
          get { return _redoCmd; }
        }

        private async Task Undo()
        {
          if (OperationManager != null)
          {
            if (OperationManager.CanUndo)
              await OperationManager.UndoAsync();
          }
        }

        private async Task Redo()
        {
          if (OperationManager != null)
          {
            if (OperationManager.CanRedo)
              await OperationManager.RedoAsync();
          }
        }

        private RelayCommand _removeOperationCmd;
        public ICommand RemoveOperationCommand
        {
          get { return _removeOperationCmd; }
        }

        private RelayCommand _clearOperationsCommand;
        public ICommand ClearOperationsCommand
        {
          get { return _clearOperationsCommand; }
        }

        private void RemoveOperation()
        {
          if (OperationManager != null)
          { 
            // find all the operations of my category
            List<Operation> ops = OperationManager.FindUndoOperations(o => o.Category == PreRel_UndoRedo.Category);
            // remove the last one
            if ((ops != null) && (ops.Count > 0))
              OperationManager.RemoveUndoOperation(ops[ops.Count - 1]);
          }
        }

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
