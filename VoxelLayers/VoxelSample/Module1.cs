/*

   Copyright 2022 Esri

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
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Mapping.Voxel.Events;

namespace VoxelSample
{
  /// <summary>
  /// Illustrates use of various aspects of the Voxel API.
  /// </summary>
  /// <remarks>
  /// 1. This sample specificially requires the sample data in CommunitySampleData-VoxelLayer-mm-dd-yyyy.zip (see Sample data link above).
  /// 1. Make sure you unzip to c:\data and have C:\Data\VoxelData available before you run this sample.
  /// 1. Build the sample and start a debug session.
  /// 1. In ArcGIS Pro open a new 'Local Scene' and then use the 'CreateLayer' button to create a sample voxel layer.
  /// 1. The voxel layer should look like this:
  /// ![UI](Screenshots/Screenshot1.png)
  /// The sample primarily focuses on: 
  /// - Layer creation 
  /// - Isosurfaces  
  /// - Slices  
  /// - Sections  
  /// - Locked Sections  
  /// For additional examples refer to &lt;a href="https://github.com/Esri/arcgis-pro-sdk/wiki/ProSnippets-VoxelLayers"&gt;Voxel Snippets&lt;/a&gt;
  /// For concepts refer to the &lt;a href="https://github.com/Esri/arcgis-pro-sdk/wiki/ProConcepts-Voxel-Layers&gt;Voxel Pro Guide&lt;/a&gt;.
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
        if (args.EventHints.Any(hint => hint == MapMemberEventHint.VoxelSelectedVariable))
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
        else if (args.EventHints.Any(hint => hint == MapMemberEventHint.VoxelSelectedVariable))
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
