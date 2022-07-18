using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.SystemCore;
using ArcGIS.Desktop.Core.UnitFormats;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COGOLineFeatures
{
  class COGOUtils
  {
    internal static string FormatDirectionDashesToDegMinSecSymbols(string Bearing)
    {
      string InitialBearingString = Bearing;
      Bearing = Bearing.ToUpper().Trim();
      try
      {
        Bearing = Bearing.Replace(" ", "");
        if (Bearing.EndsWith("E") || Bearing.EndsWith("W"))
          Bearing = Bearing.Insert(Bearing.Length - 1, "\"");
        else
          Bearing = Bearing.Insert(Bearing.Length, "\"");
        int i = Bearing.LastIndexOf('-');

        if (i > -1)
        {
          Bearing = Bearing.Insert(i, "'");
          i = Bearing.IndexOf('-');
          Bearing = Bearing.Insert(i, "°");
          Bearing = Bearing.Replace("-", "");
        }
        else if (i == -1)
          Bearing = Bearing.Replace("\"", "°");
      }
      catch
      {
        return InitialBearingString;
      }
      return Bearing;
    }

    internal static double PolarRadiansToNorthAzimuthDecimalDegrees(double InPolarRadians)
    {
      var AngConv = DirectionUnitFormatConversion.Instance;
      var ConvDef = new ConversionDefinition()
      {
        DirectionTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar,
        DirectionUnitsIn = ArcGIS.Core.SystemCore.DirectionUnits.Radians,
        DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth,
        DirectionUnitsOut = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees
      };
      return AngConv.ConvertToDouble(InPolarRadians, ConvDef);
    }
    internal static double ConvertToAngleInRadians(DisplayUnitFormat incomingAngleFormat, string InAngleString)
    {
      var dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar;
      var angleMeasurementUnit = incomingAngleFormat.MeasurementUnit;
      if (angleMeasurementUnit.FactoryCode == 909004)//Degrees Minutes Seconds
      {
        InAngleString = InAngleString.Replace('°', '-');
        InAngleString = InAngleString.Replace("'", "-");
        InAngleString = InAngleString.Replace('"', '-');
      }
      else
      {
        InAngleString = InAngleString.Replace("°", "");
        InAngleString = StripInvalidTrailingDirectionCharacters(InAngleString);
      }
      var dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.Radians;
      if (angleMeasurementUnit.FactoryCode == 909004)//Degrees Minutes Seconds
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.DegreesMinutesSeconds;
      else if (angleMeasurementUnit.FactoryCode == 9102)//Decimal Degrees
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees;
      else if (angleMeasurementUnit.FactoryCode == 9105)//Gradians
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.Gradians;
      else if (angleMeasurementUnit.FactoryCode == 9106)//Gons
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.Gons;

      var AngConv = DirectionUnitFormatConversion.Instance;
      var ConvDef = new ConversionDefinition()
      {
        DirectionTypeIn = dirTypeIn,
        DirectionUnitsIn = dirUnitIn,
        DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.Polar,
        DirectionUnitsOut = ArcGIS.Core.SystemCore.DirectionUnits.Radians
      };
      return AngConv.ConvertToDouble(InAngleString, ConvDef);
    }
    internal static double ConvertToNorthAzimuthDecimalDegrees(DisplayUnitFormat incomingDirectionFormat, string InDirectionString)
    {
      if (incomingDirectionFormat == null)
        return 0.0;

      var dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar;
      var directionUnitFormat = incomingDirectionFormat.UnitFormat as CIMDirectionFormat;

      if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.NorthAzimuth)
        dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth;
      else if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.SouthAzimuth)
        dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.SouthAzimuth;
      else if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.Polar)
        dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar;
      else if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.QuadrantBearing)
        dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.QuadrantBearing;

      if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.DegreesMinutesSeconds)
      {
        InDirectionString = InDirectionString.Replace('°', '-');
        InDirectionString = InDirectionString.Replace("'", "-");
        InDirectionString = InDirectionString.Replace('"', '-');
      }
      else
      {
        InDirectionString = InDirectionString.Replace("°", "");
        InDirectionString = StripInvalidTrailingDirectionCharacters(InDirectionString);
      }

      var dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.Radians;
      if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.DegreesMinutesSeconds)
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.DegreesMinutesSeconds;
      else if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.DecimalDegrees)
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees;
      else if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.Gradians)
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.Gradians;
      else if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.Gons) //same as gradians
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.Gons;

      var AngConv = DirectionUnitFormatConversion.Instance;
      var ConvDef = new ConversionDefinition()
      {
        DirectionTypeIn = dirTypeIn,
        DirectionUnitsIn = dirUnitIn,
        DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth,
        DirectionUnitsOut = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees
      };
      return AngConv.ConvertToDouble(InDirectionString, ConvDef);
    }
    internal static double ConvertToPolarDirectionRadians(DisplayUnitFormat incomingDirectionFormat, string InDirectionString)
    {
      var dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar;
      var directionUnitFormat = incomingDirectionFormat.UnitFormat as CIMDirectionFormat;

      if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.NorthAzimuth)
        dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth;
      else if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.SouthAzimuth)
        dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.SouthAzimuth;
      else if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.Polar)
        dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar;
      else if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.QuadrantBearing)
        dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.QuadrantBearing;

      if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.DegreesMinutesSeconds)
      {
        InDirectionString = InDirectionString.Replace('°', '-');
        InDirectionString = InDirectionString.Replace("'", "-");
        InDirectionString = InDirectionString.Replace('"', '-');
      }
      else
      {
        InDirectionString = InDirectionString.Replace("°", "");
        InDirectionString = StripInvalidTrailingDirectionCharacters(InDirectionString);
      }

      var dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.Radians;
      if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.DegreesMinutesSeconds)
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.DegreesMinutesSeconds;
      else if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.DecimalDegrees)
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees;
      else if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.Gradians)
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.Gradians;
      else if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.Gons) //same as gradians
        dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.Gons;

      var AngConv = DirectionUnitFormatConversion.Instance;
      var ConvDef = new ConversionDefinition()
      {
        DirectionTypeIn = dirTypeIn,
        DirectionUnitsIn = dirUnitIn,
        DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.Polar,
        DirectionUnitsOut = ArcGIS.Core.SystemCore.DirectionUnits.Radians
      };
      return AngConv.ConvertToDouble(InDirectionString, ConvDef);
    }

    internal static string ConvertPolarCartesianRadiansToDirectionFormat(double PolarRadians, DisplayUnitFormat ReturnDirectionFormat, bool UseDegMinSecSymbols)
    {
      var dirTypeOut = ArcGIS.Core.SystemCore.DirectionType.Polar;
      var directionUnitFormat = ReturnDirectionFormat.UnitFormat as CIMDirectionFormat;
      int iRounding = directionUnitFormat.DecimalPlaces;
      if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.NorthAzimuth)
        dirTypeOut = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth;
      else if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.SouthAzimuth)
        dirTypeOut = ArcGIS.Core.SystemCore.DirectionType.SouthAzimuth;
      else if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.Polar)
        dirTypeOut = ArcGIS.Core.SystemCore.DirectionType.Polar;
      else if (directionUnitFormat.DirectionType == ArcGIS.Core.CIM.DirectionType.QuadrantBearing)
        dirTypeOut = ArcGIS.Core.SystemCore.DirectionType.QuadrantBearing;

      var dirUnitOut = ArcGIS.Core.SystemCore.DirectionUnits.Radians;
      if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.DegreesMinutesSeconds)
        dirUnitOut = ArcGIS.Core.SystemCore.DirectionUnits.DegreesMinutesSeconds;
      else if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.DecimalDegrees)
        dirUnitOut = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees;
      else if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.Gradians)
        dirUnitOut = ArcGIS.Core.SystemCore.DirectionUnits.Gradians;
      else if (directionUnitFormat.Units == ArcGIS.Core.CIM.DirectionUnits.Gons) //same as gradians
        dirUnitOut = ArcGIS.Core.SystemCore.DirectionUnits.Gons;

      var AngConv = DirectionUnitFormatConversion.Instance;
      var ConvDef = new ConversionDefinition()
      {
        DirectionTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar,
        DirectionUnitsIn = ArcGIS.Core.SystemCore.DirectionUnits.Radians,
        DirectionTypeOut = dirTypeOut,
        DirectionUnitsOut = dirUnitOut
      };
      string sFormatted = AngConv.ConvertToString(PolarRadians, iRounding, ConvDef);
      if (UseDegMinSecSymbols)
        sFormatted = FormatDirectionDashesToDegMinSecSymbols(sFormatted);
      return sFormatted;
    }
    internal static string ConvertAngleInRadiansToAngleStringFormat(double PolarRadians, DisplayUnitFormat ReturnAngleFormat, bool UseDegMinSecSymbols)
    {
      var dirTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar;
      var dirUnitIn = ArcGIS.Core.SystemCore.DirectionUnits.Radians;
      var angleMeasurementUnit = ReturnAngleFormat.MeasurementUnit;
      bool isDMS = false;
      //retrieve decimal places from backstage Angular Unit, if not DMS
      int iRounding = 6;
      if (ReturnAngleFormat.AngleFormat.RoundingOption == esriRoundingOptionEnum.esriRoundNumberOfDecimals)
        iRounding = ReturnAngleFormat.AngleFormat.RoundingValue;
      var dirUnitOut = ArcGIS.Core.SystemCore.DirectionUnits.Radians;
      if (angleMeasurementUnit.FactoryCode == 909004)//Degrees Minutes Seconds
      {
        dirUnitOut = ArcGIS.Core.SystemCore.DirectionUnits.DegreesMinutesSeconds;
        isDMS = true;
        iRounding = 0; //degrees minutes seconds does not have option for decimal seconds in Angular unit
      }
      else if (angleMeasurementUnit.FactoryCode == 9102)//Decimal Degrees
        dirUnitOut = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees;
      else if (angleMeasurementUnit.FactoryCode == 9105)//Gradians
        dirUnitOut = ArcGIS.Core.SystemCore.DirectionUnits.Gradians;
      else if (angleMeasurementUnit.FactoryCode == 9106)//Gons
        dirUnitOut = ArcGIS.Core.SystemCore.DirectionUnits.Gons;

      var AngConv = DirectionUnitFormatConversion.Instance;
      var ConvDef = new ConversionDefinition()
      {
        DirectionTypeIn = dirTypeIn,
        DirectionUnitsIn = dirUnitIn,
        DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.Polar,
        DirectionUnitsOut = dirUnitOut
      };
      string sFormatted = AngConv.ConvertToString(PolarRadians, iRounding, ConvDef);
      if (UseDegMinSecSymbols && isDMS)
        sFormatted = FormatDirectionDashesToDegMinSecSymbols(sFormatted);
      return sFormatted;
    }

    internal static double ConvertFromNegativeNorthAzimuthDecimalDegrees(double NegativeDirection)
    {
      if (NegativeDirection >= 0)
        return NegativeDirection;
      var AngConv = DirectionUnitFormatConversion.Instance;
      var ConvDef = new ConversionDefinition()
      {
        DirectionTypeIn = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth,
        DirectionUnitsIn = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees,
        DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth,
        DirectionUnitsOut = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees
      };
      return AngConv.ConvertToDouble(NegativeDirection, ConvDef);
    }
    private static string StripInvalidTrailingDirectionCharacters(string IncomingDirectionString)
    {
      char[] MyChar = IncomingDirectionString.ToArray();
      char[] Valid = new char[] { 'E', 'W', 'e', 'w', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
      string NewString = "";
      int cntTrailing = 0;
      int iLength = MyChar.Length;
      for (int i = iLength - 1; i >= 0; i--)
      {
        double dd;
        if (Double.TryParse(MyChar[i].ToString(), out dd))
        {
          break;
        }
        else
        {
          if (!Valid.Contains(MyChar[i]))
            cntTrailing++;
        }
      }
      if (cntTrailing == 0)
        return IncomingDirectionString;

      NewString = IncomingDirectionString.Remove(iLength - cntTrailing);
      return NewString;
    }
    internal bool GetCOGOFromGeometry(Polyline myLineFeature, SpatialReference MapSR, double ScaleFactor, double DirectionOffset, out object[] COGODirectionDistanceRadiusArcLength)
    {
      COGODirectionDistanceRadiusArcLength = new object[4] { DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value };
      try
      {

        COGODirectionDistanceRadiusArcLength[0] = DBNull.Value;
        COGODirectionDistanceRadiusArcLength[1] = DBNull.Value;

        var GeomSR = myLineFeature.SpatialReference;
        if (GeomSR.IsGeographic && MapSR.IsGeographic)
          return false; //Future work: Make use of API for Geodesics.
        double UnitConversion = 1;

        if (GeomSR.IsGeographic && MapSR.IsProjected)
        { //only need to project if dataset is in a GCS.
          UnitConversion = MapSR.Unit.ConversionFactor; // Meters per unit. Only need this for converting to metric for GCS datasets.
          myLineFeature = GeometryEngine.Instance.Project(myLineFeature, MapSR) as Polyline;
        }
        EllipticArcSegment pCircArc;
        ICollection<Segment> LineSegments = new List<Segment>();
        myLineFeature.GetAllSegments(ref LineSegments);
        int numSegments = LineSegments.Count;

        IList<Segment> iList = LineSegments as IList<Segment>;
        Segment FirstSeg = iList[0];
        Segment LastSeg = iList[numSegments - 1];

        var pLine = LineBuilderEx.CreateLineSegment(FirstSeg.StartCoordinate, LastSeg.EndCoordinate);
        COGODirectionDistanceRadiusArcLength[0] =
        PolarRadiansToNorthAzimuthDecimalDegrees(pLine.Angle - DirectionOffset * Math.PI / 180);
        COGODirectionDistanceRadiusArcLength[1] = pLine.Length * UnitConversion / ScaleFactor;
        //check if the last segment is a circular arc
        var pCircArcLast = LastSeg as EllipticArcSegment;
        if (pCircArcLast == null)
          return true; //we already know there is no circluar arc COGO
                       //Keep a copy of the center point
        var LastCenterPoint = pCircArcLast.CenterPoint;
        COGODirectionDistanceRadiusArcLength[2] = pCircArcLast.IsCounterClockwise ?
                -pCircArcLast.SemiMajorAxis : Math.Abs(pCircArcLast.SemiMajorAxis); //radius
        double dArcLengthSUM = 0;
        //use 30 times xy tolerance for circular arc segment tangency test
        double dTangencyToleranceTest = MapSR.XYTolerance * 30; //around 3cms if using default XY Tolerance - recommended
        for (int i = 0; i < numSegments; i++)
        {
          pCircArc = iList[i] as EllipticArcSegment;
          if (pCircArc == null)
          {
            COGODirectionDistanceRadiusArcLength[2] = DBNull.Value; //radius
            COGODirectionDistanceRadiusArcLength[3] = DBNull.Value; //arc length
            return true;
          }
          var tolerance = LineBuilderEx.CreateLineSegment(LastCenterPoint, pCircArc.CenterPoint).Length;
          if (tolerance > dTangencyToleranceTest)
          {
            COGODirectionDistanceRadiusArcLength[2] = DBNull.Value; //radius
            COGODirectionDistanceRadiusArcLength[3] = DBNull.Value; //arc length
            return true;
          }
          dArcLengthSUM += pCircArc.Length; //arc length sum
        }
        //now check to see if the radius and arclength survived and if so, clear the distance
        if (COGODirectionDistanceRadiusArcLength[2] != DBNull.Value)
          COGODirectionDistanceRadiusArcLength[1] = DBNull.Value;

        COGODirectionDistanceRadiusArcLength[3] = dArcLengthSUM * UnitConversion / ScaleFactor;
        COGODirectionDistanceRadiusArcLength[2] = (double)COGODirectionDistanceRadiusArcLength[2] * UnitConversion / ScaleFactor;

        return true;
      }
      catch
      {
        return false;
      }
    }
    internal static double InverseDirectionAsNorthAzimuth(Coordinate2D FromCoordinate, Coordinate2D ToCoordinate, bool Reversed)
    {
      var DirectionInPolarRadians = LineBuilderEx.CreateLineSegment(FromCoordinate, ToCoordinate).Angle;
      if (Reversed)
        DirectionInPolarRadians += Math.PI;
      return PolarRadiansToNorthAzimuthDecimalDegrees(DirectionInPolarRadians);
    }
    internal static Coordinate2D PointInDirection(Coordinate2D FromCoordinate, double NAzimuthDecimalDegrees, double Distance)
    {
      Coordinate3D pVec1 = new Coordinate3D(FromCoordinate.X, FromCoordinate.Y, 0);
      Coordinate3D pVec2 = new Coordinate3D();
      double NAzimuthRadians = NAzimuthDecimalDegrees * Math.PI / 180;
      pVec2.SetPolarComponents(NAzimuthRadians, 0, Distance);
      Coordinate2D ComputedCoordinate = new Coordinate2D(pVec1.AddCoordinate3D(pVec2));
      return ComputedCoordinate;
    }

    internal static string TangentDirectionFromCircularArc(EllipticArcSegment CircArc,
      DisplayUnitFormat ReturnDirectionFormat, double DirectionOffsetInRadians)
    {
      double d90 = CircArc.IsCounterClockwise ? Math.PI / 2.0 : -Math.PI / 2.0;
      return ConvertPolarCartesianRadiansToDirectionFormat(CircArc.StartAngle + d90 - DirectionOffsetInRadians, ReturnDirectionFormat, true);
    }

    internal static string ChordDirectionFromCircularArc(EllipticArcSegment CircArc,
      DisplayUnitFormat ReturnDirectionFormat, double DirectionOffsetInRadians)
    {
      var line = LineBuilderEx.CreateLineSegment(CircArc.StartCoordinate, CircArc.EndCoordinate);
      return ConvertPolarCartesianRadiansToDirectionFormat(line.Angle - DirectionOffsetInRadians, ReturnDirectionFormat, true);
    }

    internal static string RadialDirectionFromCircularArc(EllipticArcSegment CircArc,
      DisplayUnitFormat ReturnDirectionFormat, double DirectionOffsetInRadians)
    {
      var line = LineBuilderEx.CreateLineSegment(CircArc.StartCoordinate, CircArc.EndCoordinate);
      var chordDirection = line.Angle;
      var chordDistance = line.Length;
      var radius = CircArc.SemiMinorAxis;
      var halfDelta = Math.Abs(Math.Asin(chordDistance / (2.0 * radius)));
      var radialDirection = CircArc.IsCounterClockwise ? chordDirection + Math.PI / 2.0 - halfDelta : chordDirection - Math.PI / 2.0 + halfDelta;
      return ConvertPolarCartesianRadiansToDirectionFormat(radialDirection - DirectionOffsetInRadians, ReturnDirectionFormat, true);
    }

    internal static string DeltaAngleFromCircularArc(EllipticArcSegment CircArc, DisplayUnitFormat ReturnAngleFormat)
    {
      bool isDMS = false;
      int iRounding = 6;
      //retrieve decimal places from backstage angular unit, if not DMS
      if (ReturnAngleFormat.AngleFormat.RoundingOption == esriRoundingOptionEnum.esriRoundNumberOfDecimals)
        iRounding = ReturnAngleFormat.AngleFormat.RoundingValue;
      var angleMeasurementUnit = ReturnAngleFormat.MeasurementUnit;
      var angleUnitReturn = ArcGIS.Core.SystemCore.DirectionUnits.Radians;
      if (angleMeasurementUnit.FactoryCode == 909004)//Degrees Minutes Seconds
      {
        angleUnitReturn = ArcGIS.Core.SystemCore.DirectionUnits.DegreesMinutesSeconds;
        isDMS = true;
        iRounding = 0; //degrees minutes seconds does not have option for decimal seconds in Angular unit
      }
      else if (angleMeasurementUnit.FactoryCode == 9102)//Decimal Degrees
        angleUnitReturn = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees;
      else if (angleMeasurementUnit.FactoryCode == 9105)//Gradians
        angleUnitReturn = ArcGIS.Core.SystemCore.DirectionUnits.Gradians;
      else if (angleMeasurementUnit.FactoryCode == 9106)//Gons
        angleUnitReturn = ArcGIS.Core.SystemCore.DirectionUnits.Gons;
      var AngConv = DirectionUnitFormatConversion.Instance;
      var ConvDef = new ConversionDefinition()
      {
        DirectionTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar,
        DirectionUnitsIn = ArcGIS.Core.SystemCore.DirectionUnits.Radians,
        DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.Polar,
        DirectionUnitsOut = angleUnitReturn
      };
      var deltaAngleInRadians = Math.Abs(CircArc.CentralAngle);
      var angle = AngConv.ConvertToString(deltaAngleInRadians, iRounding, ConvDef);
      if (isDMS)
        angle = FormatDirectionDashesToDegMinSecSymbols(angle);
      return angle;
    }

    internal static string ArcLengthFromCircularArc(EllipticArcSegment CircArc, DisplayUnitFormat ReturnDistanceFormat, double DistanceScaleFactor)
    {
      double datasetMetersPerUnit = 1;

      if (CircArc.SpatialReference.IsProjected)
        datasetMetersPerUnit = CircArc.SpatialReference.Unit.ConversionFactor;

      var returnMetersPerUnit = ReturnDistanceFormat.MeasurementUnit.ConversionFactor;
      var circArcLengthInMeters = CircArc.Length * datasetMetersPerUnit;
      var arcLength = circArcLengthInMeters / returnMetersPerUnit;

      //if (ReturnDistanceFormat.RoundingOption == esriRoundingOptionEnum.esriRoundNumberOfDecimals)
      //  iRounding = ReturnDistanceFormat.RoundingValue;

      arcLength /= DistanceScaleFactor;

      int iRounding = 2;
      string sFormat = new string('0', iRounding);
      sFormat = "0." + sFormat; ;

      return arcLength.ToString(sFormat);
    }

    internal static string ChordLengthFromCircularArc(EllipticArcSegment CircArc, DisplayUnitFormat ReturnDistanceFormat, double DistanceScaleFactor)
    {
      double datasetMetersPerUnit = 1;
      var chordLength = LineBuilderEx.CreateLineSegment(CircArc.StartCoordinate, CircArc.EndCoordinate).Length;

      if (CircArc.SpatialReference.IsProjected)
        datasetMetersPerUnit = CircArc.SpatialReference.Unit.ConversionFactor;

      var returnMetersPerUnit = ReturnDistanceFormat.MeasurementUnit.ConversionFactor;
      var chordLengthInMeters = chordLength * datasetMetersPerUnit;
      chordLength = chordLengthInMeters / returnMetersPerUnit;

      chordLength /= DistanceScaleFactor;

      //if (ReturnDistanceFormat.RoundingOption == esriRoundingOptionEnum.esriRoundNumberOfDecimals)
      //  iRounding = ReturnDistanceFormat.RoundingValue;

      int iRounding = 2;
      string sFormat = new string('0', iRounding);
      sFormat = "0." + sFormat; ;

      return chordLength.ToString(sFormat);
    }

    internal static EllipticArcSegment CreateCircularArcFromParams(MapPoint StartPoint, double DirectionPolarRadians,
      string DirectionType, double Radius, bool IsCounterClockwise, object Chord = null, object DeltaAngleRadians = null,
      object ArcLength = null, double ScaleFactor = 1.0, double DirectionOffsetInRadians = 0.0)
    {
      bool hasChordDirection = DirectionType.ToLower() == "chord direction";
      bool hasTangentDirection = DirectionType.ToLower() == "tangent direction";
      bool hasRadialDirection = DirectionType.ToLower() == "radial direction";
      bool hasRadius = Radius != 0.0;
      bool hasChord = Chord != null;
      bool hasDelta = DeltaAngleRadians != null;
      bool hasArcLength = ArcLength != null;

      double chordDirection = 0.0;
      double radialDirection = 0.0;
      double tangentDirection = 0.0;

      ArcOrientation CCW =
        IsCounterClockwise ? ArcOrientation.ArcCounterClockwise : ArcOrientation.ArcClockwise;
      MinorOrMajor minMaj = MinorOrMajor.Minor; //default to minor arc

      if (hasChordDirection)
        chordDirection = DirectionPolarRadians + DirectionOffsetInRadians;
      else if (hasRadialDirection)
        radialDirection = DirectionPolarRadians + DirectionOffsetInRadians;
      else if (hasTangentDirection)
        tangentDirection = DirectionPolarRadians + DirectionOffsetInRadians;

      double dRadius = Math.Abs(Radius) * ScaleFactor;
      double dChord = 0.0;
      double dArcLength = 0.0;
      double dDelta = 0.0;

      if (hasChord)
        dChord = (double)Chord * ScaleFactor;
      if (hasArcLength)
        dArcLength = (double)ArcLength * ScaleFactor;
      if (hasDelta)
        dDelta = (double)DeltaAngleRadians;

      EllipticArcSegment circArcSegment = null;
      try
      {//chord bearing
        if (hasRadius && hasChord && hasChordDirection)
        {
          try
          {
            circArcSegment = EllipticArcBuilderEx.CreateCircularArc(StartPoint, dChord, chordDirection,
            dRadius, CCW, MinorOrMajor.Minor, null);
            return circArcSegment;
          }
          catch { return null; }
        }
        else if (hasRadius && hasArcLength && hasChordDirection)
        {
          var dCentralAngle = dArcLength / dRadius;
          dChord = 2.0 * dRadius * Math.Sin(dCentralAngle / 2.0);
          minMaj = dCentralAngle > Math.PI ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasRadius && hasDelta && hasChordDirection)
        {
          dChord = 2.0 * dRadius * Math.Sin(dDelta / 2.0);
          minMaj = Math.Abs(dDelta) > Math.PI ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasChord && hasDelta && hasChordDirection)
        {
          dRadius = 0.5 * dChord / Math.Sin(dDelta / 2.0);
          minMaj = dDelta > Math.PI ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasChord && hasArcLength && hasChordDirection)
        {
          try
          {
            //Newton's approximation method
            circArcSegment = EllipticArcBuilderEx.CreateCircularArc(StartPoint, dChord, chordDirection,
                dArcLength, CCW, null);
          }
          catch { return null; }
        }
        //tangent bearing
        else if (hasRadius && hasChord && hasTangentDirection)
        {
          var dHalfDelta = Math.Abs(Math.Asin(dChord / (2.0 * dRadius)));
          //get chord bearing from given tangent bearing in polar radians
          if (CCW == ArcOrientation.ArcCounterClockwise)
            chordDirection = tangentDirection + dHalfDelta;
          else
            chordDirection = tangentDirection - dHalfDelta;
          minMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasRadius && hasArcLength && hasTangentDirection)
        {
          var dHalfDelta = dArcLength / dRadius / 2.0;
          dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
          //get chord bearing from given tangent bearing in north azimuth radians
          if (CCW == ArcOrientation.ArcCounterClockwise)
            chordDirection = tangentDirection + dHalfDelta;
          else
            chordDirection = tangentDirection - dHalfDelta;

          minMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasRadius && hasDelta && hasTangentDirection)
        {
          var dHalfDelta = dDelta / 2.0;
          dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
          //get chord bearing from given tangent bearing in north azimuth radians
          if (CCW == ArcOrientation.ArcCounterClockwise)
            chordDirection = tangentDirection + dHalfDelta;
          else
            chordDirection = tangentDirection - dHalfDelta;

          minMaj = dDelta > Math.PI ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasChord && hasDelta && hasTangentDirection)
        {
          var dHalfDelta = dDelta / 2.0;
          dRadius = dChord / (2.0 * Math.Sin(dHalfDelta));
          //get chord bearing from given tangent bearing in north azimuth radians
          if (CCW == ArcOrientation.ArcCounterClockwise)
            chordDirection = tangentDirection + dHalfDelta;
          else
            chordDirection = tangentDirection - dHalfDelta;

          minMaj = dDelta > Math.PI ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasChord && hasArcLength && hasTangentDirection)
        {
          try
          {
            //Newton's approximation method. First computation uses a 0 bearing just to get the initial curve geometry, and central angle:
            //the computed central angle is then used to apply to the tangent bearing
            circArcSegment = EllipticArcBuilderEx.CreateCircularArc(StartPoint, dChord, 0.0,
              dArcLength, CCW, null);
            //Get the Delta central angle from the approximation, and then ...
            double dHalfDelta = Math.Abs(circArcSegment.CentralAngle) / 2.0;
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              chordDirection = tangentDirection + dHalfDelta;
            else
              chordDirection = tangentDirection - dHalfDelta;

            circArcSegment = EllipticArcBuilderEx.CreateCircularArc(StartPoint, dChord, chordDirection,
              dArcLength, CCW, null);
          }
          catch { return null; }
        }
        else if (hasRadius && hasChord && hasRadialDirection)
        {
          var dHalfDelta = Math.Abs(Math.Asin(dChord / (2.0 * dRadius)));
          //get chord bearing from given radial bearing in north azimuth radians
          if (CCW == ArcOrientation.ArcCounterClockwise)
            chordDirection = radialDirection - Math.PI / 2.0 + dHalfDelta;
          else
            chordDirection = radialDirection + Math.PI / 2.0 - dHalfDelta;

          minMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasRadius && hasArcLength && hasRadialDirection)
        {
          var dHalfDelta = (dArcLength / dRadius) / 2.0;
          dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
          //get chord bearing from given tangent bearing in north azimuth radians
          if (CCW == ArcOrientation.ArcCounterClockwise)
            chordDirection = radialDirection - Math.PI / 2.0 + dHalfDelta;
          else
            chordDirection = radialDirection + Math.PI / 2.0 - dHalfDelta;

          minMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasRadius && hasDelta && hasRadialDirection)
        {
          var dHalfDelta = dDelta / 2.0;
          dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
          //get chord bearing from given tangent bearing in north azimuth radians
          if (CCW == ArcOrientation.ArcCounterClockwise)
            chordDirection = radialDirection - Math.PI / 2.0 + dHalfDelta;
          else
            chordDirection = radialDirection + Math.PI / 2.0 - dHalfDelta;

          minMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasChord && hasDelta && hasRadialDirection)
        {
          var dHalfDelta = dDelta / 2.0;
          dRadius = dChord / (2.0 * Math.Sin(dHalfDelta));
          //get chord bearing from given radial bearing in north azimuth radians
          if (CCW == ArcOrientation.ArcCounterClockwise)
            chordDirection = radialDirection - Math.PI / 2.0 + dHalfDelta;
          else
            chordDirection = radialDirection + Math.PI / 2.0 - dHalfDelta;

          minMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasChord && hasArcLength && hasRadialDirection)
        {
          try
          {
            //Newton's approximation method. First computation uses a 0 bearing just to get the initial curve geometry, and central angle:
            //the computed central angle is then used to apply to the radial bearing from the previous course
            circArcSegment = EllipticArcBuilderEx.CreateCircularArc(StartPoint, dChord, 0.0,
              dArcLength, CCW, null);
            //Get the Delta central angle from the approximation, and then ...
            var dHalfDelta = Math.Abs(circArcSegment.CentralAngle) / 2.0;
            //get chord bearing from given radial bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              chordDirection = radialDirection - Math.PI / 2.0 + dHalfDelta;
            else
              chordDirection = radialDirection + Math.PI / 2.0 - dHalfDelta;

            circArcSegment = EllipticArcBuilderEx.CreateCircularArc(StartPoint, dChord, chordDirection,
              dArcLength, CCW, null);
          }
          catch { return null; }
        }
        else if (hasDelta && hasArcLength && hasChordDirection)
        {
          //get the radius and chord from the Central Angle and Arc Length
          dRadius = dArcLength / dDelta;
          var dHalfDelta = dDelta / 2.0;
          dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
          minMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasDelta && hasArcLength && hasTangentDirection)
        {
          //get the radius and chord from the Central Angle and Arc Length
          dRadius = dArcLength / dDelta;
          var dHalfDelta = dDelta / 2.0;
          dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
          //get chord bearing from given tangent bearing in north azimuth radians
          if (CCW == ArcOrientation.ArcCounterClockwise)
            chordDirection = tangentDirection + dHalfDelta;
          else
            chordDirection = tangentDirection - dHalfDelta;

          minMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }
        else if (hasDelta && hasArcLength && hasRadialDirection)
        {
          //get the radius and chord from the Central Angle and Arc Length
          dRadius = dArcLength / dDelta;
          var dHalfDelta = dDelta / 2.0;
          dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
          //get chord bearing from given radial bearing in north azimuth radians
          if (CCW == ArcOrientation.ArcCounterClockwise)
            chordDirection = radialDirection - Math.PI / 2.0 + dHalfDelta;
          else
            chordDirection = radialDirection + Math.PI / 2.0 - dHalfDelta;

          minMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
        }

        if (circArcSegment == null)
          circArcSegment = EllipticArcBuilderEx.CreateCircularArc(StartPoint, dChord, chordDirection,
            dRadius, CCW, minMaj, null);
        return circArcSegment;
      }
      catch { return null; }
    }

    internal static EllipticArcSegment CreateCircularArcByEndpoints(MapPoint StartPoint, MapPoint EndPoint,
      double Radius, bool IsCounterClockwise, bool IsMajor, double ScaleFactorForRadius = 1.0)
    {

      bool hasRadius = Radius != 0.0;
      var pNewSeg = LineBuilderEx.CreateLineSegment(StartPoint, EndPoint);

      double chordDirection = pNewSeg.Angle;
      double dChord = pNewSeg.Length;

      ArcOrientation CCW =
        IsCounterClockwise ? ArcOrientation.ArcCounterClockwise : ArcOrientation.ArcClockwise;
      MinorOrMajor minMaj =
        IsMajor ? MinorOrMajor.Major : MinorOrMajor.Minor;

      double dRadius = Math.Abs(Radius) * ScaleFactorForRadius;

      EllipticArcSegment circArcSegment;
      try
      {
        circArcSegment = EllipticArcBuilderEx.CreateCircularArc(StartPoint, dChord, chordDirection,
        dRadius, CCW, minMaj, null);
        return circArcSegment;
      }
      catch { return null; }
    }

    #region Traverse file processing functions
    internal static List<string> TraverseCoursesFromFiles(List<string> TraverseFiles)
    {
      //make a temporary file for the combined traverse files if more than one was selected
      string sTempPath = System.IO.Path.GetTempPath();
      string sCombinedTraverseFile = System.IO.Path.Combine(sTempPath, "LastUpdatedCombinedTraverseFileFromBatch.txt");

      const int chunkSize = 2 * 1024; // 2KB
      using (var output = System.IO.File.Create(sCombinedTraverseFile))
      {
        foreach (var file in TraverseFiles)
        {
          string[] s = file.Split(System.IO.Path.DirectorySeparatorChar);
          string FileName = s[s.Length - 1].Replace(".txt", "");
          using (var input = System.IO.File.OpenRead(file))
          {
            var buffer = new byte[chunkSize];
            byte[] bytes = Encoding.ASCII.GetBytes("FN " + FileName + Environment.NewLine);
            output.Write(bytes, 0, bytes.Length);
            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
              output.Write(buffer, 0, bytesRead);
          }
        }
      }

      System.IO.TextReader tr;
      try
      {
        tr = new System.IO.StreamReader(sCombinedTraverseFile);
      }
      catch
      {
        return null;
      }

      List<string> sFileLines = new List<string>();
      List<string> sValidCodes = new List<string>();
      sValidCodes.Add("DT ");
      sValidCodes.Add("DU ");
      sValidCodes.Add("SP ");
      sValidCodes.Add("EP ");
      sValidCodes.Add("DD ");
      sValidCodes.Add("AD ");
      sValidCodes.Add("TC ");
      sValidCodes.Add("NC ");

      string sCourse;
      try
      {
        sCourse = tr.ReadLine();
        //fill the array with the lines from the file
        while (sCourse != null)
        {
          try
          {
            if (sCourse.Trim().Length >= 3) //test for valid string length
            {
              if (sValidCodes.Contains(sCourse.Trim().Substring(0, 3).ToUpper()))
                sFileLines.Add(sCourse);
              if (sCourse.Trim().Substring(0, 3).ToUpper() == "FN ")
                sFileLines.Add(sCourse);
            }
            sCourse = tr.ReadLine();
          }
          catch
          {
            return null;
          }
        }
      }
      catch
      {
        return null;
      }
      finally
      {
        tr.Close(); //close the file and release resources
      }
      return sFileLines;
    }
    internal static async Task<bool> CreateFeaturesFromCourses(List<string> sFileLines,
          FeatureLayer featLyr, EditOperation op)
    {
      var fcDefinition = featLyr.GetFeatureClass().GetDefinition();
      bool bIsCOGOEnabled = fcDefinition.IsCOGOEnabled();
      /*Lines in Parcel Fabrics are already COGO enabled. This line of code is here in case 
       * this function is used for targeting other Line layers
      */

      bool bParcelLinesFieldChkOK = (bIsCOGOEnabled && fcDefinition.FindField("CreatedByRecord") > -1
                                   && fcDefinition.FindField("RetiredByRecord") > -1
                                   && fcDefinition.FindField("COGOType") > -1
                                   && fcDefinition.FindField("Scale") > -1
                                   && fcDefinition.FindField("Rotation") > -1);

      var mapView = MapView.Active;
      if (mapView?.Map == null)
        return false;

      var cimDefinition = mapView.Map?.GetDefinition();
      if (cimDefinition == null) return false;
      var cimG2G = cimDefinition.GroundToGridCorrection;

      double dScaleFactor = cimG2G.GetConstantScaleFactor();
      double dDirectionOffsetCorrection = cimG2G.GetDirectionOffset() * Math.PI / 180;

      Coordinate2D StartCoord = new Coordinate2D(); //This will be updated for each new segment
      Coordinate2D PointOfBeginning = new Coordinate2D(); //To hold onto initial start point
      Coordinate2D EndCoord = new Coordinate2D();

      try
      {
        #region Create features
        List<Segment> segments = new List<Segment>();
        LineSegment ExitTangent;
        LineSegment EntryTangent = null;
        EllipticArcSegment pCircArc;

        Polygon newPolygon = null;
        Polyline newPolyline = null;

        GeometryType geomType = fcDefinition.GetShapeType();

        var spatRef = featLyr.Map.SpatialReference;
        string shapeFldName = fcDefinition.GetShapeField();

        Dictionary<string, object> COGOAttributes = new Dictionary<string, object>();

        string sCourse = "";
        bool bNameChoiceToggleFirst = true;
        string[] sParcelName = new string[2] { "New", "New" }; //default fall-back name if one's not found while creating a parcel polygon
        var DirType = new ArcGIS.Core.SystemCore.DirectionType();
        var DirUnit = new ArcGIS.Core.SystemCore.DirectionUnits();

        foreach (string sTraverseCourse in sFileLines)
        {
          sCourse = sTraverseCourse.ToLower();
          if (sCourse.Contains("fn"))
          {
            string[] sName = sTraverseCourse.Split(' ');
            if (bNameChoiceToggleFirst)
            {
              sParcelName[0] = sParcelName[1] = sName[1];
              bNameChoiceToggleFirst = !bNameChoiceToggleFirst;
            }
            else sParcelName[1] = sName[1];
          }
          if (sCourse.Contains("dt"))
          {
            if (sCourse.Contains("qb"))
              DirType = ArcGIS.Core.SystemCore.DirectionType.QuadrantBearing;
            else if (sCourse.Contains("na"))
              DirType = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth;
            else if (sCourse.Contains("sa"))
              DirType = ArcGIS.Core.SystemCore.DirectionType.SouthAzimuth;
            else if (sCourse.Contains("p"))
              DirType = ArcGIS.Core.SystemCore.DirectionType.Polar;
          }
          if (sCourse.Contains("du"))
          {
            if (sCourse.Contains("dms"))
              DirUnit = ArcGIS.Core.SystemCore.DirectionUnits.DegreesMinutesSeconds;
            else if (sCourse.Contains("dd"))
              DirUnit = ArcGIS.Core.SystemCore.DirectionUnits.DecimalDegrees;
            else if (sCourse.Contains("g"))
              DirUnit = ArcGIS.Core.SystemCore.DirectionUnits.Gradians;
            else if (sCourse.Contains("r"))
              DirUnit = ArcGIS.Core.SystemCore.DirectionUnits.Radians;
          }

          bool IsLoopTraverse;
          bool IsClosedTraverse;
          //note  that EP code comes immediately after SP code, but EP is optional.
          //use presence of EP to indicate an adjustable closed traverse that may or may not be loop

          if (sCourse.Contains("sp"))
          {//start point

            ////TODO: may need MapPoint for map projection...
            ////var MapPt1 = MapPointBuilder.CreateMapPoint(StartPoint, spatRef);

            //Since this code supports multiple traverses in the same file, and multiple files,
            //we create the polygon from the previous loop of courses when geometry type is polygon,
            //and if the next start point is found.

            if (geomType == GeometryType.Polygon && segments.Count > 0)
            {
              newPolygon = PolygonBuilderEx.CreatePolygon(segments, spatRef);
              segments.Clear();
              if (newPolygon != null)
              {
                COGOAttributes.Add(fcDefinition.GetShapeField(), newPolygon);
                COGOAttributes.Add("Name", sParcelName[0]);
                sParcelName[0] = sParcelName[1];//store 2 copies of the name 
                op.Create(featLyr, COGOAttributes);
                COGOAttributes.Clear();//this is OK. op.Create makes a clone / copy of the attributes
              }
            }

            string[] XY = sTraverseCourse.Split(' ');
            double x = Convert.ToDouble(XY[1]);
            double y = Convert.ToDouble(XY[2]);
            StartCoord.X = x;
            StartCoord.Y = y;
            PointOfBeginning = StartCoord;
          }

          if (sCourse.Contains("ep"))
          {//end point, indicates closed traverse
            IsClosedTraverse = true;
            ////TODO: may need MapPoint for map projection...
            ////var MapPt2 = MapPointBuilder.CreateMapPoint(EndPoint, spatRef);

            string[] XY = sTraverseCourse.Split(' ');
            double x = Convert.ToDouble(XY[1]);
            double y = Convert.ToDouble(XY[2]);
            EndCoord.X = x;
            EndCoord.Y = y;
            //Test for loop traverse. NB: as currently written this is dependent on the EP code directly following after the SP code
            if (Math.Abs(EndCoord.X - StartCoord.X) < 0.001 && Math.Abs(EndCoord.Y - StartCoord.Y) < 0.001)
              IsLoopTraverse = true;
            else if (EndCoord.X == 0 && EndCoord.Y == 0 && StartCoord.X == 0 && StartCoord.Y == 0)
              IsLoopTraverse = true;
            else
              IsLoopTraverse = false;
          }

          if (cimG2G.UsingElevationMode())
          {
            double dElevation = 0;
            if (StartCoord.X != 0 && StartCoord.Y != 0)
            {
              var ptSpotXY = MapPointBuilderEx.CreateMapPoint(StartCoord, spatRef);
              if (ptSpotXY != null)
              {
                //check if the elevation mode is using a surface, and get the value from surface.
                if (ElevationCapturing.CaptureMode == ElevationCaptureMode.Surface)
                {
                  var ptSpotZ = (await mapView.Map.GetZsFromSurfaceAsync(ptSpotXY)).Geometry;
                  //scale factor is computed using elevation obtained from the location of each feature.
                  if (ptSpotZ != null)
                  {
                    dElevation = (ptSpotZ as MapPoint).Coordinate3D.Z;
                    dScaleFactor = await mapView.Map.ComputeCombinedScaleFactor(StartCoord.X, StartCoord.Y, StartCoord.X + 100, StartCoord.Y + 100, dElevation);
                  }
                }
                //otherwise, if the mode is Constant, then get the elevation directly
                else if (ElevationCapturing.CaptureMode == ElevationCaptureMode.Constant)
                {
                  dElevation = ElevationCapturing.ElevationConstantValue;
                  dScaleFactor = await mapView.Map.ComputeCombinedScaleFactor(StartCoord.X, StartCoord.Y, StartCoord.X + 100, StartCoord.Y + 100, dElevation);
                }
              }
            }
          }

          var pSeg = CreateSegmentFromStringCourse(sTraverseCourse, StartCoord, DirType, DirUnit,
            EntryTangent, dScaleFactor, dDirectionOffsetCorrection, out ExitTangent, out pCircArc);

          if (pSeg == null)
            continue;

          //do a test here to see if the End Coordinate is within tolerance of the Point of beginning or end point
          double dCheckCloseTolerance = 0.2;
          double distX2 = Math.Pow(pSeg.EndCoordinate.X - PointOfBeginning.X, 2);
          double distY2 = Math.Pow(pSeg.EndCoordinate.Y - PointOfBeginning.Y, 2);
          double dist = Math.Sqrt(distX2 + distY2);
          bool HasClosure = dist <= dCheckCloseTolerance;

          bool bLastLineInTraverse;

          if (HasClosure)
          {
            var pNewSeg = LineBuilderEx.CreateLineSegment(pSeg.StartCoordinate, PointOfBeginning);
            //Tweak the end point of the circular arc and the pSeg to match the PointOfBeginning value
            if (pCircArc != null)
            {
              double dRadius = pCircArc.SemiMajorAxis;
              ArcOrientation ArcOrientation = pCircArc.IsCounterClockwise ? ArcOrientation.ArcCounterClockwise : ArcOrientation.ArcClockwise;
              MinorOrMajor MinMajor = pCircArc.IsMinor ? MinorOrMajor.Minor : MinorOrMajor.Major;
              var pArcNewSeg = EllipticArcBuilderEx.CreateCircularArc(pNewSeg.StartPoint, pNewSeg.Length, pNewSeg.Angle, dRadius, ArcOrientation, MinMajor);
              pCircArc = pArcNewSeg;
            }
            else
            {
              pSeg = pNewSeg;
            }
          }

          StartCoord = pSeg.EndCoordinate; //set last computed point for start of next line
          EntryTangent = ExitTangent; //set the Entry tangent to be the previous course exit tangent

          if (pCircArc != null)
            segments.Add(pCircArc);
          else
            segments.Add(pSeg);

          if (geomType == GeometryType.Polyline)
          {
            newPolyline = PolylineBuilderEx.CreatePolyline(segments, spatRef);
            segments.Clear();
          }

          if (newPolyline != null)
          {
            COGOAttributes.Add(shapeFldName, newPolyline);

            if (bIsCOGOEnabled)
            {
              COGOAttributes.Add("Direction", PolarRadiansToNorthAzimuthDecimalDegrees(pSeg.Angle - dDirectionOffsetCorrection));
              if (pCircArc != null)
              {
                if (pCircArc.IsCounterClockwise)
                  COGOAttributes.Add("Radius", -1 * pCircArc.SemiMajorAxis / dScaleFactor);
                else
                  COGOAttributes.Add("Radius", pCircArc.SemiMajorAxis / dScaleFactor);
                COGOAttributes.Add("ArcLength", pCircArc.Length / dScaleFactor);
              }
              else
                COGOAttributes.Add("Distance", pSeg.Length / dScaleFactor);
            }

            if (bParcelLinesFieldChkOK) // extra check to confirm fields are available
            {
              if (cimG2G.UsingDistanceFactor() || cimG2G.UsingDirectionOffset())
                COGOAttributes.Add("COGOType", 1); //since this is coming in from a file we will set this COGO type as From measurements = 1

              if (cimG2G.UsingDistanceFactor())
                COGOAttributes.Add("Scale", dScaleFactor);

              if (cimG2G.UsingDirectionOffset())
                COGOAttributes.Add("Rotation", dDirectionOffsetCorrection < Math.PI ? dDirectionOffsetCorrection * 180 / Math.PI : (dDirectionOffsetCorrection * 180 / Math.PI) - 360);
            }

            op.Create(featLyr, COGOAttributes);
            COGOAttributes.Clear();//this is OK. Create makes a clone / copy of the attributes
          }
        }

        if (geomType == GeometryType.Polygon)
        {
          newPolygon = PolygonBuilderEx.CreatePolygon(segments, spatRef);
          segments.Clear();
          if (newPolygon != null)
          {
            COGOAttributes.Add(fcDefinition.GetShapeField(), newPolygon);
            COGOAttributes.Add("Name", sParcelName[0]);
            op.Create(featLyr, COGOAttributes);
            COGOAttributes.Clear(); //this is OK. Create makes a clone / copy of the attributes, prior to op.Execute();
          }
        }

        #endregion
      }
      catch
      { return false; }

      return true;
    }
    private static LineSegment CreateSegmentFromStringCourse(String inCourse, Coordinate2D FromCoordinate, ArcGIS.Core.SystemCore.DirectionType DirType,
      ArcGIS.Core.SystemCore.DirectionUnits DirUnits, LineSegment EntryTangent, double ScaleFactor,
      double DirectionOffsetInRadians, out LineSegment ExitTangent, out EllipticArcSegment outCircularArc)
    {
      string[] sCourse = inCourse.Split(' ');
      outCircularArc = null;
      ExitTangent = null;
      var AngConv = DirectionUnitFormatConversion.Instance;
      var ConvDef = new ConversionDefinition()
      {
        DirectionTypeIn = DirType,
        DirectionUnitsIn = DirUnits,
        DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth,
        DirectionUnitsOut = ArcGIS.Core.SystemCore.DirectionUnits.Radians
      };

      string item = (string)sCourse.GetValue(0);
      if (item.ToLower() == "dd")
      {//Direction distance -- straight line course

        double vecDirn = AngConv.ConvertToDouble((string)sCourse.GetValue(1), ConvDef);

        Coordinate3D pVec1 = new Coordinate3D(FromCoordinate.X, FromCoordinate.Y, 0);
        Coordinate3D pVec2 = new Coordinate3D();
        pVec2.SetPolarComponents(vecDirn - DirectionOffsetInRadians, 0, Convert.ToDouble(sCourse.GetValue(2)) * ScaleFactor);
        Coordinate2D coord2 = new Coordinate2D(pVec1.AddCoordinate3D(pVec2));
        var pSeg = LineBuilderEx.CreateLineSegment(FromCoordinate, coord2);

        ExitTangent = pSeg;
        return pSeg;
      }
      if (item.ToLower() == "ad")
      {//Angle deflection distance
       //  double dDeflAngle = Angle_2_Radians((string)sCourse.GetValue(1), inDirectionUnits);
        ConvDef.DirectionTypeIn = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth; //temp setting
        double dDeflAngle = AngConv.ConvertToDouble((string)sCourse.GetValue(1), ConvDef);
        //now need to take the previous tangent segment, reverse its orientation and 
        //add +ve clockwise to get the bearing
        var calcLine = EntryTangent; //this is currently in polar radians

        ConvDef.DirectionUnitsIn = ArcGIS.Core.SystemCore.DirectionUnits.Radians; //temp setting
        ConvDef.DirectionTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar; //temp setting
        ConvDef.DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.SouthAzimuth; //temp setting
        double dBear = AngConv.ConvertToDouble(calcLine.Angle, ConvDef) + dDeflAngle;
        ConvDef.DirectionUnitsIn = DirUnits; //set it back
        ConvDef.DirectionTypeIn = DirType; //set it back

        ConvDef.DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth; //set it back

        Coordinate3D pVec1 = new Coordinate3D(FromCoordinate.X, FromCoordinate.Y, 0);
        Coordinate3D pVec2 = new Coordinate3D();
        pVec2.SetPolarComponents(dBear, 0, Convert.ToDouble(sCourse.GetValue(2)) * ScaleFactor);
        Coordinate2D coord2 = new Coordinate2D(pVec1.AddCoordinate3D(pVec2));
        var pSeg = LineBuilderEx.CreateLineSegment(FromCoordinate, coord2);

        ExitTangent = pSeg;
        return pSeg;
      }
      else if ((item.ToLower() == "nc") || (item.ToLower() == "tc"))
      {
        double dChordlength;
        double dChordBearing;
        var pArcSeg = ConstructCurveFromString(inCourse, EntryTangent, DirType, DirUnits, ScaleFactor,
          out dChordlength, out dChordBearing);

        if (pArcSeg == null)
          return null;

        double dRadius = pArcSeg.SemiMajorAxis;
        ArcOrientation ArcOrientation = pArcSeg.IsCounterClockwise ? ArcOrientation.ArcCounterClockwise : ArcOrientation.ArcClockwise;
        MinorOrMajor MinMajor = pArcSeg.IsMinor ? MinorOrMajor.Minor : MinorOrMajor.Major;

        if (item.ToLower() == "nc")
          dChordBearing += DirectionOffsetInRadians; // only correct non-tangent curve, because the tangent curve's previous course was already corrected.

        pArcSeg = EllipticArcBuilderEx.CreateCircularArc(FromCoordinate.ToMapPoint(),
          dChordlength, dChordBearing, dRadius, ArcOrientation, MinMajor);

        try
        {
          ConvDef.DirectionUnitsIn = ArcGIS.Core.SystemCore.DirectionUnits.Radians; //temp setting
          ConvDef.DirectionTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar; //temp setting

          double d90 = pArcSeg.IsCounterClockwise ? -Math.PI / 2 : Math.PI / 2;
          var ExitTangentBearing = AngConv.ConvertToDouble(pArcSeg.EndAngle, ConvDef) + d90;//convert to north az radians
          dChordBearing = AngConv.ConvertToDouble(dChordBearing, ConvDef);

          ConvDef.DirectionUnitsIn = DirUnits; //set it back
          ConvDef.DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.NorthAzimuth; //set it back

          Coordinate3D pVec1 = new Coordinate3D(pArcSeg.StartCoordinate.X, pArcSeg.StartCoordinate.Y, 0);
          Coordinate3D pVec2 = new Coordinate3D();
          pVec2.SetPolarComponents(ExitTangentBearing, 0, 100);
          Coordinate2D coord2 = new Coordinate2D(pVec1.AddCoordinate3D(pVec2));
          ExitTangent = LineBuilderEx.CreateLineSegment(pArcSeg.StartCoordinate, coord2);

          pVec1 = new Coordinate3D(FromCoordinate.X, FromCoordinate.Y, 0);
          pVec2 = new Coordinate3D();
          pVec2.SetPolarComponents(dChordBearing, 0, dChordlength);
          coord2 = new Coordinate2D(pVec1.AddCoordinate3D(pVec2));
          var pSeg = LineBuilderEx.CreateLineSegment(FromCoordinate, coord2);

          outCircularArc = pArcSeg;
          return pSeg;
        }
        catch { }
      }
      return null;
    }
    private static EllipticArcSegment ConstructCurveFromString(string inString, LineSegment ExitTangentFromPreviousCourse,
          ArcGIS.Core.SystemCore.DirectionType DirType, ArcGIS.Core.SystemCore.DirectionUnits DirUnits, double ScaleFactor, out double outChordLength, out double outChordBearing)
    {
      var AngConv = DirectionUnitFormatConversion.Instance;
      var ConvDef = new ConversionDefinition()
      {
        DirectionTypeIn = DirType,
        DirectionUnitsIn = DirUnits,
        DirectionTypeOut = ArcGIS.Core.SystemCore.DirectionType.Polar,
        DirectionUnitsOut = ArcGIS.Core.SystemCore.DirectionUnits.Radians
      };

      EllipticArcSegment ArcSeg = null;

      Coordinate2D pPt = new Coordinate2D() { X = 1000, Y = 1000 };
      //initialize the curve params
      bool bHasRadius = false; double dRadius = -1;
      bool bHasChord = false; double dChord = -1;
      bool bHasArc = false; double dArcLength = -1;
      bool bHasDelta = false; double dDelta = -1;
      bool bCCW = false; //assume curve to right unless proven otherwise
      //now initialize bearing types for non-tangent curves
      bool bHasRadialBearing = false; double dRadialBearing = -1;
      bool bHasChordBearing = false; double dChordBearing = -1;
      bool bHasTangentBearing = false; double dTangentBearing = -1;

      int iItemPosition = 0;
      string[] sCourse = inString.ToLower().Split(' ');
      int UpperBound = sCourse.GetUpperBound(0);
      bool bIsTangentCurve = (((string)sCourse.GetValue(0)).ToLower() == "tc");
      foreach (string item in sCourse)
      {
        if (item == null)
          break;
        if (item == "r" && !bHasRadius && iItemPosition <= 3)
        {// this r is for radius
          dRadius = Convert.ToDouble(sCourse.GetValue(iItemPosition + 1));
          dRadius *= ScaleFactor;
          bHasRadius = true; //found a radius
        }
        if (item == "c" && !bHasChord && iItemPosition <= 3)
        {// this c is for chord length
          dChord = Convert.ToDouble(sCourse.GetValue(iItemPosition + 1));
          dChord *= ScaleFactor;
          bHasChord = true; //found a chord length
        }
        if (item == "a" && !bHasArc && iItemPosition <= 3)
        {// this a is for arc length
          dArcLength = Convert.ToDouble(sCourse.GetValue(iItemPosition + 1));
          dArcLength *= ScaleFactor;
          bHasArc = true; //found an arc length
        }
        if (item == "d" && !bHasDelta && iItemPosition <= 3)
        {// this d is for delta or central angle
          ConvDef.DirectionTypeIn = ArcGIS.Core.SystemCore.DirectionType.Polar;//temp
          dDelta = AngConv.ConvertToDouble((string)sCourse.GetValue(iItemPosition + 1), ConvDef);
          ConvDef.DirectionTypeIn = DirType;//setting it back
          bHasDelta = true; //found a central angle
        }
        if (item == "r" && !bHasRadialBearing && iItemPosition > 3 && iItemPosition < 7 && UpperBound > 5)
        {// this r is for radial bearing
          try
          {
            dRadialBearing = AngConv.ConvertToDouble((string)sCourse.GetValue(iItemPosition + 1), ConvDef);
            bHasRadialBearing = true; //found a radial bearing
          }
          catch
          {
            bHasRadialBearing = true; //found a radial bearing. This will catch case of final R meaning a curve right and not radial bearing
          }
        }
        if (item == "c" && !bHasChordBearing && iItemPosition > 3)
        {// this c is for chord bearing
          dChordBearing = AngConv.ConvertToDouble((string)sCourse.GetValue(iItemPosition + 1), ConvDef);
          bHasChordBearing = true; //found a chord bearing
        }
        if (item == "t" && !bHasTangentBearing && iItemPosition > 3)
        {// this t is for tangent bearing
          dTangentBearing = AngConv.ConvertToDouble((string)sCourse.GetValue(iItemPosition + 1), ConvDef);
          bHasTangentBearing = true; //found a tangent bearing
        }
        if (item == "l")
          // this l is for defining a curve to the left
          bCCW = true;

        iItemPosition++;
      }

      ArcOrientation CCW = bCCW ? ArcOrientation.ArcCounterClockwise : ArcOrientation.ArcClockwise;
      var MapPt1 = MapPointBuilderEx.CreateMapPoint(pPt, null);

      if (!(bIsTangentCurve)) //non-tangent curve
      {//chord bearing
        if (bHasRadius && bHasChord && bHasChordBearing) // A.
        {
          try
          {
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
            dRadius, CCW, MinorOrMajor.Minor, null);
          }
          catch { };
        }

        if (bHasRadius && bHasArc && bHasChordBearing) // B.
        {
          try
          {
            double dCentralAngle = dArcLength / dRadius;
            dChord = 2 * dRadius * Math.Sin(dCentralAngle / 2);
            MinorOrMajor MinMaj = dCentralAngle > Math.PI ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasRadius && bHasDelta && bHasChordBearing) // C.
        {
          try
          {
            dChord = 2 * dRadius * Math.Sin(dDelta / 2);
            MinorOrMajor MinMaj = Math.Abs(dDelta) > Math.PI ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasChord && bHasDelta && bHasChordBearing) // D.
        {
          try
          {
            dRadius = (0.5 * dChord) / Math.Sin(dDelta / 2);
            MinorOrMajor MinMaj = dDelta > Math.PI ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);

          }
          catch { };
        }

        if (bHasChord && bHasArc && bHasChordBearing) // E.
        {
          try
          {
            //Newton's approximation method
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
                dArcLength, CCW, null);
          }
          catch { };
        }

        //tangent bearing
        if (bHasRadius && bHasChord && bHasTangentBearing) // F.
        {
          try
          {
            double dHalfDelta = Math.Abs(Math.Asin(dChord / (2 * dRadius)));
            //get chord bearing from given tangent bearing in polar radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dTangentBearing + dHalfDelta;
            else
              dChordBearing = dTangentBearing - dHalfDelta;
            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasRadius && bHasArc && bHasTangentBearing) // G.
        {
          try
          {
            double dHalfDelta = (dArcLength / dRadius) / 2;
            dChord = 2 * dRadius * Math.Sin(dHalfDelta);
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dTangentBearing + dHalfDelta;
            else
              dChordBearing = dTangentBearing - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasRadius && bHasDelta && bHasTangentBearing) // H.
        {
          try
          {
            double dHalfDelta = (dDelta) / 2;
            dChord = 2 * dRadius * Math.Sin(dHalfDelta);
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dTangentBearing + dHalfDelta;
            else
              dChordBearing = dTangentBearing - dHalfDelta;

            MinorOrMajor MinMaj = dDelta > Math.PI ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasChord && bHasDelta && bHasTangentBearing) // I.
        {
          try
          {
            double dHalfDelta = (dDelta) / 2;
            dRadius = dChord / (2 * Math.Sin(dHalfDelta));
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dTangentBearing + dHalfDelta;
            else
              dChordBearing = dTangentBearing - dHalfDelta;

            MinorOrMajor MinMaj = dDelta > Math.PI ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasChord && bHasArc && bHasTangentBearing)
        {
          try
          {
            //Newton's approximation method. First computation uses a 0 bearing just to get the initial curve geometry, and central angle:
            //the computed central angle is then used to apply to the tangent bearing
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, 0,
              dArcLength, CCW, null);
            //Get the Delta central angle from the approximation, and then ...
            double dHalfDelta = Math.Abs(ArcSeg.CentralAngle) / 2.0;
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dTangentBearing + dHalfDelta;
            else
              dChordBearing = dTangentBearing - dHalfDelta;

            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dArcLength, CCW, null);
          }
          catch { };
        }

        if (bHasRadius && bHasChord && bHasRadialBearing)
        {
          try
          {
            double dHalfDelta = Math.Abs(Math.Asin(dChord / (2.0 * dRadius)));

            //get chord bearing from given radial bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dRadialBearing - Math.PI / 2.0 + dHalfDelta;
            else
              dChordBearing = dRadialBearing + Math.PI / 2.0 - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasRadius && bHasArc && bHasRadialBearing)
        {
          try
          {
            double dHalfDelta = (dArcLength / dRadius) / 2.0;
            dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dRadialBearing - Math.PI / 2.0 + dHalfDelta;
            else
              dChordBearing = dRadialBearing + Math.PI / 2.0 - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasRadius && bHasDelta && bHasRadialBearing)
        {
          try
          {
            double dHalfDelta = (dDelta) / 2.0;
            dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dRadialBearing - Math.PI / 2.0 + dHalfDelta;
            else
              dChordBearing = dRadialBearing + Math.PI / 2.0 - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasChord && bHasDelta && bHasRadialBearing)
        {
          try
          {
            double dHalfDelta = (dDelta) / 2.0;
            dRadius = dChord / (2.0 * Math.Sin(dHalfDelta));
            //get chord bearing from given radial bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dRadialBearing - Math.PI / 2.0 + dHalfDelta;
            else
              dChordBearing = dRadialBearing + Math.PI / 2.0 - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasChord && bHasArc && bHasRadialBearing)
        {
          try
          {
            //Newton's approximation method. First computation uses a 0 bearing just to get the initial curve geometry, and central angle:
            //the computed central angle is then used to apply to the radial bearing from the previous course
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, 0.0,
              dArcLength, CCW, null);
            //Get the Delta central angle from the approximation, and then ...
            double dHalfDelta = Math.Abs(ArcSeg.CentralAngle) / 2.0;
            //get chord bearing from given radial bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dRadialBearing - Math.PI / 2.0 + dHalfDelta;
            else
              dChordBearing = dRadialBearing + Math.PI / 2.0 - dHalfDelta;

            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dArcLength, CCW, null);

          }
          catch { };
        }

        if (bHasDelta && bHasArc && bHasChordBearing)
        {
          try
          {
            //get the radius and chord from the Central Angle and Arc Length
            dRadius = dArcLength / dDelta;
            double dHalfDelta = dDelta / 2.0;
            dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);

          }
          catch { };
        }

        if (bHasDelta && bHasArc && bHasTangentBearing)
        {
          try
          {
            //get the radius and chord from the Central Angle and Arc Length
            dRadius = dArcLength / dDelta;
            double dHalfDelta = dDelta / 2.0;
            dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dTangentBearing + dHalfDelta;
            else
              dChordBearing = dTangentBearing - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);

          }
          catch { };
        }

        if (bHasDelta && bHasArc && bHasRadialBearing)
        {
          try
          {
            //get the radius and chord from the Central Angle and Arc Length
            dRadius = dArcLength / dDelta;
            double dHalfDelta = dDelta / 2.0;
            dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
            //get chord bearing from given radial bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = dRadialBearing - Math.PI / 2.0 + dHalfDelta;
            else
              dChordBearing = dRadialBearing + Math.PI / 2.0 - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);

          }
          catch { };
        }
      }
      else
      { //tangent curve
        if (bHasRadius && bHasChord)
        {
          try
          {
            double dHalfDelta = Math.Abs(Math.Asin(dChord / (2.0 * dRadius)));
            //get chord bearing from given tangent bearing in polar radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = ExitTangentFromPreviousCourse.Angle + dHalfDelta;
            else
              dChordBearing = ExitTangentFromPreviousCourse.Angle - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasRadius && bHasArc)
        {
          try
          {
            double dHalfDelta = (dArcLength / dRadius) / 2.0;
            dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = ExitTangentFromPreviousCourse.Angle + dHalfDelta;
            else
              dChordBearing = ExitTangentFromPreviousCourse.Angle - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasRadius && bHasDelta)
        {
          try
          {
            double dHalfDelta = dDelta / 2.0;
            dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = ExitTangentFromPreviousCourse.Angle + dHalfDelta;
            else
              dChordBearing = ExitTangentFromPreviousCourse.Angle - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasChord && bHasDelta)
        {
          try
          {
            double dHalfDelta = (dDelta) / 2.0;
            dRadius = dChord / (2.0 * Math.Sin(dHalfDelta));
            //get chord bearing from given tangent bearing in north azimuth radians
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = ExitTangentFromPreviousCourse.Angle + dHalfDelta;
            else
              dChordBearing = ExitTangentFromPreviousCourse.Angle - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

        if (bHasChord && bHasArc)
        {
          try
          {
            //Newton's approximation method. First computation uses a 0 bearing just to get the initial curve geometry, and central angle:
            //the computed central angle is then used to apply to the exit tangent from the previous course
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, 0.0,
              dArcLength, CCW, null);
            //Get the Delta central angle from the approximation, and then ...
            double dHalfDelta = Math.Abs(ArcSeg.CentralAngle) / 2.0;
            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = ExitTangentFromPreviousCourse.Angle + dHalfDelta;
            else
              dChordBearing = ExitTangentFromPreviousCourse.Angle - dHalfDelta;

            //recompute here...
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dArcLength, CCW, null);
          }
          catch { };
        }

        if (bHasDelta && bHasArc)
        {
          try
          {
            //get the radius and chord from the Central Angle and Arc Length
            dRadius = dArcLength / dDelta;
            double dHalfDelta = dDelta / 2.0;
            dChord = 2.0 * dRadius * Math.Sin(dHalfDelta);

            if (CCW == ArcOrientation.ArcCounterClockwise)
              dChordBearing = ExitTangentFromPreviousCourse.Angle + dHalfDelta;
            else
              dChordBearing = ExitTangentFromPreviousCourse.Angle - dHalfDelta;

            MinorOrMajor MinMaj = dHalfDelta > Math.PI / 2.0 ? MinorOrMajor.Major : MinorOrMajor.Minor;
            ArcSeg = EllipticArcBuilderEx.CreateCircularArc(MapPt1, dChord, dChordBearing,
              dRadius, CCW, MinMaj, null);
          }
          catch { };
        }

      }
      try
      {
        var pSeg = LineBuilderEx.CreateLineSegment(ArcSeg.StartPoint, ArcSeg.EndPoint);
        outChordLength = pSeg.Length;
        outChordBearing = pSeg.Angle;
      }
      catch
      {
        outChordLength = -1; outChordBearing = -1;
        return null;
      }

      return ArcSeg;
    }
    #endregion

  }
}
