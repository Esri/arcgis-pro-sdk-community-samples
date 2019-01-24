//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ProgressDialog
{
    /// <summary>
    /// Exercises a simple non-cancelable progress dialog
    /// </summary>
    internal class RunDialogButtonsSimple : Button
    {
        protected override async void OnClick()
        {
            //If you run this in the DEBUGGER you will NOT see the dialog
            var ps = new ProgressorSource("Doing my thing...");
            ps.Max = 5;
            await ProgressDialogModule.RunProgress(ps, 5);
        }
    }

    /// <summary>
    /// Exercises a simple cancelable progress dialog
    /// </summary>
    internal class RunDialogButtonsCancel : Button
    {
        protected override async void OnClick()
        {   //If you run this in the DEBUGGER you will NOT see the dialog
            uint maxSteps = 10;
            var pd = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog(
                "Doing my thing - cancelable", "Canceled", maxSteps, false);
            await
                ProgressDialogModule.RunCancelableProgress(
                    new CancelableProgressorSource(pd), maxSteps);
        }
    }

    /// <summary>
    /// Exercises a simple non-cancelable progress dialog manual controlled
    /// </summary>
    internal class RunDialogButtonsManual : Button
    {
        private ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog progDialog;

        protected override async void OnClick()
        {   //#2 create just a ProgressDialog for manual control
            progDialog = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog ("Manual Dialog with programmatic show and hide");
            progDialog.Show();
            await DoSomeWork(progDialog);
        }

        private static Task DoSomeWork(ArcGIS.Desktop.Framework.Threading.Tasks.ProgressDialog progDialog)
        {
            return QueuedTask.Run(async () =>
            {
                for (uint iSeconds = 0; iSeconds < 10; iSeconds++)
                {
                    await Task.Delay(1000);
                }
                progDialog.Hide();
            });

        }

    }


}
