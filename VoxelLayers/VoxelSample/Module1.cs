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
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Mapping.Voxel.Events;

namespace VoxelSample
{
	/// <summary>
	/// Illustrates use of various aspects of the Voxel API.
	/// </summary>
	/// <remarks>
	/// The sample primarily focuses on: 
	/// - Layer creation 
	/// - Isosurfaces  
	/// - Slices  
	/// - Sections  
	/// - Locked Sections  
	/// Refer to the 
	/// &lt;a href="https://github.com/Esri/arcgis-pro-sdk/wiki/ProSnippets-VoxelLayers"&gt;Voxel Snippets&lt;/a&gt;
	/// for additional examples.
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
				return _this ?? (_this = (Module1)FrameworkApplication.FindModule("VoxelSample_Module"));
			}
		}

		/// <summary>
		/// Subscribes to events fired in response to changes in the voxel layer
		/// </summary>
		/// <returns></returns>
		protected override bool Initialize()
		{
			ArcGIS.Desktop.Mapping.Events.MapMemberPropertiesChangedEvent.Subscribe((args) =>
			{
				var voxel = args.MapMembers.OfType<VoxelLayer>().FirstOrDefault();
				if (voxel == null)
					return;
				//Anything changed on a voxel layer?
				if (args.EventHints.Any(hint => hint == MapMemberEventHint.VoxelSelectedVariableProfileIndex))
				{
					System.Diagnostics.Debug.WriteLine("");
					System.Diagnostics.Debug.WriteLine($"{voxel.Name} " +
						                      $"Voxel Variable Profile changed: {voxel.SelectedVariableProfile.Variable}");
				}
				else if (args.EventHints.Any(hint => hint == MapMemberEventHint.Renderer))
				{
					//This can fire when a renderer becomes ready on a new layer, or the selected variable profile
					//is changed or visualization is changed, etc,etc
					System.Diagnostics.Debug.WriteLine("");
					System.Diagnostics.Debug.WriteLine($"{voxel.Name} renderer event");
				}
				else if (args.EventHints.Any(hint => hint == MapMemberEventHint.VoxelSelectedVariableProfileIndex))
				{
					//This can fire when a renderer becomes ready on a new layer, or the selected variable profile
					//is changed or visualization is changed, etc,etc
					System.Diagnostics.Debug.WriteLine("");
					System.Diagnostics.Debug.WriteLine($"{voxel.Name} renderer event");
				}
			});

			ArcGIS.Desktop.Mapping.Voxel.Events.VoxelAssetChangedEvent.Subscribe((args) =>
			{
				//An asset changed on a voxel layer
				System.Diagnostics.Debug.WriteLine("");
				System.Diagnostics.Debug.WriteLine("VoxelAssetChangedEvent");
				System.Diagnostics.Debug.WriteLine($" AssetType: {args.AssetType}, ChangeType: {args.ChangeType}");

				if (args.ChangeType == VoxelAssetEventArgs.VoxelAssetChangeType.Remove)
					return;
				//Get "what"changed - add or update
				//eg IsoSurface
				if (args.AssetType == VoxelAssetEventArgs.VoxelAssetType.Isosurface)
				{
					var surface = MapView.Active.GetSelectedIsosurfaces().FirstOrDefault();
					//there will only be one selected...
					if (surface != null)
					{
						var voxel = surface.Layer;
						//use it
					}
				}
				//Slices, Sections, LockedSections...
				//GetSelectedSlices(), GetSelectedSections(), GetSelectedLockedSections();
			});

				return true;
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
