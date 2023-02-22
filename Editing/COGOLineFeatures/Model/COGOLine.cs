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
using System.ComponentModel;

namespace COGOLineFeatures
{
  public class COGOLine: INotifyPropertyChanged
  {
    #region INotifyPropertyChanged Members  

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }
    #endregion

    private string _direction;
    private string _distance;
    private bool _endPointFixed;
    private string _distanceUnit;
    private string _directionFormat;

    public string Direction
    {
      get
      {
        return _direction;
      }
      set
      {
        _direction = value;
        NotifyPropertyChanged(nameof(Direction));
      } //use nameof to avoid hard coding strings. IE, instead of "Direction"
    }
    public string DirectionFormat
    {
      get
      {
        return _directionFormat;
      }
      set
      {
        _directionFormat = value;
        NotifyPropertyChanged(nameof(DirectionFormat));
      }
    }
    public string Distance
    {
      get
      {
        return _distance;
      }
      set
      {
        _distance = value;
        NotifyPropertyChanged(nameof(Distance));
      }
    }
    public string DistanceUnit
    {
      get
      {
        return _distanceUnit;
      }
      set
      {
        _distanceUnit = value;
        NotifyPropertyChanged(nameof(DistanceUnit));
      }
    }
    public bool EndPointFixed
    {
      get
      {
        return _endPointFixed;
      }
      set
      {
        _endPointFixed = value;
        NotifyPropertyChanged(nameof(EndPointFixed));
      }
    }
  }
}
