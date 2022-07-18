using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.UnitFormats;
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
using System.Windows;

namespace COGOLineFeatures
{
  internal class CreateCOGOLine : MapTool
  {
    public CreateCOGOLine()
    {
      IsSketchTool = true;
      UseSnapping = true;
      // The type of construction tool.  
       SketchType = SketchGeometryType.Line;
      //Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
      UsesCurrentTemplate = true;
      //Gets or sets whether the tool supports firing sketch events when the map sketch changes. 
      //Default value is false.
      FireSketchEvents = true;
    }
    internal static string _COGODirectionString;
    internal static double _COGODirection;
    internal static string _COGODirectionFormatString; //example: quadrant bearing
    internal static string _COGODistanceString;
    internal static string _COGODistanceUnitString;

    internal static string _COGOCircularArcDirectionString;
    internal static string _COGOCircularArcDirectionFormatString;
    internal static string _COGOCircularArcRadiusUnitString;
    internal static string _COGOParameter1String;
    internal static string _COGOParameter2String;

    internal static string _StringTangentDirection = "Tangent Direction";
    internal static string _StringChordDirection = "Chord Direction";
    internal static string _StringRadialDirection = "Radial Direction";
    internal static string _StringRadius = "Radius";
    internal static string _StringArcLength= "Arc Length";
    internal static string _StringChordLength = "Chord Length";
    internal static string _StringDeltaAngle= "Delta Angle";

    private COGOLineViewModel _VM = new COGOLineViewModel();
    private COGOCircularArcViewModel _circVM = new COGOCircularArcViewModel();
    private DisplayUnitFormat _inputDialogDirectionUnit = null;
    private DisplayUnitFormat _inputDialogDistanceUnit = null;
    private DisplayUnitFormat _inputDialogAngleUnit = null;
    private double _metersPerUnitDataset = 1.0;
    private double _metersPerUnitDataEntry = 1.0;
    private double _scaleFactor = 1.0;
    private bool _scaleFactorActive = false;
    private double _directionOffsetCorrection = 0.0;
    private bool _offsetCorrectionActive = false;
    private bool _isCOGOEnabled = false;
    private bool _isCircularArc = false;
    private bool _isControlledByFabric = false;
    private string _shapeFldName = "Shape";

    /// <summary>
    /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch.</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>

    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (CurrentTemplate == null || geometry == null)
        return Task.FromResult(false);

      var newCOGOAttributes = new Dictionary<string, object>();
      var COGOLineGeom = geometry as Polyline;
      ICollection<Segment> LineSegments = new List<Segment>();
      COGOLineGeom.GetAllSegments(ref LineSegments);

      if(LineSegments.Count() > 1) //multi-segment sketch
        return Task.FromResult(false);

