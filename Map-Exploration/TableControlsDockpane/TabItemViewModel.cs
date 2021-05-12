/*

   Copyright 2020 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TableControlsDockpane
{
	public class TabItemViewModel : INotifyPropertyChanged
	{
		public ICommand TableCloseCommand { get; set; }

		public TabItemViewModel(MapMember mapMember,
														DependencyObject visualTreeRoot)
		{
			TableContent = TableControlContentFactory.Create(mapMember);
			MapMember = mapMember;
			VisualTreeRoot = visualTreeRoot;

			MenuItem zoomItem = new MenuItem()
			{
				Header = "Zoom to Feature",
				Command = ZoomToRowCommand,
				CommandParameter = this
			};
			RowContextMenu = new ContextMenu();
			RowContextMenu.Items.Add(zoomItem);
		}

		public TableControlContent TableContent { get; set; }

		public MapMember MapMember { get; set; }

		private int _lastRowIdx = -1;
private int _ActiveRowIdx;
public int ActiveRowIdx
{
	get { return _ActiveRowIdx; }
	set
	{
		_ActiveRowIdx = value;
		NotifyPropertyChanged(nameof(ActiveRowIdx));
		var tableControl = VisualTreeRoot.GetChildOfType<TableControl>();
		if (tableControl != null)
		{
			var rowIdx = tableControl.ActiveRowIndex;
					if (rowIdx == _lastRowIdx) return;
					_lastRowIdx = rowIdx;
			System.Diagnostics.Debug.WriteLine($@"row: {rowIdx}");
			_ = GetObjectIdAsync(tableControl, rowIdx);
		}
	}
}

public async Task<long> GetObjectIdAsync (TableControl tableControl, int rowIdx)
{
	var oid = await tableControl.GetObjectIdAsync(rowIdx);
	ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show ($@"rowIdx:  {rowIdx} has OID: {oid}");
	return oid;
}

		public DependencyObject VisualTreeRoot { get; set; }

		public ContextMenu RowContextMenu { get; set; }

		public string TableName => MapMember?.Name;

		private ICommand _zoomToRowCommand = null;
		public ICommand ZoomToRowCommand
		{
			get
			{
				if (_zoomToRowCommand == null)
				{
					_zoomToRowCommand = new RelayCommand(async () =>
					{
											// if we have some content, a map and our data is added to the map
											if (MapView.Active != null && MapMember != null && VisualTreeRoot != null)
						{
							var tableControl = VisualTreeRoot.GetChildOfType<TableControl>();
							if (tableControl != null)
							{
								var rowIdx = tableControl.ActiveRowIndex;
								System.Diagnostics.Debug.WriteLine($@"row: {rowIdx}");
								var oid = await tableControl.GetObjectIdAsync(rowIdx);

								var insp = new Inspector();
								_ = insp.LoadAsync(MapMember, oid).ContinueWith((t) =>
														{
																		// zoom
																		MapView.Active.ZoomToAsync(insp.Shape.Extent, new TimeSpan(0, 0, 0, 1));
																		// flash 
																		var basicFl = MapMember as BasicFeatureLayer;
													if (basicFl != null)
														MapView.Active.FlashFeature(basicFl, oid);
												});
							}
						}
					});
				}
				return _zoomToRowCommand;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
