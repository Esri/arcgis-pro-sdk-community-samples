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
