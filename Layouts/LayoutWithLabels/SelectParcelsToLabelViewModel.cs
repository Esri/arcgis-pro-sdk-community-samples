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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
using ArcGIS.Desktop.Internal.Layouts.Control;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;


namespace LayoutWithLabels
{
	internal class SelectParcelsToLabelViewModel : DockPane
	{
		private const string _dockPaneID = "LayoutWithLabels_SelectParcelsToLabel";
		private readonly SetPages _setPages = new SetPages();
		private readonly ObservableCollection<SetPage> _mapPageLayouts = new ObservableCollection<SetPage>();
		private bool _cmdLoadLyrPackageEnabled = true;
		private bool _cmdReadLabelsEnabled = false;
		private bool _cmdMakeLayoutEnabled = false;
		private DataTable _LabelsLayer;

		protected SelectParcelsToLabelViewModel() 
		{
		}

		#region Commands
		public bool CmdLoadLyrPackageEnabled
		{
			get
			{
				return _cmdLoadLyrPackageEnabled;
			}
			set { SetProperty(ref _cmdLoadLyrPackageEnabled, value, () => CmdLoadLyrPackageEnabled); }
		}

		public bool CmdReadLabelsEnabled
		{
			get
			{
				return _cmdReadLabelsEnabled;
			}
			set { SetProperty(ref _cmdReadLabelsEnabled, value, () => CmdReadLabelsEnabled); }
		}

		public bool CmdMakeLayoutEnabled
		{
			get
			{
				return _cmdMakeLayoutEnabled;
			}
			set { SetProperty(ref _cmdMakeLayoutEnabled, value, () => CmdMakeLayoutEnabled); }
		}

		public ICommand CmdLoadLyrPackage
		{
			get
			{
				return new RelayCommand(async () =>
				{
					try
					{
						var lyrPckg = @"C:\Data\LocalGovernment\PolygonLabelLayer.lpkx";
						if (!System.IO.File.Exists(lyrPckg))
							throw new Exception($@"Layer Package not found: {lyrPckg}");
						await QueuedTask.Run(() =>
						{
							Uri myUri = new Uri(lyrPckg);
							var labelLayer = LayerFactory.Instance.CreateFeatureLayer(myUri,
								MapView.Active.Map, LayerPosition.AddToTop) as FeatureLayer;
							// clear any old data
							var qf = new QueryFilter()
							{
								WhereClause = "1=1"
							};
							labelLayer.GetTable().DeleteRows(qf);
						});
						CmdLoadLyrPackageEnabled = false;
						CmdReadLabelsEnabled = true;
					}
					catch (Exception ex)
					{
						MessageBox.Show($@"Error: {ex.ToString()}");
					}
				}, true);
			}
		}

		/// <summary>
		/// Method (disabled) to add a record manually
		/// </summary>
		public ICommand CmdAddLabel
		{
			get
			{
				return new RelayCommand(async () =>
				{
					//MessageBox.Show($@"Add id: {LabelId} color: {LabelType.ColorDescription} Label: {LabelLabel} Description: {LabelDescription} ");


					// find the labeling layer and add the table content
					var fcName = "PolygonLabels";
					var labelLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where((l) => l.Name == fcName).FirstOrDefault() as FeatureLayer;
					if (labelLayer == null)
					{
						MessageBox.Show($@"Unable to find {fcName} in the active map");
						return;
					}
					var parcelLayerName = "TaxParcels";
					var parcelLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(f => f.Name == parcelLayerName);
					if (parcelLayer == null)
					{
						MessageBox.Show($@"Unable to find {parcelLayerName} in the active map");
						return;
					}
					await QueuedTask.Run(() =>
					{
						var fcSource = parcelLayer.GetTable() as FeatureClass;
						if (fcSource == null) return;
						Geometry parcelPolygon = null;

						// search the source of the matching parcel record
						var parcelIdFieldName = "LOC_ID";
						var qf = new QueryFilter()
						{
							WhereClause = $@"{parcelIdFieldName} = '{LabelId}'"
						};
						using (var cursor = fcSource.Search(qf))
						{
							if (cursor.MoveNext())
							{
								using (var parcelFeature = cursor.Current as Feature)
								{
									parcelPolygon = parcelFeature.GetShape().Clone();
								}
							}
						}
						if (parcelPolygon == null)
						{
							MessageBox.Show($@"The add-in was not able to find parcel where '{qf.WhereClause}' is true.");
							return;
						}
						// TODO: it is possible that the shape geometry has to be projected 
						// if source and destination spatial references are different
						// fill in all attribute data that we need for the label polygon feature
						var attributes = new Dictionary<string, object>
						{
							{ labelLayer.GetFeatureClass().GetDefinition().GetShapeField(), parcelPolygon },
							{ "Type", LabelType.Value },
							{ "Label", LabelLabel },
							{ "ID", LabelId },
							{ "Description", LabelDescription }
						};

						var editOperation = new EditOperation
						{
							Name = "Add a new Label"
						};
						// create the polygon label feature
						editOperation.Create(labelLayer, attributes);
						if (!editOperation.Execute())
						{
							MessageBox.Show($@"Unable to create a new '{labelLayer}' record: {editOperation.ErrorMessage}");
						}
					});

					_ = LoadLabelTableAsync(labelLayer);
				}, true);
			}
		}

