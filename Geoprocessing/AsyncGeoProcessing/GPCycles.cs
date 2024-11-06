/*

   Copyright 2024 Esri

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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncGeoProcessing
{
	/// <summary>
	/// Represents the ComboBox
	/// </summary>
	internal class GPCycles : ComboBox
	{

		private bool _isInitialized;

		/// <summary>
		/// Combo Box constructor
		/// </summary>
		public GPCycles()
		{
			UpdateCombo();
		}

		/// <summary>
		/// Updates the combo box with all the items.
		/// </summary>

		private void UpdateCombo()
		{
			if (_isInitialized)
				SelectedItem = ItemCollection.FirstOrDefault();
			if (!_isInitialized)
			{
				Clear();

				int[] values = [10, 100, 1000, 10000];
				
				foreach (int value in values)
				{
					Add(new ComboBoxItem($@"{value}"));
				}
				_isInitialized = true;
			}
			Enabled = true; //enables the ComboBox
			SelectedItem = ItemCollection.FirstOrDefault();
		}

		/// <summary>
		/// The on comboBox selection change event. 
		/// </summary>
		/// <param name="item">The newly selected combo box item</param>
		protected override void OnSelectionChange(ComboBoxItem item)
		{
			if (item == null)
				return;
			if (string.IsNullOrEmpty(item.Text))
				return;
			if (uint.TryParse (item.Text, out uint value))
			{
				Module1.Current.GPCycles = value;
			}
		}

	}
}
