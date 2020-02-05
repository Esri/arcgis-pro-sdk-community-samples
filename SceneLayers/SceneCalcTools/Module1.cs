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
using System.Windows.Input;
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
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Mapping.Events;
using System.Windows;

namespace SceneCalcTools
{
	/// <summary>
	/// This sample illustrates how an add-in can be used to provide tools which leverage a 3D scene layer package and point cloud dataset created from a drone survey with Drone2Map for ArcGIS software.  The tools allow you to interact with the scene layers and illustrate a workflow to calculate volume and other metrics on asphalt gravel stockpiles for general supply tracking.
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data). The sample data contains a dataset called VolumeCalcTools.  Make sure that the Sample data is unzipped under C:\Data and the folder "C:\Data\VolumeCalcTools\" is available.  
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.
	/// 1. Click Start button to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open. 
	/// 1. Open the project "Scene_Volume_Calculation.aprx" found in folder: "C:\Data\VolumeCalcTools\Scene_Volume_Calculation\"
	/// 1. The demo dataset contains a 3D integrated mesh scene layer, Asphalt3D_132, which is visible as the project opens.  The default view is zoomed in to show three asphalt stockpiles.  There is also a bookmark, Asphalt Stockpiles, to restore this view for trying the add-in tools. There is a single “volume polygon” in green seen around the base of the largest stockpile, which is contained in the polygon feature layer, Clip_Polygon_Asphalt.
	/// 1. Click on the Add-In Tab and you will find a new button provided by the add-in with the caption "Scene Volume Calculation".  Click the button to open the new dockpane which has the same caption.  Resize the dockpane as needed to view the text boxes.
	/// ![UI](Screenshot/Screen1.png) 
	/// 1. To view information on previously-calculated polygons, like the single feature provided, simply select them.  Use the Select By Rectangle tool (Map Tab > Selection group > Select By Rectangle) to select the existing volume polygon, and you should then see its values populated in the dockpane.
	/// ![UI](Screenshot/Screen2.png) 
	/// 1. Next, you will create a new volume polygon for another stockpile and calculate its values.  Clear the selection (Map Tab > Selection group > Clear) and then select the Bookmark, New Volume Polygon. The view will show directly above the stockpile.  Open the attribute table for the layer, Clip_Polygon_Asphalt, and adjust its size to view a few records.
	/// 1. On the dockpane, click the Sketch tool at the top, which should change its color showing it as activated.  In the scene, the cursor will appear as a crosshair. Using the lasso, sketch a polygon outline around the base of the stockpile, and then double-click to complete.  You should see the new record appear in the attribute table, but you will not see the polygon until you set the polygon’s elevation in the next step.  
	/// ![UI](Screenshot/Screen3.png) 
	/// 1. Click the Elevation tool to active it and in the scene, click on a spot near the base of the asphalt pile.  This will apply the elevation of the clicked location to the feature’s PlaneHeight attribute, which drives the polygon’s elevation via the layer properties.  You can select another elevation point and see how the height of the selected polygon adjusts.
	/// 1. Next, in the Volume Calc Direction combobox, set the value to “Above” which specifies that you will calculate the volume of the stockpile above the reference plane of the volume polygon. Finally, with the single new polygon selected, click the Volume button on the dockpane.  When prompted, confirm “yes” to run the routine.
	/// 1. Review the values for the selected feature in the dockpane, which shows the volume, surface area and estimated weight.  The Volume routine runs the Polygon Volume geoprocessing tool and calculates additional attributes in the edit operation and displays these in the dockpane.
	/// ![UI](Screenshot/Screen4.png) 
	/// 1. As a final step in the workflow, you can compare the new stockpile polygon’s volume value to the existing larger stockpile polygon.  Select both records within the attribute table and you will then see the values appear for the differences in volume, surface area and weight between the two polygons.  This works with any two calculated volume polygon features selected.
	/// ![UI](Screenshot/Screen5.png) 
	/// 1. Additional things you can try:
	/// - Remove existing volume polygons and create new ones.  
	/// - Create multiple polygons for the same stockpile using different elevations and compare volume differences.
	/// - Re-calculate the same elevation for multiple selected polygons.
	/// </remarks>
	internal class Module1 : Module
	{
		private static Module1 _this = null;

		private const string StateId = "ShowPerMapPane";

		/// <summary>
		/// Retrieve the singleton instance to this module here
		/// </summary>
		public static Module1 Current
		{
			get
			{
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("SceneCalcTools_Module"));
			}
		}

		#region ViewModel Access

		/// <summary>
		/// Stores the instance of the dock pane viewmodel
		/// </summary>
		private static SceneCalcDockpaneViewModel SVM;
		internal static SceneCalcDockpaneViewModel SceneCalcVM
		{
			get
			{
				if (SVM == null)
				{
					SVM = FrameworkApplication.DockPaneManager.Find("SceneCalcTools_SceneCalcDockpane") as SceneCalcDockpaneViewModel;
				}
				return SVM;
			}
		}

		#endregion

		#region Overrides

		/// <summary>
		/// Called by Framework when ArcGIS Pro is closing
		/// </summary>
		/// <returns>False to prevent Pro from closing, otherwise True</returns>
		protected override bool CanUnload()
		{
			return true;
		}

		protected override bool Initialize()
		{
			ActiveMapViewChangedEvent.Subscribe((Action<ActiveMapViewChangedEventArgs>)((args) =>
			{
				if (args.IncomingView == null) return;

				var featLayer = MapView.Active.Map.FindLayers("Clip_Polygon_Asphalt").FirstOrDefault() as FeatureLayer;
				// If this layer is present, activate the dockpane
				if (featLayer != null)
				{
					FrameworkApplication.State.Activate(StateId);
					if (SceneCalcVM != null)
					{
						SceneCalcVM.DockpaneVisible = Visibility.Visible;
					}
				}
				else
				// Or else deactivate the dockpane
				{
					FrameworkApplication.State.Deactivate(StateId);
					if (SceneCalcVM != null)
						SceneCalcVM.DockpaneVisible = Visibility.Collapsed;
				}
			}));
			return base.Initialize();
		}

		#endregion Overrides

		#region Business Logic

		public void RunPolygonTool()
		{
			var cmd = FrameworkApplication.GetPlugInWrapper("SceneCalcTools_SketchPolygonMapTool") as ICommand;
			if (cmd.CanExecute(null))
				cmd.Execute(null);
		}

		public void RunElevationTool()
		{
			var cmd = FrameworkApplication.GetPlugInWrapper("SceneCalcTools_ElevationMapTool") as ICommand;
			if (cmd.CanExecute(null))
				cmd.Execute(null);
		}

		public async void FeatureSelectionChanged()
		{
			// if (_activeMap == null) return;
			if (MapView.Active == null) return;

			await QueuedTask.Run((Action)(() =>
			{
				var featLayer = MapView.Active.Map.FindLayers("Clip_Polygon_Asphalt").FirstOrDefault() as FeatureLayer;
				var selFeatures = featLayer.GetSelection().GetCount();
				// Update view model with the count
				SceneCalcVM.SelectedPolygonsCount = selFeatures;

				// Get the sum of the values and update view model appropriately
				if (selFeatures > 0)
				{
					RowCursor cursorPolygons = featLayer.GetSelection().Search(null);
					double refPlaneAreaValue = 0;
					double refPlaneElevValue = 0;
					double totalVolumeValue = 0;
					double totalAreaValue = 0;
					double totalWeightInTonsValue = 0;
					double volumeDiff1 = 0;
					double volumeDiff2 = 0;
					double surfaceAreaDiff1 = 0;
					double surfaceAreaDiff2 = 0;
					double weightDiff1 = 0;
					double weightDiff2 = 0;
					List<string> meshFileList = new List<string> { };
					List<string> cloudFileList = new List<string> { };
					string meshContentList = string.Empty;
					string cloudContentList = string.Empty;

					bool volumeFlag = false;
					int x = 0;
					while (cursorPolygons.MoveNext())
					{
						using (Row currentRow = cursorPolygons.Current)
						{
							refPlaneAreaValue += Convert.ToDouble(currentRow["Shape_Area"]);

							// Check if Volume attribute is null for any of the selected records
							if (currentRow["Volume"] == null) volumeFlag = true;
							if (currentRow["ElevationMeshFile"] != null)
							{
								meshFileList.Add(Convert.ToString(currentRow["ElevationMeshFile"]));
							}
							if (currentRow["PointCloudFile"] != null)
							{
								cloudFileList.Add(Convert.ToString(currentRow["PointCloudFile"]));
							}
							totalVolumeValue = totalVolumeValue + Convert.ToDouble(currentRow["Volume"]);
							totalAreaValue = totalAreaValue + Convert.ToDouble(currentRow["SArea"]);
							totalWeightInTonsValue = totalWeightInTonsValue + Convert.ToDouble(currentRow["EstWeightInTons"]);

							if (currentRow["PlaneHeight"] != null && selFeatures == 1)
							{
								refPlaneElevValue = Convert.ToDouble(currentRow["PlaneHeight"]);
							}
							if (currentRow["Volume"] != null && selFeatures == 2)
							{
								if (x == 0)
								{
									volumeDiff1 = Convert.ToDouble(currentRow["Volume"]);
									surfaceAreaDiff1 = Convert.ToDouble(currentRow["SArea"]);
									weightDiff1 = Convert.ToDouble(currentRow["EstWeightInTons"]);
								}
								else if (x == 1)
								{
									volumeDiff2 = Convert.ToDouble(currentRow["Volume"]);
									surfaceAreaDiff2 = Convert.ToDouble(currentRow["SArea"]);
									weightDiff2 = Convert.ToDouble(currentRow["EstWeightInTons"]);
								}
							}
							x++;
						}
					}

					// Set values on view model
					SceneCalcVM.ReferencePlaneArea = refPlaneAreaValue;
					SceneCalcVM.ReferencePlaneElevation = refPlaneElevValue;

					// If there are no null Volume values, show combined values in view model
					if (volumeFlag == false)
					{
						SceneCalcVM.TotalVolume = totalVolumeValue;
						SceneCalcVM.TotalSurfaceArea = totalAreaValue;
						SceneCalcVM.TotalWeightInTons = totalWeightInTonsValue;

						//  If selFeatures = 2, show the difference in volume and elevation, plus
						if (selFeatures == 2)
						{
							SceneCalcVM.DiffInVolume = Math.Max(volumeDiff1, volumeDiff2) - Math.Min(volumeDiff1, volumeDiff2);
							SceneCalcVM.DiffInSurfaceArea = Math.Max(surfaceAreaDiff1, surfaceAreaDiff2) - Math.Min(surfaceAreaDiff1, surfaceAreaDiff2);
							SceneCalcVM.DiffInWeight = Math.Max(weightDiff1, weightDiff2) - Math.Min(weightDiff1, weightDiff2);
						}
						else
						{
							SceneCalcVM.DiffInVolume = 0;
							SceneCalcVM.DiffInSurfaceArea = 0;
							SceneCalcVM.DiffInWeight = 0;
						}
					}
					// Or else clear the values in the view model
					else
					{
						SceneCalcVM.TotalVolume = 0;
						SceneCalcVM.TotalSurfaceArea = 0;
						SceneCalcVM.TotalWeightInTons = 0;
						SceneCalcVM.DiffInVolume = 0;
						SceneCalcVM.DiffInSurfaceArea = 0;
						SceneCalcVM.DiffInWeight = 0;
					}
					if (meshFileList.Count > 0)
					{
						// int count;
						// Get unique values and counts in the list
						foreach (var item in meshFileList.Distinct())
						{
							// count = LandUseCodeList.Where(x => x.Equals(item)).Count();
							meshContentList = meshContentList + item + "\r\n";
							// luKv.Add(new KeyValueWithTooltip() { Code = $@"LU Code: {item}", Key = item.ToString(), Value = count });
						}
						SceneCalcVM.MeshFile = meshContentList;
					}
					if (cloudFileList.Count > 0)
					{
						// int count;
						// Get unique values and counts in the list
						foreach (var item in cloudFileList.Distinct())
						{
							// count = LandUseCodeList.Where(x => x.Equals(item)).Count();
							cloudContentList = cloudContentList + item + "\r\n";
							// luKv.Add(new KeyValueWithTooltip() { Code = $@"LU Code: {item}", Key = item.ToString(), Value = count });
						}
						SceneCalcVM.PointCloudFile = cloudContentList;
					}
				} // if selFeatures > 0
				else  // Clear pane values.
				{
					SceneCalcVM.ReferencePlaneArea = 0;
					SceneCalcVM.TotalSurfaceArea = 0;
					SceneCalcVM.TotalVolume = 0;
					SceneCalcVM.TotalWeightInTons = 0;
					SceneCalcVM.ReferencePlaneElevation = 0;
					SceneCalcVM.ReferencePlaneDirection = string.Empty;
					SceneCalcVM.DiffInVolume = 0;
					SceneCalcVM.DiffInSurfaceArea = 0;
					SceneCalcVM.DiffInWeight = 0;
					SceneCalcVM.MeshFile = string.Empty;
					SceneCalcVM.PointCloudFile = string.Empty;
				}
			}));
		}

		public async void CalculateVolume()
		{
			// Update the viewmodel with values
			await FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");

			// Check for an active mapview
			if (MapView.Active == null)
			{
				ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No MapView currently active. Exiting...", "Info");
				return;
			}

			// Prompt before proceeding with calculation work
			var response = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Save edits and calculate volume on selected features?", "Calculate Volume", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
			if (response == MessageBoxResult.No) return;

			// Save edits for reading by GP Tool
			await Project.Current.SaveEditsAsync();
			try
			{
				await QueuedTask.Run((Func<Task>)(async () =>
				 {
					 var featLayer = MapView.Active.Map.FindLayers("Clip_Polygon_Asphalt").FirstOrDefault() as FeatureLayer;

					// Get the selected records, and check/exit if there are none:
					var featSelectionOIDs = featLayer.GetSelection().GetObjectIDs();
					 if (featSelectionOIDs.Count == 0)
					 {
						 ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No records selected for layer, " + featLayer.Name + ". Exiting...", "Info");
						 return;
					 }

					// Ensure value for reference plane direction combobox
					else if (SceneCalcVM.ReferencePlaneDirection == string.Empty)
					 {
						 ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Choose the reference plane direction for volume calculation. Exiting...", "Value Needed");
						 return;
					 }

					// Ensure there is a valid reference plane height/elevation value for all selected records
					RowCursor cursorPolygons = featLayer.GetSelection().Search(null);
					 while (cursorPolygons.MoveNext())
					 {
						 using (Row currentRow = cursorPolygons.Current)
						 {
							// Get values for dockpane
							if (currentRow["PlaneHeight"] == null || Convert.ToDouble(currentRow["PlaneHeight"]) == 0)
							 {
								 string currentObjectID = Convert.ToString(currentRow["ObjectID"]);
								 ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Empty or invalid Plane Height value for polygon ObjectID: " + currentObjectID + ". Exiting...", "Value Needed");
								 return;
							 }
						 }
					 }

					// Get the name of the attribute to update, and the value to set:
					double refPlaneElevation = SceneCalcVM.ReferencePlaneElevation;
					 string refPlaneDirection = SceneCalcVM.ReferencePlaneDirection;

					// Start progress dialog
					var progDialog = new ProgressDialog("Calculating Volume");
					 var progSource = new ProgressorSource(progDialog);
					 progDialog.Show();

					// Prepare for run of GP tool -- Get the path to the LAS point layer
					string surfaceLASDataset = "Asphalt3D_132_point_cloud.las";
					 var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector(true);
					 inspector.Load(featLayer, featSelectionOIDs);
					 inspector["PlaneDirection"] = refPlaneDirection;
					 inspector["DateOfCapture"] = DateTime.Today;
					 inspector["ElevationMeshFile"] = "Asphalt3D_132_3d_mesh.slpk";
					 inspector["PointCloudFile"] = surfaceLASDataset;

					 var editOp = new EditOperation();
					 editOp.Name = "Edit " + featLayer.Name + ", " + Convert.ToString(featSelectionOIDs.Count) + " records.";
					 editOp.Modify(inspector);
					 await editOp.ExecuteAsync();
					 await Project.Current.SaveEditsAsync();

					// Get the path to the layer's feature class
					string infc = featLayer.Name;
					// Place parameters into an array
					var parameters = Geoprocessing.MakeValueArray(surfaceLASDataset, infc, "PlaneHeight", refPlaneDirection);
					// Place environment settings in an array, in this case, OK to over-write
					var environments = Geoprocessing.MakeEnvironmentArray(overwriteoutput: true);
					// Execute the GP tool with parameters
					var gpResult = await Geoprocessing.ExecuteToolAsync("PolygonVolume_3d", parameters, environments);
					// Save edits again
					await Project.Current.SaveEditsAsync();

					// var selFeatures = featLayer.GetSelection().GetCount();
					RowCursor cursorPolygons2 = featLayer.GetSelection().Search(null);

					 double totalVolumeValue = 0;
					 double totalAreaValue = 0;
					 double totalWeightInTons = 0;
					 double currentVolume;
					 double currentSArea;
					 double currentWeightInTons;
					 long currentOID;

					 while (cursorPolygons2.MoveNext())
					 {
						 using (Row currentRow = cursorPolygons2.Current)
						 {
							// Get values for dockpane
							currentOID = currentRow.GetObjectID();
							// Convert volume in cubic meters from source data to cubic feet:
							currentVolume = Convert.ToDouble(currentRow["Volume"]) * 35.3147;
							// Convert surface area value from square meters from source data to square feet:
							currentSArea = Convert.ToDouble(currentRow["SArea"]) * 10.7639;
							// Calculate estimated weight in tons = (volume in square foot * 103.7 pounds per square foot) / 2000 pounds per ton
							currentWeightInTons = (currentVolume * 103.7) / 2000;

							// Update the new cubic feet and square feet values for the feature:
							inspector.Load(featLayer, currentOID);
							 inspector["Volume"] = currentVolume;
							 inspector["SArea"] = currentSArea;
							 inspector["EstWeightInTons"] = currentWeightInTons;
							 await inspector.ApplyAsync();

							// Combine values for display of total volume and surface area values in the dockpane:
							totalVolumeValue += currentVolume;
							 totalAreaValue += currentSArea;
							 totalWeightInTons += currentWeightInTons;
						 }
					 }

					// Apply the values and refresh selection update
					await Project.Current.SaveEditsAsync();
					 FeatureSelectionChanged();
					// Close progress dialog
					progDialog.Hide();
					 progDialog.Dispose();
				 }));
			}
			catch (Exception exc)
			{
				// Catch any exception found and display a message box.
				ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Exception caught while trying to perform update: " + exc.Message);
				return;
			}
		}

		#endregion
	}
}
