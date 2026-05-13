//   Copyright 2026 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Collections.ObjectModel;

namespace DDLCreateFeatureClassCHUI.ViewModel
{

  /// <summary>
  /// This class coordinates the steps of the wizard for creating feature classes.
  /// </summary>
  public class WizardViewModel
  {
    public Step0ViewModel Step0 { get; set; } = new Step0ViewModel();
    public Step1ViewModel Step1 { get; set; } = new Step1ViewModel();
    public Step2ViewModel Step2 { get; set; } = new Step2ViewModel();
  }
}