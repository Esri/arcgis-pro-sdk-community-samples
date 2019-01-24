/*

   Copyright 2019 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows;

namespace SLR_Analyst
{
	internal class SLR_DockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "SLR_Analyst_SLR_Dockpane";

		protected SLR_DockpaneViewModel()
		{
			_btnSelectLayerBySLRCmd = new RelayCommand(() => SelectLayerBySLR(), () => true);
			SliderValue = 0;
			CkbLandUseChecked = true;
		}

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show()
		{
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null) return;

			pane.Activate();
		}

		// Parcel layer selection check:
		private bool _ckbParcelChecked;
		public bool CkbParcelChecked
		{
			get { return _ckbParcelChecked; }
			set
			{
				SetProperty(ref _ckbParcelChecked, value, () => CkbParcelChecked);
			}
		}

		// LandUse layer selection check:
		private bool _ckbLandUseChecked;
		public bool CkbLandUseChecked
		{
			get { return _ckbLandUseChecked; }
			set
			{
				SetProperty(ref _ckbLandUseChecked, value, () => CkbLandUseChecked);
			}
		}

		// Street layer selection check:
		private bool _ckbStreetChecked;
		public bool CkbStreetChecked
		{
			get { return _ckbStreetChecked; }
			set
			{
				SetProperty(ref _ckbStreetChecked, value, () => CkbStreetChecked);
			}
		}

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "Sea Level Rise Viewer";
		public string Heading
		{
			get { return _heading; }
			set
			{
				SetProperty(ref _heading, value, () => Heading);
			}
		}

		public class ChartItem
		{
			public string Title { get; set; } // coil456
			public double Value { get; set; } // 334

			public string TooltipLabel
			{
				get { return string.Format("Code = {0}", this.Value); }
			}
		}
		
		// Slider layer selection check:
		private int _sliderValue;
		public int SliderValue
		{
			get { return _sliderValue; }
			set
			{
				SetProperty(ref _sliderValue, value, () => SliderValue);
			}
		}

		#region Commands

		private RelayCommand _btnSelectLayerBySLRCmd;
		public ICommand BtnSelectLayerBySLRCmd
		{
			get
			{
				return _btnSelectLayerBySLRCmd;
			}
		}

		#endregion

		#region SliderUpdate

		public void SliderUpdate(double sliderValue)
		{

			// ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Dude, slider value: " + Convert.ToString(sliderValue));
			// Set up the list of SLR layers and turn them on/off depending on what's selected
			// IReadOnlyList<Layer> layers = ArcGIS.Desktop.Mapping.MapView.Active.Map.FindLayers("dc_slr/slr_3ft", true);
			//  TOGGLE THE VISIBILITY
			//  https://github.com/esri/arcgis-pro-sdk/wiki/ProSnippets-MapAuthoring#find-a-layer  

			// turn off visibility of any other SLR layer
			//var SLRLayer1 = MapView.Active.Map.FindLayers("dc_slr/slr_3ft").FirstOrDefault() as ServiceLayer;
			//SLRLayer1.SetVisibility(false);

			// turn on visibility of current SLR layer
			//var SLRLayer2 = MapView.Active.Map.FindLayers("dc_slr/slr_6ft").FirstOrDefault() as ServiceLayer;
			//SLRLayer2.SetVisibility(true);

			QueuedTask.Run(() =>
			{
				var activeMapView = MapView.Active;
				if (activeMapView == null) return;
				var myLayers = activeMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();     // OfType<ServiceLayer>();
				foreach (var myLayer in myLayers)
				{
					// ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(myLayer.Name, "ServiceLayers");
					//// if(myLayer.Name.StartsWith("dc_slr"))
					if (myLayer.Name.StartsWith("FL_SLR_Study"))
						myLayer.SetVisibility(false);
				}

				// if (sliderValue == 0) return;
				// var SLRLayer1 = MapView.Active.Map.FindLayers("dc_slr/slr_" + sliderValue + "ft").FirstOrDefault() as ServiceLayer;
				// FL_MFL2_slr_6ft
				var SLRLayer1 = activeMapView.Map.FindLayers("FL_SLR_Study_" + sliderValue + "ft").FirstOrDefault() as FeatureLayer;

				SLRLayer1.SetVisibility(true);

			});

		}

		#endregion

		public async void SelectLayerBySLR()
		{

			// Get checkbox values for selection
			// checkBox.IsChecked.Value
            
			// Get the slider value for the current SLR Layer
			string strSLRLayerName = "FL_SLR_Study_" + Convert.ToString(_sliderValue) + "ft";

			// If SLR layer is set to Zero feet, or no layers are checked for selection, then exit.
			if (_sliderValue == 0)
			{
				ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Set slider to an SLR height of 1 - 6 feet", "Set Slider Value");
				return;
			}
			if (_ckbLandUseChecked == false && _ckbParcelChecked == false && _ckbStreetChecked == false)
			{
				ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Select checkbox of study layer(s) to review", "Select Study Layer(s)");
				return;
			}

			// Select Study Area Layers based on what is visible 
			await QueuedTask.Run(async () =>
			{
				var activeMapView = MapView.Active;
				if (activeMapView == null) return;

				// For sub-selection
				SpatialQueryFilter spatialFilter = new SpatialQueryFilter
				{
					FilterGeometry = new PolygonBuilder(MapView.Active.Extent).ToGeometry(),
					SpatialRelationship = SpatialRelationship.Intersects
				};

				string luContentList = "\r\n** LAND USE **\r\n";
				string parcelContentList = "\r\n\r\n** PARCELS **\r\n";
				string streetContentList = "\r\n\r\n** STREETS **\r\n";

				IList<KeyValueWithTooltip> luKv = new List<KeyValueWithTooltip>();
				IList<KeyValueWithTooltip> pKv = new List<KeyValueWithTooltip>();
				IList<KeyValueWithTooltip> sKv = new List<KeyValueWithTooltip>();

				// set titles & report configuration
				var paneConfiguration = _ckbLandUseChecked ? "Land Use" : string.Empty;
				if (_ckbParcelChecked)
				{
					paneConfiguration += paneConfiguration.Length > 0 ?
											(_ckbStreetChecked ? " , Pacels, and Street" : " and Parcels") :
											(_ckbStreetChecked ? "Pacels and Street" : "Parcels");
				}
				else
				{
					paneConfiguration += paneConfiguration.Length > 0 ?
											 (_ckbStreetChecked ? " and Street" : string.Empty) :
											 (_ckbStreetChecked ? " Street" : string.Empty);
				}
				paneConfiguration = "This report is showing effects on: " + paneConfiguration;
				var paneTitle = $@"Sea Level Rise of {_sliderValue} Feet {Environment.NewLine}";

				// ** PARCELS
				//if (_ckbParcelChecked == true)
				{
					var ParcelsLayer = MapView.Active.Map.FindLayers("Parcels_Study").FirstOrDefault() as BasicFeatureLayer;
					ParcelsLayer.ClearSelection();

					var args = Geoprocessing.MakeValueArray("Parcels_Study", "intersect", strSLRLayerName);
					await Geoprocessing.ExecuteToolAsync("SelectLayerByLocation_management", args);

					Selection ParcelsSelection = ParcelsLayer.Select(spatialFilter, SelectionCombinationMethod.And);
					int parcelsSelectionCount;
					parcelsSelectionCount = ParcelsSelection.GetCount();

					// Format parcels report for Textbox output:
					string parcelsSelectionCountstring = Convert.ToString(parcelsSelectionCount);
					parcelContentList = parcelContentList + "* TOTAL:  " + parcelsSelectionCountstring + " Parcels affected.\r\n \r\n* Unique Codes in Affected Area: \r\n";

					//  LOOP THROUGH THE PARCEL SELECTION AND GET THE UNIQUE LIST OF SUBCODES.
					// Create list
					List<string> ParcelCodeList = new List<string> { };
					// Populate list
					using (RowCursor ParcelRowCursor = ParcelsSelection.Search(null, false))
					{
						while (ParcelRowCursor.MoveNext())
						{
							using (Row currentRow = ParcelRowCursor.Current)
							{
								ParcelCodeList.Add(Convert.ToString(currentRow["SUBCODE"]));
							}
						}
					}
					ParcelCodeList.Sort();
					int count;

					// Get unique values and counts in the list
					foreach (var item in ParcelCodeList.Distinct())
					{
						count = ParcelCodeList.Where(x => x.Equals(item)).Count();
						parcelContentList = parcelContentList + Convert.ToString(count) + " instances of Code:  " + item + "\r\n";
						pKv.Add(new KeyValueWithTooltip() { Code = $@"Sub Code: {item}", Key = item.ToString(), Value = count });
					}
					// Dispose of data classes
					ParcelsSelection.Dispose();
				}

				// ** STREETS
				//if (_ckbStreetChecked == true)
				{
					var StreetsLayer = MapView.Active.Map.FindLayers("Streets_Study").FirstOrDefault() as BasicFeatureLayer;
					StreetsLayer.ClearSelection();

					var args = Geoprocessing.MakeValueArray("Streets_Study", "intersect", strSLRLayerName);
					await Geoprocessing.ExecuteToolAsync("SelectLayerByLocation_management", args);

					Selection StreetsSelection = StreetsLayer.Select(spatialFilter, SelectionCombinationMethod.And);
					int streetsSelectionCount;
					streetsSelectionCount = StreetsSelection.GetCount();
					// Format streets report for Textbox output:
					string streetSelectionCountstring = Convert.ToString(streetsSelectionCount);
					streetContentList = streetContentList + "* TOTAL:  " + streetSelectionCountstring + " Streets affected.\r\n \r\n* Unique Codes in Affected Area: \r\n";

                    //  LOOP THROUGH THE STREET SELECTION AND GET THE UNIQUE LIST OF CODES.
                    // Create list
                    List<string> StreetCodeList = new List<string> { };
					// Populate list
					using (RowCursor StreetRowCursor = StreetsSelection.Search(null, false))
					{
						while (StreetRowCursor.MoveNext())
						{
							using (Row currentRow = StreetRowCursor.Current)
							{
								StreetCodeList.Add(Convert.ToString(currentRow["CLASS"]));
							}
						}
					}
					StreetCodeList.Sort();
					int count;
					// Get unique values and counts in the list
					foreach (var item in StreetCodeList.Distinct())
					{
						count = StreetCodeList.Where(x => x.Equals(item)).Count();
						streetContentList = streetContentList + Convert.ToString(count) + " instances of Code:  " + item + "\r\n";
						sKv.Add(new KeyValueWithTooltip() { Code = $@"Class: {item}", Key = item, Value = count });
					}
					// Dispose of data classes
					StreetsSelection.Dispose();
				}

				// ** LAND USE
				//if (_ckbLandUseChecked == true)
				{
					//  Get selection and make selection on map extent only if needed
					var LandUseLayer = MapView.Active.Map.FindLayers("Land_Use_Study").FirstOrDefault() as BasicFeatureLayer;
					LandUseLayer.ClearSelection();

					var args = Geoprocessing.MakeValueArray("Land_Use_Study", "intersect", strSLRLayerName);
					await Geoprocessing.ExecuteToolAsync("SelectLayerByLocation_management", args);

					Selection LandUseSelection = LandUseLayer.Select(spatialFilter, SelectionCombinationMethod.And);
					int luSelectionCount;
					luSelectionCount = LandUseSelection.GetCount();
					// Format land use report for Textbox output:
					string luSelectionCountstring = Convert.ToString(luSelectionCount);
					luContentList = luContentList + "* TOTAL:  " + luSelectionCountstring + " Land Use areas affected.\r\n \r\n* Unique Codes in Affected Area: \r\n";

                    //  LOOP THROUGH THE LANDUSE SELECTION AND GET THE UNIQUE LIST OF CODES.
                    // Create list
                    List<string> LandUseCodeList = new List<string> { };
					// Populate list
					using (RowCursor LandUseRowCursor = LandUseSelection.Search(null, false))
					{
						while (LandUseRowCursor.MoveNext())
						{
							using (Row currentRow = LandUseRowCursor.Current)
							{
								LandUseCodeList.Add(Convert.ToString(currentRow["LU"]));
							}
						}
					}
					LandUseCodeList.Sort();
					int count;
					// Get unique values and counts in the list
					foreach (var item in LandUseCodeList.Distinct())
					{
						count = LandUseCodeList.Where(x => x.Equals(item)).Count();
						luContentList = luContentList + Convert.ToString(count) + " instances of Code:  " + item + "\r\n";
						luKv.Add(new KeyValueWithTooltip() { Code = $@"LU Code: {item}", Key = item.ToString(), Value = count });
					}
					// Dispose of data classes
					LandUseSelection.Dispose();
				} // End Parcel Routine

				string reportText = "SEA LEVEL RISE REPORT:  " + SliderValue + "-FEET LEVEL EFFECTS \r\n";
				if (_ckbLandUseChecked) reportText += luContentList;
				if (_ckbParcelChecked) reportText += parcelContentList;
				if (_ckbStreetChecked) reportText += streetContentList;

				await System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
					new Action(() =>
					{
						Module1.CreateNewSlrPane (paneTitle, paneConfiguration, reportText,
							_ckbLandUseChecked, _ckbParcelChecked, _ckbStreetChecked,
							luKv, pKv, sKv);
					}));
			});  // Close QueuedTask

		}

	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class SLR_Dockpane_ShowButton : Button
	{
		protected override void OnClick()
		{
			SLR_DockpaneViewModel.Show();
		}
	}

}
