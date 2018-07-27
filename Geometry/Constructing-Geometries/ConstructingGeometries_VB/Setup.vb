'Copyright 2018 Esri

'Licensed under the Apache License, Version 2.0 (the "License");
'you may not use this file except in compliance with the License.
'You may obtain a copy of the License at

'    http://www.apache.org/licenses/LICENSE-2.0

'Unless required by applicable law or agreed to in writing, software
'distributed under the License is distributed on an "AS IS" BASIS,
'WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

'See the License for the specific language governing permissions and
'limitations under the License.
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

