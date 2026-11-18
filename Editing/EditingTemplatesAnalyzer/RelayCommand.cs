/*

   Copyright 2026 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EditingTemplatesAnalyzer
{
	internal class RelayCommand : ICommand
	{
		private readonly Func<Task> _execute;
		private readonly Func<bool>? _canExecute;

		public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

		public async void Execute(object? parameter) => await _execute();

		public event EventHandler? CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}
}
