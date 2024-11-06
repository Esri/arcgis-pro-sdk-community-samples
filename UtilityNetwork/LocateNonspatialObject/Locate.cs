using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ArcGIS.Core.Data.UtilityNetwork;
using Element = ArcGIS.Core.Data.UtilityNetwork.Element;

namespace LocateNonspatialObject
{
  internal class Locate : Button
  {
    protected override void OnClick()
    {
      Element unElement = null;

      QueuedTask.Run(() =>
      {
        // Object element for which the spatial containers or ancestors will be identified from an adjacency graph
        string unObjectDatasetName = "CommunicationsJunctionObject";
        Guid unObjectGlobalId = new Guid("30c14e2e-19c6-4091-ac10-fb3e024b358a");

        // Get the UN layer from the Map TOC
        UtilityNetworkLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<UtilityNetworkLayer>().First();

        // Find the utility network's object element
        QueryFilter queryFilter = new QueryFilter { WhereClause = $"GLOBALID = '{{{unObjectGlobalId}}}'" };
        using (UtilityNetwork utilityNetwork = layer.GetUtilityNetwork())
        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        using (NetworkSource junctionObjectNetworkSource = utilityNetworkDefinition.GetNetworkSource(unObjectDatasetName))
        using (Table junctionObjectTable = utilityNetwork.GetTable(junctionObjectNetworkSource))
        using (RowCursor rowCursor = junctionObjectTable.Search(queryFilter, false))
        {
          if (rowCursor.MoveNext())
          {
            using (Row row = rowCursor.Current)
            {
              unElement = utilityNetwork.CreateElement(row);
            }
          }

          // Ascending traversal to find associated elements
          TraverseAssociationsDescription traverseAssociationsDescription = new TraverseAssociationsDescription(TraversalDirection.Ascending);
          TraverseAssociationsResult traverseAssociationsResult = utilityNetwork.TraverseAssociations(new List<Element>(){unElement}, traverseAssociationsDescription);
          IReadOnlyList<Association> associations = traverseAssociationsResult.Associations;

          // Adjacency dataset to hold association results
          Dictionary<Element, HashSet<Element>> networkElements = new Dictionary<Element, HashSet<Element>>();

          // Build adjacency dataset
          foreach (Association association in associations)
          {
            if (networkElements.ContainsKey(association.FromElement))
            {
              networkElements[association.FromElement].Add(association.ToElement);
            }
            else
            {
              networkElements.Add(association.FromElement, new HashSet<Element>() { association.ToElement });
            }
          }

          // Traverse adjacency dataset to find ancestors or spatial containers
          List<Element> allSpatialAncestors = GetSpatialAncestors(networkElements, unElement);

          // Get the first spatial container
          Element firstSpatialAncestor = allSpatialAncestors.First();
          MessageBox.Show($"The first spatial container of the CommunicationsJunctionObject (Global ID: 30c14e2e-19c6-4091-ac10-fb3e024b358a) " +
                          $"is a {firstSpatialAncestor.AssetType.Name} with Global ID: {firstSpatialAncestor.GlobalID}");

          foreach (Element spatialAncestor in allSpatialAncestors)
          {
            // If you want to do something with the other spatial containers, put that code here
          }
        }
      });
    }

    /// <summary>
    /// Gets a unique set of spatial ancestors or spatial containers of an object dataset type
    /// </summary>
    /// <param name="networkElements">Adjacency object of elements to traverse through to find the ancestor </param>
    /// <param name="element">Child element from where an ancestor or spatial container search starts </param>
    /// <returns>A sorted unique set of spatial containers in the adjacency list </returns>
    private static List<Element> GetSpatialAncestors(Dictionary<Element, HashSet<Element>> networkElements, Element element)
    {
      List<Element> ancestors = new List<Element>();
      FindAncestorsHelper(networkElements, element, ancestors);
      return ancestors;
    }

    /// <summary>
    /// Helper method to find the spatial ancestors or spatial containers recursively
    /// </summary>
    /// <param name="networkElements">Adjacency object of elements to traverse through to find the ancestor </param>
    /// <param name="element">Child element from where an ancestor or spatial container search starts </param>
    /// <param name="ancestors">A sorted unique set of spatial containers</param>
    private static void FindAncestorsHelper(Dictionary<Element, HashSet<Element>> networkElements, Element element, List<Element> ancestors)
    {
      foreach (KeyValuePair<Element, HashSet<Element>> kvpElements in networkElements)
      {
        if (kvpElements.Value.Any(e => e.GlobalID.Equals(element.GlobalID)))
        {
          Element keyElement = kvpElements.Key;

          switch (keyElement.NetworkSource.UsageType)
          {
            case SourceUsageType.EdgeObject or SourceUsageType.JunctionObject:
              break;
            default:
              // Add the current parent to the spatial ancestors list
              if (ancestors.Contains(keyElement)) continue;

              ancestors.Add(kvpElements.Key);
              break;
          }

          // Recursively find spatial ancestors of the current parent
          FindAncestorsHelper(networkElements, kvpElements.Key, ancestors);
        }
      }
    }
  }
}
