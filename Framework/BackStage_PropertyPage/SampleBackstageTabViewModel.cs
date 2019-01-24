//   Copyright 2019 Esri
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
using  ArcGIS.Desktop.Framework.Contracts;

namespace BackStage_PropertyPage
{
  /// <summary>
  /// Sample backstage tab.  It contains a simple checkbox.
  /// </summary>
  internal class SampleBackstageTabViewModel : BackstageTab
  {
    public SampleBackstageTabViewModel()
    {
      // no-op
    }

    /// <summary>
    /// Initializes the tab.  
    /// </summary>
    /// <returns>A Task that represents InitializeAsync </returns>
    protected override async Task InitializeAsync()
    {
      await base.InitializeAsync();
    }

    /// <summary>
    ///  Uninitializes the tab. 
    /// </summary>
    /// <returns>A Task that represents UninitializeAsync</returns>
    protected override Task UninitializeAsync()
    {
      return base.UninitializeAsync();
    }

    /// <summary>
    /// Gets and sets the state of the "Do Something here" checkbox
    /// </summary>
    private bool _isSomethingChecked;
    public bool IsSomethingChecked
    {
      get { return _isSomethingChecked; }
      set { SetProperty(ref _isSomethingChecked, value, () => IsSomethingChecked); }    
    }
  }
}
