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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockpaneWithComboDropdown
{
  internal class DockpaneComboViewModel : DockPane
  {
    private const string _dockPaneID = "DockpaneWithComboDropdown_DockpaneCombo";

    // Create a dictionary by all state names with the capitals as the value
    // This is used to populate the ComboBox in the DockPane
    private static readonly Dictionary<string, string> _capitals = new Dictionary<string, string>
    {
      { "Alabama", "Montgomery" },
      { "Alaska", "Juneau" },
      { "Arizona", "Phoenix" },
      { "Arkansas", "Little Rock" },
      { "California", "Sacramento" },
      { "Colorado", "Denver" },
      { "Connecticut", "Hartford" },
      { "Delaware", "Dover" },
      { "Florida", "Tallahassee" },
      { "Georgia", "Atlanta" },
      { "Hawaii", "Honolulu" },
      { "Idaho", "Boise" },
      { "Illinois", "Springfield" },
      { "Indiana", "Indianapolis" },
      { "Iowa", "Des Moines" },
      { "Kansas", "Topeka" },
      { "Kentucky", "Frankfort" },
      { "Louisiana", "Baton Rouge" },
      { "Maine", "Augusta" },
      { "Maryland", "Annapolis" },
      { "Massachusetts", "Boston" },
      { "Michigan", "Lansing" },
      { "Minnesota", "St. Paul" },
      { "Mississippi", "Jackson" },
      { "Missouri", "Jefferson City" },
      { "Montana", "Helena" },
      { "Nebraska", "Lincoln" },
      { "Nevada", "Carson City" },
      { "New Hampshire", "Concord" },
      { "New Jersey", "Trenton" },
      { "New Mexico", "Santa Fe" },
      { "New York", "Albany" },
      { "North Carolina", "Raleigh" },
      { "North Dakota", "Bismarck" },
      { "Ohio", "Columbus" },
      { "Oklahoma", "Oklahoma City" },
      { "Oregon", "Salem" },
      { "Pennsylvania", "Harrisburg" },
      { "Rhode Island", "Providence" },
      { "South Carolina",   "Columbia"},
      {"South Dakota","Pierre"},
      {"Tennessee","Nashville"},
      {"Texas","Austin"},
      {"Utah","Salt Lake City"},
      {"Vermont","Montpelier"},
      {"Virginia","Richmond"},
      {"Washington","Olympia"},
      {"West Virginia","Charleston"},
      {"Wisconsin","Madison"},
      {"Wyoming","Cheyenne"}
    };

    static public List<string> CapitalList = [.. _capitals.Values];

    protected DockpaneComboViewModel() { }

    #region Properties

    public ObservableCollection<string> States
    {
      get { return [.. _capitals.Keys]; }
    }

    public ObservableCollection<string> Capitals
    {
      get { return [.. _capitals.Values]; }
    }

    private string _selectedState;
    public string State
    {
      get { return _selectedState; }
      set
      {
        SetProperty(ref _selectedState, value, () => State);
        MessageBox.Show($"You selected {value} as the state.");
      }
    }

    private string _selectedCapital;
    public string Capital
    {
      get { return _selectedCapital; }
      set
      {
        SetProperty(ref _selectedCapital, value, () => Capital);
        MessageBox.Show($"You selected {value} as the capital.");
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
    private string _heading = "Combobox Demo DockPane";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }
  }

  #endregion Properties

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  internal class DockpaneCombo_ShowButton : Button
  {
    protected override void OnClick()
    {
      DockpaneComboViewModel.Show();
    }
  }
}
