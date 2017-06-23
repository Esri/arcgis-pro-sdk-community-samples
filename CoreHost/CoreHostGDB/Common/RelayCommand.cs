//Copyright 2017 Esri

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

namespace CoreHostGDB.Common {
    class RelayCommand : ICommand {
        Action _executeMethod;
        Func<bool> _canExecuteMethod;
        private bool _canExecute = true;

        /// <summary>
        /// Default constructor - provide the function to be executed
        /// </summary>
        /// <param name="execMethod">The function to be executed</param>
        public RelayCommand(Action execMethod)
            : this(execMethod, () => { return true; }) {
        }
        /// <summary>
        /// Provide a function to be executed and a canExecuteMethod to check if it is
        /// valid for the button to execute...
        /// </summary>
        /// <param name="execMethod">The execute method</param>
        /// <param name="canExecuteMethod">A function to check if the tool can be executed.</param>
        public RelayCommand(Action execMethod, Func<bool> canExecuteMethod) {
            this._executeMethod = execMethod;
            this._canExecuteMethod = canExecuteMethod;
        }

        #region Implement ICommand

        public bool CanExecute(object parameter) {
            if (_canExecuteMethod != null) {
                bool _result = _canExecuteMethod();
                if (_canExecute != _result) {
                    _canExecute = _result;
                    CanExecuteChanged?.Invoke(parameter, EventArgs.Empty);
                }
            }

            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) {
            _executeMethod();
            if (_canExecuteMethod != null) {
                CanExecuteChanged(parameter, EventArgs.Empty);
            }
        }

        public void RaiseCanExecuteChanged() {
          CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }  
}
