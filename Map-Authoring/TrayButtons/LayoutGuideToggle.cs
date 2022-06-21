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
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Layouts.Events;
using ArcGIS.Desktop.Mapping;

namespace TrayButtons
{
  internal class LayoutGuideToggle : LayoutTrayButton
  {
    /// <summary>
    /// Invoked after construction, and after all DAML settings have been loaded. 
    /// Use this to perform initialization such as setting ButtonType.
    /// </summary>
    protected override void Initialize()
    {
      base.Initialize();

      // set the button type
      //  change for different button types
      ButtonType = TrayButtonType.ToggleButton;

      // subscribe to LayoutEvent for when the user
      // toggles the Guides via other parts of the UI
      // (for example the Layout context menu in the TOC)
      LayoutEvent.Subscribe(OnLayoutChanged);
    }

    /// <summary>
    /// Override to perform some button initialization.  This is called the first time the botton is loaded.
    /// </summary>
    protected override void OnButtonLoaded()
    {
      base.OnButtonLoaded();

      // get the initial state
      var lyt = this.Layout;
      if (lyt != null)
      {
        QueuedTask.Run(() =>
        {
          var lyt_cim = lyt.GetDefinition();
          this.SetCheckedNoCallback(lyt_cim.Page.ShowGuides);
        });
      }
    }

    #region TrayButtonType.ToggleButton / TrayButtonType.PopupToggleButton
    // 
    // this method fires when ButtonType = TrayButtonType.ToggleButton or PopupToggleButton
    // 

    private bool _ignoreEvent = false;

    /// <summary>
    /// Called when the toggle button check state changes
    /// </summary>
    protected override void OnButtonChecked()
    {
      // get the checked state
      var isChecked = this.IsChecked;

      // Turn on guides that are already setup in an existing layout
      Layout lyt = this.Layout;

      QueuedTask.Run(() =>
      {
        // updating the layout definition will cause a LayoutEvent with
        // hint = LayoutEventHint.PageChanged to fire. 
        // When we make the change programmatically we dont want to respond 
        // to the event.
        _ignoreEvent = true;
        var lyt_cim = lyt.GetDefinition();
        lyt_cim.Page.ShowGuides = isChecked;

        lyt.SetDefinition(lyt_cim);
        _ignoreEvent = false;
      });
    }

    private void OnLayoutChanged(LayoutEventArgs args)
    {
      var layout = args.Layout;
      if (args.Hint == LayoutEventHint.PageChanged)
      {
        // exit if we're ignoring the event
        if (_ignoreEvent)
          return;

        var oldpge = args.OldPage;
        QueuedTask.Run(() =>
        {
          var lyt_cim = layout.GetDefinition();

          // compare the ShowGuides property in the CIMPage
          if (lyt_cim.Page.ShowGuides != oldpge.ShowGuides)
            // if they are different, make sure the traybutton shows 
            // the correct checked state.
            // SetCheckedNoCallback ensures OnButtonChecked isn't fired.
            SetCheckedNoCallback(lyt_cim.Page.ShowGuides);
        });
      }
    }

    #endregion

  }
}
