using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace COGOLineFeatures
{
  class COGOCircularArc : INotifyPropertyChanged
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

    public string CircularArcDirection
    {
      get
      {
        return _circularArcDirection;
      }
      set
      {
        _circularArcDirection = value;
        NotifyPropertyChanged(nameof(CircularArcDirection));
      } //use nameof to avoid hard coding strings. IE, instead of "CircularArcDirection"
    }
    public string CircularArcDirectionType
    {
      get
      {
        return _circularArcDirectionType;
      }
      set
      {
        _circularArcDirectionType = value;
        NotifyPropertyChanged(nameof(CircularArcDirectionType));
      }
    }

    public string CircularArcDirectionFormat
    {
      get
      {
        return _circularArcDirectionFormat;
      }
      set
      {
        _circularArcDirectionFormat = value;
        NotifyPropertyChanged(nameof(CircularArcDirectionFormat));
      }
    }

    public string CircularArcRadiusUnit
    {
      get
      {
        return _circularArcRadiusUnit;
      }
      set
      {
        _circularArcRadiusUnit = value;
        NotifyPropertyChanged(nameof(CircularArcRadiusUnit));
      }
    }

    public List<string> CircularArcDirectionTypeList
    {
      get
      {
        return _circularArcDirectionTypeList;
      }
      set
      {
        _circularArcDirectionTypeList = value;
        NotifyPropertyChanged(nameof(CircularArcDirectionTypeList));
      }
    }

    public string Parameter1
    {
      get
      {
        return _parameter1;
      }
      set
      {
        _parameter1 = value;
        NotifyPropertyChanged(nameof(Parameter1));
      }
    }
    public string Parameter1Type
    {
      get
      {
        return _parameter1Type;
      }
      set
      {
        _parameter1Type = value;
        NotifyPropertyChanged(nameof(Parameter1Type));
      }
    }
    public List<string> Parameter1TypeList
    {
      get
      {
        return _parameter1TypeList;
      }
      set
      {
        _parameter1TypeList = value;
        NotifyPropertyChanged(nameof(Parameter1TypeList));
      }
    }
    public string Parameter2
    {
      get
      {
        return _parameter2;
      }
      set
      {
        _parameter2 = value;
        NotifyPropertyChanged(nameof(Parameter2));
      }
    }
    public string Parameter2Type
    {
      get
      {
        return _parameter2Type;
      }
      set
      {
        _parameter2Type = value;
        NotifyPropertyChanged(nameof(Parameter2Type));
      }
    }
    public List<string> Parameter2TypeList
    {
      get
      {
        return _parameter2TypeList;
      }
      set
      {
        _parameter2TypeList = value;
        NotifyPropertyChanged(nameof(Parameter2TypeList));
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
    public bool[] Side
    {
      get { return _side; }
    }
    public int SelectedSide
    {
      get { return Array.IndexOf(_side, true); }
    }
  }
}
