/*

   Copyright 2025 Esri

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
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DockpaneWithTextBox
{
  internal class Dockpane1ViewModel : DockPane
  {
    private const string _dockPaneID = "DockpaneWithTextBox_Dockpane1";

    protected Dockpane1ViewModel() { }

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
    private int _selectCountOrig;
    private double _percentageOrig;
    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "TextBox Bindings";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    private int _totalFeatures = 100;
    public int TotalFeatures
    {
      get => _totalFeatures;
      set {
        SetProperty(ref _totalFeatures, value);
        SelectionOfText = $"of {TotalFeatures}";
        Percentage = (double)SelectionCount / TotalFeatures * 100;
      } 
    }
    private int _selectionCount;
    public int SelectionCount
    {
      get => _selectionCount;
      set
      {
        SetProperty(ref _selectionCount, value);
        _selectCountOrig = SelectionCount;
        var tempPercentage = (double)SelectionCount / TotalFeatures * 100;
        if (tempPercentage != Percentage)
          Percentage = tempPercentage;
      }
    }
    private string _selectionOfText = "of 100";
    public string SelectionOfText
    {
      get => _selectionOfText;
      set => SetProperty(ref _selectionOfText, value);
    }

    private double _percentage;
    public double Percentage
    {
      get => _percentage;
      set
      {
        SetProperty(ref _percentage, value);
        if (Percentage == 0) return;
        var tempSelectionCount = (int)((Percentage / 100) * TotalFeatures);
        if (tempSelectionCount != SelectionCount)
          SelectionCount = tempSelectionCount;
      }
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class Dockpane1_ShowButton : Button
  {
    protected override void OnClick()
    {
      Dockpane1ViewModel.Show();
    }
  }
}
