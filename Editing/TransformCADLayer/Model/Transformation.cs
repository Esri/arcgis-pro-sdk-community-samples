using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TransformCADLayer
{
  public class Transformation : INotifyPropertyChanged
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

    private string originX;
    private string originY;
    private string gridX;
    private string gridY;
    private string scaleFactor;
    private string rotation;
    private bool updateG2G;

    public string OriginX
    {
      get
      {
        return originX;
      }
      set
      {
        originX = value;
        NotifyPropertyChanged(nameof(OriginX));
      } //use nameof to avoid hard coding strings. IE, instead of "OriginX"
    }
    public string OriginY
    {
      get
      {
        return originY;
      }
      set
      {
        originY = value;
        NotifyPropertyChanged(nameof(OriginY));
      }
    }
    public string GridX
    {
      get
      {
        return gridX;
      }
      set
      {
        gridX = value;
        NotifyPropertyChanged(nameof(GridX));
      }
    }
    public string GridY
    {
      get
      {
        return gridY;
      }
      set
      {
        gridY = value;
        NotifyPropertyChanged(nameof(GridY));
      }
    }
    public string ScaleFactor
    {
      get
      {
        return scaleFactor;
      }
      set
      {
        scaleFactor = value;
        NotifyPropertyChanged(nameof(ScaleFactor));
      }
    }
    public string Rotation
    {
      get
      {
        return rotation;
      }
      set
      {
        rotation = value;
        NotifyPropertyChanged(nameof(Rotation));
      }
    }
    public bool UpdateGround2Grid
    { 
      get 
      { 
        return updateG2G; 
      } 
      set 
      {
        if (updateG2G == value) return;
        updateG2G = value;
        NotifyPropertyChanged(nameof(UpdateGround2Grid));
      } 
    }
  }
}
