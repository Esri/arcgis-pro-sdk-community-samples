using System;
using System.Collections.Generic;
using System.Linq;
using ArcGIS.Core.Hosting;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Topology;
using ArcGIS.Core.Geometry;

namespace TopologyAPI {
  /// <summary>
  /// Corehost standalone application shows how to open a Topology dataset and inspect the Topology rules and definitions. 
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the "Resources" section for downloading sample data).  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Topology\GrandTeton.gdb" is available.
  /// 1. Open this solution in Visual Studio 
  /// 1. Make sure that the path specified in GRAND_TETON_GDB_FULL_PATH is valid  
  /// 1. Compile and run the application.
  /// 1. View the topology’s metadata output by the sample app using the TopologyDefinition object.
  /// ![UI](Screenshots/Screen1.png)
  /// </remarks>
  class Program
  {
    static readonly string GRAND_TETON_GDB_FULL_PATH = @"C:\Data\Topology\GrandTeton.gdb";
    const string GRAND_TETON_TOPO_NAME = "Backcountry_Topology";

    //[STAThread] must be present on the Application entry point
    [STAThread]
    static void Main(string[] args)
    {
      Host.Initialize();
      ExploreTopologyAPI();
    }

    private static void ExploreTopologyAPI()
    {
      // 1) Open a geodatabase topology dataset from a file geodatabase.

      using (Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(GRAND_TETON_GDB_FULL_PATH))))
      using (Topology topology = geodatabase.OpenDataset<Topology>(GRAND_TETON_TOPO_NAME))
      {
        /*
         * This API also works with *feature service* topology.  For example, to connect to a feature service topology:
         *
         * const string TOPOLOGY_LAYER_ID = "0";
         *
         * using (Geodatabase geodatabase = new Geodatabase(new ServiceConnectionProperties(new Uri("https://sdkexamples.esri.com/server/rest/services/GrandTeton/FeatureServer")))
         * using (Topology topology = geodatabase.OpenDataset<Topology>(TOPOLOGY_LAYER_ID))
         * {
         * }
         */

        Console.WriteLine($"Topology name => {topology.GetName()}");

        // 2) Explore the topology definition.

        ExploreTopologyDefinition(geodatabase, topology);

        // 3) Explore topology rules.

        ExploreTopologyRules(topology);

        // 4) Explore topology errors.

        ExploreTopologyErrors(topology);

        // 5) Explore topology validation.

        ExploreTopologyValidation(geodatabase, topology);

        // 6) Explore the topology graph.

        ExploreTopologyGraph(geodatabase, topology);
      }
    }

    #region ExploreTopologyDefinition

    private static void ExploreTopologyDefinition(Geodatabase geodatabase, Topology topology)
    {
      // Similar to the rest of the Definition objects in the Core.Data API, there are two ways to open a dataset's 
      // definition -- via the Topology dataset itself or via the Geodatabase.

      using (TopologyDefinition definitionViaTopology = topology.GetDefinition())
      {
        OutputDefinition(geodatabase, definitionViaTopology);
      }

      using (TopologyDefinition definitionViaGeodatabase = geodatabase.GetDefinition<TopologyDefinition>(GRAND_TETON_TOPO_NAME))
      {
        //OutputDefinition(geodatabase, definitionViaGeodatabase);
      }
    }

    private static void OutputDefinition(Geodatabase geodatabase, TopologyDefinition topologyDefinition)
    {
      Console.WriteLine("***************************************************************************");
      Console.WriteLine($"Topology cluster tolerance => {topologyDefinition.GetClusterTolerance()}");
      Console.WriteLine($"Topology Z cluster tolerance => {topologyDefinition.GetZClusterTolerance()}");

      IReadOnlyList<string> featureClassNames = topologyDefinition.GetFeatureClassNames();
      Console.WriteLine($"There are {featureClassNames.Count} feature classes that are participating in the topology:");

      foreach (string name in featureClassNames)
      {
        // Open each feature class that participates in the topology.

        using (FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(name))
        using (FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition())
        {
          Console.WriteLine($"\t{featureClass.GetName()} ({featureClassDefinition.GetShapeType()})");
        }
      }
    }

    #endregion ExploreTopologyDefinition

    #region ExploreTopologyRules

    private static void ExploreTopologyRules(Topology topology)
    {
      using (TopologyDefinition topologyDefinition = topology.GetDefinition())
      {
        IReadOnlyList<TopologyRule> rules = topologyDefinition.GetRules();

        Console.WriteLine("***************************************************************************");
        Console.WriteLine($"There are {rules.Count} topology rules defined for the topology:");
        Console.WriteLine("ID \t Origin Class \t Origin Subtype \t Destination Class \t Destination Subtype \t Rule Type");

        foreach (TopologyRule rule in rules)
        {
          Console.Write($"{rule.ID}");

          Console.Write(!String.IsNullOrEmpty(rule.OriginClass) ? $"\t{rule.OriginClass}" : "\t\"\"");

          Console.Write(rule.OriginSubtype != null ? $"\t{rule.OriginSubtype.GetName()}" : "\t\"\"");

          Console.Write(!String.IsNullOrEmpty(rule.DestinationClass) ? $"\t{rule.DestinationClass}" : "\t\"\"");

          Console.Write(rule.DestinationSubtype != null ? $"\t{rule.DestinationSubtype.GetName()}" : "\t\"\"");

          Console.Write($"\t{rule.RuleType}");

          Console.WriteLine();
        }
      }
    }

    #endregion ExploreTopologyRules

    #region ExploreTopologyErrors

    private static void ExploreTopologyErrors(Topology topology)
    {
      // Get all the errors and exceptions currently associated with the topology.

      Console.WriteLine("***************************************************************************");
      IReadOnlyList<TopologyError> allErrorsAndExceptions = topology.GetErrors(new ErrorDescription(topology.GetExtent()));
      Console.WriteLine($"There are {allErrorsAndExceptions.Count} errors and exceptions associated with the topology.");

      Console.WriteLine("OriginClassName \t OriginObjectID \t DestinationClassName \t DestinationObjectID \t RuleType \t IsException \t Shape type \t Shape width & height \t  Rule ID \t");

      foreach (TopologyError error in allErrorsAndExceptions)
      {
        Console.WriteLine($"'{error.OriginClassName}' \t {error.OriginObjectID} \t '{error.DestinationClassName}' \t " +
                          $"{error.DestinationObjectID} \t {error.RuleType} \t {error.IsException} \t {error.Shape.GeometryType} \t " +
                          $"{error.Shape.Extent.Width},{error.Shape.Extent.Height} \t {error.RuleID}");
      }

      // Get all the errors due to features violating the "PointProperlyInsideArea" topology rule.

      TopologyDefinition definition = topology.GetDefinition();
      TopologyRule pointProperlyInsideAreaRule = definition.GetRules().First(rule => rule.RuleType == TopologyRuleType.PointProperlyInsideArea);

      ErrorDescription errorDescription = new ErrorDescription(topology.GetExtent())
      {
        TopologyRule = pointProperlyInsideAreaRule
      };

      IReadOnlyList<TopologyError> errorsDueToViolatingPointProperlyInsideAreaRule = topology.GetErrors(errorDescription);
      Console.WriteLine($"There are {errorsDueToViolatingPointProperlyInsideAreaRule.Count} feature violating the 'PointProperlyInsideArea' topology rule.");

      // Mark all errors from features violating the 'PointProperlyInsideArea' topology rule as exceptions.

      foreach (TopologyError error in errorsDueToViolatingPointProperlyInsideAreaRule)
      {
        topology.MarkAsException(error);
      }

      // Now verify all the errors from features violating the 'PointProperlyInsideArea' topology rule have indeed been
      // marked as exceptions.
      //
      // By default, ErrorDescription is initialized to ErrorType.ErrorAndException.  Here we want ErrorType.ErrorOnly.

      errorDescription = new ErrorDescription(topology.GetExtent())
      {
        ErrorType = ErrorType.ErrorOnly,
        TopologyRule = pointProperlyInsideAreaRule
      };

      IReadOnlyList<TopologyError> errorsAfterMarkedAsExceptions = topology.GetErrors(errorDescription);
      Console.WriteLine($"There are {errorsAfterMarkedAsExceptions.Count} feature violating the 'PointProperlyInsideArea' topology rule after all the errors have been marked as exceptions.");

      // Finally, reset all the exceptions as errors by unmarking them as exceptions.

      foreach (TopologyError error in errorsDueToViolatingPointProperlyInsideAreaRule)
      {
        topology.UnmarkAsException(error);
      }

      IReadOnlyList<TopologyError> errorsAfterUnmarkedAsExceptions = topology.GetErrors(errorDescription);
      Console.WriteLine($"There are {errorsAfterUnmarkedAsExceptions.Count} feature violating the 'PointProperlyInsideArea' topology rule after all the exceptions have been reset as errors.");
    }

    #endregion ExploreTopologyErrors

    #region ExploreTopologyValidation

    private static void ExploreTopologyValidation(Geodatabase geodatabase, Topology topology)
    {
      // If the topology currently does not have dirty areas, calling Validate() returns an empty envelope.

      Console.WriteLine("***************************************************************************");
      ValidationResult result = topology.Validate(new ValidationDescription(topology.GetExtent()));
      Console.WriteLine($"'AffectedArea' after validating a topology that has not been edited => {result.AffectedArea.ToJson()}");

      // Now create a feature that purposely violates the "PointProperlyInsideArea" topology rule.  This action will
      // create dirty areas.

      Feature newFeature = null;

      try
      {
        // Fetch the feature in the Campsites feature class whose objectID is 2.  Then create a new geometry slightly
        // altered from this and use it to create a new feature.

        using (Feature featureViaCampsites2 = GetFeature(geodatabase, "Campsites", 2))
        {
          Geometry currentGeometry = featureViaCampsites2.GetShape();
          Geometry newGeometry = GeometryEngine.Instance.Move(currentGeometry, (currentGeometry.Extent.XMax / 8),
            (currentGeometry.Extent.YMax / 8));

          using (FeatureClass campsitesFeatureClass = featureViaCampsites2.GetTable())
          using (FeatureClassDefinition definition = campsitesFeatureClass.GetDefinition())
          using (RowBuffer rowBuffer = campsitesFeatureClass.CreateRowBuffer())
          {
            rowBuffer[definition.GetShapeField()] = newGeometry;

            geodatabase.ApplyEdits(() =>
            {
              newFeature = campsitesFeatureClass.CreateRow(rowBuffer);
            });
          }
        }

        // After creating a new feature in the 'Campsites' participating feature class, the topology's state should be 
        // "Unanalyzed" because it has not been validated.

        Console.WriteLine($"The topology state after an edit has been applied => {topology.GetState()}");

        // Now validate the topology.  The result envelope corresponds to the dirty areas.

        result = topology.Validate(new ValidationDescription(topology.GetExtent()));
        Console.WriteLine($"'AffectedArea' after validating a topology that has just been edited => {result.AffectedArea.ToJson()}");

        // After Validate(), the topology's state should be "AnalyzedWithErrors" because the topology currently has errors.

        Console.WriteLine($"The topology state after validate topology => {topology.GetState()}");

        // If there are no dirty areas, the result envelope should be empty.

        result = topology.Validate(new ValidationDescription(topology.GetExtent()));
        Console.WriteLine($"'AffectedArea' after validating a topology that has just been validated => {result.AffectedArea.ToJson()}");
      }
      finally
      {
        if (newFeature != null)
        {
          geodatabase.ApplyEdits(() =>
          {
            newFeature.Delete();
          });

          newFeature.Dispose();
        }
      }

      // Validate again after deleting the newly-created feature.

      topology.Validate(new ValidationDescription(topology.GetExtent()));
    }

    #endregion ExploreTopologyValidation

    #region ExploreTopologyGraph

    private static void ExploreTopologyGraph(Geodatabase geodatabase, Topology topology)
    {
      // Build a topology graph using the extent of the topology dataset.

      topology.BuildGraph(topology.GetExtent(), (topologyGraph) =>
      {
        // To visualize where the query point is, see "QueryPoint.PNG" in the "..\Data" folder.

        Feature featureViaTrailPoints66 = GetFeature(geodatabase, "Trail_Points", 66);

        try
        {
          MapPoint queryPointViaTrailPoints66 = featureViaTrailPoints66.GetShape() as MapPoint;
          double searchRadius = 1.0;

          TopologyNode topologyNodeViaTrailPoints66 = topologyGraph.FindClosestElement<TopologyNode>(queryPointViaTrailPoints66, searchRadius);

          System.Diagnostics.Debug.Assert(topologyNodeViaTrailPoints66 != null, "There should be a topology node corresponding to 'queryPointViaTrailPoints66' within the 'searchRadius' units.");

          IReadOnlyList<FeatureInfo> parentFeatures = topologyNodeViaTrailPoints66.GetParentFeatures();
          Console.WriteLine("***************************************************************************");
          Console.WriteLine("The parent features that spawn 'topologyNodeViaTrailPoints66' are:");
          foreach (FeatureInfo parentFeature in parentFeatures)
          {
            Console.WriteLine($"\t{parentFeature.FeatureClassName}; OID: {parentFeature.ObjectID}");
          }

          IReadOnlyList<TopologyEdge> allEdgesConnectedToNodeViaTrailPoints66 = topologyNodeViaTrailPoints66.GetEdges();

          foreach (TopologyEdge edge in allEdgesConnectedToNodeViaTrailPoints66)
          {
            IReadOnlyList<FeatureInfo> parents = edge.GetParentFeatures();
            System.Diagnostics.Debug.Assert(parents.Count == 1, "The edge should have only 1 corresponding parent feature.");
            FeatureInfo parentFeature = parents[0];

            // To determine whether the edge's "from node" is coincident with "topologyNodeViaTrailPoints66".

            bool fromNodeIsCoincidentWithNodeViaTrailPoints66 = edge.GetFromNode() == topologyNodeViaTrailPoints66;

            // To determine whether the edge's "to node" is coincident with "topologyNodeViaTrailPoints66".

            bool toNodeIsCoincidentWithNodeViaTrailPoints66 = edge.GetToNode() == topologyNodeViaTrailPoints66;

            if (fromNodeIsCoincidentWithNodeViaTrailPoints66)
              Console.WriteLine($"The [{parentFeature.FeatureClassName},{parentFeature.ObjectID}] edge's FromNode is coincident with 'topologyNodeViaTrailPoints66'.");

            if (toNodeIsCoincidentWithNodeViaTrailPoints66)
              Console.WriteLine($"The [{parentFeature.FeatureClassName},{parentFeature.ObjectID}] edge's ToNode is coincident with 'topologyNodeViaTrailPoints66'.");

            // Get only polygon features lying to the left of this edge if available (specified by the "boundedByEdge" optional argument).

            IReadOnlyList<FeatureInfo> leftParentFeatures = edge.GetLeftParentFeatures();

            if (leftParentFeatures.Count == 0)
            {
              // There are no polygon features lying to the left of this edge; get the set of polygon features that cover this edge.

              Console.WriteLine("The set of polygon features that cover this edge:");

              leftParentFeatures = edge.GetLeftParentFeatures(false);
            }
            else
            {
              Console.WriteLine("The set of polygon features lying to the left of this edge (i.e., left parent polygon features bounded by this edge):");
            }

            foreach (FeatureInfo leftParentFeature in leftParentFeatures)
            {
              Console.WriteLine($"\t{leftParentFeature.FeatureClassName}; OID: {leftParentFeature.ObjectID}");

              // To get additional information from the parent feature (e.g., shape, globalID), use leftParentFeature.GetFeature().
            }

            // Get only polygon features lying to the right of this edge if available (specified by the "boundedByEdge" optional argument).

            IReadOnlyList<FeatureInfo> rightParentFeatures = edge.GetRightParentFeatures();

            if (rightParentFeatures.Count != 0)
            {
              Console.WriteLine("The set of polygon features lying to the right of this edge (i.e., right parent polygon features bounded by this edge):");

              foreach (FeatureInfo rightParentFeature in rightParentFeatures)
              {
                Console.WriteLine($"\t{rightParentFeature.FeatureClassName}; OID: {rightParentFeature.ObjectID}");
              }
            }
          }
        }
        finally
        {
          if (featureViaTrailPoints66 != null)
            featureViaTrailPoints66.Dispose();
        }
      });
     
    }

    #endregion

    #region Helper functions

    private static Feature GetFeature(Geodatabase geodatabase, string featureClassName, long objectID)
    {
      using (FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(featureClassName))
      {
        QueryFilter queryFilter = new QueryFilter()
        {
          ObjectIDs = new List<long>() { objectID }
        };

        using (RowCursor cursor = featureClass.Search(queryFilter))
        {
          System.Diagnostics.Debug.Assert(cursor.MoveNext());
          return (Feature)cursor.Current;
        }
      }
    }

    #endregion
  }
}
