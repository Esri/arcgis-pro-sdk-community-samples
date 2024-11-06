//
// Copyright 2024 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KnowledgeGraphConstructionTools
{
  internal enum RelateToolState
  {
    IdentifyOriginFeature = 0,
    IdentifyDestinationFeature,
    CreateRelationship
  }

  internal class CreateRelate : MapTool
  {
    public CreateRelate()
    {
      IsSketchTool = true;
      UseSnapping = true;
      // Select the type of construction tool you wish to implement.  
      // Make sure that the tool is correctly registered with the correct component category type in the daml 
      SketchType = SketchGeometryType.Point;
      // SketchType = SketchGeometryType.Line;
      // SketchType = SketchGeometryType.Polygon;
      //Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
      UsesCurrentTemplate = true;
      //Gets or sets whether the tool supports firing sketch events when the map sketch changes. 
      //Default value is false.
      FireSketchEvents = true;

      _helper = new RelateToolHelper();
    }

    // tracks the state of the tool
    private RelateToolState State;
    // the helper
    private RelateToolHelper _helper;

    protected override Task OnToolActivateAsync(bool hasMapViewChanged)
    {
      // initialize state
      State = RelateToolState.IdentifyOriginFeature;
      // initialize sketchTip and cursor
      SketchTip = _helper.GetSketchTip(RelateToolState.IdentifyOriginFeature, CurrentTemplate);
      Cursor = System.Windows.Input.Cursors.Arrow;

      // initialize symbols
      _helper.InitSymbols();

      return base.OnToolActivateAsync(hasMapViewChanged);
    }

    protected override Task OnToolDeactivateAsync(bool hasMapViewChanged)
    {
      _invoker = null;
      // clear overlay and symbols
      _helper.ClearOverlay();
      _helper.ClearSymbols();

      return base.OnToolDeactivateAsync(hasMapViewChanged);
    }

    /// <summary>
    /// Called when the sketch finishes. 
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch.</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      // Avoid default finish sketch handling - we are doing it instead in CreateRelationship 
      //   which we call explicitly on the second mouse click
      return Task.FromResult(true);
    }


    private DelayedInvoker _invoker;
    private System.Windows.Point _lastPoint;
    protected override void OnToolMouseMove(MapViewMouseEventArgs args)
    {
      base.OnToolMouseMove(args);
      _invoker ??= new(10);
      _lastPoint = args.ClientPoint;

      // use a delayInvoker to thin out mouseMove notifications
      // don't await
      _invoker.InvokeTask(() => OnMouseMoveImpl(_lastPoint));
    }

    private Task OnMouseMoveImpl(System.Windows.Point mouseLocation)
    {
      return QueuedTask.Run(async () =>
      {
        // try to find an entity feature under the mouse location
        var (l, oid, pt) = await _helper.FindEntityFeature(mouseLocation);
        if (l is null)
        {
          // if nothing found, update the sketch tips, state
          if (State == RelateToolState.IdentifyOriginFeature)
            SketchTip = _helper.GetSketchTip(RelateToolState.IdentifyOriginFeature, CurrentTemplate);
          else if (State == RelateToolState.IdentifyDestinationFeature)
          {
            if (_helper.Layer0 != null)
              SketchTip = _helper.GetSketchTip(RelateToolState.IdentifyDestinationFeature, CurrentTemplate);
            else
              State = RelateToolState.IdentifyOriginFeature;
          }
        }
        // process results (could be nothing if cursor wasn't over an entity feature)
        await ProcessEntityFeature(l, oid, pt);
      });
    }

    // Process a layer, oid pair
    // layer could be null, oid can be -1 if nothing to process
    private async Task ProcessEntityFeature(Layer layer, long oid, MapPoint pt)
    {
      // make sure on MCT
      if (!QueuedTask.OnWorker)
        return;

      // determine the display expression
      // (used to update the SketchTip)
      var disp = oid.ToString();
      if (layer is FeatureLayer fl)
      {
        var displayExpressions = fl.GetDisplayExpressions([oid]);
        if (displayExpressions?.Count == 1)
          disp = displayExpressions[0];
      }

      // if identifying origin
      if (State == RelateToolState.IdentifyOriginFeature)
      {
        // set it on the helper
        _helper.SetFeature(State, disp, layer, oid);
        // update cursor, sketchTIp according to whether something was found
        if (oid == -1)
        {
          SetCursorNo();
        }
        else
        {
          SetCursorYes();
          SketchTip = _helper.GetSketchTip(RelateToolState.IdentifyDestinationFeature, CurrentTemplate);
        }
      }
      // else if identifying destination
      else if (State == RelateToolState.IdentifyDestinationFeature)
      {
        // set it on the helper
        _helper.SetFeature(State, disp, layer, oid);

        // clear overlay
        _helper.ClearOverlay();

        // if I have origin and destination feature geometries
        if (_helper.HasOriginDestinationGeometries())
        {
          // add the relationship line to the overlay
          _helper.AddRelationshipLineToOverlay();
          // update cursor, sketchTip
          SetCursorYes();
          SketchTip = _helper.GetSketchTip(RelateToolState.CreateRelationship, CurrentTemplate);
        }
        else
        {
          // add a line between origin and "pt" (mouse cursor)
          _helper.AddLineOverlay(pt);
          // update cursor, sketchTIp
          SetCursorNo();
          SketchTip = _helper.GetSketchTip(RelateToolState.IdentifyDestinationFeature, CurrentTemplate);
        }
      }
    }

    protected override void OnToolMouseDown(MapViewMouseButtonEventArgs args)
    {
      if (args.ChangedButton == MouseButton.Left)
      {
        // progress the state when the mouse is clicked
        if (State == RelateToolState.IdentifyOriginFeature)
        {
          State = RelateToolState.IdentifyDestinationFeature;
        }
        else if (State == RelateToolState.IdentifyDestinationFeature)
        {
          CreateRelationship();
          _helper.ClearOverlay();
          State = RelateToolState.IdentifyOriginFeature;
        }
      }

      base.OnToolMouseDown(args);
    }

    // create relationship with the cached origin and destination info
    internal Task<bool> CreateRelationship() => CreateRelationship(CurrentTemplate, _helper.Layer0, _helper.OID0, _helper.Layer1, _helper.OID1);

    // create relationship with the specified entity info
    internal async Task<bool> CreateRelationship(EditingTemplate template, Layer fromLayer, long fromOID, Layer toLayer, long toOID)
    {
      if (template == null || template.MapMember == null || fromLayer == null || toLayer == null || fromOID == -1L || toOID == -1L)
        return false;

      // Create an edit operation
      var createOperation = new EditOperation();
      createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
      createOperation.SelectNewFeatures = true;

      // create the row handles
      var originRowHandle = new RowHandle(fromLayer, fromOID);
      var destinationRowHandle = new RowHandle(toLayer, toOID);
      // create relationship description
      var rd = new KnowledgeGraphRelationshipDescription(originRowHandle, destinationRowHandle);
      // queue the Create
      createOperation.Create(template.MapMember as Layer, rd);

      // Execute the operation
      await createOperation.ExecuteAsync();

      return createOperation.IsSucceeded;
    }


    #region Cursor Management

    internal void SetCursorNo() => SetCursor(true);
    internal void SetCursorYes() => SetCursor(false);

    internal bool wasNo => Cursor == System.Windows.Input.Cursors.No;
    internal void SetCursor(bool no)
    {
      if (wasNo != no)
      {
        Cursor = (no) ? System.Windows.Input.Cursors.No : System.Windows.Input.Cursors.Arrow;
      }
    }

    #endregion

  }
}
