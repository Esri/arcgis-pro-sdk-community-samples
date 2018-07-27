using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifyNewlyAddedFeatures
{
  public static class GeometryExtension
  {
    /// <summary>
    /// Checks if a geometry is null or empty
    /// </summary>
    /// <param name="geometry"></param>
    /// <returns>returns true if the geometry is null or empty</returns>
    public static bool IsNullOrEmpty(this Geometry geometry)
    {
      if (geometry == null) return true;
      return geometry.IsEmpty;
    }
  }
}