		public ICommand CmdReadLabels
		{
			get
			{
				return new RelayCommand(async () =>
				{
					// read csv file with labels
					// Columns: ID, Type, Description, Label
					var labelLines = System.IO.File.ReadAllLines(@"C:\Data\LocalGovernment\Labels.csv");

					// find the labeling layer and add the table content
					var fcName = "PolygonLabels";
					var labelLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where((l) => l.Name == fcName).FirstOrDefault() as FeatureLayer;
					if (labelLayer == null)
					{
						MessageBox.Show($@"Unable to find {fcName} in the active map");
						return;
					}
					var parcelLayerName = "TaxParcels";
					var parcelLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(f => f.Name == parcelLayerName);
					if (parcelLayer == null)
					{
						MessageBox.Show($@"Unable to find {parcelLayerName} in the active map");
						return;
					}
					await QueuedTask.Run(() =>
					{
                        if (!(parcelLayer.GetTable() is FeatureClass fcSource)) return;
                        Geometry parcelPolygon = null;

						foreach (var line in labelLines)
						{
							if (string.IsNullOrEmpty(line)) continue;
							var parts = line.Split(',');
							if (parts.Length != 4) continue;
							var theLabelId = parts[0];
							var theType = parts[1];
							var theDescription = parts[2];
							var theLabel = parts[3];
							// search the source of the matching parcel record
							var parcelIdFieldName = "LOC_ID";
							var qf = new QueryFilter()
							{
								WhereClause = $@"{parcelIdFieldName} = '{theLabelId}'"
							};
							using (var cursor = fcSource.Search(qf))
							{
								if (cursor.MoveNext())
								{
									using (var parcelFeature = cursor.Current as Feature)
									{
										parcelPolygon = parcelFeature.GetShape().Clone();
									}
								}
							}
							if (parcelPolygon == null)
							{
								MessageBox.Show($@"The add-in was not able to find parcel where '{qf.WhereClause}' is true.");
								return;
							}
							// TODO: it is possible that the shape geometry has to be projected 
							// if source and destination spatial references are different
							// fill in all attribute data that we need for the label polygon feature
							var attributes = new Dictionary<string, object>
							{
								{ labelLayer.GetFeatureClass().GetDefinition().GetShapeField(), parcelPolygon },
								{ "Type", Convert.ToInt16(theType) },
								{ "Label", theLabel },
								{ "ID", theLabelId },
								{ "Description", theDescription }
							};

							var editOperation = new EditOperation
							{
								Name = "Add a new Label"
							};
							// create the polygon label feature
							editOperation.Create(labelLayer, attributes);
							if (!editOperation.Execute())
							{
								MessageBox.Show($@"Unable to create a new '{labelLayer}' record: {editOperation.ErrorMessage}");
							}
						}
					});
					_ = LoadLabelTableAsync(labelLayer);
					CmdReadLabelsEnabled = false;
					CmdMakeLayoutEnabled = true;
				}, false);
			}
		}

		public ICommand CmdStartLabel
		{
			get
			{
				return new RelayCommand(() =>
				{
					// find the labeling layer and add the table content
					var fcName = "PolygonLabels";
					var fcLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where((l) => l.Name == fcName).FirstOrDefault() as FeatureLayer;
					if (fcLayer == null)
					{
						MessageBox.Show($@"Unable to find {fcName} in the active map");
						return;
					}
					CreateTableControlContent(fcLayer);

					_ = LoadLabelTableAsync(fcLayer);
				}, true);
			}
		}

		public DataTable LabelsLayer
		{
			get {
				return _LabelsLayer;
			}
			set { SetProperty(ref _LabelsLayer, value, () => LabelsLayer); }
		}


		private DataRowView _selectedLabel;

