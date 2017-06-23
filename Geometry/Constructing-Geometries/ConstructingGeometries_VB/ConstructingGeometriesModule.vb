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
Imports ArcGIS.Desktop.Framework
Imports System.Threading.Tasks
Imports System.Windows.Input
Imports System.Runtime.CompilerServices
Imports ArcGIS.Core.Geometry
Imports ArcGIS.Desktop.Core
Imports ArcGIS.Desktop.Framework.Threading.Tasks
Imports ArcGIS.Desktop.Core.Geoprocessing
Imports ArcGIS.Desktop.Mapping

''' <summary>
''' This sample provide four buttons showing the construction of geometry types of type MapPoint, Multipoint, Polyline, and Polygon and shows how to:
''' * Construct and manipulate geometries
''' * Use GeometryEngine functionality
''' * Search and retrieve features
''' </summary>
''' <remarks>
''' 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
''' 1. Make sure that the Sample data is unzipped in c:\data 
''' 1. The project used for this sample is 'C:\Data\FeatureTest\FeatureTest.aprx'
''' 1. In Visual Studio click the Build menu. Then select Build Solution.
''' 1. Click Start button to open ArcGIS Pro.
''' 1. ArcGIS Pro will open, select the FeatureTest.aprx project
''' 1. Click on the ADD-IN tab and make sure that your active map contains Setup/point/multipoint/line/polygon features buttons as shown below.
''' ![UI](Screenshots/ScreenPoints.png)
''' 1. Click on Setup button to enable the create point and create multipoint buttons 
''' ![UI](Screenshots/ScreenPoint1.png)
''' 1. Click the createPoints button to create random points over the current extent of the map
''' 1. The map extent shows the random created points and also enables create polylines button
''' ![UI](Screenshots/ScreenPoint2.png)
''' 1. Click the createPolylines button to create random lines the current extent of the map
''' 1. The map extent shows the random lines and also enables create polygons button
''' ![UI](Screenshots/ScreenPoint3.png)
''' 1. Click the createPolygons button to create random polygon over the current extent of the map
''' ![UI](Screenshots/ScreenPoint4.png)
''' </remarks>
Friend Class ConstructingGeometriesModule
    Inherits ArcGIS.Desktop.Framework.Contracts.Module

    Private Shared Property _this As Object

    ''' <summary>
    ''' Retrieve the singleton instance to this module here
    ''' </summary>
    Public Shared ReadOnly Property Current() As ConstructingGeometriesModule
        Get
            If (_this Is Nothing) Then
                _this = DirectCast(FrameworkApplication.FindModule("ConstructingGeometries_VB_Module"), ConstructingGeometriesModule)
            End If

            Return _this
        End Get
    End Property

    ''' <summary>
    ''' The method ensures that there are point, multipoint, line, and polygon layers in the map of the active view.
    ''' In case the layer is missing, then a default feature class will be created in the default geodatabase of the project.
    ''' </summary>
    Public Shared Async Sub PrepareTheSample()
        Dim pointLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType(Of FeatureLayer).Where(Function(lyr) lyr.ShapeType = ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint).FirstOrDefault()

        If (IsNothing(pointLayer)) Then
            Await CreateLayer("sdk_points", "POINT")
        End If

        Dim multiPointLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType(Of FeatureLayer).Where(Function(lyr) lyr.ShapeType = ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultipoint).FirstOrDefault()

        If (IsNothing(multiPointLayer)) Then
            Await CreateLayer("sdk_multipoints", "MULTIPOINT")
        End If

        Dim polylineLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType(Of FeatureLayer).Where(Function(lyr) lyr.ShapeType = ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline).FirstOrDefault()

        If (IsNothing(polylineLayer)) Then
            Await CreateLayer("sdk_polyline", "POLYLINE")
        End If

        Dim polygonLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType(Of FeatureLayer).Where(Function(lyr) lyr.ShapeType = ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolygon).FirstOrDefault()

        If (IsNothing(polygonLayer)) Then
            Await CreateLayer("sdk_polygon", "POLYGON")
        End If
    End Sub

    ''' <summary>
    ''' Create a feature class in the default geodatabase of the project.
    ''' </summary>
    ''' <param name="featureclassName">Name of the feature class to be created.</param>
    ''' <param name="featureclassType">Type of feature class to be created. Options are:
    ''' <list type="bullet">
    ''' <item>POINT</item>
    ''' <item>MULTIPOINT</item>
    ''' <item>POLYLINE</item>
    ''' <item>POLYGON</item></list></param>
    ''' <returns></returns>
    Private Shared Async Function CreateLayer(featureclassName As String, featureclassType As String) As Task(Of IGPResult)
    ' store the results in the default geodatabase
    ' name of the feature class
    ' type of geometry
    ' no template
    ' no z values
    ' no m values
    Dim arguments = New List(Of Object) From {
      CoreModule.CurrentProject.DefaultGeodatabasePath,
      featureclassName,
      featureclassType,
      "",
      "DISABLED",
      "DISABLED"
    }

    Await QueuedTask.Run(
            Sub()
              ' spatial reference
              arguments.Add(SpatialReferenceBuilder.CreateSpatialReference(3857))
            End Sub)

    Return Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", Geoprocessing.MakeValueArray(arguments.ToArray()))
    End Function

