/*

   Copyright 2020 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsLayers.Helpers
{
  internal static class GL_Extensions
  {

    //public static bool CanGroupSelection(this ArcGIS.Desktop.Mapping.GraphicsLayer graphicsLayer)
    //{
    //  var elements = graphicsLayer.GetSelectedElements().ToList();
    //  if (elements?.Count() < 2)//must be at least 2.
    //    return false;
    //  return SameParent(elements);
    //}

    public static bool CanGroupSelection(this ArcGIS.Desktop.Mapping.GraphicsLayer graphicsLayer)
    {
      var elements = graphicsLayer.GetSelectedElements().ToList();
      if (elements?.Count() < 2)//must be at least 2.
        return false;
      return SameParent(elements);
    }

    public static bool CanUnGroupSelection(this ArcGIS.Desktop.Mapping.GraphicsLayer graphicsLayer)
    {
      var elements = graphicsLayer.GetSelectedElements().ToList();
      if (elements?.Any() == false)//must be at least 1.
        return false;
      return AllAreGroupElementsSameParent(elements);
    }

    private static bool SameParent(IList<Element> elements)
    {
      var first_parent = elements.First().GetParent();
      foreach (var element in elements)
      {
        var parent = element.GetParent();
        if (parent != first_parent)
          return false;
      }
      return true;
    }

    private static bool AllAreGroupElementsSameParent(IList<Element> elements)
    {
      if (elements?.Any() == false)//must be at least 1.
        return false;
      if (!SameParent(elements))
        return false;
      return elements.Count() == elements.OfType<GroupElement>().Count();
    }

    /// <summary>
    /// Copy the elements to an offset location
    /// </summary>
    /// <param name="graphicsLayer"></param>
    /// <param name="elements"></param>
    internal static void CustomCopyElements(this GraphicsLayer graphicsLayer, IEnumerable<Element> elements)
    {
      if (elements.Count() == 0) return;
      //Copy the elements.
      var copyElements = graphicsLayer.CopyElements(elements);
      //Iterate through the elements to move the anchor point for the copy.
      foreach (var element in copyElements)
      {
        var elementPoly = PolygonBuilder.CreatePolygon(element.GetBounds());
        var pointsList = elementPoly.Copy2DCoordinatesToList();
        element.SetAnchorPoint(pointsList[1]);
      }
    }
  }

}
