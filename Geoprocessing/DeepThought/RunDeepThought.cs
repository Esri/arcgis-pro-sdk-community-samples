/*

   Copyright 2023 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepThought
{
  internal class RunDeepThought : Button
  {
    protected override async void OnClick()
    {
      try
      {
        // no default parameters
        var progDlg = new ProgressDialog($@"Waiting for the answer to the meaning of life", "Canceled", false);
        var progsrc = new CancelableProgressorSource(progDlg);

        var args = Geoprocessing.MakeValueArray();
        var gpResult = await Geoprocessing.ExecuteToolAsync("DeepThought.answer", args,
                  null, progsrc.Progressor);

        // gpResult is the returned result object from a call to ExecuteToolAsync
        if (gpResult.IsFailed)
        {
          // display error messages if the tool fails, otherwise shows the default messages
          if (gpResult.Messages.Count() != 0)
          {
            Geoprocessing.ShowMessageBox(gpResult.Messages, "GP Messages",
                            gpResult.IsFailed ?
                            GPMessageBoxStyle.Error : GPMessageBoxStyle.Default);
          }
          else
          {
            MessageBox.Show($@"GP Tool DeepThought.answer failed with errorcode [{gpResult.ErrorCode}], check parameters.");
          }
        }
        else
        {
          Geoprocessing.ShowMessageBox(gpResult.Messages, $@"Result: {gpResult.ReturnValue}",
                                        GPMessageBoxStyle.Default);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error: {ex.ToString()}");
      }
    }
  }
}
