//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace BackStage_PropertyPage
{
  /// <summary>
  /// The viewmodel for the ProjectSettings view.  Encapsulates the sample set of project settings.
  /// </summary>
  /// <remarks>
  /// Project settings are retrieved and stored using the <see cref="ArcGIS.Desktop.Core.Project.OnReadStateAsync"> OnReadStateAsync </see> and 
  /// <see cref="ArcGIS.Desktop.Core.Project.OnWriteStateAsync"> OnWriteStateAsync </see> methods on the <see cref="ArcGIS.Desktop.Core.Project"> project.</see>
  /// </remarks>
  internal class ProjectSettingsViewModel : Page
  {
    public ProjectSettingsViewModel() { }

    #region Properties

    private bool _origModuleSetting1;

    /// <summary>
    /// Gets and sets the first setting
    /// </summary>
    /// <remarks>
    /// Use the base.IsModified flag to indicate when the page has changed
    /// </remarks>
    private bool _moduleSetting1;
    public bool ModuleSetting1
    {
      get { return _moduleSetting1; }
      set
      {
        if (SetProperty(ref _moduleSetting1, value, () => ModuleSetting1))
          base.IsModified = true;
      }
    }

    private string _origModuleSetting2;

    /// <summary>
    /// Gets and sets the second setting
    /// </summary>
    /// <remarks>
    /// Use the base.IsModified flag to indicate when the page has changed
    /// </remarks>
    private string _moduleSetting2;
    public string ModuleSetting2
    {
      get { return _moduleSetting2; }
      set
      {
        if (SetProperty(ref _moduleSetting2, value, () => ModuleSetting2))
          base.IsModified = true;
      }
    }
    #endregion

    #region Page Overrides

    /// <summary>
    /// Initializes the page using the settings.
    /// </summary>
    /// <returns>A Task that represnets the InitializeAsync method</returns>
    protected override Task InitializeAsync()
    {
      // get the settings
      Dictionary<string, string> settings = Module1.Current.Settings;

      // assign to the values biniding to the controls
      if (settings.ContainsKey("Setting1"))
        _moduleSetting1 = System.Convert.ToBoolean(settings["Setting1"]);
      else
        _moduleSetting1 = true;

      if (settings.ContainsKey("Setting2"))
        _moduleSetting2 = settings["Setting2"];
      else
        _moduleSetting2 = "";


      // keep track of the original values (used for comparison when saving)
      _origModuleSetting1 = ModuleSetting1;
      _origModuleSetting2 = ModuleSetting2;

      return Task.FromResult(0);
    }

    /// <summary>
    /// Perform special actions when the page is to be cancelled.
    /// </summary>
    /// <returns>A Task that represents CancelAsync</returns>
    protected override Task CancelAsync()
    {
      return Task.FromResult(0);
    }

    /// <summary>
    /// Perform special actions when the page is to be committed.
    /// </summary>
    /// <remarks>
    /// Stores the current state of the settings.  Ensure that the project is set dirty if the settings have changed from the original values. 
    /// Setting the project dirty ensure that the application asks to "save changes" when the project is closed.  The settings will be
    /// saved when the project is saved.
    /// </remarks>
    /// <returns>A Task that represents CommitAsync</returns>
    protected override Task CommitAsync()
    {
      if (IsDirty())
      {
        // store the new settings in the dictionary ... save happens in OnProjectSave
        Dictionary<string, string> settings = Module1.Current.Settings;

        if (settings.ContainsKey("Setting1"))
          settings["Setting1"] = ModuleSetting1.ToString();
        else
          settings.Add("Setting1", ModuleSetting1.ToString());

        if (settings.ContainsKey("Setting2"))
          settings["Setting2"] = ModuleSetting2;
        else
          settings.Add("Setting2", ModuleSetting2);

        // set the project dirty
        Project.Current.SetDirty(true);
      }

      return Task.FromResult(0);
    }

    #endregion

    /// <summary>
    /// Determines if the current settings are different from the original.
    /// </summary>
    /// <returns>true if the current settings are different</returns>
    private bool IsDirty()
    {
      if (_origModuleSetting1 != ModuleSetting1)
        return true;

      if (_origModuleSetting2 != ModuleSetting2)
        return true;

      return false;
    }
  }
}
