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
using ArcGIS.Desktop.Internal.Catalog.PropertyPages.NetworkDataset;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ZoomToSelectedFeatures
{
	/// <summary>
	/// Represents the ComboBox
	/// </summary>
	internal class SelectLayerComboBox : ComboBox
	{

		private bool _isInitialized;
		private Dictionary<MapMember, List<long>> _currentSelection { get; set; }
		private KeyValuePair<MapMember, List<long>>? _selectedItem { get; set; }

		/// <summary>
		/// Use to expand the view after zooming to the selected features
		/// </summary>
		public double ExpandValue { get; set; } = 0;


		private int CurrentSelectionIndex { get; set; } = 0;

		/// <summary>
		/// Combo Box constructor
		/// </summary>
		public SelectLayerComboBox()
		{
			Module1.TheSelectLayerComboBox = this;
			UpdateCombo();
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

			if (_currentSelection == null)
			{
				_selectedItem = null;
				return;
			}
			_selectedItem = _currentSelection.FirstOrDefault((e) => e.Key.Name == item.Text);
			CurrentSelectionIndex = 0;
			if (Module1.TheMoveBackButton != null)
				Module1.TheMoveBackButton.Enabled = CurrentSelectionIndex != 0;
			if (Module1.TheMoveForwardButton != null)
				Module1.TheMoveForwardButton.Enabled = CurrentSelectionIndex != _selectedItem.Value.Value.Count - 1;
			if (MapView.Active == null)
				return;
			ShowSelection(MapView.Active);
		}

		/// <summary>
		/// Updates the combo box with all the items.
		/// </summary>

		private async void UpdateCombo()
		{
			// customize this method to populate the combobox
			if (_isInitialized)
				SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
			if (!_isInitialized)
			{
				Clear();
				// setup events when selection changes
				MapSelectionChangedEvent.Subscribe(OnMapSelectionChangedEvent);

				// Add all layers with selections to this map
				if (MapView.Active?.Map != null)
				{
					var currentSelection = await QueuedTask.Run<SelectionSet>(() =>
					{
						return MapView.Active.Map.GetSelection();
					});
					OnMapSelectionChangedEvent(new MapSelectionChangedEventArgs(MapView.Active.Map, currentSelection, false));
				}
				_isInitialized = true;
			}
			Enabled = true; //enables the ComboBox
			SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
		}

		//Event handler when the MapSelection event is triggered.
		private async void OnMapSelectionChangedEvent(MapSelectionChangedEventArgs mapSelectionChangedArgs)
		{
			var currentSelection = mapSelectionChangedArgs.Selection.ToDictionary();
			Clear();
			// For each map member with a selection on it.
			foreach (var selectedKeyValue in currentSelection)
			{
				var mapMember = selectedKeyValue.Key;
				if (!(mapMember is BasicFeatureLayer layer))
					continue;
				var comboBoxItem = await QueuedTask.Run<ComboBoxItem>(() =>
				{
					return MakeComboBoxItem(layer.GetDefinition() as CIMFeatureLayer);
				});
				Add(comboBoxItem);
			}
			_currentSelection = currentSelection;
		}

		static ComboBoxItem MakeComboBoxItem(CIMFeatureLayer cimFeatureLayer)
		{
			var toolTip = $@"Select this feature layer: {cimFeatureLayer.Name}";
			if (cimFeatureLayer.Renderer is not CIMSimpleRenderer cimRenderer)
			{
				return new ComboBoxItem(cimFeatureLayer.Name, null, toolTip);
			}
			var si = new SymbolStyleItem()
			{
				Symbol = cimRenderer.Symbol.Symbol,
				PatchHeight = 16,
				PatchWidth = 16
			};
			var bm = si.PreviewImage as BitmapSource;
			bm.Freeze();
			return new ComboBoxItem(cimFeatureLayer.Name, bm, toolTip);
		}


		public void MoveSelection(bool forward)
		{
			if (_selectedItem == null)
				return;
			if (MapView.Active?.Map == null)
				return;
			var mapView = MapView.Active;
			if (forward && CurrentSelectionIndex < _selectedItem.Value.Value.Count - 1)
				CurrentSelectionIndex++;
			else if (!forward && CurrentSelectionIndex > 0)
				CurrentSelectionIndex--;
			else
				return;
			if (Module1.TheMoveBackButton != null)
				Module1.TheMoveBackButton.Enabled = CurrentSelectionIndex != 0;
			if (Module1.TheMoveForwardButton != null)
				Module1.TheMoveForwardButton.Enabled = CurrentSelectionIndex != _selectedItem.Value.Value.Count - 1;
			ShowSelection(mapView);
		}

		private async void ShowSelection (MapView mapView)
		{
			var mapMember = _selectedItem.Value.Key;
			if (mapMember is FeatureLayer featureLayer)
			{
				var geometry = await QueuedTask.Run<Geometry>(() =>
				{
					QueryFilter qf = new()
					{
						ObjectIDs = [_selectedItem.Value.Value[CurrentSelectionIndex]]
					};
					using RowCursor rowCursor = featureLayer.Search(qf);
					while (rowCursor.MoveNext())
					{
						return (rowCursor.Current as Feature).GetShape().Clone();
					}
					return null;
				});
				if (geometry != null)
				{
					var envelopeGeometry = await QueuedTask.Run<Geometry>(() =>
					{
						double dPercent = 1.0 + ExpandValue / 100.0;
						var g = geometry.Extent.Expand(dPercent, dPercent, true);
						return new PolygonBuilderEx(g).ToGeometry();
					});
					//await mapView.ZoomToAsync(geometry);
					await mapView.ZoomToAsync(envelopeGeometry, new TimeSpan(0, 0, 1));
					mapView.FlashFeature(featureLayer, _selectedItem.Value.Value[CurrentSelectionIndex]);
				}
			}
		}

	}
}
