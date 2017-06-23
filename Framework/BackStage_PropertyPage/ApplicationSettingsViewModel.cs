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
using ArcGIS.Desktop.Framework.Contracts;

namespace BackStage_PropertyPage
{
  /// <summary>
  /// The viewmodel for the ApplicationSettings view.  Encapsulates the sample set of applications settings.
  /// </summary>
  /// <remarks>
  /// Application settings are retrieved and stored using the Visual Studio application settings paradigm.  
  /// </remarks>
  internal class ApplicationSettingsViewModel : Page
  {
    public ApplicationSettingsViewModel() { }

    #region Properties

    /// <summary>
    /// Gets and sets the expansion state of the General expander
    /// </summary>
    private static bool _isGeneralExpanded = true;
    public bool IsGeneralExpanded
    {
      get { return _isGeneralExpanded; }
      set { SetProperty(ref _isGeneralExpanded, value, () => IsGeneralExpanded); }
    }

    /// <summary>
    /// Gets and sets the expansion state of the Other expander
    /// </summary>
    private static bool _isOtherExpanded = true;
    public bool IsOtherExpanded
    {
      get { return _isOtherExpanded; }
      set { SetProperty(ref _isOtherExpanded, value, () => IsOtherExpanded); }
    }

    private bool _origGeneralSetting;

    /// <summary>
    /// Gets and sets the state of the General setting
    /// </summary>
    /// <remarks>
    /// Use the base.IsModified flag to indicate when the page has changed
    /// </remarks>
    private bool _generalSetting;
    public bool GeneralSetting
    {
      get { return _generalSetting; }
      set
      {
        if (SetProperty(ref _generalSetting, value, () => GeneralSetting))
          base.IsModified = true;
      }
    }

    private bool _origOtherSetting;

    /// <summary>
    /// Gets and sets the state of the Other setting
    /// </summary>
    /// <remarks>
    /// Use the base.IsModified flag to indicate when the page has changed
    /// </remarks>
    private bool _otherSetting;
    public bool OtherSetting
    {
      get { return _otherSetting; }
      set
      {
        if (SetProperty(ref _otherSetting, value, () => OtherSetting))
            base.IsModified = true;
      }
    }
    #endregion

    #region Page Overrides

    /// <summary>
    /// Initializes the page using the settings.
    /// </summary>
    /// <returns>A Task that represents the InitializeAsync method</returns>
    protected override Task InitializeAsync()
    {
      // get the default settings
      BackStage_PropertyPage.Properties.Settings settings = BackStage_PropertyPage.Properties.Settings.Default;

      // assign to the values binding to the controls
      _generalSetting = settings.GeneralSetting;
      _otherSetting = settings.OtherSetting;

      // keep track of the original values (used for comparison when saving)
      _origGeneralSetting = GeneralSetting;
      _origOtherSetting = OtherSetting;

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
    /// Save the current state of the settings.
    /// </remarks>
    /// <returns>A Task that represents CommitAsync</returns>
    protected override Task CommitAsync()
    {
      if (IsDirty())
      {
        // save the new settings
        BackStage_PropertyPage.Properties.Settings settings = BackStage_PropertyPage.Properties.Settings.Default;

        settings.GeneralSetting = GeneralSetting;
        settings.OtherSetting = OtherSetting;

        settings.Save();
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
      if (_origGeneralSetting != GeneralSetting)
      {
        return true;
      }
      if (_origOtherSetting != OtherSetting)
      {
          return true;
      }

      return false;
    }
  }
}