      #region Circular arc
      if (_isCircularArc)
      {
        if (_circVM.COGOCircularArc.CircularArcDirection == null)
          _circVM.COGOCircularArc.CircularArcDirection = "";

        var bNullEntryParameter = _circVM.COGOCircularArc.CircularArcDirection.Trim() == "" |
        _circVM.COGOCircularArc.Parameter1.Trim() == "" |
        _circVM.COGOCircularArc.Parameter2.Trim() == "";

        var enteredDirectionInPolarRadians =
          COGOUtils.ConvertToPolarDirectionRadians(_inputDialogDirectionUnit,
          _circVM.COGOCircularArc.CircularArcDirection);

        var enteredRadiusValueInDatasetUnits = 0.0;
        if (_circVM.COGOCircularArc.Parameter1Type == _StringRadius)
        {
          if (!Double.TryParse(_circVM.COGOCircularArc.Parameter1, out double dRadiusDoubleFromString))
            dRadiusDoubleFromString = 0.0;

          var enteredRadiusValueInMeters = dRadiusDoubleFromString * _metersPerUnitDataEntry;
          enteredRadiusValueInDatasetUnits = enteredRadiusValueInMeters / _metersPerUnitDataset;
        }

        object enteredArcLengthValueInDatasetUnits = null;
        if (_circVM.COGOCircularArc.Parameter2Type == _StringArcLength)
        {
          if (!Double.TryParse(_circVM.COGOCircularArc.Parameter2, out double dArcLengthDoubleFromString))
            dArcLengthDoubleFromString = 0.0;

          double enteredArcLengthValueInMeters = dArcLengthDoubleFromString * _metersPerUnitDataEntry;
          enteredArcLengthValueInDatasetUnits = enteredArcLengthValueInMeters / _metersPerUnitDataset;
        }

        object enteredChordLengthValueInDatasetUnits = null;
        if (_circVM.COGOCircularArc.Parameter2Type == _StringChordLength)
        {
          if (!Double.TryParse(_circVM.COGOCircularArc.Parameter2, out double dChordLengthDoubleFromString))
            dChordLengthDoubleFromString = 0.0;

          var enteredChordLengthValueInMeters = dChordLengthDoubleFromString * _metersPerUnitDataEntry;
          enteredChordLengthValueInDatasetUnits = enteredChordLengthValueInMeters / _metersPerUnitDataset;
        }

        object enteredDeltaAngleInRadians = null;
        if (_circVM.COGOCircularArc.Parameter2Type == _StringDeltaAngle)
          enteredDeltaAngleInRadians = COGOUtils.ConvertToAngleInRadians(_inputDialogAngleUnit,
            _circVM.COGOCircularArc.Parameter2);

        if (!(LineSegments.FirstOrDefault() is EllipticArcSegment circArcGeom))
          return Task.FromResult(false);

        var isCounterClockwise = _circVM.COGOCircularArc.SelectedSide == 0;
        var directionType = _circVM.COGOCircularArc.CircularArcDirectionType;
        var mapPoint = circArcGeom.StartPoint;

        var sideChange = isCounterClockwise != circArcGeom.IsCounterClockwise;

        //compute the circular arc first, then get back the equivalent for radius/arclength/chord direction
        //to use for COGO attributes
        EllipticArcSegment newCircArcForCOGO = COGOUtils.CreateCircularArcFromParams(mapPoint, enteredDirectionInPolarRadians,
                    directionType, enteredRadiusValueInDatasetUnits, isCounterClockwise,
                    enteredChordLengthValueInDatasetUnits, enteredDeltaAngleInRadians, enteredArcLengthValueInDatasetUnits);
        if (newCircArcForCOGO == null)
          return Task.FromResult(false);
        var northAzimuthChordDirectionAttribute =
          COGOUtils.InverseDirectionAsNorthAzimuth(newCircArcForCOGO.StartCoordinate, newCircArcForCOGO.EndCoordinate, false);
        var radiusAttribute = newCircArcForCOGO.IsCounterClockwise ? -newCircArcForCOGO.SemiMajorAxis : newCircArcForCOGO.SemiMajorAxis;
        var arcLengthAttribute = Math.Abs(newCircArcForCOGO.Length);

        //updating circular arc geometry
        EllipticArcSegment newCircArcForGeometry;

        if (_circVM.COGOCircularArc.EndPointFixed)
        {//try to make the curve params fit between the two fixed points, if it fails returns null and bails
          newCircArcForGeometry = COGOUtils.CreateCircularArcByEndpoints(mapPoint, circArcGeom.EndPoint,
            enteredRadiusValueInDatasetUnits, isCounterClockwise, !newCircArcForCOGO.IsMinor, _scaleFactor);
          if (newCircArcForGeometry == null)
            return Task.FromResult(false);
        }
        else
        {
          newCircArcForGeometry = COGOUtils.CreateCircularArcFromParams(mapPoint, enteredDirectionInPolarRadians,
                  directionType, enteredRadiusValueInDatasetUnits, isCounterClockwise,
                  enteredChordLengthValueInDatasetUnits, enteredDeltaAngleInRadians, 
                  enteredArcLengthValueInDatasetUnits, _scaleFactor, _directionOffsetCorrection * Math.PI / 180.0);
        }

        if (_circVM.COGOCircularArc.EndPointFixed && !sideChange)
          newCOGOAttributes.Add(_shapeFldName, geometry);
        else
        {
          var newPolyline = PolylineBuilderEx.CreatePolyline(newCircArcForGeometry);
          newCOGOAttributes.Add(_shapeFldName, newPolyline);
        }
        if (_isCOGOEnabled)
        {
          if (!bNullEntryParameter)
          {
            newCOGOAttributes.Add("Direction", northAzimuthChordDirectionAttribute);
            newCOGOAttributes.Add("Radius", radiusAttribute);
            newCOGOAttributes.Add("ArcLength", arcLengthAttribute);
          }
        }
      }
      #endregion
      #region Straight line
      else
      { //straight line
        var bDirectionIsNull = _VM.COGOLine.Direction.Trim() == "";
        var bDistanceIsNull = _VM.COGOLine.Distance.Trim() == "";
        var enteredDirectionInNorthAzimuthDegrees =
            COGOUtils.ConvertToNorthAzimuthDecimalDegrees(_inputDialogDirectionUnit, _VM.COGOLine.Direction);

        if (!Double.TryParse(_VM.COGOLine.Distance, out double dDistanceDoubleFromString))
          dDistanceDoubleFromString = 0.0;

        var enteredDistanceValueInMeters = dDistanceDoubleFromString * _metersPerUnitDataEntry;
        var enteredDistanceValueInDatasetUnits = enteredDistanceValueInMeters / _metersPerUnitDataset;

        var theSegment = LineSegments.FirstOrDefault();

        if (theSegment == null)
          return Task.FromResult(false);

        var t = Math.Abs(enteredDirectionInNorthAzimuthDegrees - _COGODirection) % 360.0; //fMOD in C++
        var delta = 180.0 - Math.Abs(t - 180.0);
        //if geometry and entered values are different by more than a whole 90° quadrant:
        var bReversedGeometryOnly = delta >= 90.0;
        var bReverseGeometryAndAttributes = enteredDistanceValueInDatasetUnits < 0.0;
        enteredDistanceValueInDatasetUnits = Math.Abs(enteredDistanceValueInDatasetUnits);
        if (bReverseGeometryAndAttributes && !bReversedGeometryOnly)
        {
          enteredDirectionInNorthAzimuthDegrees -= 180.0;
          //account for negative directions
          if (enteredDirectionInNorthAzimuthDegrees < 0.0)
          {
            enteredDirectionInNorthAzimuthDegrees =
              COGOUtils.ConvertFromNegativeNorthAzimuthDecimalDegrees(enteredDirectionInNorthAzimuthDegrees);
          }
        }

        if (_VM.COGOLine.EndPointFixed)
        {
          if (bReverseGeometryAndAttributes || bReversedGeometryOnly)
          {
            var newSegment = LineBuilderEx.CreateLineSegment(theSegment.EndCoordinate,
              theSegment.StartCoordinate, geometry.SpatialReference); //switch start and end points
            var newPolyline = PolylineBuilderEx.CreatePolyline(newSegment, geometry.SpatialReference);
            newCOGOAttributes.Add(_shapeFldName, newPolyline);
          }
          else
            newCOGOAttributes.Add(_shapeFldName, geometry);
        }
        else
        {//the end point is not held fixed so re-compute it
         //updating line geometry
          double dCorrectedNorthAzimuthDegrees = enteredDirectionInNorthAzimuthDegrees - _directionOffsetCorrection;
          if (bDirectionIsNull) //use the geometry direction as a fall-back
            dCorrectedNorthAzimuthDegrees =
              COGOUtils.InverseDirectionAsNorthAzimuth(theSegment.StartCoordinate,
                    theSegment.EndCoordinate, bReverseGeometryAndAttributes);

          double dCorrectedDistanceValue = enteredDistanceValueInDatasetUnits * _scaleFactor;
          if (bDistanceIsNull) //use the geometry distance as a fall-back
            dCorrectedDistanceValue = theSegment.Length;

          var pEndCoordXY =
            COGOUtils.PointInDirection(theSegment.StartCoordinate, dCorrectedNorthAzimuthDegrees,
                  dCorrectedDistanceValue);
          var newSegment = LineBuilderEx.CreateLineSegment(theSegment.StartCoordinate, pEndCoordXY, geometry.SpatialReference);
          var newPolyline = PolylineBuilderEx.CreatePolyline(newSegment, geometry.SpatialReference);
          newCOGOAttributes.Add(_shapeFldName, newPolyline);
        }
 
        //---------------------------------------------------------
        //if it's not a COGO enabled line, then don't write to the COGO fields.
        if (_isCOGOEnabled)
        {
          if (!bDirectionIsNull)
            newCOGOAttributes.Add("Direction", enteredDirectionInNorthAzimuthDegrees);
          if (!bDistanceIsNull)
            newCOGOAttributes.Add("Distance", enteredDistanceValueInDatasetUnits);
        }

        if (_isControlledByFabric)
        {
          newCOGOAttributes.Add("COGOType", 1); // always considered to be 'Entered'
          if (_scaleFactorActive && !_VM.COGOLine.EndPointFixed && !bDistanceIsNull)
          {
            newCOGOAttributes.Add("Scale", _scaleFactor);
            newCOGOAttributes.Add("IsCOGOGround", 1);
          }
          if (_offsetCorrectionActive && !_VM.COGOLine.EndPointFixed && !bDirectionIsNull)
            newCOGOAttributes.Add("Rotation", _directionOffsetCorrection);
        }
      }
      #endregion
      // Create an edit operation
      var createOperation = new EditOperation();
      createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
      createOperation.SelectNewFeatures = true;

