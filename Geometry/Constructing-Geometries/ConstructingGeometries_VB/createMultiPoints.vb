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
''' This code sample shows how to build Multipoint objects. 
''' 20 random points are generated in the extent of the map extent of the active view.
''' </summary>
Friend Class CreateMultiPoints
  Inherits Button

  Protected Overrides Sub OnClick()
    ' to work in the context of the active display retrieve the current map 
    Dim activeMap = MapView.Active.Map

    ' retrieve the first multi-point layer in the map
    Dim multiPointFeatureLayer = activeMap.GetLayersAsFlattenedList().OfType(Of FeatureLayer).Where(
            Function(lyr) lyr.ShapeType = ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultipoint).FirstOrDefault()

    If (IsNothing(multiPointFeatureLayer)) Then
      Return
    End If

    ' construct multipoint
    ConstructSampleMultiPoints(multiPointFeatureLayer)
  End Sub

  ''' <summary>
  ''' Create a single multi-point feature that is comprised of 20 points.
  ''' </summary>
  ''' <param name="multiPointLayer">Multi-point geometry feature layer used to add the multi-point feature.</param>
  ''' <returns></returns>
  Private Function ConstructSampleMultiPoints(multiPointLayer As FeatureLayer) As Task
    ' create a random number generator
    Dim randomGenerator = New Random()

    ' the database and geometry interactions are considered fine-grained and need to be executed on
    ' a separate thread
    Return QueuedTask.Run(
            Function()
              ' get the feature class associated with the layer
              Dim featureClass = DirectCast(multiPointLayer.GetTable(), FeatureClass)
              Dim featureClassDefinition = DirectCast(featureClass.GetDefinition(), FeatureClassDefinition)

              ' store the spatial reference as its own variable
              Dim spatialReference = featureClassDefinition.GetSpatialReference()

              ' define an area of interest. Random points are generated in the allowed
              ' confines of the allow extent range
              Dim areaOfInterest = MapView.Active.Extent

              ' start an edit operation to create new (random) multi-point feature
              Dim createOperation = New EditOperation() With {
                .Name = "Generate multi-point"
              }

              ' retrieve the class definition of the point feature class
              Dim classDefinition = DirectCast(featureClass.GetDefinition(), FeatureClassDefinition)
              Dim multipoint As Multipoint

              If (classDefinition.HasZ()) Then
                ' create a list to hold the 20 coordinates of the multi-point feature
                Dim coordinateList = New List(Of Coordinate3D)(20)
                For index As Integer = 1 To 10
                  ' generate either 2D or 3D geometries
                  coordinateList.Add(randomGenerator.NextCoordinate3D(areaOfInterest))
                Next
                multipoint = MultipointBuilder.CreateMultipoint(coordinateList, classDefinition.GetSpatialReference())
              Else
                ' create a list to hold the 20 coordinates of the multi-point feature
                Dim coordinateList = New List(Of Coordinate2D)(20)
                For index As Integer = 1 To 10
                  coordinateList.Add(randomGenerator.NextCoordinate2D(areaOfInterest))
                Next
                multipoint = MultipointBuilder.CreateMultipoint(coordinateList, classDefinition.GetSpatialReference())
              End If
              ' create and execute the feature creation operation
              createOperation.Create(multiPointLayer, multipoint)

              Return createOperation.ExecuteAsync()
            End Function)
  End Function

End Class

