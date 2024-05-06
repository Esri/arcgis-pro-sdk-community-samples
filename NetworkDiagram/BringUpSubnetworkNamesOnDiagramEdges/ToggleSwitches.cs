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
using ArcGIS.Desktop.Framework.Contracts;
using Dialogs = ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using static BringUpSubnetworkNamesOnDiagramEdges.CommonTools;
using static BringUpSubnetworkNamesOnDiagramEdges.BringUpSubnetworkNamesOnDiagramEdgesModule;
using System;

namespace BringUpSubnetworkNamesOnDiagramEdges
{
  internal class ToggleSwitches : Button
  {
    protected override async void OnClick()
    {
      if (GlobalIsRunning || MapView.Active?.Map == null || MapView.Active.Map.MapType != ArcGIS.Core.CIM.MapType.NetworkDiagram)
        return;

      // If you run this in the DEBUGGER you will NOT see the dialog progressor
      var pd = new ProgressDialog("Toggle switches - cancelable", "Canceled", 5, false);

      try
      {
        string returnedValue = await RunCancelableToggleSwitches(new CancelableProgressorSource(pd), MapView.Active?.Extent);

        if (!string.IsNullOrEmpty(returnedValue))
          Dialogs.MessageBox.Show(returnedValue);
      }
      catch (Exception ex)
      {
        ShowException(ex);
      }
    }
  }
}
