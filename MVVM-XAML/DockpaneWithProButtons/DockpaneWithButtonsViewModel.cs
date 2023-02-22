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
using ArcGIS.Desktop.Internal.Mapping.Locate;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DockpaneWithProButtons
{
  internal class DockpaneWithButtonsViewModel : DockPane
  {
    private const string _dockPaneID = "DockpaneWithProButtons_DockpaneWithButtons";

    protected DockpaneWithButtonsViewModel() { }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    #region Button Commands

    public ICommand CmdOpenProject
    {
      get
      {
        return new RelayCommand(() =>
        {
          IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("esri_core_openProjectButton");
          var command = wrapper as ICommand;
          if ((command != null) && command.CanExecute(null))
            command.Execute(null);
        });
      }
    }

    public ICommand CmdEsriStyleButton
    {
      get
      {
        return new RelayCommand((cmdParams) =>
        {
          MessageBox.Show($@"Esri styled button was clicked with this parameter {Environment.NewLine}{cmdParams}");
        }, () => true);
      }
    }

    public ICommand CmdBorderlessButton
    {
      get
      {
        return new RelayCommand((cmdParams) =>
        {
          MessageBox.Show($@"Esri Borderless styled button was clicked with this parameter {Environment.NewLine}{cmdParams}");
        }, () => true);
      }
    }

    public ICommand CmdImageButton
    {
      get
      {
        return new RelayCommand((cmdParams) =>
        {
          MessageBox.Show($@"Esri image button was clicked with this parameter {Environment.NewLine}{cmdParams}");
        }, () => true);
      }
    }

    public ICommand CmdStackedButton
    {
      get
      {
        return new RelayCommand((cmdParams) =>
        {
          MessageBox.Show($@"Esri stacked button was clicked with this parameter {Environment.NewLine}{cmdParams}");
        }, () => true);
      }
    }

    #endregion Button Commands

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "XAML test dockpane";
    public string Heading
    {
      get { return _heading; }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class DockpaneWithButtons_ShowButton : Button
  {
    protected override void OnClick()
    {
      DockpaneWithButtonsViewModel.Show();
    }
  }
}
