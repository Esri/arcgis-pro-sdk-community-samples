/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVChargers
{
  internal class ConnectorTypeItem : PropertyChangedBase
  {
    private string _connectorName;
    private bool _isSelected;

    public string ConnectorName
    {
      get => _connectorName;
      set => SetProperty(ref _connectorName, value);
    }

    public bool IsSelected
    {
      get => _isSelected;
      set => SetProperty(ref _isSelected, value);
    }

  }
}
