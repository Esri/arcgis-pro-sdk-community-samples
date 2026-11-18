/*

   Copyright 2026 Esri

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
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace SolarSuitability
{
  internal class TargetPropertyInformationViewModel : EmbeddableControl
  {
    public TargetPropertyInformationViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) {
      ControlVisibility = Visibility.Collapsed;
      Module1.TargetPropInfoVM = this;
		}

    /// <summary>
    /// Text shown in the control.
    /// </summary>
    private string _text = "Embeddable Control";
    public string Text
    {
      get => _text;
      set => SetProperty(ref _text, value);
    }

    private Visibility _ControlVisibility;

		public Visibility ControlVisibility
    {
      get => _ControlVisibility;
      set
      {
        SetProperty(ref _ControlVisibility, value);
      }
		}

	}
}
