/*

   Copyright 2019 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

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
using System.Windows.Controls;
using System.Xml.Linq;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace Inspector_AddAttributeAsync
{
  internal class AttributeControlViewModel : EmbeddableControl
  {
    private EmbeddableControl _inspectorViewModel = null;
    private UserControl _inspectorView = null;

    public AttributeControlViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions)  { }

    /// <summary>
    /// Property for the inspector UI.
    /// </summary>
    public UserControl InspectorView
    {
      get { return _inspectorView; }
      set { SetProperty(ref _inspectorView, value, () => InspectorView); }
    }


    /// <summary>
    /// Access to the view model of the inspector
    /// </summary>
    public EmbeddableControl InspectorViewModel
    {
      get { return _inspectorViewModel; }
      set
      {
        if (value != null)
        {
          _inspectorViewModel = value;
          _inspectorViewModel.OpenAsync();

        }
        else if (_inspectorViewModel != null)
        {
          _inspectorViewModel.CloseAsync();
          _inspectorViewModel = value;
        }

        NotifyPropertyChanged(() => InspectorViewModel);
      }
    }
  }
}
