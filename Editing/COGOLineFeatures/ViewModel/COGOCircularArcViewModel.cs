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
  internal class COGOCircularArcViewModel : PropertyChangedBase
  {
    private COGOCircularArc _COGOCircularArc;
    private string _circularArcDirection;
    private string _circularArcDirectionType;
    private string _circularArcDirectionFormat;
    private string _circularArcRadiusUnit;
    private List<string> _circularArcDirectionTypeList;
    private string _parameter1;
    private string _parameter1Type;
    private List<string> _parameter1TypeList;
    private string _parameter2;
    private string _parameter2Type;
    private List<string> _parameter2TypeList;
    private bool[] _side = new bool[] { true, false };
    private bool _endPointFixed;

    internal static string _StringTangentDirection = "Tangent Direction";
    internal static string _StringChordDirection = "Chord Direction";
    internal static string _StringRadialDirection = "Radial Direction";
    internal static string _StringRadius = "Radius";
    internal static string _StringArcLength = "Arc Length";
    internal static string _StringChordLength = "Chord Length";
    internal static string _StringDeltaAngle = "Delta Angle";

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

    public COGOCircularArcViewModel()
    {
      _circularArcDirection = "0.0";
      _circularArcDirectionFormat = "Quadrant Bearing";
      _circularArcRadiusUnit = "meters";
      _parameter1 = "0.0";
      _parameter2 = "0.0";
      _side[0] = true;
      _endPointFixed = true;
      _circularArcDirectionTypeList = new List<string>{_StringTangentDirection,
        _StringChordDirection, _StringRadialDirection};
      _parameter1TypeList = new List<string> { _StringRadius};
      _parameter2TypeList = new List<string> { _StringArcLength, _StringChordLength, _StringDeltaAngle};

      string sParamString = COGOLineDialog.Default["LastUsedParams"] as string;
      string[] sParams = sParamString.Split('|'); //"0|0|0.10"
      if (sParams.Length == 0)
      {
        _circularArcDirectionType = _StringTangentDirection; //[0]
        _parameter1Type = _StringRadius; //[1]
        _parameter2Type = _StringArcLength; //[2]
      }
      else
      {
        try
        {
          _circularArcDirectionType = sParams[0];
          if (String.IsNullOrEmpty(_circularArcDirectionType))
            _circularArcDirectionType = _StringTangentDirection;
          else
            _circularArcDirectionType = sParams[0];
        }
        catch { _circularArcDirectionType = _StringTangentDirection; }
        try
        {
          _parameter1Type = sParams[1];
          if (String.IsNullOrEmpty(_parameter1Type))
            _parameter1Type = _StringRadius;
          else
            _parameter1Type = sParams[1];
        }
        catch { _parameter1Type = _StringRadius; }
        try
        {
          _parameter2Type = sParams[2];
          if (String.IsNullOrEmpty(_parameter2Type))
            _parameter2Type = _StringArcLength;
          else
            _parameter2Type = sParams[2];
        }
        catch { _parameter2Type = _StringArcLength; }
      }

      _COGOCircularArc = new COGOCircularArc
      {
        CircularArcDirection = _circularArcDirection,
        CircularArcDirectionType = _circularArcDirectionType,
        CircularArcDirectionFormat = _circularArcDirectionFormat,
        CircularArcDirectionTypeList = _circularArcDirectionTypeList,
        CircularArcRadiusUnit = _circularArcRadiusUnit,
        Parameter1 = _parameter1,
        Parameter1Type = _parameter1Type,
        Parameter1TypeList = _parameter1TypeList,
        Parameter2 = _parameter2,
        Parameter2Type = _parameter2Type,
        Parameter2TypeList = _parameter2TypeList,
        EndPointFixed = _endPointFixed
      };

    }
    public COGOCircularArc COGOCircularArc
    {
      get { return _COGOCircularArc; }
      set { _COGOCircularArc = value; }
    }
  }
}
