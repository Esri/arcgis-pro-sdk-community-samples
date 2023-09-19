﻿using ArcGIS.Core.CIM;
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
using System.Threading.Tasks;

namespace Shortcuts.Panes
{
  internal class SamplePaneViewModel : ViewStatePane
  {
    private const string _viewPaneID = "Shortcuts_Panes_SamplePane";

    /// <summary>
    /// Consume the passed in CIMView. Call the base constructor to wire up the CIMView.
    /// </summary>
    public SamplePaneViewModel(CIMView view)
      : base(view) { }

    /// <summary>
    /// Create a new instance of the pane.
    /// </summary>
    internal static SamplePaneViewModel Create()
    {
      var view = new CIMGenericView();
      view.ViewType = _viewPaneID;
      return FrameworkApplication.Panes.Create(_viewPaneID, new object[] { view }) as SamplePaneViewModel;
    }

    #region Pane Overrides

    /// <summary>
    /// Must be overridden in child classes used to persist the state of the view to the CIM.
    /// </summary>
    public override CIMView ViewState
    {
      get
      {
        _cimView.InstanceID = (int)InstanceID;
        return _cimView;
      }
    }

    /// <summary>
    /// Called when the pane is initialized.
    /// </summary>
    protected async override Task InitializeAsync()
    {
      await base.InitializeAsync();
    }

    /// <summary>
    /// Called when the pane is uninitialized.
    /// </summary>
    protected async override Task UninitializeAsync()
    {
      await base.UninitializeAsync();
    }
    protected override void OnKeyCommand(string commandID)
    {
      switch (commandID)
      {
        case "Pane_Cmd":
          MessageBox.Show("Pane shortcut");
          break;
      }
    }
    #endregion Pane Overrides
  }

  /// <summary>
  /// Button implementation to create a new instance of the pane and activate it.
  /// </summary>
  internal class SamplePane_OpenButton : Button
  {
    protected override void OnClick()
    {
      SamplePaneViewModel.Create();
    }
  }
}
