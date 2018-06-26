/*

   Copyright 2018 Esri

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;

namespace ConfigWithMap
{
  class WorkOrdersVM : DockPane
  {
    protected override Task InitializeAsync()
    {
      return base.InitializeAsync();
    }

    public string LayerName { get { return ConfigWithMapModule.Current.FeatureLayerName; } }

    public ObservableCollection<FeatureRepresentation> Features
    {
      get { return ConfigWithMapModule.Current.Features; }
    }

    public FeatureRepresentation SelectedItem
    {
      get
      {
        return null;
      }
      set
      {
        if (value == null)
          return;

        ConfigWithMapModule.Current.ZoomToFeature(value.Id);

      }
    }
  }
}
