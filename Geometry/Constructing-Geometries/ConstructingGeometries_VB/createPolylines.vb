'   Copyright 2017 Esri
'   Licensed under the Apache License, Version 2.0 (the "License");
'   you may not use this file except in compliance with the License.
'   You may obtain a copy of the License at

'       http://www.apache.org/licenses/LICENSE-2.0

'   Unless required by applicable law or agreed to in writing, software
'   distributed under the License is distributed on an "AS IS" BASIS,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'   See the License for the specific language governing permissions and
'   limitations under the License. 

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports ArcGIS.Desktop.Framework
Imports ArcGIS.Desktop.Framework.Contracts
Imports ArcGIS.Desktop.Mapping
Imports ArcGIS.Core.Data
Imports ArcGIS.Core.Geometry
Imports ArcGIS.Desktop.Editing
Imports ArcGIS.Desktop.Framework.Threading.Tasks

''' <summary>
''' This code sample shows how to build Polyline objects. 
''' The code will take point geometries from the point feature layer and construct polylines with 5 vertices each.
''' </summary>
Friend Class CreatePolylines
  Inherits Button

  Protected Overrides Async Sub OnClick()
    ' to work in the context of the active display retrieve the current map 
    Dim activeMap = MapView.Active.Map

    ' retrieve the first point layer in the map
    Dim pointFeatureLayer = activeMap.GetLayersAsFlattenedList().OfType(Of FeatureLayer).Where(
            Function(lyr) lyr.ShapeType = ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint).FirstOrDefault()

    If (IsNothing(pointFeatureLayer)) Then
      Return
    End If

    ' retrieve the first polyline feature layer in the map
    Dim polylineFeatureLayer = activeMap.GetLayersAsFlattenedList().OfType(Of FeatureLayer).Where(
            Function(lyr) lyr.ShapeType = ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline).FirstOrDefault()

    If (IsNothing(polylineFeatureLayer)) Then
      Return
    End If

    ' construct polyline from points
    Await ConstructSamplePolylines(polylineFeatureLayer, pointFeatureLayer)

    ' activate the button completed state
    FrameworkApplication.State.Activate("geometry_lines_constructed")
  End Sub

  ''' <summary>
  ''' Create sample polyline feature using the geometries from the point feature layer.
  ''' </summary>
  ''' <param name="polylineLayer">Polyline geometry feature layer used to add the new features.</param>
  ''' <param name="pointLayer">The geometries from the point layer are used as vertices for the new line features.</param>
  ''' <returns></returns>
  Private Function ConstructSamplePolylines(polylineLayer As FeatureLayer, pointLayer As FeatureLayer) As Task(Of Boolean)
    ' execute the fine grained API calls on the CIM main thread
    Return QueuedTask.Run(
            Function()
              ' get the underlying feature class for each layer
              Dim polylineFeatureClass = DirectCast(polylineLayer.GetTable(), FeatureClass)
              Dim pointFeatureClass = DirectCast(pointLayer.GetTable(), FeatureClass)

              ' retrieve the feature class schema information for the feature classes
              Dim polylineDefinition = DirectCast(polylineFeatureClass.GetDefinition(), FeatureClassDefinition)
              Dim pointDefinition = DirectCast(pointFeatureClass.GetDefinition(), FeatureClassDefinition)

              ' construct a cursor for all point features, since we want all feature there is no
              ' QueryFilter required
              Dim pointCursor = pointFeatureClass.Search(Nothing, False)

              ' initialize a counter variable
              Dim pointCounter As Integer = 0
              ' initialize a list to hold 5 coordinates that are used as vertices for the polyline
              Dim lineMapPoints = New List(Of MapPoint)(5)

              ' set up the edit operation for the feature creation
              Dim createOperation = New EditOperation() With {
                .Name = "Create polylines",
                .SelectNewFeatures = False
              }

              ' loop through the point features
              Do While (pointCursor.MoveNext())
                pointCounter += 1

                Dim pointFeature = DirectCast(pointCursor.Current, Feature)
                ' add the feature point geometry as a coordinate into the vertex list of the line
                ' - ensure that the projection of the point geometry is converted to match the spatial reference of the line
                lineMapPoints.Add(DirectCast(GeometryEngine.Instance.Project(pointFeature.GetShape(), polylineDefinition.GetSpatialReference()), MapPoint))

                ' for every 5 geometries, construct a new polyline and queue a feature create
                If (pointCounter Mod 5 = 0) Then
                  ' construct a new polyline by using the 5 point coordinate in the current list
                  Dim newPolyline = PolylineBuilder.CreatePolyline(lineMapPoints, polylineDefinition.GetSpatialReference())
                  ' queue the create operation as part of the edit operation
                  createOperation.Create(polylineLayer, newPolyline)
                  ' reset the list of coordinates
                  lineMapPoints = New List(Of MapPoint)(5)
                End If
              Loop

              ' execute the edit (create) operation
              Return createOperation.ExecuteAsync()
            End Function)
  End Function
End Class

