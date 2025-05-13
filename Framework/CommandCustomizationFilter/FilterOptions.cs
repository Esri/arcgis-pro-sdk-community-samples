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

namespace CommandCustomizationFilter
{
	/// <summary>
	/// Represents the ComboBox
	/// </summary>
	internal class FilterOptions : ComboBox
	{
		private List<string> _items = ["Filter Off / UI Enabled", "Filter On", "UI Disabled"];

		private bool _isInitialized;

		/// <summary>
		/// Combo Box constructor
		/// </summary>
		public FilterOptions()
		{
			UpdateCombo();
		}

		/// <summary>
		/// Updates the combo box with all the items.
		/// </summary>

		private void UpdateCombo()
		{
			// TODO – customize this method to populate the combobox with your desired items  
			if (_isInitialized)
				SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox


			if (!_isInitialized)
			{
				Clear();

				foreach (var item in _items)
				{
					Add(new ComboBoxItem(item));
				}
				_isInitialized = true;
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

			switch (item.Text)
			{
				case "Filter Off / UI Enabled":
					Module1.Current.MyCommandFilter.FilterOn = false;
					Module1.Current.MyCommandFilter.UIEnableOn = false;
					break;
				case "Filter On":
					Module1.Current.MyCommandFilter.FilterOn = true;
					Module1.Current.MyCommandFilter.UIEnableOn = false;
					break;
				case "UI Disabled":
					Module1.Current.MyCommandFilter.FilterOn = false;
					Module1.Current.MyCommandFilter.UIEnableOn = true;
					break;
			}
		}

	}
}