		/// <summary>
		/// One row of the label feature grid was selected
		/// </summary>
		public DataRowView SelectedLabel
		{
			get
			{
				return _selectedLabel;
			}
			set
			{
				SetProperty(ref _selectedLabel, value, () => SelectedLabel);
				if (_selectedLabel == null || LabelsLayer == null) return;
				FlashSelectedLabel(_selectedLabel);
			}
		}


		public ICommand CmdMakeLayout
		{
			get
			{
				return new RelayCommand(async () =>
				{
					var currentCamera = MapView.Active?.Camera;
					try
					{
						var theLayout = await QueuedTask.Run<Layout>(() =>
						{
							//Set up a page
							CIMPage newPage = new CIMPage
							{
								//required
								Width = SelectedPageLayout.Width,
								Height = SelectedPageLayout.Height,
								Units = SelectedPageLayout.LinearUnit
							};
							Layout layout = LayoutFactory.Instance.CreateLayout(newPage);
							layout.SetName(SelectedPageLayout.LayoutName);

							//Add Map Frame
							Coordinate2D llMap = new Coordinate2D(SelectedPageLayout.MarginLayout, SelectedPageLayout.MarginLayout);
							Coordinate2D urMAP = new Coordinate2D(SelectedPageLayout.WidthMap, SelectedPageLayout.Height - SelectedPageLayout.MarginLayout);
							Envelope envMap = EnvelopeBuilder.CreateEnvelope(llMap, urMAP);

							//Reference map, create Map Frame and add to layout
							MapProjectItem mapPrjItem = Project.Current.GetItems<MapProjectItem>().FirstOrDefault(item => item.Name.Equals("Map"));
							Map theMap = mapPrjItem.GetMap();
							MapFrame mfElm = LayoutElementFactory.Instance.CreateMapFrame(layout, envMap, theMap);
							mfElm.SetName(SelectedPageLayout.MapFrameName);
							if (currentCamera != null)
								mfElm.SetCamera(currentCamera);

							//Scale bar
							Coordinate2D llScalebar = new Coordinate2D(2 * SelectedPageLayout.MarginLayout, 2 * SelectedPageLayout.MarginLayout);
							LayoutElementFactory.Instance.CreateScaleBar(layout, llScalebar, mfElm);

							//NorthArrow
							Coordinate2D llNorthArrow = new Coordinate2D(SelectedPageLayout.WidthMap - 2 * SelectedPageLayout.MarginLayout, 2 * SelectedPageLayout.MarginLayout);
							var northArrow = LayoutElementFactory.Instance.CreateNorthArrow(layout, llNorthArrow, mfElm);
							northArrow.SetAnchor(Anchor.CenterPoint);
							northArrow.SetLockedAspectRatio(true);
							northArrow.SetWidth(2 * northArrow.GetWidth());

							// Title: dynamic text: <dyn type="page" property="name"/>
							var title = @"Title goes here";
							Coordinate2D llTitle = new Coordinate2D(SelectedPageLayout.XOffsetMapMarginalia, SelectedPageLayout.Height - 2 * SelectedPageLayout.MarginLayout);
							var titleGraphics = LayoutElementFactory.Instance.CreatePointTextGraphicElement(layout, llTitle, null) as TextElement;
							titleGraphics.SetTextProperties(new TextProperties(title, "Arial", 16, "Bold"));

							// Table 1
							AddTableToLayout(layout, theMap, mfElm, "PolygonLabels", SelectedPageLayout, 3 * SelectedPageLayout.HeightPartsMarginalia);

							// legend
							Coordinate2D llLegend = new Coordinate2D(SelectedPageLayout.XOffsetMapMarginalia, SelectedPageLayout.MarginLayout);
							Coordinate2D urLegend = new Coordinate2D(SelectedPageLayout.XOffsetMapMarginalia + SelectedPageLayout.XWidthMapMarginalia, SelectedPageLayout.HeightPartsMarginalia);
							System.Diagnostics.Debug.WriteLine(mfElm);
							LayoutElementFactory.Instance.CreateLegend(layout, EnvelopeBuilder.CreateEnvelope(llLegend, urLegend), mfElm);

							return layout;
						});

						//CREATE, OPEN LAYOUT VIEW (must be in the GUI thread)
						ILayoutPane layoutPane = await LayoutFrameworkExtender.CreateLayoutPaneAsync(ProApp.Panes, theLayout);
					}
					catch (Exception ex)
					{
						MessageBox.Show($@"Error in create layout: {ex}");
					}
				}, () => SelectedPageLayout != null);
			}
		}

