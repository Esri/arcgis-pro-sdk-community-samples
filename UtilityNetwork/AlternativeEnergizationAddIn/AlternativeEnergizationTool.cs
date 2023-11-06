using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
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
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Element = ArcGIS.Core.Data.UtilityNetwork.Element;
using Geometry = ArcGIS.Core.Geometry.Geometry;
using QueryFilter = ArcGIS.Core.Data.QueryFilter;

namespace AlternativeEnergizationAddIn
{
  //   Copyright 2019 Esri
  //   Licensed under the Apache License, Version 2.0 (the "License");
  //   you may not use this file except in compliance with the License.
  //   You may obtain a copy of the License at

  //       https://www.apache.org/licenses/LICENSE-2.0

  //   Unless required by applicable law or agreed to in writing, software
  //   distributed under the License is distributed on an "AS IS" BASIS,
  //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  //   See the License for the specific language governing permissions and
  //   limitations under the License. 
  internal class AlternativeEnergizationTool : Button
  {
    MapView _activeMap = null;            
    IReadOnlyList<Result> results = null;
    List<Element> _initialIsolationElements = null;
    List<Element> _openPoints = null;
    List<Element> _openBeforeProtective = null;
    List<Element> _downstreamProtective = null;
    List<Element> _downstreamProtectiveFinal = null;
    bool _foundOpenPoint = false;
    Element _startingPointElement = null;