#Region "Overrides"
    ''' <summary>
    ''' Called by Framework when ArcGIS Pro is closing
    ''' </summary>
    ''' <returns>False to prevent Pro from closing, otherwise True</returns>
    Protected Overrides Function CanUnload() As Boolean
        'TODO - add your business logic
        'return false to ~cancel~ Application close
        Return True
    End Function

    ''' <summary>
    ''' Generic implementation of ExecuteCommand to allow calls to
    ''' <see cref="FrameworkApplication.ExecuteCommand"/> to execute commands in
    ''' your Module.
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Protected Overrides Function ExecuteCommand(id As String) As Func(Of Task)

        'TODO: replace generic implementation with custom logic
        'etc as needed for your Module
        Dim command = TryCast(FrameworkApplication.GetPlugInWrapper(id), ICommand)
        If command Is Nothing Then
            Return Function() Task.FromResult(0)
        End If
        If Not command.CanExecute(Nothing) Then
            Return Function() Task.FromResult(0)
        End If

        Return Function()
                   command.Execute(Nothing)
                   ' if it is a tool, execute will set current tool
                   Return Task.FromResult(0)

               End Function
    End Function

#End Region


End Class

''' <summary>
''' Extension methods to generate random coordinates within a given extent.
''' </summary>
Module RandomExtension
    ''' <summary>
    ''' Generate a random double number between the min and max values.
    ''' </summary>
    ''' <param name="random">Instance of a random class.</param>
    ''' <param name="minValue">The min value for the potential range.</param>
    ''' <param name="maxValue">The max value for the potential range.</param>
    ''' <returns>Random number between min and max</returns>
    ''' <remarks>The random result number will always be less than the max number.</remarks>
    <Extension()>
    Public Function NextDouble(ByVal random As Random, minValue As Double, maxValue As Double) As Double
        Return random.NextDouble() * (maxValue - minValue) + minValue
    End Function

  ''' <summary>
  ''' /Generate a random coordinate (only x,y values) within the provided envelope.
  ''' </summary>
  ''' <param name="random">Instance of a random class.</param>
  ''' <param name="withinThisExtent">Area of interest in which the random coordinate will be created.</param>
  ''' <returns>A coordinate (only x,y values) with random values within the extent.</returns>
  <Extension()>
  Public Function NextCoordinate2D(ByVal random As Random, withinThisExtent As Envelope) As Coordinate2D
    Return New Coordinate2D(random.NextDouble(withinThisExtent.XMin, withinThisExtent.XMax),
                random.NextDouble(withinThisExtent.YMin, withinThisExtent.YMax))
  End Function

  ''' <summary>
  ''' /Generate a random coordinate (containing x,y,z values) within the provided envelope.
  ''' </summary>
  ''' <param name="random">Instance of a random class.</param>
  ''' <param name="withinThisExtent">Area of interest in which the random coordinate will be created.</param>
  ''' <returns>A coordinate (containing x,y,z values) with random values within the extent.</returns>
  <Extension()>
  Public Function NextCoordinate3D(ByVal random As Random, withinThisExtent As Envelope) As Coordinate3D
    Return New Coordinate3D(random.NextDouble(withinThisExtent.XMin, withinThisExtent.XMax),
                random.NextDouble(withinThisExtent.YMin, withinThisExtent.YMax), 0)
  End Function
End Module
