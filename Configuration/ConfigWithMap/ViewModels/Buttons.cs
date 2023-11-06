/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ConfigWithMap
{
  class ShowDockPane : Button
  {
    protected override void OnClick()
    {
      DockPane dp = FrameworkApplication.DockPaneManager.Find("Acme_WorkOrders");

      dp.IsVisible = true;
      dp.Activate();
    }

    protected override void OnUpdate()
    {
      if (ConfigWithMapModule.Current.Features != null && ConfigWithMapModule.Current.Features.Count > 0)
        Enabled = true;
      else
        Enabled = false;
    }
  }

  class SelectByLocation : Button
  {
    protected override void OnClick()
    {
      ConfigWithMapModule.Current.SelectByLocation();
    }
  }

  class RebuildNetwork : Button
  {
    protected override void OnClick()
    {
      ConfigWithMapModule.Current.BuildNetwork();
    }
  }

  class CreatePackage : Button
  {
    protected override void OnClick()
    {
      ConfigWithMapModule.Current.CreatePackage();
    }
  }

  class EditButton : Button
  {
    private bool _isEditing;

    protected override void OnClick()
    {
      if (!_isEditing)
        FrameworkApplication.State.Activate("acme_edit");
      else
        FrameworkApplication.State.Deactivate("acme_edit");

      _isEditing = !_isEditing;
    }

    protected override void OnUpdate()
    {
      if (!_isEditing)
        Caption = "Start Editing";
      else
        Caption = "Stop Editing";

    }
  }
}