		#endregion Commands

		#region Label Add Properties

		private SetPage _myPageLyt = null;

		public SetPage SelectedPageLayout
		{
			get { return _myPageLyt; }
			set { SetProperty(ref _myPageLyt, value, () => SelectedPageLayout); }
		}

		public IReadOnlyCollection<SetPage> PageLayouts
		{
			get
			{
				if (_mapPageLayouts.Count == 0)
				{
					foreach (var setPg in _setPages.SetPageList)
					{
						_mapPageLayouts.Add(setPg);
					}
				}
				return _mapPageLayouts;
			}
		}

		/// <summary>
		/// F_365342_2845718
		/// F_366982_2847867
		/// F_366770_2847990
		/// F_366808_2847835
		/// </summary>
		private string _LabelId = "F_365342_2845718";
		public string LabelId
		{
			get { return _LabelId; }
			set
			{
				SetProperty(ref _LabelId, value, () => LabelId);
			}
		}

		public List<TypeColor> TypeColors
		{
			get
			{
				var result = TypeColor.GetTypeColors();
				if (result.Count > 0) _lt = result[0];
				return result;
			}
		}

		private Int16 _LabelType = -1;
		private TypeColor _lt = null;
		public TypeColor LabelType
		{
			get { return _lt; }
			set
			{
				SetProperty(ref _lt, value, () => LabelType);
				_LabelType = _lt.Value;
			}
		}
		
		private string _LabelDescription = "F_365342_2845718 Description";
		public string LabelDescription
		{
			get { return _LabelDescription; }
			set
			{
				SetProperty(ref _LabelDescription, value, () => LabelDescription);
			}
		}

		private string _LabelLabel = "Comp 1";
		public string LabelLabel
		{
			get { return _LabelLabel; }
			set
			{
				SetProperty(ref _LabelLabel, value, () => LabelLabel);
			}
		}

		private double _LeaderOffset;
		public double LeaderOffset
		{
			get { return _LeaderOffset; }
			set
			{
				SetProperty(ref _LeaderOffset, value, () => LeaderOffset);

				var fcName = "PolygonLabels";
				var labelLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where((l) => l.Name == fcName).FirstOrDefault() as FeatureLayer;
				if (labelLayer == null)
				{
					MessageBox.Show($@"Unable to find {fcName} in the active map");
					return;
				}
				QueuedTask.Run(() =>
				{
					//Get the layer's definition
					var lyrDefn = labelLayer.GetDefinition() as CIMFeatureLayer;
					//Get the label classes - we need the first one
					var listLabelClasses = lyrDefn.LabelClasses.ToList();
					var theLabelClass = listLabelClasses.FirstOrDefault();
					// change leader line length
					theLabelClass.MaplexLabelPlacementProperties.PrimaryOffset = _LeaderOffset;
					theLabelClass.MaplexLabelPlacementProperties.PrimaryOffsetUnit = MaplexUnit.MM;
					lyrDefn.LabelClasses = listLabelClasses.ToArray(); //Set the labelClasses back
					labelLayer.SetDefinition(lyrDefn);
				});
			}
		}

		private TableControlContent _labelsTableContent = null;
		public TableControlContent LabelsTableContent
		{
			get { return _labelsTableContent; }
			set
			{
				SetProperty(ref _labelsTableContent, value, () => LabelsTableContent);
			}
		}

		private void CreateTableControlContent(MapMember mapTableToAdd)
		{
			TableControlContent tableContent = null;

			if (TableControlContentFactory.IsMapMemberSupported(mapTableToAdd))
				tableContent = TableControlContentFactory.Create(mapTableToAdd);

			LabelsTableContent = tableContent;
		}


		#endregion

		private async void FlashSelectedLabel(DataRowView selectedLabelrv)
		{
			// Flash the Feature
			// find the labeling layer and add the table content
			var fcName = "PolygonLabels";
			var labelLayer = MapView.Active.Map.GetLayersAsFlattenedList().Where((l) => l.Name == fcName).FirstOrDefault() as FeatureLayer;
			if (labelLayer == null)
			{
				MessageBox.Show($@"Unable to find {fcName} in the active map");
				return;
			}
			var oid = await QueuedTask.Run(() =>
			{
				var qf = new QueryFilter()
				{
					WhereClause = $@"{selectedLabelrv.Row.Table.Columns[0]} = '{selectedLabelrv.Row[0]}'"
				};
				long ret = -1;
				using (var cursor = labelLayer.Search(qf))
				{
					if (cursor.MoveNext())
					{
						using (var parcelFeature = cursor.Current as Feature)
						{
							ret = parcelFeature.GetObjectID();
						}
					}
				}
				return ret;
			});
			if (oid == -1) return;
			IReadOnlyDictionary<BasicFeatureLayer, List<long>> flashFeature = new Dictionary<BasicFeatureLayer, List<long>>()
										{{labelLayer as BasicFeatureLayer, new List<long>(){oid}}};
			FlashFeaturesAsync(flashFeature);

		}

