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
Imports ArcGIS.Desktop.Editing
Imports ArcGIS.Core.Data
Imports ArcGIS.Core.Geometry

''' <summary>
''' This code sample shows how to build MapPoint objects. 
''' 20 random points are generated in the extent of the map extent of the active view.
''' </summary>
Friend Class CreatePoints
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

    ' first generate some random points
    Await ConstructSamplePoints(pointFeatureLayer)

    ' activate the button completed state
    FrameworkApplication.State.Activate("geometry_points_constructed")
  End Sub

  ''' <summary>
  ''' Create random sample points in the extent of the spatial reference.
  ''' </summary>
  ''' <param name="pointFeatureLayer">Point geometry feature layer used to the generate the points.</param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Private Function ConstructSamplePoints(pointFeatureLayer As FeatureLayer) As Task(Of Boolean)
    ' create a random number generator
    Dim randomGenerator = New Random()

    ' the database and geometry interactions are considered fine-grained and must be executed on
    ' the main CIM thread
    Return QueuedTask.Run(
            Function()
              ' start an edit operation to create new (random) point features
              Dim createOperation = New EditOperation() With {
                .Name = "Generate points",
                .SelectNewFeatures = False
              }

              ' get the feature class associated with the layer
              Dim featureClass = DirectCast(pointFeatureLayer.GetTable(), FeatureClass)

              ' define an area of interest. Random points are generated in the allowed
              ' confines of the allow extent range
              Dim areaOfInterest = MapView.Active.Extent

              Dim newMapPoint As MapPoint

              ' retrieve the class definition of the point feature class
              Dim classDefinition = DirectCast(featureClass.GetDefinition(), FeatureClassDefinition)

              ' store the spatial reference as its own variable
              Dim spatialReference = classDefinition.GetSpatialReference()

              ' create 20 new point geometries and queue them for creation
              For i As Integer = 0 To 20
                ' generate either 2D or 3D geometries
                If (classDefinition.HasZ()) Then
                  newMapPoint = MapPointBuilder.CreateMapPoint(randomGenerator.NextCoordinate3D(areaOfInterest), spatialReference)
                Else
                  newMapPoint = MapPointBuilder.CreateMapPoint(randomGenerator.NextCoordinate2D(areaOfInterest), spatialReference)
                End If

                ' queue feature creation
                createOperation.Create(pointFeatureLayer, newMapPoint)
              Next

              ' execute the edit (feature creation) operation
              Return createOperation.ExecuteAsync()
            End Function)
  End Function
End Class

