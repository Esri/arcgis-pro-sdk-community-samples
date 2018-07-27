using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifyNewlyAddedFeatures
{
  /// <summary>
  /// Extension method to search and retrieve rows
  /// </summary>
  public static class LayerExtensions
  {
    /// <summary>
    /// Performs a spatial query against a feature layer.
    /// </summary>
    /// <remarks>It is assumed that the feature layer and the search geometry are using the same spatial reference.</remarks>
    /// <param name="searchLayer">The feature layer to be searched.</param>
    /// <param name="searchGeometry">The geometry used to perform the spatial query.</param>
    /// <param name="spatialRelationship">The spatial relationship used by the spatial filter.</param>
    /// <returns>Cursor containing the features that satisfy the spatial search criteria.</returns>
    public static RowCursor Search(this BasicFeatureLayer searchLayer, Geometry searchGeometry, SpatialRelationship spatialRelationship)
    {
      RowCursor rowCursor = null;
      // define a spatial query filter
      var spatialQueryFilter = new SpatialQueryFilter
      {
        // passing the search geometry to the spatial filter
        FilterGeometry = searchGeometry,
        // define the spatial relationship between search geometry and feature class
        SpatialRelationship = spatialRelationship
      };
      // apply the spatial filter to the feature layer in question
      rowCursor = searchLayer.Search(spatialQueryFilter);
      return rowCursor;
    }
  }
}
