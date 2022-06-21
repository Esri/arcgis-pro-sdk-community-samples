/*

   Copyright 2022 Esri

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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;

namespace ToggleSwitches
{
  internal class ToggleSwitches : Button
  {
    protected override async void OnClick()
    {
      // If you run this in the DEBUGGER you will NOT see the dialog progressor
      var pd = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog("Toggle switches - cancelable", "Canceled", 6, false);

      string returnedValue = await ToggleSwitchesModule.RunCancelableToggleSwwitches(new CancelableProgressorSource(pd), MapView.Active.Extent);

      if (!String.IsNullOrEmpty(returnedValue))
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(returnedValue);
    }
  }
}
