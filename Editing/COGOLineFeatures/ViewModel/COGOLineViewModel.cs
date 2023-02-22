/*

   Copyright 2022 Esri

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;

namespace COGOLineFeatures
{
  internal class COGOLineViewModel : PropertyChangedBase
  {
    private COGOLine _COGOLine;
    private string _Direction;
    private string _Distance;
    private string _DistanceUnit;
    private string _DirectionFormat;
    private bool _EndPointFixed;

    public ICommand OKCommand
    {
      get
      {
        return new RelayCommand((dlgParam) =>
        {
          ProWindow param = dlgParam as ProWindow;
          param.DialogResult = true;
        }, () => true);
      }
    }

    public COGOLineViewModel()
    {
      _Direction = "0.000";
      _DirectionFormat = "Quadrant Bearing";
      _Distance = "0.000";
      _DistanceUnit = "m";
      _EndPointFixed = true;

      _COGOLine = new COGOLine
      {
        Direction = _Direction,
        DirectionFormat = _DirectionFormat,
        Distance = _Distance,
        DistanceUnit = _DistanceUnit,
        EndPointFixed = _EndPointFixed
      };

    }
    public COGOLine COGOLine
    {
      get { return _COGOLine; }
      set { _COGOLine = value; }
    }
  }
}
