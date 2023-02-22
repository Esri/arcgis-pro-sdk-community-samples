//   Copyright 2023 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
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
using ArcGIS.Desktop.Mapping.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TransformationsControl
{
  internal class TransformationsControlDockpaneViewModel : DockPane
  {
    private const string _dockPaneID = "TransformationsControl_TransformationsControlDockpane";

    protected TransformationsControlDockpaneViewModel() 
    {
      IsConfigureEmpty = true;
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    #region Properties

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Transformations Control";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    private bool _isConfigureEmpty = false;
    public bool IsConfigureEmpty
    {
      get => _isConfigureEmpty;
      set
      {
        SetProperty(ref _isConfigureEmpty, value);
        if (value)
        {
          IsConfigureSourceSR = false;
          IsConfigureFixedSR = false;
          IsConfigureSelectedTransformation = false;
          IsConfigureFixedSRExtent = false;
          IsConfigureMultipleTransformation = false;

          Configure();
        }
      }
    }
    private bool _isConfigureSourceSR = false;
    public bool IsConfigureSourceSR
    {
      get => _isConfigureSourceSR;
      set
      {
        SetProperty(ref _isConfigureSourceSR, value);
        if (value)
        {
          IsConfigureEmpty = false;
          IsConfigureFixedSR = false;
          IsConfigureSelectedTransformation = false;
          IsConfigureFixedSRExtent = false;
          IsConfigureMultipleTransformation = false;

          Configure();
        }
      }
    }
    private bool _isConfigureFixedSR = false;
    public bool IsConfigureFixedSR
    {
      get => _isConfigureFixedSR;
      set
      {
        SetProperty(ref _isConfigureFixedSR, value);
        if (value)
        {
          IsConfigureEmpty = false;
          IsConfigureSourceSR = false;
          IsConfigureSelectedTransformation = false;
          IsConfigureFixedSRExtent = false;
          IsConfigureMultipleTransformation = false;

          Configure();
        }
      }
    }
    private bool _isConfigureSelectedTransformation = false;
    public bool IsConfigureSelectedTransformation
    {
      get => _isConfigureSelectedTransformation;
      set
      {
        SetProperty(ref _isConfigureSelectedTransformation, value);
        if (value)
        {
          IsConfigureEmpty = false;
          IsConfigureSourceSR = false;
          IsConfigureFixedSR = false;
          IsConfigureFixedSRExtent = false;
          IsConfigureMultipleTransformation = false;

          Configure();
        }
      }
    }

    private bool _isConfigureFixedSRExtent = false;
    public bool IsConfigureFixedSRExtent
    {
      get => _isConfigureFixedSRExtent;
      set
      {
        SetProperty(ref _isConfigureFixedSRExtent, value);
        if (value)
        {
          IsConfigureEmpty = false;
          IsConfigureSourceSR = false;
          IsConfigureFixedSR = false;
          IsConfigureSelectedTransformation = false;
          IsConfigureMultipleTransformation = false;

          Configure();
        }
      }
    }

    private bool _isConfigureMultipleTransformation = false;
    public bool IsConfigureMultipleTransformation
    {
      get => _isConfigureMultipleTransformation;
      set
      {
        SetProperty(ref _isConfigureMultipleTransformation, value);
        if (value)
        {
          IsConfigureEmpty = false;
          IsConfigureSourceSR = false;
          IsConfigureFixedSR = false;
          IsConfigureSelectedTransformation = false;
          IsConfigureFixedSRExtent = false;

          Configure();
        }
      }
    }

    private string _projectionResults;
    public string ProjectionResults
    {
      get => _projectionResults;
      set => SetProperty(ref _projectionResults, value);
    }

    #endregion

    private void Configure()
    {
      // clear results
      ProjectionResults = "";

      if (IsConfigureEmpty)
        PrepareTransformationProperties_ChooseSRs_Empty();
      else if (IsConfigureSourceSR)
        PrepareTransformationProperties_ChooseSRs();
      else if (IsConfigureFixedSR)
        PrepareTransformationProperties_PresetSRs();
      else if (IsConfigureSelectedTransformation)
        PrepareTransformationProperties_PresetTransformation();
      else if (IsConfigureFixedSRExtent)
        PrepareTransformationProperties_PresetSRsExtent();
      else if (IsConfigureMultipleTransformation)
        PrepareTransformationProperties_Multiple();
    }

    #region Transformations Control 

    private TransformationsControlProperties _tProps = null;
    public TransformationsControlProperties TControlProperties
    {
      get => _tProps;
      set => SetProperty(ref _tProps, value);
    }

    private DatumTransformation _dt = null;
    public DatumTransformation SelectedTransformation
    {
      get => _dt;
      set => SetProperty(ref _dt, value);
    }

    #region Tranformation Control configuration
    private void PrepareTransformationProperties_ChooseSRs_Empty()
    {
      var tInfos = new List<TransformationInfo>();

      var props = new TransformationsControlProperties()
      {
        CanEditCoordinateSystems = true,    // allow choice of SR
        CanEditTransformationCollection = true,   // allow multiple transformations to be displayed
        ShowColumnNames = true,   // show column names
        ShowNoTransformationsMessage = true,    // show a message if no transformations are listed
        NoTransformationsMessage = "No transformations exist",    // the message
        TransformationsInfo = tInfos,   // the transformations to be displayed
      };

      this.TControlProperties = props;
    }

    private void PrepareTransformationProperties_ChooseSRs()
    {
      // create a spatial reference for the source
      SpatialReference sr4267 = SpatialReferenceBuilder.CreateSpatialReference(4267);

      // set up an initial transformation info object
      var tInfo = new TransformationInfo();
      tInfo.SourceSpatialReference = sr4267;

      // add it to the list
      var tInfos = new List<TransformationInfo>();
      tInfos.Add(tInfo);

      var props = new TransformationsControlProperties()
      {
        CanEditCoordinateSystems = true,    // allow choice of SR
        CanEditTransformationCollection = true,   // allow multiple transformations to be displayed
        ShowColumnNames = true,   // show column names
        ShowNoTransformationsMessage = true,    // show a message if no transformations are listed
        NoTransformationsMessage = "No transformations exist",    // the message
        TransformationsInfo = tInfos,   // the transformations to be displayed
      };

      this.TControlProperties = props;
    }

    private void PrepareTransformationProperties_PresetSRs()
    {
      // create the spatial references
      SpatialReference sr4267 = SpatialReferenceBuilder.CreateSpatialReference(4267);
      SpatialReference sr4483 = SpatialReferenceBuilder.CreateSpatialReference(4483);

      // set up the transformation info object
      var tInfo = new TransformationInfo();
      tInfo.SourceSpatialReference = sr4267;
      tInfo.TargetSpatialReference = sr4483;

      // add it to the list
      var tInfos = new List<TransformationInfo>();
      tInfos.Add(tInfo);

      // WGS_1984_(ITRF00)_To_NAD_1983
      var props = new TransformationsControlProperties()
      {
        CanEditCoordinateSystems = false,   // dont allow spatial references to be changed
        CanEditTransformationCollection = false,    // dont allow additional transformations to be added
        ShowColumnNames = true,   // show column names
        ShowNoTransformationsMessage = true,    // show a message if no transformations are listed
        NoTransformationsMessage = "No transformations exist",    // the message
        TransformationsInfo = tInfos,   // the transformations to be displayed
      };
      this.TControlProperties = props;
    }

    private void PrepareTransformationProperties_PresetTransformation()
    {
      SpatialReference sr4267 = SpatialReferenceBuilder.CreateSpatialReference(4267);
      SpatialReference sr4326 = SpatialReferences.WGS84;

      var tInfo = new TransformationInfo();
      tInfo.SourceSpatialReference = sr4267;
      tInfo.TargetSpatialReference = sr4326;
      tInfo.TransformationName = "NAD_1927_To_WGS_1984_13";   // use this to have a specific transformation chosen


      // or perhaps the following rather than hardcoding a name
      //var transformations = ProjectionTransformation.FindTransformations(sr4267, sr4326);
      //if (transformations.Count > 0)
      //{
      //  var t = transformations[0];
      //  if (t.Transformation is GeographicTransformation gt)
      //    tInfo.TransformationName = gt.Name;
      //}

      var tInfos = new List<TransformationInfo>();
      tInfos.Add(tInfo);

      // WGS_1984_(ITRF00)_To_NAD_1983
      var props = new TransformationsControlProperties()
      {
        CanEditCoordinateSystems = false,   // dont allow spatial references to be changed
        CanEditTransformationCollection = false,    // dont allow additional transformations to be added
        ShowColumnNames = true,   // show column names
        ShowNoTransformationsMessage = true,    // show a message if no transformations are listed
        NoTransformationsMessage = "No transformations exist",    // the message
        TransformationsInfo = tInfos,   // the transformations to be displayed
      };
      this.TControlProperties = props;
    }

    private void PrepareTransformationProperties_PresetSRsExtent()
    {
      SpatialReference sr4267 = SpatialReferenceBuilder.CreateSpatialReference(4267);
      SpatialReference sr4326 = SpatialReferences.WGS84;

      // using this envelope as a filter with the transformation should give a reduced list of transformations
      Envelope envelope = EnvelopeBuilderEx.CreateEnvelope(-161, 61, -145, 69);

      var tInfo = new TransformationInfo();
      tInfo.SourceSpatialReference = sr4267;
      tInfo.TargetSpatialReference = sr4326;
      tInfo.SpatialFilter = envelope;


      var tInfos = new List<TransformationInfo>();
      tInfos.Add(tInfo);

      // WGS_1984_(ITRF00)_To_NAD_1983
      var props = new TransformationsControlProperties()
      {
        CanEditCoordinateSystems = false,   // dont allow spatial references to be changed
        CanEditTransformationCollection = false,    // dont allow additional transformations to be added
        ShowColumnNames = true,   // show column names
        ShowNoTransformationsMessage = true,    // show a message if no transformations are listed
        NoTransformationsMessage = "No transformations exist",    // the message
        TransformationsInfo = tInfos,   // the transformations to be displayed
      };
      this.TControlProperties = props;
    }

    private void PrepareTransformationProperties_Multiple()
    {
      SpatialReference sr4267 = SpatialReferenceBuilder.CreateSpatialReference(4267);
      SpatialReference sr4326 = SpatialReferences.WGS84;

      var tInfo = new TransformationInfo();
      tInfo.SourceSpatialReference = sr4267;
      tInfo.TargetSpatialReference = sr4326;

      var tInfos = new List<TransformationInfo>();
      tInfos.Add(tInfo);

      // another set of SRs with vertical component
      SpatialReference inSR = SpatialReferenceBuilder.CreateSpatialReference(4267, 5702);
      SpatialReference outSR = SpatialReferenceBuilder.CreateSpatialReference(4269, 5703);

      var tInfo2 = new TransformationInfo();
      tInfo2.SourceSpatialReference = inSR;
      tInfo2.TargetSpatialReference = outSR;
      tInfos.Add(tInfo2);

      // WGS_1984_(ITRF00)_To_NAD_1983
      var props = new TransformationsControlProperties()
      {
        CanEditCoordinateSystems = false,   // dont allow spatial references to be changed
        CanEditTransformationCollection = false,    // dont allow additional transformations to be added
        ShowColumnNames = true,   // show column names
        ShowNoTransformationsMessage = true,    // show a message if no transformations are listed
        NoTransformationsMessage = "No transformations exist",    // the message
        TransformationsInfo = tInfos,   // the transformations to be displayed
      };
      this.TControlProperties = props;
    }
    #endregion


    #endregion

    #region Project Command
    private ICommand _projectCmd;
    public ICommand ProjectCmd
    {
      get
      {
        if (_projectCmd == null)
          _projectCmd = new RelayCommand(() => ProjectPolygon(), true);
        return _projectCmd;
      }
    }

    internal void ProjectPolygon()
    {
      ProjectionResults = "";
      if (SelectedTransformation == null)
      {
        MessageBox.Show("no transfomation selected");
        return;
      }

      var inpsutSR = SelectedTransformation.InputSpatialReference;
      var outputSR = SelectedTransformation.OutputSpatialReference;

      // create the transformation
      var transformation = ProjectionTransformation.CreateEx(inpsutSR, outputSR, SelectedTransformation);

      // create some geometries
      MapPoint point = MapPointBuilderEx.CreateMapPoint(1, 1, inpsutSR);
      Envelope env = EnvelopeBuilderEx.CreateEnvelope(new Coordinate2D(2, 2), new Coordinate2D(3, 3), inpsutSR);

      List<Coordinate2D> coords = new List<Coordinate2D>()
      { new Coordinate2D(-120, 13 ), new Coordinate2D(-110, 15),
        new Coordinate2D(-100, 15), new Coordinate2D(-100, 20),
        new Coordinate2D(-120, 20)
      };

      Polygon polygon = PolygonBuilderEx.CreatePolygon(coords, inpsutSR);

      // project
      MapPoint projectedPoint = GeometryEngine.Instance.ProjectEx(point, transformation) as MapPoint;
      var projectedEnvelope = GeometryEngine.Instance.ProjectEx(env, transformation) as Envelope;
      Polygon projectedPolygon = GeometryEngine.Instance.ProjectEx(polygon, transformation) as Polygon;

      var sb = new StringBuilder();
      sb.AppendLine("MapPoint: " + ExportPoint(point));
      sb.AppendLine("MapPoint projected: " + ExportPoint(projectedPoint));
      sb.AppendLine("");
      sb.AppendLine("Envelope: " + ExportEnvelope(env));
      sb.AppendLine("Envelope projected: " + ExportEnvelope(projectedEnvelope));
      sb.AppendLine("");
      sb.AppendLine("Polygon: " + ExportPolygon(polygon));
      sb.AppendLine("Polygon projected: " + ExportPolygon(projectedPolygon));
      sb.AppendLine("");

      ProjectionResults = sb.ToString();
    }

    internal string ExportPoint(MapPoint point)
    {
      return "X = " + String.Format("{0:0.000000}", point.X) + ", Y = " + String.Format("{0:0.000000}", point.Y);
    }

    internal string ExportEnvelope(Envelope env)
    {
      return "XMin = " + String.Format("{0:0.000000}", env.XMin) + ", YMin = " + String.Format("{0:0.000000}", env.YMin) +
              ", XMax = " + String.Format("{0:0.000000}", env.XMax) + ", YMax = " + String.Format("{0:0.000000}", env.YMax);
    }

    internal string ExportPolygon(Polygon poly)
    {
      return "Length = " + String.Format("{0:0.000000}", poly.Length) + ", Area = " + String.Format("{0:0.000000}", poly.Area);
    }
    #endregion

  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class TransformationsControlDockpane_ShowButton : Button
  {
    protected override void OnClick()
    {
      TransformationsControlDockpaneViewModel.Show();
    }
  }
}
