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
Imports ArcGIS.Desktop.Framework.Threading.Tasks
Imports ArcGIS.Core.Geometry
Imports ArcGIS.Core.Data
Imports ArcGIS.Desktop.Editing

''' <summary>
''' This code sample shows how to build Polygon objects. 
''' The code will take line geometries from the line feature layer and construct a polygon from a convex hull for all lines.
''' </summary>
Friend Class CreatePolygons
  Inherits Button

  Protected Overrides Async Sub OnClick()
    ' to work in the context of the active display retrieve the current map 
    Dim activeMap = MapView.Active.Map

    ' retrieve the first line layer in the map
    Dim lineFeatureLayer = activeMap.GetLayersAsFlattenedList().OfType(Of FeatureLayer).Where(
            Function(lyr) lyr.ShapeType = ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline).FirstOrDefault()

    If (IsNothing(lineFeatureLayer)) Then
      Return
    End If

    ' retrieve the first polygon feature layer in the map
    Dim polygonFeatureLayer = activeMap.GetLayersAsFlattenedList().OfType(Of FeatureLayer).Where(
            Function(lyr) lyr.ShapeType = ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon).FirstOrDefault()

    If (IsNothing(polygonFeatureLayer)) Then
      Return
    End If

    ' construct the polyline based of the convex hull of all polylines
    Await ConstructSamplePolygon(polygonFeatureLayer, lineFeatureLayer)
  End Sub

  ''' <summary>
  ''' Create sample polygon feature using the point geometries from the multi-point feature using the 
  ''' ConvexHull method provided by the GeometryEngine.
  ''' </summary>
  ''' <param name="polygonLayer">Polygon geometry feature layer used to add the new feature.</param>
  ''' <param name="lineLayer">The polyline feature layer containing the features used to construct the polygon.</param>
  ''' <returns></returns>
  Private Function ConstructSamplePolygon(polygonLayer As FeatureLayer, lineLayer As FeatureLayer) As Task(Of Boolean)
    ' execute the fine grained API calls on the CIM main thread
    Return QueuedTask.Run(
            Function()
              ' get the underlying feature class for each layer
              Dim polygonFeatureClass = DirectCast(polygonLayer.GetTable(), FeatureClass)
              Dim polygonDefinition = DirectCast(polygonFeatureClass.GetDefinition(), FeatureClassDefinition)
              Dim lineFeatureClass = DirectCast(lineLayer.GetTable(), FeatureClass)

              ' construct a cursor to retrieve the line features
              Dim lineCursor = lineFeatureClass.Search(Nothing, False)

              ' set up the edit operation for the feature creation
              Dim createOperation = New EditOperation() With
                  {
                      .Name = "Create polygons",
                      .SelectNewFeatures = False
                  }

              Dim polylineBuilder = New PolylineBuilder(polygonDefinition.GetSpatialReference())

              Do While (lineCursor.MoveNext())
                ' retrieve the first feature
                Dim lineFeature = DirectCast(lineCursor.Current, Feature)

                ' add the coordinate collection of the current geometry into our overall list of collections
                Dim polylineGeometry = DirectCast(lineFeature.GetShape(), Polyline)
                polylineBuilder.AddParts(polylineGeometry.Parts)
              Loop

              ' use the ConvexHull method from the GeometryEngine to construct the polygon geometry
              Dim newPolygon = DirectCast(GeometryEngine.Instance.ConvexHull(polylineBuilder.ToGeometry()), Polygon)

              ' queue the polygon creation
              createOperation.Create(polygonLayer, newPolygon)

              ' execute the edit (polygon create) operation
              Return createOperation.ExecuteAsync()
            End Function)
  End Function
End Class

