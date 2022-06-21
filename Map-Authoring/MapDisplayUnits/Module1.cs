/*

   Copyright 2020 Esri

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
using System.Windows.Threading;

namespace MapUnits
{
	/// <summary>
	/// This sample demonstrates Map Display Units. The Pro SDK allows you can change the defaults for the current Project Units.  You can also change the Display Units used by a Map or Scene and the Elevation units used by a Scene.
	/// </summary>
	/// <remarks>
	/// 1. In Visual Studio click the Build menu. Then select Build Solution.  
	/// 1. Launch the debugger to open ArcGIS Pro.
	/// 1. Open any project with Maps and Scenes.
	/// 1. Click the Add-in tab
	/// 1. In the Add-in tab, click the "Show Project Units" button.
	/// 1. The dockpane that is displayed allows you to:
	///     * Change the current project's default Unit Format Types. Unit Format Types are Distance Units, Angular Units, Area Units, Location Units, Direction Units,  Page Units, 2D Symbol Display Units and 3D Symbol Display Units. 
	///     * Change the Display Units for a Map or Scene in the given project. Display Units for a map uses "Location Units" UnitFormatType.
	///     * Change the Elevation Units for a Scene. Elevation Units for a Scene uses "Distance Units" UnitFormatType.
	/// ![UI](screenshots/DisplayUnits.png)
	/// #### Change Project Default Units:
	/// 1. For a given UnitFormatType such as "Distance", "Location", etc., you can see the available DisplayUnitFormats available. The project default is specified in the dockpane.
	/// 1. If you want to change the default DisplayUnitFormatType for a given UnitFormatType, use the "Make Default" button.
	/// #### Change Display and Elevation Units for Maps and Scenes
	/// 1. Using the available maps combo box drop down, select any map or scene in the project.
	/// 1. Notice the Spatial Reference of the Map shown.
	/// 1. The map/scene's current Display Unit is selected by default. The other Display Units avaialble for the map is listed in the combox box below.  
	/// 1. Select any other Display Unit you want to use for the selected Map.  Click the Apply Button. The map/scene's Display Unit will be changed to the selected DisplayUnitFormat.
	/// 1. If a scene is selected, you can change its Elevation Units.
	/// 1. The Elevation Unit for a given scene is selected by default.  
	/// 1. Select any other Elevation Unit you want to use for the selected Scene.  Click the Apply Button. The scene's Elevation Unit will be changed to the selected DisplayUnitFormat.
	/// </remarks>
	internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current
    {
      get
      {
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("MapUnits_Module"));
      }
    }

		/// <summary>
		/// Stores the instance of the Feature Selection dock pane viewmodel
		/// </summary>
		private static ProjectMapUnitsViewModel _dockPane;
		internal static ProjectMapUnitsViewModel ProjectMapUnitsVM
		{
			get
			{
				if (_dockPane == null)
				{
					_dockPane = FrameworkApplication.DockPaneManager.Find("MapUnits_ProjectMapUnits") as ProjectMapUnitsViewModel;
				}
				return _dockPane;
			}
		}
		protected override bool Initialize()
		{
			//Subscribe to the ProjectUnitFormatsChangedEvent to see if the Project Defaults events have changed.
			ArcGIS.Desktop.Core.Events.ProjectUnitFormatsChangedEvent.Subscribe((args) =>
			{
				foreach (var hint in args.DefaultsChangedHint)
				{
					//Checking the "DefaultsChangedHint" and updated the Project default values in the custom dockpane.
					var defSelectionAction = (Action)(() =>
					{
						ProjectMapUnitsVM.UpdateDisplayUnits();
					});
					ActionOnGuiThread(defSelectionAction);					
				}

			});

			//Subscribe to the MapUnitFormatChangedEvent to see if the Map's Location Display unit has changed. 
			//Also check if the Scene's elevation unit has changed.
			ArcGIS.Desktop.Mapping.Events.MapUnitFormatChangedEvent.Subscribe((args) =>
			{
				var whatChanged = args.EventHint;
				
				if (args.Map == null) return;
				System.Diagnostics.Debug.WriteLine($"Map: {args.Map.Name}");
				//Map's Location UnitFormatType has changed
				if (whatChanged == ArcGIS.Desktop.Mapping.Events.MapUnitFormatChanged.Location)
				{
					//Get the new Location DisplayUnitFormatType
					var mapDisplayUnit = args.Map?.GetLocationUnitFormat();
					//Set the default in the combo box as the new DisplayUnitFormatType
					ProjectMapUnitsVM.SelectedMapAvailableDisplayUnit = ProjectMapUnitsVM.MapAvailableDisplayUnits.FirstOrDefault(u => u.UnitCode == mapDisplayUnit.UnitCode);
					//Enable/Disable Apply Button
					ProjectMapUnitsVM.CanChangeMapDU = mapDisplayUnit?.UnitCode == ProjectMapUnitsVM.SelectedMapAvailableDisplayUnit?.UnitCode ? false : true;				
					
				}
				else
				{
					//Get the new Scene Elevation DisplayUnitFormatType
					var sceneElevUnit = args.Map?.GetElevationUnitFormat();
					//Set the default in the combo box as the new DisplayUnitFormatType
					ProjectMapUnitsVM.SelectedSceneAvailableElevUnit = ProjectMapUnitsVM.SceneAvailableElevUnits.FirstOrDefault(e => e.UnitCode == sceneElevUnit.UnitCode);
					//Enable/Disable Apply Button
					ProjectMapUnitsVM.CanChangeSceneEU = sceneElevUnit?.UnitCode == ProjectMapUnitsVM.SelectedSceneAvailableElevUnit?.UnitCode ? false : true;
				}
			});
			return true;
		}

		/// <summary>
		/// We have to ensure that GUI updates are only done from the GUI thread.
		/// </summary>
		public void ActionOnGuiThread(Action theAction)
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

		#region Overrides
		/// <summary>
		/// Called by Framework when ArcGIS Pro is closing
		/// </summary>
		/// <returns>False to prevent Pro from closing, otherwise True</returns>
		protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

  }
}
