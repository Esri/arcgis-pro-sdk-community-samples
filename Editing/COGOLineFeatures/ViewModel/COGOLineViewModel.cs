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
