using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ParcelFabricAPI
{
	internal class ImportPlatViewModel : DockPane
	{
		private const string DockPaneID = "ParcelLayerAPI_ImportPlat";
		private const string LyrNameImport = "ImportPlats";
		private const string FieldNameIsland = "Island";
		private const string FieldNameZone = "Zone";
		private const string FieldNameSect = "Section";
		private const string FieldNamePlat = "Plat";
		private const string FieldNameParcel = "Parcel";
		private const string FieldNameTmk = "TMK";
		private const string FieldNameName = "Name";
		private const string FieldNameLink = "qpub_link";
		private const string LyrNameLot = "Lots";
		private const string LyrNameTax = "Tax";

		private Dictionary<long, Dictionary<long, Dictionary<string, long>>> _zoneSectPlat = new Dictionary<long, Dictionary<long, Dictionary<string, long>>>();
		private FeatureLayer _importParcelLineLayer = null;
		private ParcelLayer _parcelFabricLayer = null;
		private FeatureLayer _recordLayer = null;
		private FeatureLayer _lotLayerPolys = null;
		private FeatureLayer _taxLayerPolys = null;
		private FeatureLayer _lotLayerLines = null;
		private FeatureLayer _taxLayerLines = null;
		private int _importParcelCount;

		protected ImportPlatViewModel()
		{
			CheckNewLayers();
			ProjectClosedEvent.Subscribe(OnProjectClosed);
			//Subscribe to ActiveMapViewChangedEvent in order to get the layers in the map
			ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
		}

		#region Properties

		private string _importStatus;
		public string ImportStatus
		{
			get { return _importStatus; }
			set
			{
				SetProperty(ref _importStatus, value, () => ImportStatus);
				ImportStatusBrush = _importStatus.StartsWith("Err") ? Brushes.Red : Brushes.Black;
			}
		}

		private Brush _importStatusBrush = Brushes.AliceBlue;
		public Brush ImportStatusBrush
		{
			get { return _importStatusBrush; }
			set
			{
				SetProperty(ref _importStatusBrush, value, () => ImportStatusBrush);
			}
		}

		private ObservableCollection<long> _zones = new ObservableCollection<long>();
		public ObservableCollection<long> Zones
		{
			get { return _zones; }
			set
			{
				SetProperty(ref _zones, value, () => Zones);
			}
		}

		private ObservableCollection<long> _sections = new ObservableCollection<long>();
		public ObservableCollection<long> Sections
		{
			get { return _sections; }
			set
			{
				SetProperty(ref _sections, value, () => Sections);
			}
		}

		private ObservableCollection<string> _plats = new ObservableCollection<string>();
		public ObservableCollection<string> Plats
		{
			get { return _plats; }
			set
			{
				SetProperty(ref _plats, value, () => Plats);
			}
		}

		private string _platsLabel = LyrNameImport;
		public string PlatsLabel
		{
			get { return _platsLabel; }
			set
			{
				SetProperty(ref _platsLabel, value, () => PlatsLabel);
			}
		}

		private long _selectedZone;
		public long SelectedZone
		{
			get { return _selectedZone; }
			set
			{
				SetProperty(ref _selectedZone, value, () => SelectedZone);
				SelectSetAndZoomAsync(_importParcelLineLayer, $@"{FieldNameZone} = {_selectedZone}");
				Sections.Clear();
				var lst = new List<long>();
				lst.AddRange(_zoneSectPlat[_selectedZone].Keys);
				lst.Sort();
				Sections.AddRange(lst);
			}
		}

		private long _selectedSection;
		public long SelectedSection
		{
			get { return _selectedSection; }
			set
			{
				SetProperty(ref _selectedSection, value, () => SelectedSection);
				SelectSetAndZoomAsync(_importParcelLineLayer, $@"{FieldNameZone} = {_selectedZone} and {FieldNameSect} = {_selectedSection}");
				Plats.Clear();
				var lst = new List<string>();
				lst.AddRange(_zoneSectPlat[_selectedZone][_selectedSection].Keys);
				lst.Sort();
				Plats.AddRange(lst);
			}
		}

		private string _selectedPlat;
		public string SelectedPlat
		{
			get { return _selectedPlat; }
			set
			{
				SetProperty(ref _selectedPlat, value, () => SelectedPlat);
				if (_selectedPlat == null)
				{
					PlatSelectedText = $@"Select a plat [{LyrNameImport}]";
				}
				else
				{
					PlatSelectedText = $@"Import {_zoneSectPlat[_selectedZone][_selectedSection][_selectedPlat]} parcel boundaries";
					var qry = $@"{FieldNameZone} = {_selectedZone} and {FieldNameSect} = {_selectedSection} and {FieldNamePlat} = '{_selectedPlat}'";
					if (!string.IsNullOrEmpty(ParcelNo))
						qry += $@" and {FieldNameParcel} = '{ParcelNo}'";
					SelectSetAndZoomAsync(_importParcelLineLayer, qry);
				}
			}
		}

		private string _platSelectedText;
		public string PlatSelectedText
		{
			get { return _platSelectedText; }
			set
			{
				SetProperty(ref _platSelectedText, value, () => PlatSelectedText);
			}
		}

		private string _taxToLotText;
		public string TaxToLotText
		{
			get { return _taxToLotText; }
			set
			{
				SetProperty(ref _taxToLotText, value, () => TaxToLotText);
			}
		}

		private string _parcelNo = "";
		public string ParcelNo
		{
			get { return _parcelNo; }
			set
			{
				SetProperty(ref _parcelNo, value, () => ParcelNo);
			}
		}

		public ICommand CmdImportPlat
		{
			get
			{
				return new RelayCommand(() =>
				 {
					 ProcessImportAsync ();
				 }, () => !string.IsNullOrEmpty(SelectedPlat) && SelectedPlat.Length > 0);
			}
		}

		public ICommand CmdTaxToLot
		{
			get
			{
				return new RelayCommand(() =>
				{
					ProcessCopyParcelsToLotAsync();
				}, () => !string.IsNullOrEmpty(TaxToLotText) && TaxToLotText.StartsWith("Copy"));
			}
		}

		#endregion Properties

		#region Helpers

		private async void ProcessImportAsync()
		{
			var cnt = (uint)_zoneSectPlat[_selectedZone][_selectedSection][_selectedPlat];
			var cps = GetProgressorDialogSource($@"Import {cnt} parcels", cnt + 2);
			var result = await ImportPlatAsync(cps);
			var msg = result.Item1;
			_importParcelCount = result.Item2;
			if (string.IsNullOrEmpty(msg))
			{
				msg = $@"Imported {_importParcelCount} tax parcels";
				TaxToLotText = $@"Copy {_importParcelCount} tax parcels to lot";
			}
			ActionOnGuiThread(() =>
			{
				ImportStatus = msg;
			});
		}

		private async Task<Tuple<string,int>> ImportPlatAsync(CancelableProgressorSource cps)
		{
			var result = await QueuedTask.Run<Tuple<string, int>>(async () =>
			{
				// first we  create a 'legal record' for the plat
				Dictionary<string, object> RecordAttributes = new Dictionary<string, object>();
				string sNewRecordName = $@"Plat {_selectedZone}-{_selectedSection}-{_selectedPlat}";
				int importedCount = 0;
				try
				{
					var editOper = new EditOperation()
					{
						Name = $@"Create Parcel Fabric Record: {sNewRecordName}",
						ProgressMessage = "Create Parcel Fabric Record...",
						ShowModalMessageAfterFailure = false,
						SelectNewFeatures = false,
						SelectModifiedFeatures = false
					};
					cps.Progressor.Value += 1;
					if (cps.Progressor.CancellationToken.IsCancellationRequested)
					{
						editOper.Abort();
						return new Tuple<string, int> ("Cancelled", importedCount);
					}
					cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
					cps.Progressor.Message = editOper.ProgressMessage;
					RecordAttributes.Add(FieldNameName, sNewRecordName);
					RecordAttributes.Add(FieldNameZone, _selectedZone);
					RecordAttributes.Add(FieldNameSect, _selectedSection);
					RecordAttributes.Add(FieldNamePlat, _selectedPlat);
					var editRowToken = editOper.CreateEx(_recordLayer, RecordAttributes);
					if (!editOper.Execute())
						return new Tuple<string, int>($@"Error [{editOper.Name}]: {editOper.ErrorMessage}", importedCount);

					// now make the record the active record
					var defOID = -1;
					var lOid = editRowToken.ObjectID ?? defOID;
					await _parcelFabricLayer.SetActiveRecordAsync(lOid);
				}
				catch (Exception ex)
				{
					return new Tuple<string, int>($@"Error [Exception]: {ex.Message}", importedCount);
				}
				try
				{
					// Copy the selected set of polygons into the Tax Parcels
					// However, since we need to set the polygon attributes manually we need to add each
					// parcel one at a time
					var qry = $@"{FieldNameZone} = {_selectedZone} and {FieldNameSect} = {_selectedSection} and {FieldNamePlat} = '{_selectedPlat}'";
					var lstTmks = GetDistinctValues(_importParcelLineLayer, qry, FieldNameTmk);
					lstTmks.Sort();
					foreach (var selectedTmk in lstTmks)
					{
						importedCount++;
						qry = $@"{FieldNameTmk} = {selectedTmk}";
						var cnt = SelectSet(_importParcelLineLayer, qry);
						cps.Progressor.Value += cnt;
						if (cps.Progressor.CancellationToken.IsCancellationRequested) 
							return new Tuple<string, int>("Cancelled", importedCount);
						cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
						cps.Progressor.Message = $@"Process parcel no: {selectedTmk}";
						var editOper = new EditOperation()
						{
							Name = $@"Copy new parcel lines for: {sNewRecordName}",
							ProgressMessage = "Create Parcel lines ...",
							ShowModalMessageAfterFailure = false,
							SelectNewFeatures = false,
							SelectModifiedFeatures = false
						};
						var ids = new List<long>(_importParcelLineLayer.GetSelection().GetObjectIDs());
						if (ids.Count == 0)
							return new Tuple<string, int>($@"Error [{editOper.Name}]: No selected lines were found. Please select line features and try again.", importedCount);
						var parcelEditTkn = editOper.CopyLineFeaturesToParcelType(_importParcelLineLayer, ids, _taxLayerLines, _taxLayerPolys);
						if (!editOper.Execute())
							return new Tuple<string, int>($@"Error [{editOper.Name}]: {editOper.ErrorMessage}", importedCount);

						// Update the names for all new parcel features
						var createdParcelFeatures = parcelEditTkn.CreatedFeatures;
						var editOperUpdate = editOper.CreateChainedOperation();
						// note: this only works for single parcels
						Dictionary<string, object> ParcelAttributes = new Dictionary<string, object>();
						// collect the attribute to be used for the polygon
						// unfortunately the polygon attributes are not autopopulated so we have to do this here
						foreach (KeyValuePair<MapMember, List<long>> kvp in createdParcelFeatures)
						{
							if (cps.Progressor.CancellationToken.IsCancellationRequested)
							{
								editOperUpdate.Abort();
								return new Tuple<string, int>("Cancelled", importedCount);
							}
							var mapMember = kvp.Key;
							if (mapMember.Name.EndsWith("_Lines"))
							{
								var oids = kvp.Value;
								foreach (long oid in oids)
								{
									var insp = new Inspector();
									insp.Load(mapMember, oid);
									var tmk = insp[FieldNameTmk];
									if (tmk != null)
									{
										var sTmk = tmk.ToString();
										if (sTmk.Length > 6)
										{
											var selectedIsland = sTmk.Substring(0, 1); 
											var selectedZone = sTmk.Substring(1, 1);
											var selectedSection = sTmk.Substring(2, 1);
											var selectedPlat = sTmk.Substring(3, 3);
											ParcelAttributes.Add(FieldNameName, $@"{sTmk.Substring(0, 1)}-{sTmk.Substring(1, 1)}-{sTmk.Substring(2, 1)}-{sTmk.Substring(3, 3)}-{sTmk.Substring(6)}");
											ParcelAttributes.Add(FieldNameTmk, tmk);
											ParcelAttributes.Add(FieldNameIsland, selectedIsland);
											ParcelAttributes.Add(FieldNameZone, selectedZone);
											ParcelAttributes.Add(FieldNameSect, selectedSection);
											ParcelAttributes.Add(FieldNamePlat, selectedPlat);
											ParcelAttributes.Add(FieldNameParcel, insp[FieldNameParcel]);
											ParcelAttributes.Add(FieldNameLink, insp[FieldNameLink]);
											break;
										}
									}
								}
							}
							if (ParcelAttributes.Count > 0) break;
						}
						foreach (KeyValuePair<MapMember, List<long>> kvp in createdParcelFeatures)
						{
							if (cps.Progressor.CancellationToken.IsCancellationRequested)
							{
								editOperUpdate.Abort();
								return new Tuple<string, int>("Cancelled", importedCount);
							}
							var mapMember = kvp.Key;
							if (!mapMember.Name.EndsWith("_Lines"))
							{
								var oids = kvp.Value;
								foreach (long oid in oids)
								{
									editOperUpdate.Modify(mapMember, oid, ParcelAttributes);
								}
							}
						}
						if (!editOperUpdate.Execute())
							return new Tuple<string, int>($@"Error [{editOperUpdate.Name}]: {editOperUpdate.ErrorMessage}", importedCount);
					}
				}
				catch (Exception ex)
				{
					return new Tuple<string, int>($@"Error [Exception]: {ex.Message}", importedCount);
				}
				try
				{
					// Build all Parcels for the Active record in the parcel fabric (set in step one)
					var theActiveRecord = _parcelFabricLayer.GetActiveRecord();
					var guid = theActiveRecord.Guid;
					var editOper = new EditOperation()
					{
						Name = "Build Parcels",
						ProgressMessage = "Build Parcels...",
						ShowModalMessageAfterFailure = true,
						SelectNewFeatures = true,
						SelectModifiedFeatures = true
					};
					cps.Progressor.Value += 1;
					if (cps.Progressor.CancellationToken.IsCancellationRequested)
					{
						editOper.Abort();
						return new Tuple<string, int>("Cancelled", importedCount);
					}
					cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
					cps.Progressor.Message = editOper.ProgressMessage;
					editOper.BuildParcelsByRecord(_parcelFabricLayer, guid);
					if (!editOper.Execute())
						return new Tuple<string, int>($@"Error [{editOper.Name}]: {editOper.ErrorMessage}", importedCount);
				}
				catch (Exception ex)
				{
					return new Tuple<string, int>($@"Error [Exception]: {ex.Message}", importedCount);
				}
				return new Tuple<string, int>(string.Empty, importedCount);
			}, cps.Progressor);
			return result;
		}

		private async void ProcessCopyParcelsToLotAsync()
		{
			var cnt = (uint)_zoneSectPlat[_selectedZone][_selectedSection][_selectedPlat];
			var cps = GetProgressorDialogSource($@"Copy {cnt} parcels", cnt + 2);
			var msg = await CopyParcelsToLotAsync(cps);
			if (string.IsNullOrEmpty(msg))
			{
			 msg = $@"Copy of {_zoneSectPlat[_selectedZone][_selectedSection][_selectedPlat]} completed";
			}
			ActionOnGuiThread(() =>
			{
				ImportStatus = msg;
			});
		}

		private async Task<string> CopyParcelsToLotAsync(CancelableProgressorSource cps)
		{
			var error = await QueuedTask.Run<string>(() =>
			{
				// copy tax parcels to lot
				try
				{
					if (_parcelFabricLayer == null)
						return "There is no parcel fabric in the map.";
					var theActiveRecord = _parcelFabricLayer.GetActiveRecord();
					if (theActiveRecord == null)
						return "There is no Active Record for the Parcel Fabric";
				}
				catch (Exception ex)
				{
					return $@"Error [Exception]: {ex.Message}";
				}
				try
				{
					// use CopyParcelLinesToParcelType to copy the tax parcels to the lot parcel type 
					var editOper = new EditOperation()
					{
						Name = "Copy Lines To Lot Parcel Type",
						ProgressMessage = "Copy Lines To Lot Parcel Type ...",
						ShowModalMessageAfterFailure = false,
						SelectNewFeatures = false,
						SelectModifiedFeatures = false
					};
					var qry = $@"{FieldNameZone} = {_selectedZone} and {FieldNameSect} = {_selectedSection} and {FieldNamePlat} = '{_selectedPlat}'";
					SelectSetAndZoomAsync(_taxLayerPolys, qry);
					var ids = new List<long>(_taxLayerPolys.GetSelection().GetObjectIDs());
					if (ids.Count == 0)
						return "No selected parcels found. Please select parcels and try again.";
					//add the standard feature line layers source, and their feature ids to a new KeyValuePair
					var kvp = new KeyValuePair<MapMember, List<long>>(_taxLayerPolys, ids);
					var sourceParcelFeatures = new List<KeyValuePair<MapMember, List<long>>> { kvp };
					editOper.CopyParcelLinesToParcelType(_parcelFabricLayer, sourceParcelFeatures, _lotLayerLines, _lotLayerPolys, false, true, true);
					if (!editOper.Execute())
						return editOper.ErrorMessage;
				}
				catch (Exception ex)
				{
					return $@"Error [Exception]: {ex.Message}";
				}
				try
				{
					// Build all Parcels for the Active record in the parcel fabric (set in step one)
					var theActiveRecord = _parcelFabricLayer.GetActiveRecord();
					var guid = theActiveRecord.Guid;
					var editOper = new EditOperation()
					{
						Name = "Build Parcels",
						ProgressMessage = "Build Parcels...",
						ShowModalMessageAfterFailure = true,
						SelectNewFeatures = true,
						SelectModifiedFeatures = true
					};
					cps.Progressor.Value += 1;
					if (cps.Progressor.CancellationToken.IsCancellationRequested)
					{
						editOper.Abort();
						return "Cancelled";
					}
					cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
					cps.Progressor.Message = editOper.ProgressMessage;
					editOper.BuildParcelsByRecord(_parcelFabricLayer, guid);
					if (!editOper.Execute())
						return $@"Error [{editOper.Name}]: {editOper.ErrorMessage}";
				}
				catch (Exception ex)
				{
					return $@"Error [Exception]: {ex.Message}";
				}
				return string.Empty;
			}, cps.Progressor);
			return error;
		}

		private void OnProjectClosed(ProjectEventArgs obj)
		{
			ActionOnGuiThread(() =>
			{
				Zones.Clear();
				PlatsLabel = $@"{LyrNameImport} n/a";
			});
		}

		private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
		{
			if (obj.IncomingView == null) return;
			CheckNewLayers();
		}

		private async void CheckNewLayers()
		{
			var errorMsg = await CheckParcelFabricStatusAsync();
			if (!string.IsNullOrEmpty(errorMsg))
			{
				ActionOnGuiThread(() =>
				{
					ImportStatus = errorMsg;
				});
				return;
			}
			errorMsg = await GetPlatsForLayerAsync();
			ActionOnGuiThread(() =>
			{
				if (!string.IsNullOrEmpty(errorMsg))
				{
					ImportStatus = errorMsg;
				}
				else
				{
					var lic = ArcGIS.Core.Licensing.LicenseInformation.Level;
					if (lic < ArcGIS.Core.Licensing.LicenseLevels.Standard)
					{
						ImportStatus = "Insufficient License Level for Parcel Fabric processing.";
					}
					else
					{
						ImportStatus = "License Level supports Parcel Fabric processing.";
					}
				}
				if (string.IsNullOrEmpty(errorMsg))
				{
					Zones.Clear();
					var lst = new List<long>();
					lst.AddRange(_zoneSectPlat.Keys);
					lst.Sort();
					Zones.AddRange(lst);
					PlatsLabel = $@"Select plat to import";
				}
				else
				{
					PlatsLabel = $@"{LyrNameImport} n/a";
				}
			});
		}

		private async Task<string> CheckParcelFabricStatusAsync()
		{
			try
			{
				var lic = ArcGIS.Core.Licensing.LicenseInformation.Level;
				if (lic < ArcGIS.Core.Licensing.LicenseLevels.Standard)
					return "Insufficient License Level for Parcel Fabric.";
				_taxLayerPolys = null;
				_lotLayerPolys = null;
				_taxLayerLines = null;
				_lotLayerLines = null;
				_importParcelLineLayer = null;
				if (MapView.Active?.Map == null) return "No active mapview";
				_parcelFabricLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<ParcelLayer>().FirstOrDefault();
				// if there is no fabric in the map then bail
				if (_parcelFabricLayer == null) return "No Parcel Fabric layer found in the TOC";
				var recordsLayer = await _parcelFabricLayer.GetRecordsLayerAsync();
				if (recordsLayer == null) return "No records layer for the Parcel Fabric was found";
				{
					_recordLayer = recordsLayer.FirstOrDefault();
				}
				if (MapView.Active?.Map == null) return "No active mapview";
				_taxLayerPolys = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Equals(LyrNameTax)).FirstOrDefault();
				if (_taxLayerPolys == null) return $@"{LyrNameTax} layer is missing";
				_lotLayerPolys = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Equals(LyrNameLot)).FirstOrDefault();
				if (_lotLayerPolys == null) return $@"{LyrNameLot} layer is missing";
				_taxLayerLines = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Equals(LyrNameTax + "_Lines")).FirstOrDefault();
				if (_taxLayerLines == null) return $@"{LyrNameTax} layer is missing";
				_lotLayerLines = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Equals(LyrNameLot + "_Lines")).FirstOrDefault();
				if (_lotLayerLines == null) return $@"{LyrNameLot} layer is missing";
				_importParcelLineLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Equals(LyrNameImport)).FirstOrDefault();
				if (_importParcelLineLayer == null) return $@"{LyrNameImport} layer is missing";
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($@"Error in CheckParcelFabricStatusAsync: {ex}");
			}
			return string.Empty;
		}

		private async Task<string> GetPlatsForLayerAsync()
		{
			if (MapView.Active?.Map == null) return "No active mapview";
			return await QueuedTask.Run<string>(() =>
			{
				// creates the list of all plats
				using (var featCursor = _importParcelLineLayer.Search())
				{
					while (featCursor.MoveNext())
					{
						// retrieve the first feature
						using (var feat = featCursor.Current as Feature)
						{
							var platObj = feat[FieldNamePlat];
							if (platObj != null)
							{
								var plat = platObj as string;
								var zone = Convert.ToInt64(feat[FieldNameZone]);
								var sect = Convert.ToInt64(feat[FieldNameSect]);
								if (!_zoneSectPlat.ContainsKey(zone))
								{
									// add new zone
									var platDictionary = new Dictionary<string, long>();
									var sectDictionary = new Dictionary<long, Dictionary<string, long>>();
									platDictionary.Add(plat, 1);
									sectDictionary.Add(sect, platDictionary);
									_zoneSectPlat.Add(zone, sectDictionary);
								}
								else
								{
									if (!_zoneSectPlat[zone].ContainsKey(sect))
									{
										// add new section
										var platDictionary = new Dictionary<string, long>();
										platDictionary.Add(plat, 1);
										_zoneSectPlat[zone].Add(sect, platDictionary);
									}
									else
									{
										if (!_zoneSectPlat[zone][sect].ContainsKey(plat))
										{
											// add new plat
											_zoneSectPlat[zone][sect].Add(plat, 1);
										}
										else
										{
											_zoneSectPlat[zone][sect][plat]++;
										}
									}
								}
							}
						}
					}
				}
				return string.Empty;
			});
		}

		private CancelableProgressorSource GetProgressorDialogSource(string msg, uint maxSteps)
		{
			var pd = new ProgressDialog(msg, $@"Canceled: {msg}", maxSteps, false);
			var cps = new CancelableProgressorSource(pd)
			{
				Max = maxSteps
			};
			return cps;
		}

		/// <summary>
		/// We have to ensure that GUI updates are only done from the GUI thread.
		/// </summary>
		private void ActionOnGuiThread(Action theAction)
		{
			if (System.Windows.Application.Current.Dispatcher.CheckAccess())
			{
				//We are on the GUI thread
				theAction();
			}
			else
			{
				//Using the dispatcher to perform this action on the GUI thread.
				ProApp.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, theAction);
			}
		}

		private async void SelectSetAndZoomAsync(FeatureLayer layer, string whereClause)
		{
			await QueuedTask.Run(() =>
			{
				MapView.Active?.Map.SetSelection(null);
				var qry = new QueryFilter() { WhereClause = whereClause };
				layer.Select(qry);
				MapView.Active?.ZoomTo(layer, true, new TimeSpan(0, 0, 2));
			});
		}

		private uint SelectSet(FeatureLayer layer, string whereClause)
		{
			var qry = new QueryFilter() { WhereClause = whereClause };
			var selection = layer.Select(qry);
			//MapView.Active?.PanTo(layer, true);
			return (uint)selection.GetCount();
		}

		private List<string> GetDistinctValues(FeatureLayer layer, string whereClause, string distinctField)
		{
			var qry = new QueryFilter() { WhereClause = whereClause };
			var lst = new List<string>();
			using (var cursor = layer.Search(qry))
			{
				while (cursor.MoveNext())
				{
					using (var row = cursor.Current)
					{
						var theDistinctValue = row[distinctField].ToString();
						if (!lst.Contains(theDistinctValue)) lst.Add(theDistinctValue);
					}
				}
			}
			return lst;
		}

		#endregion Helpers

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show()
		{
			DockPane pane = FrameworkApplication.DockPaneManager.Find(DockPaneID);
			if (pane == null)
				return;

			pane.Activate();
		}

	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class ImportPlat_ShowButton : Button
	{
		protected override void OnClick()
		{
			ImportPlatViewModel.Show();
		}
	}
}
