Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports ArcGIS.Desktop.Framework
Imports ArcGIS.Desktop.Framework.Contracts

''' <summary>
''' Ensure that the prerequisites for the SDK sample are met and then enable subsequently enabled the buttons to construct the geometries.
''' </summary>
Friend Class Setup
    Inherits Button

    Protected Overrides Sub OnClick()
        ConstructingGeometriesModule.PrepareTheSample()

        ' activate the prerequisite state that the expected layers exist
        FrameworkApplication.State.Activate("layers_exists")
    End Sub
End Class

