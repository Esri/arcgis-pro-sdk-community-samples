/*

   Copyright 2024 Esri

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
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EditorInspectorUI
{
  internal class PermitRecord : PropertyChangedBase
  {
    private long _oid;
    private string _jobNo;
    private object _address;
    private object _jobType;
    private string _name;
    public PermitRecord(string Name)
    {
      _name = Name;
    }
    public PermitRecord(long oid, string jobNo, string address, string jobType)
    {
      _oid = oid;
      _jobNo = jobNo;
      _address = address;
      _jobType = jobType;
    }

    public long OID
    {
      get => _oid;
      set => SetProperty(ref _oid, value, () => OID);
    }
    public string JobNo
    {
      get => _jobNo;
      set => SetProperty(ref _jobNo, value, () => JobNo);
    }
    public string Name
    {
      get
      {
        _name = $"{_jobType}, {_address}";
        return _name;
      }
      set
      {
               
        SetProperty(ref _name, value, () => Name);
      }
    }

  }
}
