using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;

namespace TransformCADLayer
{
  internal class TransformationViewModel : PropertyChangedBase
  {
    private Transformation _Transformation;
    private string _OriginX;
    private string _OriginY;
    private string _GridX;
    private string _GridY;
    private string _ScaleFactor;
    private string _Rotation;
    private bool _UpdateG2G;

    public ICommand ResetParametersCommand
    {
      get {
        return new RelayCommand( () => ResetParams());
      }    
    }

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

    private void ResetParams()
    {
      Transformation tr = this.Transformation;
      tr.OriginX = "0.000";
      tr.OriginY = "0.000";
      tr.GridX = "0.000";
      tr.GridY = "0.000";
      tr.ScaleFactor = "1.000000";
      tr.Rotation = "0.000";
      tr.UpdateGround2Grid = false;
    }

    public TransformationViewModel()
    {
      string sParamString = TransformCADDialog.Default["LastUsedParams"] as string;
      string[] sParams = sParamString.Split('|'); //"0|0|0|0|1|0"
      if (sParams.Length == 0)
      {
        _OriginX = "0.000";
        _OriginY = "0.000";
        _GridX = "0.000";
        _GridY = "0.000";
        _ScaleFactor = "1.000000";
        _Rotation = "0";
        _UpdateG2G = false;
      }
      else
      {
        try
        {
          double dOriginX = 0;
          _OriginX = sParams[0];
          if (String.IsNullOrEmpty(_OriginX))
            _OriginX = "0.000";
          else if (Double.TryParse(sParams[0], out dOriginX))
            _OriginX = dOriginX.ToString("0.000");
        }
        catch { _OriginX = "0.000"; }

        try
        {
          double dOriginY = 0;
          _OriginY = sParams[1];
          if (String.IsNullOrEmpty(_OriginY))
            _OriginY = "0.000";
          else if (Double.TryParse(sParams[1], out dOriginY))
            _OriginY = dOriginY.ToString("0.000");
        }
        catch { _OriginY = "0.000"; }

        try
        {
          double dGridX = 0;
          _GridX = sParams[2];
          if (String.IsNullOrEmpty(_GridX))
            _GridX = "0.000";
          else if (Double.TryParse(sParams[2], out dGridX))
            _GridX = dGridX.ToString("0.000");
        }
        catch { _GridX = "0.000";}

        try
        {
          double dGridY = 0;
          _GridY = sParams[3];
          if (String.IsNullOrEmpty(_GridY))
            _GridY = "0.000";
          else if (Double.TryParse(sParams[3], out dGridY))
            _GridY = dGridY.ToString("0.000");
        }
        catch { _GridY = "0.000"; }

        try
        {
          double dScaleFactor = 1;
          _ScaleFactor = sParams[4];
          if (String.IsNullOrEmpty(_ScaleFactor))
            _ScaleFactor = "1.000";
          else if (Double.TryParse(sParams[4], out dScaleFactor))
            _ScaleFactor = dScaleFactor.ToString("0.0000000000");
        }
        catch { _ScaleFactor = "1.000";}

        try
        {
          double dRotation = 0;
          _Rotation = sParams[5];
          if (String.IsNullOrEmpty(_Rotation))
            _Rotation = "0.000";
          else if (Double.TryParse(sParams[5], out dRotation))
            _Rotation = dRotation.ToString("0.000");
        }
        catch { _Rotation = "0.000"; }

        try
        {
          _UpdateG2G = sParams[6] == Boolean.TrueString;
        }
        catch { _UpdateG2G = false; }
      }

      _Transformation = new Transformation
      {
        OriginX = _OriginX,
        OriginY = _OriginY,
        GridX = _GridX,
        GridY = _GridY,
        ScaleFactor = _ScaleFactor,
        Rotation = _Rotation,
        UpdateGround2Grid = _UpdateG2G
      };

    }
    public Transformation Transformation
    {
      get { return _Transformation; }
      set { _Transformation = value;}
    }
  }
}
