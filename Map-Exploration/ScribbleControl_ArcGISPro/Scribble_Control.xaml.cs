//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArcGIS.Desktop.Framework.Contracts;
using System.ComponentModel;

namespace ScribbleControl_ArcGISPro
{
  /// <summary>
  /// Interaction logic for Scribble_ControlView.xaml
  /// </summary>
  public partial class Scribble_ControlView : UserControl , INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    // Create the OnPropertyChanged method to raise the event
    protected void OnPropertyChanged(string name)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(name));
      }
    }

    private IList<double> _sizeOptions = new List<double>(){
            5, 10, 20, 30, 40, 50
        };

    public IList<double> ScribbleControlSizeOptions
    {
      get
      {
        return _sizeOptions;
      }
    }

    private Brush _shapeColor = Brushes.Tomato;
    public Brush ShapeColor
    {
      get
      {
        return _shapeColor;
      }
      set
      {
        _shapeColor = value;
        OnPropertyChanged("ShapeColor");
      }
    }

    public Scribble_ControlView()
    {
      InitializeComponent();
    }


    private void ClrPcker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
      if (this.cvs != null)
      {
        this.cvs.DefaultDrawingAttributes.Color = (System.Windows.Media.Color)this.ClrPcker_ScribbleControl.SelectedColor;
        ShapeColor = new SolidColorBrush(this.cvs.DefaultDrawingAttributes.Color);
      }
    }

    private void Size_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.cvs != null)
      {
        this.cvs.DefaultDrawingAttributes.Width = System.Convert.ToDouble(this.Size_Combo.SelectedItem.ToString());
        this.cvs.DefaultDrawingAttributes.Height = System.Convert.ToDouble(this.Size_Combo.SelectedItem.ToString());
      }
    }

    private void RadioButtonClicked(object sender, RoutedEventArgs e)
    {
      if (this.cvs != null)
      {
        if (inkRadioBtn.IsChecked == true)
        {
          this.cvs.EditingMode = InkCanvasEditingMode.Ink;
        }
        else if (eraseRadioBtn.IsChecked == true)
        {
          this.cvs.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }
        else if (selectRadioBtn.IsChecked == true)
        {
          this.cvs.EditingMode = InkCanvasEditingMode.Select;
        }
      }
    }

    private void cvs_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
      Shape shapeToAdd = null;
      Brush b = new SolidColorBrush(this.cvs.DefaultDrawingAttributes.Color);

      if (this.cvs != null)
      {
        if (rectShape.IsChecked == true)
        {
          shapeToAdd = new Rectangle() { Fill = b, Height = 100, Width = 100, RadiusX = 0, RadiusY = 0 };
        }
        else if(circleShape.IsChecked == true)
        {
          shapeToAdd = new Ellipse() { Fill = b, Height = 100, Width = 100 } ;
        }
        else if (lineShape.IsChecked == true)
        {
          shapeToAdd = new Line() { Height = 24, Width = 210,
                                    StrokeThickness = 20, Stroke = b,
                                    X1 = 5, X2 = 200, Y1 = 5, Y2 = 5,
                                    StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
        }
      }

      if (rectShape.IsChecked == true || circleShape.IsChecked == true)
      {
        InkCanvas.SetLeft(shapeToAdd, e.GetPosition(this.cvs).X - 50);
        InkCanvas.SetTop(shapeToAdd, e.GetPosition(this.cvs).Y - 50);
      }
      else
      {
        InkCanvas.SetLeft(shapeToAdd, e.GetPosition(this.cvs).X);
        InkCanvas.SetTop(shapeToAdd, e.GetPosition(this.cvs).Y - 12);
      }
      
      this.cvs.Children.Add(shapeToAdd);
    }

    private void Clear_Canvas_Click(object sender, RoutedEventArgs e)
    {
      this.cvs.Strokes.Clear();

      int numElements = this.cvs.Children.Count;
      List<UIElement> uiElements = new List<UIElement>();
      for (int i = 0; i < numElements; i++)
      {
        uiElements.Add(this.cvs.Children[i]);
      }

      for (int i = 0; i  < numElements; i++)
      {
        this.cvs.Children.Remove(uiElements[i]);
      }
    }

    private void Select_All_Click(object sender, RoutedEventArgs e)
    {
      int numElements = this.cvs.Children.Count;
      List<UIElement> uiElements = new List<UIElement>();
      for (int i = 0; i < numElements; i++)
      {
        uiElements.Add(this.cvs.Children[i]);
      }
      this.cvs.Select(this.cvs.Strokes, uiElements);
      selectRadioBtn.IsChecked = true;
    }

    private void Copy_Selected_Click(object sender, RoutedEventArgs e)
    {
      this.cvs.CopySelection();
    }

    private void Cut_Selected_Click(object sender, RoutedEventArgs e)
    {
      this.cvs.CutSelection();
    }

    private void Paste_toCanvas_Click(object sender, RoutedEventArgs e)
    {
      selectRadioBtn.IsChecked = true;
      this.cvs.Paste();
    }
  }
}
