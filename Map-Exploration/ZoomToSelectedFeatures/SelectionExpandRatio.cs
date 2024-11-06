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

namespace ZoomToSelectedFeatures
{
	/// <summary>
	/// Represents the ComboBox
	/// </summary>
	internal class SelectionExpandRatio : ComboBox
	{

		private bool _isInitialized;

		/// <summary>
		/// Combo Box constructor
		/// </summary>
		public SelectionExpandRatio()
		{
			UpdateCombo();
		}

		/// <summary>
		/// Updates the combo box with all the items.
		/// </summary>

		private void UpdateCombo()
		{
			// populate the combobox with expand values
			if (_isInitialized)
				SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox


			if (!_isInitialized)
			{
				Clear();

				int[] expandValues = { 0,10,25,50,75,100 };
				_isInitialized = true;
				foreach (var expandValue in expandValues)
				{
					Add(new ComboBoxItem($@"{expandValue}"));
				}
			}

			Enabled = true; //enables the ComboBox
			SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox

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

			if (double.TryParse (item.Text, out double value))
			{
				if (value > 100.0) value %= 100.0;
				Module1.TheSelectLayerComboBox.ExpandValue = value;
			}

		}

	}
}
