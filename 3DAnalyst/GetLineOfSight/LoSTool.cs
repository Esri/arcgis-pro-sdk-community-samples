/*

   Copyright 2023 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Catalog.PropertyPages.NetworkDataset;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace GetLineOfSight
{
  internal class LoSTool : MapTool
  {
    private IDisposable _observerGraphic = null;
    private IDisposable _observerLabelGraphic;
    private IDisposable _targetGraphic;
    private CIMPointSymbol _observerPointSymbol = null;
    private CIMPointSymbol _targetPointSymbol = null;
    private CIMTextGraphic _observerLabelSymbol = null;
    private CIMTextGraphic _targetLabelSymbol = null;
    public LoSTool()
    {
      IsSketchTool = true;
      UseSnapping = true;
      // Select the type of construction tool you wish to implement.  
      // Make sure that the tool is correctly registered with the correct component category type in the daml 
      SketchType = SketchGeometryType.Line;
      //Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
      UsesCurrentTemplate = true;
      //Gets or sets whether the tool supports firing sketch events when the map sketch changes. 
      //Default value is false.
      FireSketchEvents = true;
    }

    #region Tool Options
    //Gets all the custom "ToolOptions" defined for this Line of Sight construction tool.
    private ReadOnlyToolOptions ToolOptions => CurrentTemplate?.GetToolOptions(ID);
    //These are all the Tool options. The values are retrieved from  ToolOptions and stored in these variables.
    //These properties are then used to perform the Line of Sight analysis.

    private TinLayer SelectedTinLayer
    {
      get
      {
        if (ToolOptions == null)
          return LoSToolOptionsViewViewModel.DefaultTinLayer;

        return ToolOptions.GetProperty(LoSToolOptionsViewViewModel.SelectedTinLayerName, LoSToolOptionsViewViewModel.DefaultTinLayer);
      }
    }
    private double ObserverHeight
    {
      get
      {
        if (ToolOptions == null)
          return LoSToolOptionsViewViewModel.DefaultObserverHeight;

        return ToolOptions.GetProperty(LoSToolOptionsViewViewModel.ObserverHeightOptionName, LoSToolOptionsViewViewModel.DefaultObserverHeight);
      }
    }
    private double TargetHeight
    {
      get
      {
        if (ToolOptions == null)
          return LoSToolOptionsViewViewModel.DefaultTargetHeight;

        return ToolOptions.GetProperty(LoSToolOptionsViewViewModel.TargetHeightOptionName, LoSToolOptionsViewViewModel.DefaultTargetHeight);
      }
    }

    private bool ApplyCurvature
    {
      get
      {
        if (ToolOptions == null)
          return false;
        return ToolOptions.GetProperty(LoSToolOptionsViewViewModel.ApplyCurvatureName, false);
      }
    }

    private bool ApplyRefraction
    {
      get
      {
        if (ToolOptions == null)
          return false;
        return ToolOptions.GetProperty(LoSToolOptionsViewViewModel.ApplyRefractionName, false);
      }
    }

    private double RefractionFactor
    {
      get
      {
        if (ToolOptions == null)
          return LineOfSightParams.DefaultRefractionFactor;
        return ToolOptions.GetProperty(LoSToolOptionsViewViewModel.RefractionFactorName, LineOfSightParams.DefaultRefractionFactor);
      }
    }
    #endregion

    //TinLayer _tinLayer = null;
    private ElevationSurfaceLayer _surfaceLayer;
    FeatureLayer _lineResults = null;
    FeatureLayer _obstructionPointResults = null;
    FeatureLayer _sketchedLoSLayer = null;

    protected override Task OnToolActivateAsync(bool mapViewChanged)
    {
      //Get the TIN layer used for the line of sight analysis
      //_tinLayer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      //The input line is stored in this feature layer
      _sketchedLoSLayer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(n => n.Name == "LoSResults_Input");
      //The line of sight created is stored in this layer
      _lineResults = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(n => n.Name == "LoSResults");
      //If there is an obstruction point, it is stored in this layer
      _obstructionPointResults = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(n => n.Name == "LosObstructionPoints");
      return Task.CompletedTask; 
    }

    /// <summary>
    /// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
    /// </summary>
    /// <param name="geometry">The geometry created by the sketch.</param>
    /// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
    protected async override Task<bool> OnSketchCompleteAsync(ArcGIS.Core.Geometry.Geometry geometry)
    {
      if (CurrentTemplate == null || geometry == null)
        return false;
      //Check if the correct layers are available to store the results
      //Get the TIN layer used for the line of sight analysis
      //_tinLayer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<TinLayer>().FirstOrDefault();
      //The input line is stored in this feature layer
      _sketchedLoSLayer = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(n => n.Name == "LoSResults_Input");
      //The line of sight created is stored in this layer
      _lineResults = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(n => n.Name == "LoSResults");
      //If there is an obstruction point, it is stored in this layer
      _obstructionPointResults = ActiveMapView.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(n => n.Name == "LosObstructionPoints");
      if ((_sketchedLoSLayer == null) || (SelectedTinLayer == null) || (_obstructionPointResults == null) || (_lineResults == null))
      {
        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Please add the required layers to the map.", "Missing layers", MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
      }          
      //Get the start and end points of the line drawn.
      ArcGIS.Core.Geometry.Polyline sketchedLine = geometry as ArcGIS.Core.Geometry.Polyline;
      var polyLineParts = sketchedLine.Parts;
      ReadOnlySegmentCollection polylineSegments = polyLineParts.First();

      //get the first segment as a LineSegment
      var losLineSegment = polylineSegments.First() as ArcGIS.Core.Geometry.LineSegment;
      //The start point
      MapPoint startPoint = losLineSegment.StartPoint;

      //Project start point
      MapPoint startPointProjected = GeometryEngine.Instance.Project(startPoint, MapView.Active.Map.SpatialReference) as MapPoint;

      //The end point
      MapPoint endPoint = losLineSegment.EndPoint;
      //Project end point
      MapPoint endPointProjected = GeometryEngine.Instance.Project(endPoint, MapView.Active.Map.SpatialReference) as MapPoint;

      List<MapPoint> startAndEndSketchedPoints = new List<MapPoint> { startPointProjected, endPointProjected };

      //Overlay graphics for Observer and Target points (start and end points)
      if (_observerPointSymbol == null)
      {
        _observerPointSymbol = await CreatePointSymbolAsync();
      }
      if (_targetPointSymbol == null)
      {
        _targetPointSymbol = await CreatePointSymbolAsync();
      }
      //_observerGraphic = await this.AddOverlayAsync(startPointProjected, _observerPointSymbol.MakeSymbolReference());
      //_targetGraphic = await this.AddOverlayAsync(endPointProjected, _targetPointSymbol.MakeSymbolReference());

      //The input sketched line.
      //This will be added as a line feature to _sketchedLoSLayer (LoSResults_Input in the Pro project)
      ArcGIS.Core.Geometry.Polyline sketchedProjectedLine = PolylineBuilderEx.CreatePolyline(startAndEndSketchedPoints, MapView.Active.Map.SpatialReference);
      double obstructionPID = 0;
      
      //Initialize this to store the Line of sight analysis result.
      LineOfSightResult lineOfSightResult = null;
      await QueuedTask.Run(() =>
      {
        //Calculate the highest current obstruction id in the dataset.
        //This is just a unique id for each line of sight analysis.
        obstructionPID = GetNewObstructionPtID(_sketchedLoSLayer, "OBSTR_MPID");
        /////////////////// GetLineOfSight method ////////////////////////////////////////////////
        ////Set the values for the parameters //////////////////////
        LineOfSightParams lineOfSightParams = new LineOfSightParams();
        lineOfSightParams.ObserverPoint = startPointProjected;
        lineOfSightParams.TargetPoint = endPointProjected;
        lineOfSightParams.ObserverHeightOffset = ObserverHeight;
        lineOfSightParams.TargetHeightOffset = TargetHeight;
        lineOfSightParams.ApplyCurvature = ApplyCurvature;
        lineOfSightParams.ApplyRefraction = ApplyRefraction;
        if (ApplyRefraction)
        {
          lineOfSightParams.RefractionFactor = RefractionFactor;
        }
        lineOfSightParams.OutputSpatialReference = MapView.Active.Map.SpatialReference;
        ///////The magic happens here ////////////////////
        if (SelectedTinLayer.CanGetLineOfSight(lineOfSightParams))
        {
          lineOfSightResult = SelectedTinLayer.GetLineOfSight(lineOfSightParams);
        }
      });

      if (lineOfSightResult == null)
      {
        return false;
      }
      //Create a line feature class for the sketched line. This can be used for the Line of Sight GP Tool also.
      var editOp = new EditOperation();
      editOp.Name = "Line Of Sight results";
      Dictionary<string, object> sketchedLosValues = new Dictionary<string, object>();
      sketchedLosValues["SHAPE"] = sketchedProjectedLine;//Geometry
      sketchedLosValues["OBSTR_MPID"] = obstructionPID;
      sketchedLosValues["OffsetA"] = ObserverHeight; //Must use offsetA and OffsetB for the GP tool
      sketchedLosValues["OffsetB"] = TargetHeight; //It does not work if you the Z value of Start and End points are incremented for the GP Tool
      editOp.Create(_sketchedLoSLayer, sketchedLosValues);
      //Create the obstruction point feature in the _obstructionPointResults (LosObstructionPoints in Pro Project)
      if (lineOfSightResult.ObstructionPoint != null)
      {
        //Define some default attribute values and create obstruction pt feature 
        Dictionary<string, object> attrObstructionPoint = new Dictionary<string, object>();
        //var projectedObstructionPoint = MapPointBuilderEx.CreateMapPoint(lineOfSightResult.ObstructionPoint, MapView.Active.Map.SpatialReference);
        attrObstructionPoint["SHAPE"] = lineOfSightResult.ObstructionPoint;//Geometry
        attrObstructionPoint["OBSTR_MPID"] = string.Format(obstructionPID.ToString());
        editOp.Create(_obstructionPointResults, attrObstructionPoint);
      }
      //Create the Visible line feature in the _lineResults (LoSResults in Pro Project)
      if (lineOfSightResult.VisibleLine != null)
      {
        //Define some default attribute values and create visible line feature
        Dictionary<string, object> attrVisibleLineOfSight = new Dictionary<string, object>();
        attrVisibleLineOfSight["SHAPE"] = lineOfSightResult.VisibleLine;//Geometry
        attrVisibleLineOfSight["OBSTR_MPID"] = string.Format(obstructionPID.ToString());
        attrVisibleLineOfSight["VisCode"] = string.Format("1");
        editOp.Create(_lineResults, attrVisibleLineOfSight);
      }
      //Create the Invisible line feature in the _lineResults (LoSResults in Pro Project)
      if (lineOfSightResult.InvisibleLine != null)
      {
        //Define some default attribute values and create invisible line feature
        Dictionary<string, object> attrInVisibleLineOfSight = new Dictionary<string, object>();
        attrInVisibleLineOfSight["SHAPE"] = lineOfSightResult.InvisibleLine;//Geometry
        attrInVisibleLineOfSight["OBSTR_MPID"] = string.Format(obstructionPID.ToString());
        attrInVisibleLineOfSight["VisCode"] = string.Format("2");
        editOp.Create(_lineResults, attrInVisibleLineOfSight);
      }

      //// Execute the operation
      return await editOp.ExecuteAsync();
      
    }

    public double GetNewObstructionPtID(FeatureLayer featureLayer, string fieldName)
    {
      var featureClass = featureLayer.GetFeatureClass();
      using (FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition())
      {
                // Get fields
                ArcGIS.Core.Data.Field field = featureClassDefinition.GetFields()
          .First(x => x.Name.Equals(fieldName));

        // Create StatisticsDescriptions
        StatisticsDescription staticsDescription = new StatisticsDescription(field,
                  new List<ArcGIS.Core.Data.StatisticsFunction>() { ArcGIS.Core.Data.StatisticsFunction.Max });


        // Create TableStatisticsDescription
        TableStatisticsDescription tableStatisticsDescription = new TableStatisticsDescription(new List<StatisticsDescription>() {
                  staticsDescription });


        // Calculate Statistics
        IReadOnlyList<TableStatisticsResult> tableStatisticsResults = featureClass.CalculateStatistics(tableStatisticsDescription);

        foreach (TableStatisticsResult tableStatisticsResult in tableStatisticsResults)
        {
          // Get the Region name
          // If multiple fields had been passed into TableStatisticsDescription.GroupBy, there would be multiple values in TableStatisticsResult.GroupBy
          //string regionName = tableStatisticsRe,sult.GroupBy.First().Value.ToString();

          // Get the statistics lineOfSightResult for the Population_1990 field
          StatisticsResult statistcis = tableStatisticsResult.StatisticsResults[0];
          double maxValue = statistcis.Max;


          return maxValue + 1;

          // Do something with the lineOfSightResult here...
        }
        return 1;
      }
    }

    /// <summary>Create a pushpin symbol</summary>
    internal static Task<CIMPointSymbol> CreatePointSymbolAsync()
    {
      StyleProjectItem style =
        Project.Current.GetItems<StyleProjectItem>().FirstOrDefault(s => s.Name == "ArcGIS 3D");
      if (style == null) return null;
      
      return QueuedTask.Run(() =>
      {
        var symbol = style.SearchSymbols(StyleItemType.PointSymbol, "Pushpin 2").FirstOrDefault();
        if (symbol == null) return null;
        var cimPointSymbol = symbol.Symbol as CIMPointSymbol;
        cimPointSymbol.SetSize(10);
        cimPointSymbol.UseRealWorldSymbolSizes = true;
        return cimPointSymbol;
      });
    }

    internal static Task<CIMTextGraphic> CreateTextGraphicAsync(ArcGIS.Core.Geometry.Geometry geometry)
    {
      StyleProjectItem style =
        Project.Current.GetItems<StyleProjectItem>().FirstOrDefault(s => s.Name == "ArcGIS 2D");
      if (style == null) return null;

      return QueuedTask.Run(() =>
      {
        var symbol = style.SearchSymbols(StyleItemType.TextSymbol, "Callout (Sans Serif)").FirstOrDefault();
        if (symbol == null) return null;
        var cimTextSymbol = symbol.Symbol as CIMTextSymbol;
        CIMTextGraphic cimTextGraphic = new CIMTextGraphic
        {
          Symbol = cimTextSymbol.MakeSymbolReference(),
          Shape = geometry
        };
        return cimTextGraphic;
      });
    }
  }
}