    protected async override void OnClick()
    {
      try
      {
        // Run isolation trace to find the upstream feature(s) that needs to be opened
        // Run a downstream trace to see if you can find an ‘open point’
        // If no open point found you are done here
        // Run a downstream trace to see if you can find that ‘open point’ before you hit another protective device
        //      (filter barrier to stop on protective, but output set for network attribute of device_status = open).
        //      If result is a selection set of 0, then you are safe to proceed.
        // If result is > 0, then no protective device before the open point, you are done here and can’t try to partial
        //      restore as you will just reenergize the same area you are initially trying to isolate.
        // Run a downstream trace to find the protective device to open
        //      for each protective device, run a downstream trace to make sure you can find the open point
        //          if you can, return the device, if you can't ignore it as it is a false positive
        if (Utilities._sharedStartingPointGeometry != null)
        {
          
          _initialIsolationElements = new List<Element>();
          _openPoints = new List<Element>();
          _openBeforeProtective = new List<Element>();
          _downstreamProtective = new List<Element>();
          _downstreamProtectiveFinal = new List<Element>();
          bool foundInitialElement = false;         

          MapView activeMap = MapView.Active;
          if (activeMap == null)
          {
            // Shouldn't happen
            MessageBox.Show("No active map");
            return;
          }
          _activeMap = activeMap;

          await QueuedTask.Run(async() =>
          {
            try
            {                                            
                using FeatureClass featureClass = Utilities._sharedFeatureLayer.GetFeatureClass();
                using UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(featureClass);                                                

                if (Utilities._sharedStartingPointElement != null)
                {
                    foundInitialElement = true;
                    _startingPointElement = Utilities._sharedStartingPointElement;

                    // run initial isolation trace
                    GetInitialIsolationElements(utilityNetwork, _startingPointElement);
                    if (_initialIsolationElements.Count == 0)
                    {
                        MessageBox.Show("No isolation capability found.");
                        return;
                    }

                    // run a downstream trace and try to find an 'open point' element
                    GetOpenPoint(utilityNetwork, _startingPointElement);
                    if (_openPoints.Count == 0)
                    {
                        // check upstream for a tap, if found run GetOpenPoint from it to avoid shutting
                        // down excessive amount of network
                        GetUpstreamTap(utilityNetwork, _startingPointElement);
                        if (_openPoints.Count == 0)
                        {
                            MessageBox.Show("No open points found to enable alternative energization. Red circles show the features that will isolate the clicked feature.");
                        }
                    }

                    if (_openPoints.Count > 0)
                    {
                        // run a downstream trace set to stop on protective but to return 'open'
                        // we are trying to see if there is a protective device that can be toggled
                        // before we reach the open point
                        GetProtectiveBeforeOpen(utilityNetwork, _startingPointElement);
                        if (_openBeforeProtective.Count != 0)
                        {
                            MessageBox.Show("Unable to find a protective device prior to the open point. Red circles show the features that will isolate the clicked feature.");
                            _openPoints.Clear();
                        }
                        else
                        {
                            // run a downstream trace and return the protective feature
                            GetDownstreamProtective(utilityNetwork, _startingPointElement);
                            MessageBox.Show("Alternative energization found!  Red circles indicate features to disable.  Green circles indicate features to enable.");
                        }
                    }
                    _activeMap.Map.ClearSelection();
                    if (foundInitialElement == true)
                    {
                        using ProgressDialog progressDialog = new ProgressDialog("Creating graphics...");
                        string errorMessage = string.Empty;

                        progressDialog.Show();

                        await QueuedTask.Run(() =>
                        {                            
                            // create a graphics layer and show red\green dots to indicate what
                            // features to toggle
                            CreateGraphicsLayer(utilityNetwork);
                        });
                    }
                }
                              
              if (Utilities._sharedFeatureLayer == null)
              {
                MessageBox.Show("Select a utility network feature.");
                return;
              }
            }
            catch (Exception e)
            {
              MessageBox.Show($"Exception: {e.Message}");
              return;
            }
          });
          
          _activeMap = null;          
          results = null;
          _initialIsolationElements = null;
          _openPoints = null;
          _openBeforeProtective = null;
          _downstreamProtective = null;
          _downstreamProtectiveFinal = null;
          _foundOpenPoint = false; 
          
        }
        else
        {
          MessageBox.Show("Use the Isolation Outage tool to first select a feature.");
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Exception in ActivateAlternativeTool: {ex.Message}");
      }
    }


    // Used to check for an upstream tap feature.  It is possible that we are on a small
    // branch that if we change the starting point to the upstream tap we will find
    // a way to do alternative energization
    private void GetUpstreamTap(UtilityNetwork utilityNetwork, Element startingPointElement)
    {
      TraceManager traceManager = utilityNetwork.GetTraceManager();
      UpstreamTracer tracer = traceManager.GetTracer<UpstreamTracer>();

      List<Element> startingPointList = new List<Element>();
      startingPointList.Add(startingPointElement);

      TraceArgument traceArgument = new TraceArgument(startingPointList);

      UtilityNetworkDefinition undef = utilityNetwork.GetDefinition();

      DomainNetwork dn = undef.GetDomainNetwork(Utilities.domainNetworkName);

      TraceConfiguration traceConfiguration = dn.GetTier(Utilities.tierName).GetTraceConfiguration();

      traceConfiguration.Filter.Barriers = new CategoryComparison(CategoryOperator.IsEqual, Utilities.isolationCategory);
      traceConfiguration.OutputCondition = new CategoryComparison(CategoryOperator.IsEqual, Utilities.subnetworkTapCategory);

      traceArgument.Configuration = traceConfiguration;

      results = tracer.Trace(traceArgument);
      foreach (Result result in results)
      {
        if (result is ElementResult er)
        {
          IReadOnlyList<Element> elements = er.Elements;
          foreach (Element element2 in elements)
          {
            GetOpenPoint(utilityNetwork, element2);
            if (_openPoints.Count != 0)
            {
              // change the starting point element to be the tap
              _startingPointElement = element2;
            }
          }
        }
      }
    }

    // Used to create a graphics layer to display red/gree circles to show
    // what feature to toggle
    private void CreateGraphicsLayer(UtilityNetwork utilityNetwork)
    {
      var map = _activeMap.Map;
      GraphicsLayer _graphicsLayer = null;

      if (map.MapType != MapType.Map)
        return;// not 2D

      var gl_param = new GraphicsLayerCreationParams { Name = "Graphics Layer" };
      _ = QueuedTask.Run(async () =>
      {
        try
        {
          var graphicsLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<ArcGIS.Desktop.Mapping.GraphicsLayer>().FirstOrDefault();
          if (graphicsLayer == null)
          {
            //By default will be added to the top of the TOC
            graphicsLayer = LayerFactory.Instance.CreateLayer<ArcGIS.Desktop.Mapping.GraphicsLayer>(gl_param, map);
          }
          else
          {
            graphicsLayer.RemoveElements();
          }
          _graphicsLayer = graphicsLayer;

          Geometry geometry = null;
          CIMMarker marker = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.RedRGB, 15.0, SimpleMarkerStyle.Pushpin);
          var pt_symbol = SymbolFactory.Instance.ConstructPointSymbol(marker);

          // create a mappoint at the starting point geometry
          MapPoint mapPoint = GeometryEngine.Instance.Centroid(Utilities._sharedStartingPointGeometry);
          //create a CIMGraphic 
          var graphic = new CIMPointGraphic()
          {
            Symbol = pt_symbol.MakeSymbolReference(),
            Location = mapPoint
          };
          // add the graphic to the layer
          graphicsLayer.AddElement(graphic);

          //specify a symbol
          pt_symbol = SymbolFactory.Instance.ConstructPointSymbol(
                                      ColorFactory.Instance.GreenRGB);

          foreach (Element element in _openPoints)
          {
            // query for the open point feature
            await QueuedTask.Run(() =>
              {
                QueryFilter filter = new QueryFilter();

                filter.WhereClause = "OBJECTID = " + element.ObjectID;
                Table table = utilityNetwork.GetTable(element.NetworkSource);

                using RowCursor rowCursor = table.Search(filter, false);
                while (rowCursor.MoveNext())
                {
                  using Row row = rowCursor.Current;
                  Feature feature = (Feature)row;
                  geometry = feature.GetShape();
                }
                return 1;
              });

            // create a mappoint at the open point geometry
            mapPoint = GeometryEngine.Instance.Centroid(geometry);
            //create a CIMGraphic 
            graphic = new CIMPointGraphic()
            {
              Symbol = pt_symbol.MakeSymbolReference(),
              Location = mapPoint
            };
            // add the graphic to the layer
            graphicsLayer.AddElement(graphic);
          }

          //specify a symbol
          pt_symbol = SymbolFactory.Instance.ConstructPointSymbol(
                                      ColorFactory.Instance.RedRGB);
          foreach (Element element in _downstreamProtectiveFinal)
          {
            // query for the protective feature
            await QueuedTask.Run(() =>
              {
                QueryFilter filter = new QueryFilter();

                filter.WhereClause = "OBJECTID = " + element.ObjectID;
                Table table = utilityNetwork.GetTable(element.NetworkSource);

                using RowCursor rowCursor = table.Search(filter, false);
                while (rowCursor.MoveNext())
                {
                  using Row row = rowCursor.Current;
                  Feature feature = (Feature)row;
                  geometry = feature.GetShape();
                }
                return 1;
              });

            // create a map point at the protective features geometry
            mapPoint = GeometryEngine.Instance.Centroid(geometry);
            //create a CIMGraphic 
            graphic = new CIMPointGraphic()
            {
              Symbol = pt_symbol.MakeSymbolReference(),
              Location = mapPoint
            };
            // add the graphic to the layer
            graphicsLayer.AddElement(graphic);
          }


          foreach (Element element in _initialIsolationElements)
          {
            // query for the isolating feature
            await QueuedTask.Run(() =>
              {
                QueryFilter filter = new QueryFilter();

                filter.WhereClause = "OBJECTID = " + element.ObjectID;
                Table table = utilityNetwork.GetTable(element.NetworkSource);

                using RowCursor rowCursor = table.Search(filter, false);
                while (rowCursor.MoveNext())
                {
                  using Row row = rowCursor.Current;
                  Feature feature = (Feature)row;
                  geometry = feature.GetShape();
                }
                return 1;
              });

            // create a map point at the isolating features geometry
            mapPoint = GeometryEngine.Instance.Centroid(geometry);
            //create a CIMGraphic 
            graphic = new CIMPointGraphic()
            {
              Symbol = pt_symbol.MakeSymbolReference(),
              Location = mapPoint
            };
            // add the graphic to the layer
            graphicsLayer.AddElement(graphic);
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show($@"Exception in CreateGraphicsLayer: {ex.Message}");
        }
      });

      // clear selections so it looks nice on the map
      _graphicsLayer.ClearSelection();
      Utilities._sharedDownstreamProtectiveElements = _downstreamProtectiveFinal;
      Utilities._sharedInitialIsolationElements = _initialIsolationElements;
      Utilities._sharedOpenPointElements = _openPoints;     
    }

    // Used to run a downstream trace that returns protective features
    private void GetDownstreamProtective(UtilityNetwork utilityNetwork, Element startingPointElement)
    {
      try { 
      TraceManager traceManager = utilityNetwork.GetTraceManager();
      DownstreamTracer tracer = traceManager.GetTracer<DownstreamTracer>();

      List<Element> startingPointList = new List<Element>();
      startingPointList.Add(startingPointElement);

      TraceArgument traceArgument = new TraceArgument(startingPointList);

      UtilityNetworkDefinition undef = utilityNetwork.GetDefinition();

      DomainNetwork dn = undef.GetDomainNetwork(Utilities.domainNetworkName);

      TraceConfiguration traceConfiguration = dn.GetTier(Utilities.tierName).GetTraceConfiguration();

      traceConfiguration.Filter.Barriers = new CategoryComparison(CategoryOperator.IsEqual, Utilities.isolationCategory);
      traceConfiguration.OutputCondition = new CategoryComparison(CategoryOperator.IsEqual, Utilities.isolationCategory);

      traceArgument.Configuration = traceConfiguration;

      results = tracer.Trace(traceArgument);
      foreach (Result result in results)
      {
        if (result is ElementResult er)
        {
          IReadOnlyList<Element> elements = er.Elements;
          foreach (Element element2 in elements)
          {
            _downstreamProtective.Add(element2);
          }
        }
      }

      // for each protective feature, run a downstream trace to see if we find the open
      // point feature.  This helps avoid false positive results.
      foreach (Element element3 in _downstreamProtective)
      {
        if (GetOpenPointCheck(utilityNetwork, element3) == true)
        {
          _downstreamProtectiveFinal.Add(element3);
        }
      }
      }
      catch (Exception ex)
      {
        throw new Exception($@"Exception in GetDownstreamProtective: {ex.Message}");
      }
    }

    // used to run a downstream trace to find a protective feature before finding an open point
    // if we find it, the results should be no elements getting returned
    private void GetProtectiveBeforeOpen(UtilityNetwork utilityNetwork, Element startingPointElement)
    {
      try {
      TraceManager traceManager = utilityNetwork.GetTraceManager();
      DownstreamTracer tracer = traceManager.GetTracer<DownstreamTracer>();

      List<Element> startingPointList = new List<Element>();
      startingPointList.Add(startingPointElement);

      TraceArgument traceArgument = new TraceArgument(startingPointList);

      UtilityNetworkDefinition undef = utilityNetwork.GetDefinition();

      DomainNetwork dn = undef.GetDomainNetwork(Utilities.domainNetworkName);

      TraceConfiguration traceConfiguration = dn.GetTier(Utilities.tierName).GetTraceConfiguration();

      traceConfiguration.OutputCondition = new NetworkAttributeComparison(undef.GetNetworkAttribute(Utilities.networkAttributeForOpenPoint), (Operator)ComparisonOperator.IsEqual, Utilities.openPointValue);
      traceConfiguration.Filter.Barriers = new CategoryComparison(CategoryOperator.IsEqual, Utilities.isolationCategory);

      traceArgument.Configuration = traceConfiguration;

      results = tracer.Trace(traceArgument);
      foreach (Result result in results)
      {
        if (result is ElementResult er)
        {
          IReadOnlyList<Element> elements = er.Elements;
          foreach (Element element2 in elements)
          {
            _openBeforeProtective.Add(element2);
          }
        }
        }
      }
      catch (Exception ex)
      {
        throw new Exception($@"Exception in GetProtectiveBeforeOpen: {ex.Message}");
      }
    }

    // Used to help avoid false positive results
    // Run a downstream trace and return any open point features
    private bool GetOpenPointCheck(UtilityNetwork utilityNetwork, Element startingPointElement)
    {
      try {
      TraceManager traceManager = utilityNetwork.GetTraceManager();
      DownstreamTracer tracer = traceManager.GetTracer<DownstreamTracer>();

      List<Element> startingPointList = new List<Element>();
      startingPointList.Add(startingPointElement);

      TraceArgument traceArgument = new TraceArgument(startingPointList);

      UtilityNetworkDefinition undef = utilityNetwork.GetDefinition();

      DomainNetwork dn = undef.GetDomainNetwork(Utilities.domainNetworkName);

      TraceConfiguration traceConfiguration = dn.GetTier(Utilities.tierName).GetTraceConfiguration();

      traceConfiguration.OutputCondition = new NetworkAttributeComparison(undef.GetNetworkAttribute(Utilities.networkAttributeForOpenPoint), (Operator)ComparisonOperator.IsEqual, Utilities.openPointValue);

      traceArgument.Configuration = traceConfiguration;

      results = tracer.Trace(traceArgument);
      foreach (Result result in results)
      {
        if (result is ElementResult er)
        {
          if (er.Elements.Count == 0)
          {
            return false;
          }
          else
          {
            return true;
          }
        }
        }
      }
      catch (Exception ex)
      {
        throw new Exception($@"Exception in GetOpenPointCheck: {ex.Message}");
      }
      return false;
    }

    // Run a downstream trace and return any open point features
    private void GetOpenPoint(UtilityNetwork utilityNetwork, Element startingPointElement)
    {
      try {
      TraceManager traceManager = utilityNetwork.GetTraceManager();
      DownstreamTracer tracer = traceManager.GetTracer<DownstreamTracer>();

      List<Element> startingPointList = new List<Element>();
      startingPointList.Add(startingPointElement);

      TraceArgument traceArgument = new TraceArgument(startingPointList);

      UtilityNetworkDefinition undef = utilityNetwork.GetDefinition();

      DomainNetwork dn = undef.GetDomainNetwork(Utilities.domainNetworkName);

      TraceConfiguration traceConfiguration = dn.GetTier(Utilities.tierName).GetTraceConfiguration();

      traceConfiguration.OutputCondition = new NetworkAttributeComparison(undef.GetNetworkAttribute(Utilities.networkAttributeForOpenPoint), (Operator)ComparisonOperator.IsEqual, Utilities.openPointValue);

      traceArgument.Configuration = traceConfiguration;

      results = tracer.Trace(traceArgument);
      foreach (Result result in results)
      {
        if (result is ElementResult er)
        {
          IReadOnlyList<Element> elements = er.Elements;
          foreach (Element element2 in elements)
          {
            // Check to make sure that the feature is in fact between 2 subnetworks
            // by looking for '::' in the subnetworkname field
            CheckToEnsureOpenPoint(utilityNetwork, element2);
            if (_foundOpenPoint == true)
            {
              _openPoints.Add(element2);
            }
          }
        }
        }
      }
      catch (Exception ex)
      {
        throw new Exception($@"Exception in GetOpenPoint: {ex.Message}");
      }
    }

    // query feature and check the subnetworkname field for '::' in the name
    // makes sure that the open point is between 2 subnetworks
    private async void CheckToEnsureOpenPoint(UtilityNetwork utilityNetwork, Element element2)
    {
      try 
      {
          _foundOpenPoint = false;
          // query for the chosen feature
          await QueuedTask.Run(() =>
          {
            QueryFilter filter = new QueryFilter();

            filter.WhereClause = "OBJECTID = " + element2.ObjectID;
            Table table = utilityNetwork.GetTable(element2.NetworkSource);

            using RowCursor rowCursor = table.Search(filter, false);
            while (rowCursor.MoveNext())
            {
              using Row row = rowCursor.Current;
              if (row.FindField("SUBNETWORKNAME") != -1)
              {
                if (Convert.ToString(row["SUBNETWORKNAME"]) != "")
                {
                  if (Convert.ToString(row["SUBNETWORKNAME"]).Contains("::"))
                  {
                    _foundOpenPoint = true;
                  }
                }
              }
            }
          });
      }
      catch (Exception ex)
      {
        throw new Exception($@"Exception in CheckToEnsureOpenPoint: {ex.Message}");
      }
    }

    // run the initial isolation trace and retrun the elements
    private void GetInitialIsolationElements(UtilityNetwork utilityNetwork, Element startingPointElement)
    {
      try
      { 
          TraceManager traceManager = utilityNetwork.GetTraceManager();
          IsolationTracer tracer = traceManager.GetTracer<IsolationTracer>();

          List<Element> startingPointList = new List<Element>();
          startingPointList.Add(startingPointElement);

          TraceArgument traceArgument = new TraceArgument(startingPointList);

          UtilityNetworkDefinition undef = utilityNetwork.GetDefinition();

          DomainNetwork dn = undef.GetDomainNetwork(Utilities.domainNetworkName);

          TraceConfiguration traceConfiguration = dn.GetTier(Utilities.tierName).GetTraceConfiguration();
          traceConfiguration.Filter.Barriers = new CategoryComparison(CategoryOperator.IsEqual, Utilities.isolationCategory);

          traceArgument.Configuration = traceConfiguration;

          results = tracer.Trace(traceArgument);
          foreach (Result result in results)
          {
            if (result is ElementResult er)
            {
              IReadOnlyList<Element> elements = er.Elements;
              foreach (Element element2 in elements)
              {
                _initialIsolationElements.Add(element2);
              }
            }
          }
      }
      catch (Exception ex)
      {
        throw new Exception($@"Exception in GetInitialIsolationElements: {ex.Message}");
      }
    }
  }
}
