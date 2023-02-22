/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CopyPaste
{
  /// <summary>
  /// This sample shows how to implement a custom copy/paste handler via the Module class.  The sample allows the operator to use the copy paste functionality of ArcGIS Pro to implement a custom implementation.
  /// </summary>
  /// <remarks>   
  /// 1. Open this solution in Visual Studio.  
  /// 1. Click the build menu and select Build Solution.
  /// 1. Launch the debugger to open ArCGIS Pro. 
  /// 1. Open any project with a map that contains Point, Line or Polygon feature layers.
  /// 1. Click on the 'Custom Copy/Paste' tab and note the 'Create Graphic' group.
  /// 1. Click the one of the button in the 'Create Graphic' group to create geometry for which you have a feature layer in your map's table of content.
  /// 1. Create a geometry.
  /// ![UI](Screenshots/Screen1.png)
  /// 1. Select a feature layer with a geometry type that matched the created graphic using the 'Paste Into Layer' combobox.
  /// ![UI](Screenshots/Screen2.png)
  /// 1. You can click the 'Show Clipboard' button in the 'Analysis' group to view the Clipboard content.
  /// ![UI](Screenshots/Screen3.png)
  /// 1. Click the paste button (this button is the out-of-box ArcGIS Pro Paste button).
  /// ![UI](Screenshots/Screen5.png)
  /// 1. The graphic's geometry has been copied to the selected 'Paste to Layer'
  /// ![UI](Screenshots/Screen4.png)
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    private Geometry _theGeometry;
    internal Geometry TheGeometry
    {
      get
      {
        return _theGeometry;
      }
      set
      {
        _theGeometry = value;

        // we have a new Geometry therefore we add a custom format to the clipboard
        System.Windows.Clipboard.Clear();
        System.Windows.Clipboard.SetData(TheGeometryType.ToString(), TheGeometry.ToJson());
      }
    }
    internal GraphicsLayer TheGraphicsLayer { get; set; }
    internal string SelectedDestinationFeatureLayer { get; set; }

    private GeometryType TheGeometryType
    {
      get
      {
        return _theGeometry != null ? _theGeometry.GeometryType : GeometryType.Unknown;
      }
    }

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("CopyPaste_Module");
        
    #region Copy

    protected override async Task<bool> CanCopyAsync()
    {
      var canCopy = TheGeometryType != GeometryType.Unknown;
      if (canCopy)
      {
        System.Diagnostics.Trace.WriteLine($@"CanCopyAsync: {canCopy}");
      }
      return canCopy;
    }

    protected override Task CopyAsync()
    {
      System.Windows.Clipboard.SetData(TheGeometryType.ToString(), TheGeometry.ToJson());
      return Task.FromResult(0);
    }

    #endregion

    #region Paste

    protected override async Task<bool> CanPasteAsync()
    {
      // first check the clipboard if we have 'custom' data that we can paste
      var canPaste = 
      System.Windows.Clipboard.ContainsData(GeometryType.Point.ToString())
        || System.Windows.Clipboard.ContainsData(GeometryType.Polygon.ToString())
        || System.Windows.Clipboard.ContainsData(GeometryType.Polyline.ToString());
      // next check that the destination layer is present
      var hasDestination = !string.IsNullOrEmpty(SelectedDestinationFeatureLayer);
      if (canPaste && hasDestination == true)
      {
        System.Diagnostics.Trace.WriteLine($@"CanPasteAsync: {canPaste && hasDestination}");
      }
      return canPaste && hasDestination;
    }

    protected override async Task PasteAsync()
    {
      FeatureLayer destinationFeatureLayer = null;
      Geometry geometry= null;
      try
      {
        var selectedGeometryType = string.Empty;
        List<GeometryType> geometryTypes = new()
        { GeometryType.Point,
          GeometryType.Polyline,
          GeometryType.Polygon };
        foreach (GeometryType geometryType in geometryTypes) 
        {
          if (System.Windows.Clipboard.ContainsData(geometryType.ToString()))
          {
            destinationFeatureLayer = await GetFeatureLayerAsync(SelectedDestinationFeatureLayer, geometryType);
            switch (geometryType)
            {
              case GeometryType.Point:
                geometry = MapPointBuilderEx.FromJson(System.Windows.Clipboard.GetData(geometryType.ToString()).ToString());
                break;
              case GeometryType.Polyline:
                geometry = PolylineBuilderEx.FromJson(System.Windows.Clipboard.GetData(geometryType.ToString()).ToString());
                break;
              case GeometryType.Polygon:
                geometry = PolygonBuilderEx.FromJson(System.Windows.Clipboard.GetData(geometryType.ToString()).ToString());
                break;
            }
            selectedGeometryType = geometryType.ToString();
            break;
          }
        }
        if (destinationFeatureLayer == null || geometry == null)
        {
          throw new Exception($@"Only paste of type Point, Polygon, Polyline are supported");
        }
        // Create a new feature using the clipboard geometry
        await QueuedTask.Run(() =>
        {
          var editOperation = new EditOperation()
          {
            Name = $@"Paste {selectedGeometryType}"
          };
          editOperation.Create(destinationFeatureLayer, geometry);
          if (!editOperation.Execute()) 
            throw new Exception($@"Paste of {selectedGeometryType} failed: {editOperation.ErrorMessage}");
          // since the graphics layer geometry has been copied we can clean up the graphics layer
          TheGraphicsLayer.RemoveElements(null);
          MapView.Active.Invalidate(TheGraphicsLayer, geometry.Extent);
        });
      }
      catch (Exception ex) 
      {
        MessageBox.Show($@"Paste operation failed: {ex.Message}");
      }
      return ;
    }

    private static async Task<FeatureLayer> GetFeatureLayerAsync (string layerName, GeometryType destinationGeomType)
    {
      return await QueuedTask.Run<FeatureLayer>(() =>
      {
        if (string.IsNullOrEmpty(layerName))
        {
          throw new Exception($@"You have to select a '{destinationGeomType}' destination feature class in order to paste the geometry");
        }
        if (MapView.Active?.Map == null) return null;
        var featureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(fl => fl.Name.Contains(layerName)).FirstOrDefault();
        if (featureLayer == null)
        {
          throw new Exception($@"You have to select a '{destinationGeomType}' destination feature class in order to paste the geometry");
        }
        var fcShapeType = featureLayer.GetFeatureClass().GetDefinition().GetShapeType();
        if (fcShapeType != destinationGeomType)
        {
          throw new Exception($@"You cannot paste geometries of type '{destinationGeomType}' into a feature class {layerName} of type {fcShapeType}''");
        }          
        return featureLayer;
      });
    }

    #endregion

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
