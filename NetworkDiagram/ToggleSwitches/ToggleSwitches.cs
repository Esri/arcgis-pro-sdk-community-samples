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
