/*

   Copyright 2024 Esri

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
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CreateLineOfSight
{
  internal class LineOfSight : Button
  {
    protected override async void OnClick()
    {
      #region Get the active map view and the layers.
      var mapView = MapView.Active;
      if (mapView == null)
        return;
      var observerPointLayer = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(layer => layer.Name.Equals("ObserverPoint"));
      var targetPointLayer = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(layer => layer.Name.Equals("TargetPoints"));
      var lineOfSightLayer = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(layer => layer.Name.Equals("LineOfSights"));
      var obstructionPtsLayer = mapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(layer => layer.Name.Equals("Obstructions"));
      var editOpTargetPointVisibility = new EditOperation();
      if (observerPointLayer == null || targetPointLayer == null || lineOfSightLayer == null || obstructionPtsLayer == null )
        return;
      //Set the environment for the Line of Sight analysis
      //Delete the old Line of Sight results and select the observer point and target points
      Module1.CallButtonClick("CreateLineOfSight_Reset");
      Module1.CallButtonClick("CreateLineOfSight_StartDemo");
      #endregion
      //Initialize this to store the Line of sight analysis result.
      IDictionary<long, LineOfSightResult> targetPtlineOfSightResultsMapping = new Dictionary<long, LineOfSightResult>();
      var tinLayer = mapView.Map.GetLayersAsFlattenedList().
                            OfType<TinLayer>().FirstOrDefault();
      await QueuedTask.Run(() => {
        #region Get the observer point and the 3 target points from the feature layers in the map
        var observerPtCursor = observerPointLayer.Search();
        MapPoint observerPoint = null; MapPoint targetPoint = null;
        MapPoint observerPointProjected = null;
        while (observerPtCursor.MoveNext())
        {
          var observerPtFeature = observerPtCursor.Current as Feature;
          observerPoint = observerPtFeature.GetShape() as MapPoint;
          observerPointProjected = GeometryEngine.Instance.
                                        Project(observerPoint, mapView.Map.SpatialReference) as MapPoint;
        }
        IDictionary<MapPoint, long> targetPoints = new Dictionary<MapPoint, long>();
        var targetPtCursor = targetPointLayer.Search();
        while (targetPtCursor.MoveNext())
        {
          var targetPtFeature = targetPtCursor.Current as Feature;
          targetPoint = targetPtFeature.GetShape() as MapPoint;
          var TargetPointProjected = GeometryEngine.Instance.Project(targetPoint, mapView.Map.SpatialReference) as MapPoint;
          targetPoints.Add(TargetPointProjected, targetPtFeature.GetObjectID());
        }
        #endregion        
        //Run the Line of Sight analysis for each target point and the single observer point
        foreach (var kvp in targetPoints)
        {
          var targetPt = kvp.Key;
          var targetPtOID = kvp.Value;
          //Step 1: Set the input values for the Line of Sight analysis
          LineOfSightParams lineOfSightParams = new LineOfSightParams();
          lineOfSightParams.TargetPoint = targetPt;
          lineOfSightParams.ObserverPoint = observerPoint;
          lineOfSightParams.OutputSpatialReference = mapView.Map.SpatialReference;
          lineOfSightParams.ObserverHeightOffset = 0; //optional
          lineOfSightParams.TargetHeightOffset = 0; //optional
          #region Advanced settings for the Line of Sight analysis
          //lineOfSightParams.ApplyCurvature = true;
          //lineOfSightParams.RefractionFactor = 0.13;
          //lineOfSightParams.ApplyRefraction = true;
          #endregion       
          //Step 2: Check if the input parameters are valid
          if (tinLayer.CanGetLineOfSight(lineOfSightParams)) 
          {
            //Step 3: Run the analysis            
            LineOfSightResult lineOfSightResult = tinLayer.GetLineOfSight(lineOfSightParams);
            targetPtlineOfSightResultsMapping.Add(targetPtOID, lineOfSightResult); //store the result
          } 
        }
        #region Target Point Visibility - for labeling purpose only

        editOpTargetPointVisibility.Name = "Target Point Visibility";
        editOpTargetPointVisibility.SelectNewFeatures = false;
        //use an inspector to modify the target feature with the visibility attribute
        var modifyTargetPtsLyrInspector = new Inspector();
        foreach (var kvp in targetPtlineOfSightResultsMapping)
        {
          var targetPointOID = kvp.Key;
          var lineOfSighResults = kvp.Value;
          var targetPointVisibility = lineOfSighResults.IsTargetVisibleFromObserverPoint ? 1 : 0;
          modifyTargetPtsLyrInspector.Load(targetPointLayer, targetPointOID);//base attributes on an existing feature
          modifyTargetPtsLyrInspector["TargetVisibleFromObserver"] = targetPointVisibility;
          editOpTargetPointVisibility.Modify(modifyTargetPtsLyrInspector);
        }            
        #endregion
      });
      #region Setup the Edit Operation to create the Line of Sight results and Obstruction Points
      bool obstructionPointsExist = false;
      var editOp = new EditOperation();
      editOp.Name = "Line Of Sight results";
      editOp.SelectNewFeatures = false;

      var editOpObsPts = new EditOperation();
      editOpObsPts.Name = "Obstruction Points";
      editOpObsPts.SelectNewFeatures = false;

      await QueuedTask.Run( () =>targetPointLayer.SetLabelVisibility(true));  //label target points
      #endregion

      //Analyze the "get Line of Sight" results and create the visible and invisible line features
      foreach (var kvp in targetPtlineOfSightResultsMapping)
      {
        var lineOfSightResult = kvp.Value;
        Polyline visibleLine = lineOfSightResult.VisibleLine;
        Polyline invisibleLine = lineOfSightResult.InvisibleLine;
        bool isTargetVisibleFromObserverPoint = lineOfSightResult.IsTargetVisibleFromObserverPoint;
        if (!isTargetVisibleFromObserverPoint)
        {
          MapPoint obstructionPoint = lineOfSightResult.ObstructionPoint;
        }
        #region Create the visible, invisible lines and the obstruction points to visualize on the map.
        var targetPointVisibility = isTargetVisibleFromObserverPoint ? 1 : 0;
        //Is there a Visible line?
        if (visibleLine != null)
        {
          //Define the attribute values for the visible line segment of the Line of Sight result
          Dictionary<string, object> attrVisibleLineOfSight = new Dictionary<string, object>();
          attrVisibleLineOfSight["SHAPE"] = lineOfSightResult.VisibleLine;//Geometry
          attrVisibleLineOfSight["VIS_CODE"] = string.Format("1");         
          attrVisibleLineOfSight["TargetVisibleFromObserver"] = string.Format($"{targetPointVisibility}");                                  
          editOp.Create(lineOfSightLayer, attrVisibleLineOfSight);         
        }
        //Is there an Invisible line?
        if (invisibleLine != null)
        {
          //Define the attribute values for the invisible line segment of the Line of Sight result
          Dictionary<string, object> attrInVisibleLineOfSight = new Dictionary<string, object>();
          attrInVisibleLineOfSight["SHAPE"] = lineOfSightResult.InvisibleLine;//Geometry
          attrInVisibleLineOfSight["VIS_CODE"] = string.Format("2");
          attrInVisibleLineOfSight["TargetVisibleFromObserver"] = string.Format($"{targetPointVisibility}");
          editOp.Create(lineOfSightLayer, attrInVisibleLineOfSight);
          //obstruction point
          if (lineOfSightResult.ObstructionPoint != null)
          {
            obstructionPointsExist = true;
            editOpObsPts.Create(obstructionPtsLayer, lineOfSightResult.ObstructionPoint);
          }         
        }
        #endregion
      }
      #region Execute the edit operations
      // Execute the operation
      await editOp.ExecuteAsync();

      if (obstructionPointsExist)
        await editOpObsPts.ExecuteAsync();

      if (!editOpTargetPointVisibility.IsEmpty)
        await editOpTargetPointVisibility.ExecuteAsync();

      await Project.Current.SaveEditsAsync();


      #endregion
    }
  }
}
