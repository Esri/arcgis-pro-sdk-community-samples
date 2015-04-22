'   Copyright 2015 Esri
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
''' This sample provide four buttons showing the construction of geometry types of type MapPoint, Multipoint, Polyline, and Polygon.
''' </summary>
''' <remarks>
''' 1. In Visual Studio click the Build menu. Then select Build Solution.
''' 2. Click Start button to open ArcGIS Pro.
''' 3. ArcGIS Pro will open.
''' 4. Go to the ADD-IN tab
''' 5. Click the Setup button to ensure that the appropriate layers are created. The setup code will ensure that we have a layer of type point,
'''    multi-point, polyline, and polygon. Once the conditions are meet then the individual buttons will become enabled.
''' 6. Step through the buttons to create the various geometry types.
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
    Private Shared Async Function CreateLayer(featureclassName As String, featureclassType As String) As Task(Of Boolean)
        Dim arguments = New List(Of Object)()
        ' store the results in the default geodatabase
        arguments.Add(CoreModule.CurrentProject.DefaultGeodatabasePath)
        ' name of the feature class
        arguments.Add(featureclassName)
        ' type of geometry
        arguments.Add(featureclassType)
        ' no template
        arguments.Add("")
        ' no z values
        arguments.Add("DISABLED")
        ' no m values
        arguments.Add("DISABLED")

        Await QueuedTask.Run(
            Sub()
                ' spatial reference
                arguments.Add(SpatialReferenceBuilder.CreateSpatialReference(3857))
            End Sub)

        Dim result = Await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", Geoprocessing.MakeValueArray(arguments.ToArray()))
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
    ''' /Generate a random coordinate within the provided envelope.
    ''' </summary>
    ''' <param name="random">Instance of a random class.</param>
    ''' <param name="withinThisExtent">Area of interest in which the random coordinate will be created.</param>
    ''' <param name="is3D">Boolean indicator if the coordinate should be 2D (only x,y values) or 3D (containing x,y,z values).</param>
    ''' <returns>A coordinate with random values within the extent.</returns>
    <Extension()>
    Public Function NextCoordinate(ByVal random As Random, withinThisExtent As Envelope, is3D As Boolean) As Coordinate
        Dim newCoordinate As Coordinate

        If (is3D) Then
            newCoordinate = New Coordinate(random.NextDouble(withinThisExtent.XMin, withinThisExtent.XMax),
                random.NextDouble(withinThisExtent.YMin, withinThisExtent.YMax), 0)
        Else
            newCoordinate = New Coordinate(random.NextDouble(withinThisExtent.XMin, withinThisExtent.XMax),
                random.NextDouble(withinThisExtent.YMin, withinThisExtent.YMax))
        End If

        Return newCoordinate
    End Function
End Module