      // Queue feature creation
      createOperation.Create(CurrentTemplate.MapMember, newCOGOAttributes);

      COGOLineDialog.Default["LastUsedParams"] = _circVM.COGOCircularArc.CircularArcDirectionType + "|" +
        _circVM.COGOCircularArc.Parameter1Type + "|" + _circVM.COGOCircularArc.Parameter2Type;
      COGOLineDialog.Default.Save(); //comment out if you only want to save settings within each app session

      return createOperation.ExecuteAsync();
    }

    private bool _Executing = false; //used to handle re-entrance use cases with 3-click circular arc creation
    protected override async Task<bool> OnSketchModifiedAsync()
    {
      if (_Executing)//re-entrance use case with 3-click circular arc creation
        return true;
      _Executing = true;
      
      var COGOLineGeom = await GetCurrentSketchAsync() as Polyline;
      if (COGOLineGeom == null)
      {
        _Executing = false;
        return true;
      }
      if (COGOLineGeom.PointCount != 2)
      {
        _Executing = false;
        return true;
      }

      // The second sketch vertex is present
      #region Get backstage units and map's ground to grid correction
      _scaleFactor = 1.0;
      _directionOffsetCorrection = 0.0;
      await QueuedTask.Run(() =>
      {
        //get the distance unit from the backstage default settings
        _inputDialogDistanceUnit = 
          DisplayUnitFormats.Instance.GetDefaultProjectUnitFormat(UnitFormatType.Distance);
        _metersPerUnitDataEntry = _inputDialogDistanceUnit.MeasurementUnit.ConversionFactor;
        _COGODistanceUnitString = _inputDialogDistanceUnit.Abbreviation;       

        //get the direction format and units from the backstage default settings
        _inputDialogDirectionUnit = 
          DisplayUnitFormats.Instance.GetDefaultProjectUnitFormat(UnitFormatType.Direction);
        _COGODirectionFormatString = _inputDialogDirectionUnit.DisplayName;

        //get the angle units from the backstage default settings
        _inputDialogAngleUnit =
          DisplayUnitFormats.Instance.GetDefaultProjectUnitFormat(UnitFormatType.Angular);

        //get the ground to grid corrections
        var mapView = MapView.Active;
        var cimMap = mapView?.Map?.GetDefinition();
        var cimGroundToGridCorrection = cimMap?.GroundToGridCorrection;
        if (cimGroundToGridCorrection != null)
        {
          _scaleFactor = cimGroundToGridCorrection.GetConstantScaleFactor();
          _scaleFactorActive = cimGroundToGridCorrection.IsCorrecting() && cimGroundToGridCorrection.UseScale;
          _directionOffsetCorrection = cimGroundToGridCorrection.GetDirectionOffset();
          _offsetCorrectionActive = cimGroundToGridCorrection.IsCorrecting() && cimGroundToGridCorrection.UseDirection;
        }
      });
      #endregion

      #region Collect parameters from dialog
      ICollection<Segment> LineSegments = new List<Segment>();
      COGOLineGeom.GetAllSegments(ref LineSegments);

      Segment theSegment = LineSegments.FirstOrDefault();
      _isCircularArc = theSegment.SegmentType == SegmentType.EllipticArc;
      if (theSegment == null || theSegment.SegmentType == SegmentType.Bezier)
      {
        await base.ClearSketchAsync();
        _Executing = false;
        return true;
      }

      var dispUnitFormat = _inputDialogDistanceUnit.UnitFormat as CIMNumericFormatBase;
      int iRounding = 5;
      if (dispUnitFormat.RoundingOption == esriRoundingOptionEnum.esriRoundNumberOfDecimals)
        iRounding = dispUnitFormat.RoundingValue;
      string sFormat = new String('0', iRounding);
      sFormat = "0." + sFormat;

      if (_isCircularArc)
      {
        var COGOCircArcDlg = new COGOCircularArcInput();
        COGOCircArcDlg.Owner = FrameworkApplication.Current.MainWindow;
        COGOCircArcDlg.CircularArcSegment = theSegment as EllipticArcSegment;
        COGOCircArcDlg.BackstageDirectionUnit = _inputDialogDirectionUnit;
        COGOCircArcDlg.BackstageAngleUnit = _inputDialogAngleUnit;
        COGOCircArcDlg.BackstageDistanceUnit = _inputDialogDistanceUnit;
        COGOCircArcDlg.DirectionOffsetCorrection = _directionOffsetCorrection;
        COGOCircArcDlg.DistanceScaleFactor = _scaleFactor;

        COGOCircArcDlg.DataContext = _circVM;
        
        var COGOCircularArcSegment = theSegment as EllipticArcSegment;
        var chordLine = LineBuilderEx.CreateLineSegment(theSegment.StartCoordinate, theSegment.EndCoordinate);

        double dGeometryChordLengthInMeters = chordLine.Length * _metersPerUnitDataset;
        double dCOGOChordLength = dGeometryChordLengthInMeters / _metersPerUnitDataEntry / _scaleFactor;

        double dGeometryArcLengthInMeters = COGOCircularArcSegment.Length * _metersPerUnitDataset;
        double dCOGOArcLength = dGeometryArcLengthInMeters / _metersPerUnitDataEntry / _scaleFactor;

        double dGeometryRadiusInMeters = COGOCircularArcSegment.SemiMajorAxis * _metersPerUnitDataset;
        double dCOGORadius = dGeometryRadiusInMeters / _metersPerUnitDataEntry / _scaleFactor;

        double dGeometryDeltaAngleInRadians = COGOCircularArcSegment.CentralAngle;
        double dCOGODeltaAngle = dGeometryDeltaAngleInRadians;

        ArcOrientation CCW = COGOCircularArcSegment.IsCounterClockwise ?
          ArcOrientation.ArcCounterClockwise : ArcOrientation.ArcClockwise;

        MinorOrMajor MinMaj = COGOCircularArcSegment.IsMinor ?
          MinorOrMajor.Minor : MinorOrMajor.Major;

        _COGOCircularArcDirectionFormatString = _inputDialogDirectionUnit.DisplayName;
        _circVM.COGOCircularArc.CircularArcDirectionFormat = _COGOCircularArcDirectionFormatString;

        _COGOCircularArcRadiusUnitString = _inputDialogDistanceUnit.DisplayNamePlural;
        _circVM.COGOCircularArc.CircularArcRadiusUnit = _COGOCircularArcRadiusUnitString;

        if (_circVM.COGOCircularArc.CircularArcDirectionType == _StringTangentDirection)
          _COGOCircularArcDirectionString =
            COGOUtils.TangentDirectionFromCircularArc(COGOCircularArcSegment, _inputDialogDirectionUnit, _directionOffsetCorrection * Math.PI / 180.0);

        if (_circVM.COGOCircularArc.CircularArcDirectionType == _StringChordDirection)
          _COGOCircularArcDirectionString = COGOUtils.ChordDirectionFromCircularArc
            (COGOCircularArcSegment, _inputDialogDirectionUnit, _directionOffsetCorrection * Math.PI / 180.0);

        if (_circVM.COGOCircularArc.CircularArcDirectionType == _StringRadialDirection)
          _COGOCircularArcDirectionString = COGOUtils.RadialDirectionFromCircularArc
            (COGOCircularArcSegment, _inputDialogDirectionUnit, _directionOffsetCorrection * Math.PI / 180.0);

        double dChordDirectionInNAz =
          COGOUtils.InverseDirectionAsNorthAzimuth(COGOCircularArcSegment.StartCoordinate,
            COGOCircularArcSegment.EndCoordinate, false);

        double dDeltaAngleInRadians = dCOGOArcLength / dCOGORadius;

        _COGOParameter1String = dCOGORadius.ToString(sFormat);

        if(_circVM.COGOCircularArc.Parameter2Type=="Arc Length")
          _COGOParameter2String = dCOGOArcLength.ToString(sFormat);
        if (_circVM.COGOCircularArc.Parameter2Type == "Chord Length")
          _COGOParameter2String = dCOGOChordLength.ToString(sFormat);
        if (_circVM.COGOCircularArc.Parameter2Type == "Delta Angle")
          _COGOParameter2String =
            COGOUtils.ConvertAngleInRadiansToAngleStringFormat(dDeltaAngleInRadians, _inputDialogAngleUnit, true);

        _circVM.COGOCircularArc.CircularArcDirection = _COGOCircularArcDirectionString;
        _circVM.COGOCircularArc.Parameter1 = _COGOParameter1String;
        _circVM.COGOCircularArc.Parameter2 = _COGOParameter2String;
        _circVM.COGOCircularArc.Side[0] = COGOCircularArcSegment.IsCounterClockwise;
        _circVM.COGOCircularArc.Side[1] = !COGOCircularArcSegment.IsCounterClockwise;
        if (COGOCircArcDlg.ShowDialog() == true)
        {
          await base.FinishSketchAsync();
        }
        else
        {
          await base.ClearSketchAsync();
        }
      }
      else
      { //straight line
        var COGOLineDlg = new COGOLineInput();
        COGOLineDlg.Owner = FrameworkApplication.Current.MainWindow;
        COGOLineDlg.DataContext = _VM;

        var COGOLineSegment = theSegment as LineSegment;

        var dGeometryDistanceInMeters = COGOLineSegment.Length * _metersPerUnitDataset;
        var dCOGODistance = dGeometryDistanceInMeters / _metersPerUnitDataEntry / _scaleFactor;

        _COGODistanceString = dCOGODistance.ToString(sFormat);
        _VM.COGOLine.Distance = _COGODistanceString;
        _VM.COGOLine.DistanceUnit = _COGODistanceUnitString;
        var dGeometryPolarRadiansDirection = COGOLineSegment.Angle;
        dGeometryPolarRadiansDirection -= _directionOffsetCorrection * Math.PI / 180.0;

        _COGODirection = COGOUtils.PolarRadiansToNorthAzimuthDecimalDegrees(dGeometryPolarRadiansDirection);
        _COGODirectionString =
          COGOUtils.ConvertPolarCartesianRadiansToDirectionFormat(dGeometryPolarRadiansDirection,
                _inputDialogDirectionUnit, true);
        _VM.COGOLine.Direction = _COGODirectionString;
        _VM.COGOLine.DirectionFormat = _COGODirectionFormatString;

        if (COGOLineDlg.ShowDialog() == true)
        {
          await base.FinishSketchAsync();
        }
        else
          await base.ClearSketchAsync();
      }
      #endregion
      _Executing = false;
        return true;
    }