		/// <summary>
		/// Flash the selected features
		/// </summary>
		/// <param name="flashFeatures"></param>
		private async void FlashFeaturesAsync(IReadOnlyDictionary<BasicFeatureLayer, List<long>> flashFeatures)
		{
			//Get the active map view.
			var mapView = MapView.Active;
			if (mapView == null)
				return;
			await QueuedTask.Run(() =>
			{
				//Flash the collection of features.
				mapView.FlashFeature(flashFeatures);
			});
		}

		private CIMPointSymbol GetPointSymbolFromLayer(Layer layer)
		{
			if (!(layer is FeatureLayer)) return null;
			var fLyr = layer as FeatureLayer;
			var renderer = fLyr.GetRenderer() as CIMSimpleRenderer;
			if (renderer == null || renderer.Symbol == null) return null;
			return renderer.Symbol.Symbol as CIMPointSymbol;
		}

		private FeatureLayer _featureLayer = null;
		private async Task LoadLabelTableAsync(FeatureLayer fcLayer)
		{
			if (fcLayer == null) fcLayer = _featureLayer;
			if (fcLayer == null) return;
			var isNew = _featureLayer == null;
			_featureLayer = fcLayer;
			var listColumnNames = new Dictionary<string, string>();
			var listValues = new List<List<string>>();
			await QueuedTask.Run(() =>
			{
				foreach (var fd in fcLayer.GetFieldDescriptions())
				{
					if (fd.IsVisible && fd.Type == FieldType.String) listColumnNames.Add(fd.Name, fd.Alias);
				}
				// Get all features for FeatureLayer
				// and populate a DataTable with data and column headers
				using (var rowCursor = fcLayer.Search(null))
				{
					while (rowCursor.MoveNext())
					{
						using (var anyRow = rowCursor.Current)
						{
							var newRow = new List<string>();
							foreach (var fld in anyRow.GetFields().Where(fld => fld.FieldType != FieldType.Geometry))
							{
								if (listColumnNames.ContainsKey(fld.Name))
								{
									newRow.Add((anyRow[fld.Name] == null) ? string.Empty : anyRow[fld.Name].ToString());
								}
							}
							listValues.Add(newRow);
						}
					}
				}
			});

			if (isNew)
			{
				_LabelsLayer = new DataTable();
				foreach (var col in listColumnNames)
				{
					_LabelsLayer.Columns.Add(new DataColumn(col.Key, typeof(string)) { Caption = col.Value });
				}
			}
			else
			{
				_LabelsLayer.Clear();
			}
			foreach (var row in listValues)
			{
				var newRow = _LabelsLayer.NewRow();
				newRow.ItemArray = row.ToArray();
				_LabelsLayer.Rows.Add(newRow);
			}
			NotifyPropertyChanged(() => LabelsLayer);
		}

		private void AddTableToLayout(Layout layout, Map theMap, MapFrame mfElm, string layerName, SetPage setPage, double yOffset)
		{
			var lyrs = theMap.FindLayers(layerName, true);
			if (lyrs.Count > 0)
			{
				Layer lyr = lyrs[0];
				
				Coordinate2D llTab1 = new Coordinate2D(setPage.XOffsetMapMarginalia, yOffset - 2*setPage.HeightPartsMarginalia);
				Coordinate2D urTab1 = new Coordinate2D(setPage.XOffsetMapMarginalia + setPage.XWidthMapMarginalia, yOffset);
				var theTable = LayoutElementFactory.Instance.CreateTableFrame(layout, EnvelopeBuilder.CreateEnvelope(llTab1, urTab1), mfElm, lyr, new string[] { "Label", "Description" });
				var def = theTable.GetDefinition() as CIMTableFrame;
				def.FillingStrategy = TableFrameFillingStrategy.ShowAllRows;
				theTable.SetDefinition(def);
			}
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
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class SelectParcelsToLabel_ShowButton : Button
	{
		protected override void OnClick()
		{
			SelectParcelsToLabelViewModel.Show();
		}
	}
}
