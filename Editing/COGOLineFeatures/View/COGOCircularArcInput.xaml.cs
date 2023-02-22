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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.UnitFormats;

namespace COGOLineFeatures
{
  /// <summary>
  /// Interaction logic for COGOCircularArcInput.xaml
  /// </summary>
  public partial class COGOCircularArcInput : ArcGIS.Desktop.Framework.Controls.ProWindow
  {
    public COGOCircularArcInput()
    {
      InitializeComponent();
      CircArcDirection.Focus();
    }
    
    internal EllipticArcSegment CircularArcSegment { get;  set; }
    internal DisplayUnitFormat BackstageDirectionUnit { get; set; }
    internal DisplayUnitFormat BackstageAngleUnit { get; set; }
    internal DisplayUnitFormat BackstageDistanceUnit { get; set; }
    internal double DirectionOffsetCorrection { get; set; }
    internal double DistanceScaleFactor { get; set; }
    

    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
      TextBox textBox = (TextBox)sender;
      textBox.Dispatcher.BeginInvoke(new Action(() => textBox.SelectAll()));
      e.Handled = true;
    }
    private void TextBox_Direction_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        CircArcParameter1.Focus();
        e.Handled = true;
      }
    }
    private void TextBox_Parameter1_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        CircArcParameter2.Focus();
        e.Handled = true;
      }
    }

    private void CircularArcDirectionType_SelectionChanged(object sender, RoutedEventArgs e)
    {
      if (CircArcDirection.Text.Trim() == "")
        return;

      if(CircArcDirectionType.SelectedItem.ToString().ToLower() == "tangent direction")
        CircArcDirection.Text = COGOUtils.TangentDirectionFromCircularArc(CircularArcSegment, 
          BackstageDirectionUnit, DirectionOffsetCorrection / 180.0 * Math.PI);

      if (CircArcDirectionType.SelectedItem.ToString().ToLower() == "radial direction")
        CircArcDirection.Text = COGOUtils.RadialDirectionFromCircularArc(CircularArcSegment, 
          BackstageDirectionUnit, DirectionOffsetCorrection / 180.0 * Math.PI);

      if (CircArcDirectionType.SelectedItem.ToString().ToLower() == "chord direction")
        CircArcDirection.Text = COGOUtils.ChordDirectionFromCircularArc(CircularArcSegment, 
          BackstageDirectionUnit, DirectionOffsetCorrection / 180.0 * Math.PI);
    }

    private void Parameter2_SelectionChanged(object sender, RoutedEventArgs e)
    {
      if (CircArcParameter2Type.Text.Trim() == "")
        return;

      if (CircArcParameter2Type.SelectedItem.ToString().ToLower() == "delta angle")
        CircArcParameter2.Text = COGOUtils.DeltaAngleFromCircularArc(CircularArcSegment, BackstageAngleUnit);

      if (CircArcParameter2Type.SelectedItem.ToString().ToLower() == "arc length")
        CircArcParameter2.Text = COGOUtils.ArcLengthFromCircularArc(CircularArcSegment, BackstageDistanceUnit, DistanceScaleFactor) ;

      if (CircArcParameter2Type.SelectedItem.ToString().ToLower() == "chord length")
        CircArcParameter2.Text = COGOUtils.ChordLengthFromCircularArc(CircularArcSegment, BackstageDistanceUnit, DistanceScaleFactor);
    }
  }
}
