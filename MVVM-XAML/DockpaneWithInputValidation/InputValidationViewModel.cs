/*

   Copyright 2025 Esri

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.KnowledgeGraph;
using System.ComponentModel;
using System.Collections;
using System.Text.RegularExpressions;

namespace DockpaneWithInputValidation
{
	internal class InputValidationViewModel : DockPane, INotifyDataErrorInfo
	{
		private const string _dockPaneID = "DockpaneWithInputValidation_InputValidation";

		protected InputValidationViewModel() { }

		#region Error Handling

		private readonly Dictionary<string, List<string>> _errorsByPropertyName = new Dictionary<string, List<string>>();

		public IEnumerable GetErrors(string propertyName) => _errorsByPropertyName.ContainsKey(propertyName)?
						_errorsByPropertyName[propertyName] : null;

		bool INotifyDataErrorInfo.HasErrors => _errorsByPropertyName.Any();

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		#endregion Error Handling

		#region MVVM Properties

		private string _IntegerInput;

		public string IntegerInput
		{
			get => _IntegerInput;
			set
			{
				if (IsIntegerRegex(value))
				{
					ClearErrors(nameof(IntegerInput));
					_IntegerInput = value;
				}
				else
				{
					AddError(nameof(IntegerInput), "Input must Integer");
				}
				SetProperty(ref _IntegerInput, value);
			}
		}

		private string _SignedIntegerInput;

		public string SignedIntegerInput
		{
			get => _SignedIntegerInput;
			set
			{
				if (IsSignedIntegerRegex(value))
				{
					ClearErrors(nameof(SignedIntegerInput));
					_SignedIntegerInput = value;
				}
				else
				{
					AddError(nameof(SignedIntegerInput), "Input must SignedInteger");
				}
				SetProperty(ref _SignedIntegerInput, value);
			}
		}

		private string _FloatInput;

		public string FloatInput
		{
			get => _FloatInput;
			set
			{
				if (IsFloatRegex(value))
				{
					ClearErrors(nameof(FloatInput));
					_FloatInput = value;
				}
				else
				{
					AddError(nameof(FloatInput), "Input must Float");
				}
				SetProperty(ref _FloatInput, value);
			}
		}
		#endregion MVVM Properties

		#region Validation Error Helpers

		private void AddError(string propertyName, string error)
		{
			if (!_errorsByPropertyName.ContainsKey(propertyName))
				_errorsByPropertyName[propertyName] = new List<string>();

			if (!_errorsByPropertyName[propertyName].Contains(error))
			{
				_errorsByPropertyName[propertyName].Add(error);
				OnErrorsChanged(propertyName);
			}
		}

		private void ClearErrors(string propertyName)
		{
			if (_errorsByPropertyName.ContainsKey(propertyName))
			{
				_errorsByPropertyName.Remove(propertyName);
				OnErrorsChanged(propertyName);
			}
		}

		private void OnErrorsChanged(string propertyName)
		{
			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

		}

		private static bool IsIntegerRegex(string value)
		{
			const string regex = @"^\d+$";
			return Regex.IsMatch(value, regex);
		}

		private static bool IsSignedIntegerRegex(string value)
		{
			const string regex = @"^[+-]?\d+$";
			return Regex.IsMatch(value, regex);
		}

		private static bool IsFloatRegex(string value)
		{
			const string regex = @"^[+-]?([0-9]*[.])?[0-9]+$";
			return Regex.IsMatch(value, regex);
		}

		#endregion Validation Error Helpers

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
		private string _heading = "Testing input validation";

		public string Heading
		{
			get => _heading;
			set => SetProperty(ref _heading, value);
		}

	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class InputValidation_ShowButton : Button
	{
		protected override void OnClick()
		{
			InputValidationViewModel.Show();
		}
	}
}
