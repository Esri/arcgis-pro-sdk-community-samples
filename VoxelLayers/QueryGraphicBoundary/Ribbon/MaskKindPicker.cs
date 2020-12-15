using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using ArcGIS.Desktop.Mapping;

namespace QueryGraphicBoundary.Ribbon
{
	/// <summary>
	/// Provides the different masking options as combobox items in the ribbon
	/// </summary>
	internal class MaskKindPicker : ComboBox
	{

		private bool _isInitialized;

		/// <summary>
		/// Combo Box constructor
		/// </summary>
		public MaskKindPicker()
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

				foreach(MaskKind kind in Enum.GetValues(typeof(MaskKind)))
				{
					Add(new ComboBoxItem($"{kind.ToString()}"));
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

			Module1.Current.SelectedMaskKind = (MaskKind)Enum.Parse(typeof(MaskKind), item.Text);
			// TODO  Code behavior when selection changes.    
		}

	}
}
