// Copyright 2024 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SketchToolWithHalos
{
  internal class HaloItem : PropertyChangedBase
  {
    public HaloItem(double radius, CIMColor color, double strokeThickness)
    {
      RadiusM = radius;
      OutlineColor = color;
      StrokeThickness = strokeThickness;
    }

    public double Height => ScreenDiameter;

    internal CIMColor OutlineColor { get; set; }
    public Brush WPFOutlineBrush { get; internal set; }
    public double StrokeThickness { get; internal set; }

    private double _x;
    public double X { get => _x; set => SetProperty(ref _x, value); }
    private double _y;
    public double Y { get => _y; set => SetProperty(ref _y, value); }

    internal double RadiusM { get; set; }      // in m
    internal double ScreenRadius { get; set; }
    internal double ScreenDiameter => ScreenRadius * 2;

    public bool IsVisible => ShowAtAllScales || IsValidScale;
    internal bool IsValidScale { get; set; }
    internal bool ShowAtAllScales { get; set; }

    internal async Task DoConversions()
    {
      // convert radius to screen coords
      await DoRadiusConversion();

      // convert the CIMColor to a UI color brush
      var brushColor = await ConvertCIMColorToUIColorBrush(OutlineColor);
      WPFOutlineBrush = new SolidColorBrush(brushColor);
    }

    internal async Task DoRadiusConversion()
    {
      if (MapView.Active == null)
        return;

      ScreenRadius = await ConvertRadiusToScreenUnits(RadiusM);
      // force a notification on the public Height property
      NotifyPropertyChanged(nameof(Height));
    }

    internal void CalculateVisibility(double minSize, bool showHalosAtAllScales)
    {
      IsValidScale = ScreenRadius >= minSize;
      ShowAtAllScales = showHalosAtAllScales;

      NotifyPropertyChanged(nameof(IsVisible));
    }

    internal void CalculatePosition(double canvasHeight)
    {
      var halfCanvasHeight = canvasHeight / 2;
      // determine left, top offset to apply to be canvas WPF margin
      double offset = halfCanvasHeight - ScreenRadius;

      X = offset;
      Y = offset;
    }

    private Task<double> ConvertRadiusToScreenUnits(double radiusInMeters)
    {
      // convert the radius (in m) to screen units by using
      // GeometryEngine.ConstructGeodeticLineFromDistance to construct a line
      // of "radiusInMeters" distance from an arbitrary point (the center of the extent)

      
      // get the center of the extent
      var centerPt = MapView.Active.Extent.Center;

      return QueuedTask.Run(() =>
      {
        // construct the geodetic line of radiusInMeters distance from the centerPt
        var polyline = GeometryEngine.Instance.ConstructGeodeticLineFromDistance(GeodeticCurveType.Geodesic, centerPt, radiusInMeters, 0, null, CurveDensifyMethod.ByLength, 3000);
        var ptCount = polyline.PointCount;
        var endPoint = polyline.Points[ptCount - 1];

        // convert the real points
        var startPtClient = MapView.Active.MapToClient(centerPt);
        var startPtScreen = MapView.Active.MapToScreen(centerPt);
        var endPtClient = MapView.Active.MapToClient(endPoint);
        var endPtScreen = MapView.Active.MapToScreen(endPoint);

        // now get the distance
        var xDiff = endPtScreen.X - startPtScreen.X;
        var yDiff = endPtScreen.Y - startPtScreen.Y;

        var dist = Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));

        return dist;
      });
    }


    private Task<System.Windows.Media.Color> ConvertCIMColorToUIColorBrush(CIMColor color)
    {
      return QueuedTask.Run(() =>
      {
        var rgbColor = ColorFactory.Instance.ConvertToRGB(color);
        var uiColor = ArcGIS.Desktop.Internal.Mapping.Symbology.ColorHelper.UIColor(rgbColor);
        return uiColor;
      });
    }
  }
}
