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

namespace ProStartPageConfig.UI
{
  internal class ShowCreateANewProjectWindow : Button
  {

    private CreateANewProjectWindow _createanewprojectwindow = null;

    protected override void OnClick()
    {
      //already open?
      if (_createanewprojectwindow != null)
        return;
      _createanewprojectwindow = new CreateANewProjectWindow();
      _createanewprojectwindow.Owner = FrameworkApplication.Current.MainWindow;
      _createanewprojectwindow.Closed += (o, e) => { _createanewprojectwindow = null; };
      _createanewprojectwindow.Show();
      //uncomment for modal
      //_createanewprojectwindow.ShowDialog();
    }

  }
}
