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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace ValidateFeatures
{
  internal class ValidateResultsPaneViewModel : DockPane
  {
    private const string _dockPaneID = "ValidateFeatures_ValidateResultsPane";

    protected ValidateResultsPaneViewModel() { }

    public ICommand CmdValidate
    {
      get
      {
        return new RelayCommand(async () =>
        {
          // Clear out the text box
          TxtValidationResults = "Validating features...";

          // Find our map
          if (MapView.Active == null)
          {
            TxtValidationResults = "No map found.";
            return;
          }

          string resultString = await MapValidator.ValidateMap(MapView.Active.Map);
          if (resultString == "")
          {
            TxtValidationResults = "No selection found.";
          }
          else
          {
            TxtValidationResults = resultString;
          }


        });
      }
    }

    private string _txtValidationResults;
    public string TxtValidationResults
    {
      get { return _txtValidationResults; }
      set
      {
        SetProperty(ref _txtValidationResults, value, () => TxtValidationResults);
      }
    }


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

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Click the button to run validation on selected features.";
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
	internal class ValidateResultsPane_ShowButton : Button
  {
    protected override void OnClick()
    {
      ValidateResultsPaneViewModel.Show();
    }
  }
}