    protected override Task OnToolActivateAsync(bool hasMapViewChanged)
    {
      return QueuedTask.Run(() =>
      {
        var featLayer = base.CurrentTemplate.Layer as FeatureLayer;
        FeatureClassDefinition fcDefinition =
          featLayer.GetFeatureClass().GetDefinition();
        if (fcDefinition == null)
          return;
        _isCOGOEnabled = fcDefinition.IsCOGOEnabled();
        if (fcDefinition.GetSpatialReference()?.IsProjected ?? false) 
          _metersPerUnitDataset = fcDefinition.GetSpatialReference()?.Unit?.ConversionFactor ?? 1;
        _shapeFldName = fcDefinition.GetShapeField();

        // Check if a layer has a parcel fabric source
        _isControlledByFabric = 
             featLayer.IsControlledByParcelFabricAsync(ParcelFabricType.ParcelFabric).Result;

        // adjust the sketch symbol
        var symbolReference = base.SketchSymbol;
        if (symbolReference == null)
        {
          var cimLineSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.BlueRGB, 3,
              SimpleLineStyle.Solid);
          base.SketchSymbol = cimLineSymbol.MakeSymbolReference();
        }
        else
        {
          symbolReference.Symbol.SetColor(ColorFactory.Instance.BlueRGB);
          base.SketchSymbol = symbolReference;
        }
      });
    }
  }
}
